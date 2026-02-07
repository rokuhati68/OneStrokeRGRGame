using UnityEngine;

public class Enemy : MonoBehaviour,ITileEffect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int Hp;
    private int Atk;
    private PlayerStatus player;

    public void SetUp(int _addValue, PlayerStatus _player)
    {
        Hp = _addValue*2;
        Atk = _addValue;
        player = _player;
    }
    public void OnPlayer()
    {
        TakeDamage(player.Atk);


    }

    void TakeDamage(int damage)
    {
        Hp -= damage;
    }
}
