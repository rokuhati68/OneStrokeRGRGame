using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    private bool isVisited = false;

    private PathManager pathManager;

    void Start()
    {
        pathManager = FindFirstObjectByType<PathManager>();
    }
    void OnMoouseEnter()
    {
        if(Input.GetMouseButton(0))
        {
            pathManager.TryAddTile(this);
        }
    }
}