using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class Tile : MonoBehaviour,IPointerEnterHandler
{
    public int idX;
    public int idY;
    [SerializeField] private Image tileImage;
    [SerializeField] private TextMeshProUGUI centerText;
    [SerializeField] public TextMeshProUGUI HpText;
    [SerializeField] private TextMeshProUGUI AtkText;
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
            Debug.Log("On");
            bool isAdd = pathManager.TryAddTile(this);
            if (isAdd)
            {
            Debug.Log("Add");
            tileEffect.OnPlayer();
            player.AddAtk(1);
            }
        }
    }
    public void GetTileEffect()
    {
        tileEffect = GetComponent<ITileEffect>();
    }

    public void SetAtkVisual(Sprite newSprite, string textValue)
    {
        tileImage.sprite = newSprite;
        centerText.text = textValue;
    }
    public void SetEnemyVisual(Sprite newSprite, string textHp, string textAtk)
    {
        tileImage.sprite = newSprite;
        HpText.text = textHp;
        AtkText.text = textAtk;
    }
    public void SetNormalVisual(Sprite newSprite)
    {
        tileImage.sprite = newSprite;
    }
    public void SetHealVisual(Sprite newSprite, string textValue)
    {
        tileImage.sprite = newSprite;
        centerText.text = textValue;
    }
}