using UnityEngine;

public class AtkAddTile : MonoBehaviour,ITileEffect
{
    public int addValue;
    private PlayerStatus player;
    public void SetUp(int _addValue, PlayerStatus _player)
    {
        addValue = _addValue;
        player = _player;
    }

    public void OnPlayer()
    {
        player.AddAtk(addValue);
    }

    
}