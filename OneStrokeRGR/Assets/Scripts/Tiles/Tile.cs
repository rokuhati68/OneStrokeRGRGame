using UnityEngine;
using UnityEngine.EventSystems;
public class Tile : MonoBehaviour,IPointerEnterHandler
{
    public int idX;
    public int idY;
    private bool isVisited = false;
    public ITileEffect tileEffect;

    private PathManager pathManager;
    public PlayerStatus player;

    void Start()
    {
        pathManager = FindFirstObjectByType<PathManager>();
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        if(Input.GetMouseButton(0))
        {
            bool isAdd = pathManager.TryAddTile(this);
            if (isAdd)
            {
            tileEffect.OnPlayer();
            }
        }
    }
    public void GetTileEffect()
    {
        tileEffect = GetComponent<ITileEffect>();
    }
}