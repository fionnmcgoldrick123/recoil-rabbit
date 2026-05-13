using UnityEngine;

public class LevelNode : MonoBehaviour
{
    [SerializeField] public string sceneName;

    [Header("Connections")]
    [SerializeField] public LevelNode rightNode;
    [SerializeField] public MapPath pathRight;

    [SerializeField] public LevelNode leftNode;
    [SerializeField] public MapPath pathLeft;

    [SerializeField] public LevelNode upNode;
    [SerializeField] public MapPath pathUp;

    [SerializeField] public LevelNode downNode;
    [SerializeField] public MapPath pathDown;

    private void OnMouseDown()
    {
        MiniPlayer player = FindFirstObjectByType<MiniPlayer>();
        if (player != null)
            player.MoveToNode(this);
    }

    public MapPath GetPathTo(LevelNode target)
    {
        if (target == rightNode) return pathRight;
        if (target == leftNode) return pathLeft;
        if (target == upNode) return pathUp;
        if (target == downNode) return pathDown;
        return null;
    }

    public bool IsConnectedTo(LevelNode node)
    {
        return node == rightNode || node == leftNode || node == upNode || node == downNode;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
