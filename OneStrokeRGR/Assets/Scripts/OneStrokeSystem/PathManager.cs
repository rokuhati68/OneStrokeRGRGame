using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class PathManager: MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private List<Tile> path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool TryAddTile(Tile tile)
    {
        if(path.Contains(tile))
        {
            return false;
        }        
        if (path.Count == 0 || IsAdjacent(path[path.Count - 1], tile))
        {
            path.Add(tile);
            tile.GetComponent<Image>().color = Color.cyan; // 通った印
            return true;
        }
        return false;

    }
    private bool IsAdjacent(Tile lastTile, Tile currentTile)
    {
        // 上下左右に隣接しているか（斜めを許容しない場合）
        int diffX = Mathf.Abs(lastTile.idX - currentTile.idX);
        int diffY = Mathf.Abs(lastTile.idY - currentTile.idY);
        bool isAdjacent =  (diffX + diffY == 1) ? true : false;
        return isAdjacent; // 距離が1なら隣接
    }
}
