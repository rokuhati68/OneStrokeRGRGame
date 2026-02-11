using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 5×5のゲームボードを管理するクラス
    /// 要件: 7.1, 7.2, 7.3, 10.1
    /// </summary>
    public class Board
    {
        private Tile[,] tiles = new Tile[5, 5];
        private List<Enemy> enemies = new List<Enemy>();

        /// <summary>ボードのサイズ（5×5）</summary>
        public const int BoardSize = 5;

        /// <summary>
        /// 指定された位置のタイルを取得
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>タイル（範囲外の場合はnull）</returns>
        public Tile GetTile(Vector2Int position)
        {
            if (!IsValidPosition(position))
            {
                Debug.LogWarning($"Board.GetTile: 無効な位置 {position}");
                return null;
            }

            return tiles[position.x, position.y];
        }

        /// <summary>
        /// 指定された位置にタイルを設定
        /// 要件: 7.2, 7.3
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="tile">設定するタイル</param>
        public void SetTile(Vector2Int position, Tile tile)
        {
            if (!IsValidPosition(position))
            {
                Debug.LogWarning($"Board.SetTile: 無効な位置 {position}");
                return;
            }

            tiles[position.x, position.y] = tile;

            if (tile != null)
            {
                tile.Position = position;
            }
        }

        /// <summary>
        /// すべての敵を取得
        /// </summary>
        /// <returns>敵のリスト</returns>
        public List<Enemy> GetEnemies()
        {
            return new List<Enemy>(enemies);
        }

        /// <summary>
        /// 敵を削除
        /// </summary>
        /// <param name="enemy">削除する敵</param>
        public void RemoveEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("Board.RemoveEnemy: enemyがnullです");
                return;
            }

            enemies.Remove(enemy);
            Debug.Log($"Board: 敵を削除 (位置: {enemy.Position})");
        }

        /// <summary>
        /// 敵を追加
        /// 要件: 10.3
        /// </summary>
        /// <param name="enemy">追加する敵</param>
        public void AddEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("Board.AddEnemy: enemyがnullです");
                return;
            }

            enemies.Add(enemy);
            Debug.Log($"Board: 敵を追加 (位置: {enemy.Position}, HP: {enemy.MaxHP}, 攻撃: {enemy.AttackPower})");
        }

        /// <summary>
        /// 位置が有効な範囲内か判定
        /// 要件: 10.1
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>有効な場合true</returns>
        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < BoardSize &&
                   position.y >= 0 && position.y < BoardSize;
        }

        /// <summary>
        /// 指定された位置のタイルを再生成
        /// 要件: 7.1, 7.2, 7.3
        /// </summary>
        /// <param name="positions">再生成する位置のリスト</param>
        /// <param name="config">タイル生成設定</param>
        public void RegenerateTiles(List<Vector2Int> positions, Config.TileSpawnConfig config)
        {
            if (positions == null || positions.Count == 0)
            {
                return;
            }

            Debug.Log($"Board: {positions.Count}個のタイルを再生成");

            foreach (var pos in positions)
            {
                // 壁マスの場合は再生成しない（要件: 7.5）
                var currentTile = GetTile(pos);
                if (currentTile != null && currentTile.Type == TileType.Wall)
                {
                    Debug.Log($"Board: {pos}は壁マスのため再生成スキップ");
                    continue;
                }

                // 新しいタイルを生成（TileFactoryで生成する想定）
                // ここでは一時的にEmptyTileを設定
                var newTile = new EmptyTile();
                SetTile(pos, newTile);
            }
        }

        /// <summary>
        /// ボード全体をクリア
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    tiles[x, y] = null;
                }
            }

            enemies.Clear();
            Debug.Log("Board: ボードをクリア");
        }

        /// <summary>
        /// ボードの状態をデバッグ出力
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log("=== Board State ===");
            for (int y = BoardSize - 1; y >= 0; y--)
            {
                string row = "";
                for (int x = 0; x < BoardSize; x++)
                {
                    var tile = tiles[x, y];
                    if (tile == null)
                    {
                        row += "[?] ";
                    }
                    else
                    {
                        switch (tile.Type)
                        {
                            case TileType.Empty: row += "[ ] "; break;
                            case TileType.AttackBoost: row += "[A] "; break;
                            case TileType.HPRecovery: row += "[H] "; break;
                            case TileType.Gold: row += "[G] "; break;
                            case TileType.Enemy: row += "[E] "; break;
                            case TileType.Thorn: row += "[T] "; break;
                            case TileType.Wall: row += "[W] "; break;
                        }
                    }
                }
                Debug.Log(row);
            }
            Debug.Log($"敵の数: {enemies.Count}");
        }
    }
}
