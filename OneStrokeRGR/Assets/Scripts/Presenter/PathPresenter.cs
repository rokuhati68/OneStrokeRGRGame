using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// パス描画と検証を制御するPresenter
    /// 要件: 1.1, 1.2, 1.3, 1.4
    /// </summary>
    public class PathPresenter
    {
        private GameState gameState;

        public PathPresenter(GameState state)
        {
            gameState = state;
        }

        /// <summary>
        /// パス全体の検証
        /// 要件: 1.1, 1.2, 1.3, 1.4
        /// </summary>
        /// <param name="path">検証するパス</param>
        /// <param name="playerPosition">プレイヤーの現在位置</param>
        /// <returns>有効な場合true</returns>
        public bool ValidatePath(List<Vector2Int> path, Vector2Int playerPosition)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("PathPresenter: パスが空です");
                return false;
            }

            // プレイヤー位置から開始しているか（要件: 1.1）
            if (!IsPathStartingFromPlayer(path, playerPosition))
            {
                Debug.Log("PathPresenter: パスがプレイヤー位置から始まっていません");
                return false;
            }

            // パスが連続しているか（要件: 1.3）
            if (!IsPathContinuous(path))
            {
                Debug.Log("PathPresenter: パスが連続していません");
                return false;
            }

            // 壁マスを含んでいないか（要件: 6.5）
            if (ContainsWallTile(path))
            {
                Debug.Log("PathPresenter: パスに壁マスが含まれています");
                return false;
            }

            // 敵マスで終了しているか（要件: 1.2）
            if (!IsPathEndingOnEnemy(path, gameState.Board))
            {
                Debug.Log("PathPresenter: パスが敵マスで終了していません");
                return false;
            }

            Debug.Log($"PathPresenter: パス検証成功（長さ: {path.Count}）");
            return true;
        }

        /// <summary>
        /// パスがプレイヤー位置から開始しているか判定
        /// 要件: 1.1
        /// </summary>
        public bool IsPathStartingFromPlayer(List<Vector2Int> path, Vector2Int playerPosition)
        {
            if (path == null || path.Count == 0)
            {
                return false;
            }

            return path[0] == playerPosition;
        }

        /// <summary>
        /// パスが敵マスで終了しているか判定
        /// 要件: 1.2
        /// </summary>
        public bool IsPathEndingOnEnemy(List<Vector2Int> path, Board board)
        {
            if (path == null || path.Count == 0 || board == null)
            {
                return false;
            }

            Vector2Int lastPosition = path[path.Count - 1];
            Tile lastTile = board.GetTile(lastPosition);

            return lastTile != null && lastTile.Type == TileType.Enemy;
        }

        /// <summary>
        /// パスが連続しているか判定（隣接かつ重複なし）
        /// 要件: 1.3
        /// </summary>
        public bool IsPathContinuous(List<Vector2Int> path)
        {
            if (path == null || path.Count < 2)
            {
                return path != null && path.Count > 0;
            }

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int current = path[i];

                // 重複チェック
                if (visited.Contains(current))
                {
                    Debug.Log($"PathPresenter: 位置{current}が重複しています");
                    return false;
                }
                visited.Add(current);

                // 次の位置との隣接チェック
                if (i < path.Count - 1)
                {
                    Vector2Int next = path[i + 1];
                    if (!AreAdjacent(current, next))
                    {
                        Debug.Log($"PathPresenter: {current}と{next}が隣接していません");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 2つの位置が隣接しているか判定（上下左右のみ）
        /// </summary>
        private bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);

            // 上下左右のみ（斜めは不可）
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// パスに壁マスが含まれているか判定
        /// 要件: 6.5
        /// </summary>
        private bool ContainsWallTile(List<Vector2Int> path)
        {
            if (path == null || gameState.Board == null)
            {
                return false;
            }

            foreach (var pos in path)
            {
                Tile tile = gameState.Board.GetTile(pos);
                if (tile != null && tile.Type == TileType.Wall)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// パスのプレビュー情報を計算
        /// 要件: UI仕様
        /// </summary>
        public PathPreview CalculatePathPreview(List<Vector2Int> path, Player player, Board board)
        {
            var preview = new PathPreview();

            if (path == null || path.Count == 0 || player == null || board == null)
            {
                return preview;
            }

            // 初期攻撃力（移動による増加）
            int attackPower = path.Count;
            preview.PredictedAttackPower = attackPower;

            int goldSpent = 0;
            int goldGained = 0;
            int hpChange = 0;

            // コンボトラッキング用
            TileType? lastEffectType = null;
            bool isCombo = false;

            // パス上の各タイルの効果を予測
            foreach (var pos in path)
            {
                Tile tile = board.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }

                preview.TileSequence.Add(tile.Type);

                // コンボ判定
                if (lastEffectType.HasValue &&
                    (tile.Type == TileType.AttackBoost || tile.Type == TileType.HPRecovery) &&
                    tile.Type == lastEffectType.Value)
                {
                    isCombo = true;
                }
                else if (tile.Type != TileType.Empty && tile.Type != TileType.Enemy)
                {
                    isCombo = false;
                    lastEffectType = tile.Type;
                }

                switch (tile.Type)
                {
                    case TileType.AttackBoost:
                        var attackTile = tile as AttackBoostTile;
                        if (attackTile != null)
                        {
                            // ゴールド消費（コンボ中は免除）
                            if (!isCombo && player.Gold - goldSpent >= 1)
                            {
                                goldSpent++;
                            }
                            // 攻撃力増加
                            attackPower += attackTile.BoostValue;
                        }
                        break;

                    case TileType.HPRecovery:
                        // ゴールド消費（コンボ中は免除）
                        if (!isCombo && player.Gold - goldSpent >= 1)
                        {
                            goldSpent++;
                            hpChange += 1;
                        }
                        break;

                    case TileType.Gold:
                        var goldTile = tile as GoldTile;
                        if (goldTile != null)
                        {
                            goldGained += goldTile.GoldValue;
                        }
                        break;

                    case TileType.Enemy:
                        var enemyTile = tile as EnemyTile;
                        if (enemyTile != null && enemyTile.Enemy != null)
                        {
                            // 敵が倒せない場合は反撃ダメージを予測
                            if (enemyTile.Enemy.CurrentHP > attackPower)
                            {
                                hpChange -= enemyTile.Enemy.AttackPower;
                            }
                        }
                        break;

                    case TileType.Thorn:
                        var thornTile = tile as ThornTile;
                        if (thornTile != null)
                        {
                            hpChange -= thornTile.Damage;
                        }
                        break;
                }
            }

            // 一筆書きボーナスの適用（全25マス使用時）
            if (path.Count == Board.BoardSize * Board.BoardSize)
            {
                attackPower += player.OneStrokeBonus;
                Debug.Log($"PathPresenter: 一筆書きボーナス+{player.OneStrokeBonus}");
            }

            preview.PredictedAttackPower = attackPower;
            preview.PredictedGoldSpent = goldSpent;
            preview.PredictedGoldGained = goldGained;
            preview.PredictedHPChange = hpChange;

            return preview;
        }
    }
}
