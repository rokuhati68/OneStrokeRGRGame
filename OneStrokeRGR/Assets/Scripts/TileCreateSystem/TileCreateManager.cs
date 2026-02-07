using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class TileCreateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Sprite AtkSprite;
    public Sprite EnemySprite;
    public PlayerStatus player;
    public List<GameObject> tiles;
    void Start()
    {
        foreach(GameObject tileObject in tiles)
        {
            int rnd = Random.Range(1,10);
            if(rnd % 3 == 0)
            {
                AddEnemyComponent(tileObject);
            }
            else
            {
                AddAtkComponent(tileObject);
            }
            Tile tile = tileObject.GetComponent<Tile>();
            tile.GetTileEffect();
        }
    }
    void AddAtkComponent(GameObject targetTile)
    {
        int addValue = 2;
        AtkAddTile newAtkAddTile = targetTile.AddComponent<AtkAddTile>();
        Tile tile = targetTile.GetComponent<Tile>();
        tile.SetAtkVisual(AtkSprite,addValue.ToString());
        newAtkAddTile.SetUp(addValue,player);
        
    }

    void AddEnemyComponent(GameObject targetTile)
    {
        int Hp = 4;
        int Atk = 2;
        Enemy newEnemyTile = targetTile.AddComponent<Enemy>();
        Tile tile = targetTile.GetComponent<Tile>();
        tile.SetEnemyVisual(EnemySprite,Hp.ToString(),Atk.ToString());
        newEnemyTile.SetUp(Hp,Atk,player,tile.HpText);
        
    }
}
