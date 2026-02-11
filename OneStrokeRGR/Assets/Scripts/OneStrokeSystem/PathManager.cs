using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// 注意: このクラスは旧実装です。後で PathDrawingView に置き換えます
public class PathManager: MonoBehaviour
{
    public bool IsActive{get;set;}
    public bool IsPathConfirmed{get; set;}

    // 旧実装 - Tileクラスは削除済み
    // [SerializeField]
    // private List<Tile> path;
    private List<Vector2Int> path = new List<Vector2Int>(); // 一時的に位置のリストに変更
    // 旧実装 - 削除されたTileクラスを参照しているためコメントアウト
    /*
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
    */

    void Update()
    {
        if(!IsActive) return;
        if(Input.GetMouseButtonUp(0))
        {
            if(path.Count > 0)
            {
                IsPathConfirmed = true;
                IsActive = false;
            }
        }
    }

    // 旧実装 - 削除されたTileクラスを参照しているためコメントアウト
    /*
    private bool IsAdjacent(Tile lastTile, Tile currentTile)
    {
        // 上下左右に隣接しているか（斜めを許容しない場合）
        int diffX = Mathf.Abs(lastTile.idX - currentTile.idX);
        int diffY = Mathf.Abs(lastTile.idY - currentTile.idY);
        bool isAdjacent =  (diffX + diffY == 1) ? true : false;
        return isAdjacent; // 距離が1なら隣接
    }
    */

    public void ResetManager()
    {
        IsPathConfirmed = false;
    }
    public void ClearPath()
    {
        path = new List<Vector2Int>();
    }
}
