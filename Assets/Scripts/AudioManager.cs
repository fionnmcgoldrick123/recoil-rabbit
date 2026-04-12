using UnityEngine;

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
    [SerializeField] private AudioSource levelGoalSource;

    [Header("SFX Pitch")]
    [Range(0.5f, 1f)] [SerializeField] private float sfxPitchMin = 0.98f;
    [Range(1f, 2f)] [SerializeField] private float sfxPitchMax = 1.02f;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private float revolverShotBaseVolume;
    private float shotgunShotBaseVolume;
    private float jumpBaseVolume;
    private float deathBaseVolume;
    private float levelGoalBaseVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        revolverShotBaseVolume = revolverShotSource != null ? revolverShotSource.volume : 1f;
        shotgunShotBaseVolume = shotgunShotSource != null ? shotgunShotSource.volume : 1f;
        jumpBaseVolume = jumpSource != null ? jumpSource.volume : 1f;
        deathBaseVolume = deathSource != null ? deathSource.volume : 1f;
        levelGoalBaseVolume = levelGoalSource != null ? levelGoalSource.volume : 1f;
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

    public void PlayLevelGoal()
    {
        PlayGameplaySfx(levelGoalSource, levelGoalBaseVolume);
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
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
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
        if (musicSource != null && musicSource.isPlaying)
            musicSource.volume = musicVolume * masterVolume;
    }
}