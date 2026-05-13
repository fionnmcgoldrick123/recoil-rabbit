using UnityEngine;

public class MapPath : MonoBehaviour
{
    private Transform[] _waypoints;

    public Transform[] Waypoints
    {
        get
        {
            if (_waypoints == null || _waypoints.Length == 0)
                PopulateFromChildren();
            return _waypoints;
        }
    }

    private void Awake()
    {
        PopulateFromChildren();
    }

    private void PopulateFromChildren()
    {
        _waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _waypoints[i] = transform.GetChild(i);
    }

    public Transform[] GetOrderedWaypoints(Vector3 fromPosition)
    {
        Transform[] points = Waypoints;
        if (points == null || points.Length == 0)
            return points;

        float distToFirst = Vector3.Distance(fromPosition, points[0].position);
        float distToLast = Vector3.Distance(fromPosition, points[points.Length - 1].position);

        if (distToFirst <= distToLast)
            return points;

        Transform[] reversed = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
            reversed[i] = points[points.Length - 1 - i];
        return reversed;
    }

    private void OnDrawGizmos()
    {
        PopulateFromChildren();
        if (_waypoints == null || _waypoints.Length < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _waypoints.Length - 1; i++)
        {
            if (_waypoints[i] != null && _waypoints[i + 1] != null)
                Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
        }

        Gizmos.color = Color.white;
        foreach (Transform wp in _waypoints)
        {
            if (wp != null)
                Gizmos.DrawWireSphere(wp.position, 0.1f);
        }
    }
}
