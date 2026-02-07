using UnityEngine;
using UnityEngine.EventSystems;
public class Tile : MonoBehaviour,IPointerEnterHandler
{
    public int idX;
    public int idY;
    private bool isVisited = false;

    private PathManager pathManager;

    void Start()
    {
        pathManager = FindFirstObjectByType<PathManager>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("On");
        if(Input.GetMouseButton(0))
        {
            Debug.Log("Add");
            pathManager.TryAddTile(this);
        }
    }
}