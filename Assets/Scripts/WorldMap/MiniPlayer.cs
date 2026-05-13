using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniPlayer : MonoBehaviour
{
    [SerializeField] private LevelNode startNode;
    [SerializeField] private float moveSpeed = 3f;

    private LevelNode _currentNode;
    private bool _isMoving;

    public LevelNode CurrentNode => _currentNode;
    public bool IsMoving => _isMoving;

    private void Start()
    {
        if (startNode == null)
            return;

        _currentNode = startNode;
        transform.position = _currentNode.transform.position;
    }

    private void Update()
    {
        if (_isMoving || _currentNode == null)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            TryMove(_currentNode.rightNode, _currentNode.pathRight);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            TryMove(_currentNode.leftNode, _currentNode.pathLeft);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            TryMove(_currentNode.upNode, _currentNode.pathUp);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            TryMove(_currentNode.downNode, _currentNode.pathDown);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            LoadCurrentLevel();
    }

    public void MoveToNode(LevelNode target)
    {
        if (_isMoving || _currentNode == null || target == null)
            return;

        if (!_currentNode.IsConnectedTo(target))
            return;

        MapPath path = _currentNode.GetPathTo(target);
        if (path == null)
            return;

        StartCoroutine(FollowPath(path, target));
    }

    private void TryMove(LevelNode target, MapPath path)
    {
        if (target == null || path == null)
            return;

        StartCoroutine(FollowPath(path, target));
    }

    private IEnumerator FollowPath(MapPath path, LevelNode destination)
    {
        _isMoving = true;

        Transform[] waypoints = path.GetOrderedWaypoints(transform.position);

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null)
                continue;

            while (Vector2.Distance(transform.position, waypoint.position) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(transform.position, waypoint.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = waypoint.position;
        }

        transform.position = destination.transform.position;
        _currentNode = destination;
        _isMoving = false;
    }

    private void LoadCurrentLevel()
    {
        if (_currentNode == null || string.IsNullOrEmpty(_currentNode.sceneName))
            return;

        SceneManager.LoadScene(_currentNode.sceneName);
    }
}
