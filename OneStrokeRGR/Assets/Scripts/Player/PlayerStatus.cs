using UnityEngine;
public class PlayerStatus : MonoBehaviour
{
    [SerializeField]
    private int Atk;
    [SerializeField]    
    private int Life;
    [SerializeField]
    private int Meat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
    }
    public void UseMeat()
    {
        Meat --;
    }
    public void AddAtk(int addValue)
    {
        Atk += addValue;
    }
}
