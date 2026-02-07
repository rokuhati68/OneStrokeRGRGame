using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class TileCreateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Sprite AtkSprite;
    public Sprite EnemySprite;
    public Sprite NormalSprite;
    public Sprite HealSprite;
    public PlayerStatus player;
    public List<GameObject> tiles;
    void Start()
    {
        foreach(GameObject tileObject in tiles)
        {
            int rnd = Random.Range(1,100);
            if(rnd % 4 == 0)
            {
                AddEnemyComponent(tileObject);
            }
            else if(rnd % 4 == 1)
            {
                AddAtkComponent(tileObject);
            }
            else if(rnd % 4 == 2)
            {
                AddHealComponent(tileObject);
            }
            else if(rnd % 4 == 3)
            {
                AddNormalComponent(tileObject);
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
    void AddHealComponent(GameObject targetTile)
    {
        int addValue = 2;
        HealTile newHealTile = targetTile.AddComponent<HealTile>();
        Tile tile = targetTile.GetComponent<Tile>();
        tile.SetHealVisual(HealSprite,addValue.ToString());
        newHealTile.SetUp(addValue,player);
        
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
    void AddNormalComponent(GameObject targetTile)
    {
        NormalTile newNormalTile =  targetTile.AddComponent<NormalTile>();
        Tile tile = targetTile.GetComponent<Tile>();
        tile.SetNormalVisual(NormalSprite);
    }
}
