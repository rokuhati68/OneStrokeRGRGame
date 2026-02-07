using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerStatus : MonoBehaviour
{
    [SerializeField]
    public int Atk;
    [SerializeField]    
    private int Life;
    [SerializeField]
    private int Meat;
    public TextMeshProUGUI AtkText;
    public TextMeshProUGUI LifeText;
    public TextMeshProUGUI MeatText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AtkText.text = Atk.ToString();
        LifeText.text = Life.ToString();
        MeatText.text = Meat.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetAtk()
    {
        Atk = 0;
    }
    public void TakeDamage()
    {
        Life --;
        LifeText.text = Life.ToString();
    }
    public void UseMeat()
    {
        Meat --;
    }
    public void AddAtk(int addValue)
    {
        Atk += addValue;    
        AtkText.text = Atk.ToString();
    }
    public void Heal()
    {
        Life ++;
        LifeText.text = Life.ToString();
    }
}
