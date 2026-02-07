using UnityEngine;
using System.Collections.Generic;
public class TileCreateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerStatus player;
    public List<GameObject> tiles;
    void Start()
    {
        foreach(GameObject tileObject in tiles)
        {
            AddAtkComponent(tileObject);
            Tile tile = tileObject.GetComponent<Tile>();
            tile.GetTileEffect();
        }
    }
    public void AddAtkComponent(GameObject targetTile)
    {
        int addValue = 2;
        AtkAddTile newAtkAddTile = targetTile.AddComponent<AtkAddTile>();
        newAtkAddTile.SetUp(addValue,player);
    }
}
