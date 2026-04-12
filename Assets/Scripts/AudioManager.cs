using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Gameplay SFX Sources")]
    [SerializeField] private AudioSource revolverShotSource;
    [SerializeField] private AudioSource shotgunShotSource;
    [SerializeField] private AudioSource jumpSource;
    [SerializeField] private AudioSource deathSource;
    [SerializeField] private AudioSource enemyDeathSource;
    [SerializeField] private AudioSource levelGoalSource;
    [SerializeField] private AudioSource springSource;

    [Header("SFX Pitch")]
    [Range(0.5f, 1f)] [SerializeField] private float sfxPitchMin = 0.98f;
    [Range(1f, 2f)] [SerializeField] private float sfxPitchMax = 1.02f;

    [Header("Music")]
    [SerializeField] private Transform musicPlaylistRoot;
    [SerializeField] private bool playMusicOnAwake = true;
    [SerializeField] private int startingTrackIndex = 0;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private float musicBaseVolume;
    private float revolverShotBaseVolume;
    private float shotgunShotBaseVolume;
    private float jumpBaseVolume;
    private float deathBaseVolume;
    private float enemyDeathBaseVolume;
    private float levelGoalBaseVolume;
    private float springBaseVolume;

    private int currentMusicIndex = -1;
    private bool isPlaylistPlaying;
    private readonly List<AudioSource> musicTracks = new List<AudioSource>();
    private readonly Dictionary<AudioSource, float> musicTrackBaseVolumes = new Dictionary<AudioSource, float>();
    private AudioSource currentMusicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicBaseVolume = musicSource != null ? musicSource.volume : 1f;

        revolverShotBaseVolume = revolverShotSource != null ? revolverShotSource.volume : 1f;
        shotgunShotBaseVolume = shotgunShotSource != null ? shotgunShotSource.volume : 1f;
        jumpBaseVolume = jumpSource != null ? jumpSource.volume : 1f;
        deathBaseVolume = deathSource != null ? deathSource.volume : 1f;
        enemyDeathBaseVolume = enemyDeathSource != null ? enemyDeathSource.volume : 1f;
        levelGoalBaseVolume = levelGoalSource != null ? levelGoalSource.volume : 1f;
        springBaseVolume = springSource != null ? springSource.volume : 1f;

        CacheMusicTracks();

        if (playMusicOnAwake)
            StartMusicPlaylist();
    }

    private void Update()
    {
        if (!isPlaylistPlaying || musicTracks.Count == 0)
            return;

        if (currentMusicSource == null)
        {
            PlayNextMusicTrack();
            return;
        }

        if (!currentMusicSource.isPlaying)
            PlayNextMusicTrack();
        else
            UpdateMusicVolume();
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
    }

    public void PlayRevolverShot()
    {
        PlayGameplaySfx(revolverShotSource, revolverShotBaseVolume);
    }

    public void PlayShotgunShot()
    {
        PlayGameplaySfx(shotgunShotSource, shotgunShotBaseVolume);
    }

    public void PlayJump()
    {
        PlayGameplaySfx(jumpSource, jumpBaseVolume);
    }

    public void PlayDeath()
    {
        PlayGameplaySfx(deathSource, deathBaseVolume);
    }

    public void PlayEnemyDeath()
    {
        PlayGameplaySfx(enemyDeathSource, enemyDeathBaseVolume);
    }

    public void PlayLevelGoal()
    {
        PlayGameplaySfx(levelGoalSource, levelGoalBaseVolume);
    }

    public void PlaySpringBounce()
    {
        PlayGameplaySfx(springSource, springBaseVolume);
    }

    private void PlayGameplaySfx(AudioSource source, float baseVolume)
    {
        if (source == null || source.clip == null)
            return;

        float minPitch = Mathf.Min(sfxPitchMin, sfxPitchMax);
        float maxPitch = Mathf.Max(sfxPitchMin, sfxPitchMax);

        source.pitch = Random.Range(minPitch, maxPitch);
        source.volume = baseVolume * sfxVolume * masterVolume;
        source.Play();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        isPlaylistPlaying = false;

        StopAllMusicTracks();

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicBaseVolume * musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StartMusicPlaylist()
    {
        if (musicTracks.Count == 0)
            return;

        currentMusicIndex = Mathf.Clamp(startingTrackIndex, 0, musicTracks.Count - 1);
        isPlaylistPlaying = true;
        PlayCurrentMusicTrack();
    }

    public void PlayNextMusicTrack()
    {
        if (musicTracks.Count == 0)
            return;

        currentMusicIndex++;
        if (currentMusicIndex >= musicTracks.Count)
            currentMusicIndex = 0;

        PlayCurrentMusicTrack();
    }

    public void StopMusic()
    {
        isPlaylistPlaying = false;
        currentMusicSource = null;
        musicSource.Stop();
        StopAllMusicTracks();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateMusicVolume()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            float baseVolume = GetMusicBaseVolume(currentMusicSource);
            currentMusicSource.volume = baseVolume * musicVolume * masterVolume;
            return;
        }

        if (musicSource != null && musicSource.isPlaying)
            musicSource.volume = musicBaseVolume * musicVolume * masterVolume;
    }

    private void PlayCurrentMusicTrack()
    {
        if (musicTracks.Count == 0)
            return;

        if (currentMusicIndex < 0 || currentMusicIndex >= musicTracks.Count)
            currentMusicIndex = 0;

        AudioSource nextTrack = musicTracks[currentMusicIndex];
        if (nextTrack == null)
        {
            PlayNextMusicTrack();
            return;
        }

        StopAllMusicTracks();

        currentMusicSource = nextTrack;
        currentMusicSource.loop = false;
        currentMusicSource.volume = GetMusicBaseVolume(currentMusicSource) * musicVolume * masterVolume;
        currentMusicSource.Play();
    }

    private void CacheMusicTracks()
    {
        musicTracks.Clear();
        musicTrackBaseVolumes.Clear();

        Transform playlistRoot = musicPlaylistRoot;
        if (playlistRoot == null)
            playlistRoot = transform.Find("AudioMusic");

        if (playlistRoot == null)
            return;

        for (int i = 0; i < playlistRoot.childCount; i++)
        {
            AudioSource trackSource = playlistRoot.GetChild(i).GetComponent<AudioSource>();
            if (trackSource == null)
                continue;

            trackSource.playOnAwake = false;
            trackSource.loop = false;
            musicTracks.Add(trackSource);
            musicTrackBaseVolumes[trackSource] = trackSource.volume;
        }
    }

    private float GetMusicBaseVolume(AudioSource trackSource)
    {
        if (trackSource != null && musicTrackBaseVolumes.TryGetValue(trackSource, out float baseVolume))
            return baseVolume;

        return 1f;
    }

    private void StopAllMusicTracks()
    {
        for (int i = 0; i < musicTracks.Count; i++)
        {
            AudioSource track = musicTracks[i];
            if (track != null)
                track.Stop();
        }
    }
}