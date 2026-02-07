using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Enemy : MonoBehaviour,ITileEffect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int Hp;
    private int Atk;
    private PlayerStatus player;
    private TextMeshProUGUI HpText;

    public void SetUp(int _Hp, int _Atk, PlayerStatus _player,TextMeshProUGUI _HpText)
    {
        Hp = _Hp;
        Atk = _Atk;
        player = _player;
        HpText = _HpText;
    }
    public void OnPlayer()
    {
        TakeDamage(player.Atk);


    }

    void TakeDamage(int damage)
    {
        Hp -= damage;
        HpText.text = Hp.ToString();
    }
}
