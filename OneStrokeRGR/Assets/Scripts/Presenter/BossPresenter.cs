using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// 敵の行動を制御するPresenter
    /// ボス・通常敵問わず、行動パターンに従って行動を実行する
    /// 要件: 6.2, 6.3, 6.4, 6.5
    /// </summary>
    public class BossPresenter
    {
        private GameState gameState;

        public BossPresenter(GameState state)
        {
            gameState = state;
        }

        /// <summary>
        /// 敵の行動を実行（行動パターンに基づく）
        /// </summary>
        /// <returns>変更されたタイル位置のリスト。行動しなかった場合はnull</returns>
        public async UniTask<List<Vector2Int>> ExecuteEnemyAction(Enemy enemy)
        {
            if (enemy == null || !enemy.HasActionPattern)
            {
                return null;
            }

            if (!enemy.ShouldPerformAction())
            {
                Debug.Log($"BossPresenter: 行動のタイミングではありません（残り{enemy.TurnsUntilAction}ターン）");
                return null;
            }

            EnemyActionEntry action = enemy.GetCurrentAction();
            if (action == null) return null;

            string typeLabel = enemy.IsBoss ? "ボス" : "敵";
            Debug.Log($"BossPresenter: {typeLabel}行動実行！ タイプ={action.actionType}, 値={action.value}");

            List<Vector2Int> changedPositions = new List<Vector2Int>();

            // 行動に応じた処理を実行
            switch (action.actionType)
            {
                case BossActionType.Attack:
                    AttackPlayer(enemy, action.value);
                    break;

                case BossActionType.DisableAttackBoost:
                    changedPositions = DisableTiles(TileType.AttackBoost, action.value);
                    break;

                case BossActionType.HealSelf:
                    HealEnemy(enemy, action.value);
                    break;

                case BossActionType.SpawnThorns:
                    changedPositions = SpawnThornTiles(action.value);
                    break;

                case BossActionType.SpawnWalls:
                    changedPositions = SpawnWallTiles(action.value);
                    break;

                case BossActionType.DisableHeal:
                    changedPositions = DisableTiles(TileType.HPRecovery, action.value);
                    break;

                case BossActionType.DisableGold:
                    changedPositions = DisableTiles(TileType.Gold, action.value);
                    break;

                case BossActionType.DecreaseAttack:
                    changedPositions = DecreaseAttackTiles(action.value);
                    break;

                case BossActionType.DecreaseGold:
                    changedPositions = DecreaseGoldTiles(action.value);
                    break;
            }

            // 次の行動へ進める
            enemy.AdvanceToNextAction();

            await UniTask.Yield();
            return changedPositions;
        }

        /// <summary>
        /// プレイヤーに攻撃
        /// </summary>
        private void AttackPlayer(Enemy enemy, int damage)
        {
            string typeLabel = enemy.IsBoss ? "ボス" : "敵";
            Debug.Log($"BossPresenter: {typeLabel}がプレイヤーに{damage}ダメージ！");
            gameState.Player.TakeDamage(damage);
        }

        /// <summary>
        /// 指定タイプのタイルを無効化（効果なしマスに変換）
        /// </summary>
        /// <returns>変更されたタイル位置のリスト</returns>
        private List<Vector2Int> DisableTiles(TileType targetType, int amount)
        {
            List<Vector2Int> changed = new List<Vector2Int>();
            List<Vector2Int> targetTiles = FindPositions(targetType);

            int changeCnt = Mathf.Min(amount, targetTiles.Count);
            for (int i = 0; i < changeCnt; i++)
            {
                int randomIndex = Random.Range(0, targetTiles.Count);
                Vector2Int pos = targetTiles[randomIndex];
                targetTiles.RemoveAt(randomIndex);

                EmptyTile emptyTile = TileFactory.CreateEmptyTile();
                gameState.Board.SetTile(pos, emptyTile);
                changed.Add(pos);
            }

            Debug.Log($"BossPresenter: {targetType}マスを{changed.Count}個無効化");
            return changed;
        }

        /// <summary>
        /// 敵のHP回復
        /// </summary>
        private void HealEnemy(Enemy enemy, int amount)
        {
            if (enemy == null) return;

            string typeLabel = enemy.IsBoss ? "ボス" : "敵";
            Debug.Log($"BossPresenter: {typeLabel}が{amount}HP回復！");
            enemy.Heal(amount);
        }

        /// <summary>
        /// とげマスを生成
        /// </summary>
        /// <returns>変更されたタイル位置のリスト</returns>
        private List<Vector2Int> SpawnThornTiles(int count)
        {
            List<Vector2Int> changed = new List<Vector2Int>();
            List<Vector2Int> emptyPositions = FindPositions(TileType.Empty);

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("BossPresenter: 空きマスがありません");
                return changed;
            }

            int spawnCount = Mathf.Min(count, emptyPositions.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, emptyPositions.Count);
                Vector2Int pos = emptyPositions[randomIndex];
                emptyPositions.RemoveAt(randomIndex);

                ThornTile thornTile = TileFactory.CreateThornTile(1);
                gameState.Board.SetTile(pos, thornTile);
                changed.Add(pos);
            }

            Debug.Log($"BossPresenter: とげマスを{changed.Count}個生成");
            return changed;
        }

        /// <summary>
        /// 壁マスを生成
        /// </summary>
        /// <returns>変更されたタイル位置のリスト</returns>
        private List<Vector2Int> SpawnWallTiles(int count)
        {
            List<Vector2Int> changed = new List<Vector2Int>();
            List<Vector2Int> emptyPositions = FindPositions(TileType.Empty);

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("BossPresenter: 空きマスがありません");
                return changed;
            }

            int spawnCount = Mathf.Min(count, emptyPositions.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, emptyPositions.Count);
                Vector2Int pos = emptyPositions[randomIndex];
                emptyPositions.RemoveAt(randomIndex);

                WallTile wallTile = TileFactory.CreateWallTile();
                gameState.Board.SetTile(pos, wallTile);
                changed.Add(pos);
            }

            Debug.Log($"BossPresenter: 壁マスを{changed.Count}個生成");
            return changed;
        }

        /// <summary>
        /// 攻撃力上昇マスの値を減少
        /// </summary>
        /// <returns>変更されたタイル位置のリスト</returns>
        private List<Vector2Int> DecreaseAttackTiles(int value)
        {
            List<Vector2Int> changed = new List<Vector2Int>();
            List<Vector2Int> attackPositions = FindPositions(TileType.AttackBoost);

            if (attackPositions.Count == 0) return changed;

            foreach (Vector2Int pos in attackPositions)
            {
                var attackBoostTile = TileFactory.CreateTileByType(TileType.AttackBoost, gameState.SpawnConfig, value);
                gameState.Board.SetTile(pos, attackBoostTile);
                changed.Add(pos);
            }

            Debug.Log($"BossPresenter: 攻撃力マスを{changed.Count}個減少");
            return changed;
        }

        /// <summary>
        /// ゴールドマスの値を減少
        /// </summary>
        /// <returns>変更されたタイル位置のリスト</returns>
        private List<Vector2Int> DecreaseGoldTiles(int value)
        {
            List<Vector2Int> changed = new List<Vector2Int>();
            List<Vector2Int> goldPositions = FindPositions(TileType.Gold);

            if (goldPositions.Count == 0) return changed;

            foreach (Vector2Int pos in goldPositions)
            {
                var goldTile = TileFactory.CreateTileByType(TileType.Gold, gameState.SpawnConfig, value);
                gameState.Board.SetTile(pos, goldTile);
                changed.Add(pos);
            }

            Debug.Log($"BossPresenter: ゴールドマスを{changed.Count}個減少");
            return changed;
        }

        /// <summary>
        /// 指定タイプのタイル位置を検索
        /// </summary>
        private List<Vector2Int> FindPositions(TileType targetTile)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = gameState.Board.GetTile(pos);

                    if (tile != null && tile.Type == targetTile)
                    {
                        positions.Add(pos);
                    }
                }
            }

            return positions;
        }
    }
}
