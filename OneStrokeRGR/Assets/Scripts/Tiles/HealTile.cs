using UnityEngine;

public class HealTile : MonoBehaviour,ITileEffect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int healValue;
    private PlayerStatus player;
    public void SetUp(int _healValue, PlayerStatus _player)
    {
        healValue = _healValue;
        player = _player;
    }
    public void OnPlayer()
    {
        player.Heal();
    }
}
