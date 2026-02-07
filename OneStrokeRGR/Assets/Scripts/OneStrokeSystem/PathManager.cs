using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class PathManager: MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<Tile> path = new List<Tile>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TryAddTile(Tile tile)
    {
        if(path.Contains(tile))
        {
            path.Remove(tile);
        }        
        if (path.Count == 0 || IsAdjacent(path[path.Count - 1], tile))
        {
            path.Add(tile);
            tile.GetComponent<Image>().color = Color.cyan; // 通った印
        }

    }

    private bool IsAdjacent(Tile lastTile, Tile currentTile)
    {
        // 上下左右に隣接しているか（斜めを許容しない場合）
        float distance = Vector2.Distance(lastTile.gridPosition, currentTile.gridPosition);
        return distance <= 100f; // 距離が1なら隣接
    }
}
