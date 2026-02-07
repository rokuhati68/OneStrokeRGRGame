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
        GetTileEffect();
        tileEffect.Create(3,player);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("On");
        if(Input.GetMouseButton(0))
        {
            Debug.Log("Add");
            bool isAdd = pathManager.TryAddTile(this);
            if (isAdd)
            {
            tileEffect.OnPlayer();
            }
        }
    }
    void GetTileEffect()
    {
        tileEffect = GetComponent<ITileEffect>();
    }
}