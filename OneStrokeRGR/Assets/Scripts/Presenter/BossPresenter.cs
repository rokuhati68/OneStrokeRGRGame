using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// ボスの特殊行動を制御するPresenter
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
        /// ボス行動を実行
        /// 要件: 6.2, 6.3
        /// </summary>
        public async UniTask ExecuteBossAction(Enemy boss)
        {
            if (boss == null || !boss.IsBoss)
            {
                Debug.LogWarning("BossPresenter: ボスではありません");
                return;
            }

            if (!boss.ShouldPerformBossAction())
            {
                Debug.Log($"BossPresenter: ボス行動のタイミングではありません（ターン: {boss.TurnsSinceLastAction}/{boss.BossActionInterval}）");
                return;
            }

            Debug.Log("BossPresenter: ボス行動実行！");

            // ボス行動タイプをランダム選択
            BossActionType actionType = SelectRandomBossAction();

            // 行動に応じた処理を実行
            switch (actionType)
            {
                case BossActionType.DisableAttackBoost:
                    await DisableAttackBoostTiles();
                    break;

                case BossActionType.HealSelf:
                    await HealBoss(boss, boss.MaxHP / 4); // 最大HPの25%回復
                    break;

                case BossActionType.SpawnThorns:
                    await SpawnThornTiles(3); // 3個のとげマス生成
                    break;

                case BossActionType.SpawnWalls:
                    await SpawnWallTiles(2); // 2個の壁マス生成
                    break;
            }

            // ボス行動カウンターをリセット
            boss.ResetBossActionCounter();
        }

        /// <summary>
        /// ランダムなボス行動タイプを選択
        /// 要件: 6.3
        /// </summary>
        public BossActionType SelectRandomBossAction()
        {
            var values = System.Enum.GetValues(typeof(BossActionType));
            int randomIndex = Random.Range(0, values.Length);
            BossActionType selected = (BossActionType)values.GetValue(randomIndex);

            Debug.Log($"BossPresenter: ボス行動選択 - {selected}");
            return selected;
        }

        /// <summary>
        /// 攻撃力上昇マスを無効化（効果なしマスに変換）
        /// 要件: 6.3
        /// </summary>
        public async UniTask DisableAttackBoostTiles()
        {
            Debug.Log("BossPresenter: 攻撃力上昇マス無効化！");

            int disabledCount = 0;

            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = gameState.Board.GetTile(pos);

                    if (tile != null && tile.Type == TileType.AttackBoost)
                    {
                        // 効果なしマスに置き換え
                        EmptyTile emptyTile = new EmptyTile();
                        gameState.Board.SetTile(pos, emptyTile);
                        disabledCount++;
                    }
                }
            }

            Debug.Log($"BossPresenter: {disabledCount}個の攻撃力上昇マスを無効化しました");
            await UniTask.Yield();
        }

        /// <summary>
        /// ボスのHP回復
        /// 要件: 6.3
        /// </summary>
        public async UniTask HealBoss(Enemy boss, int amount)
        {
            if (boss == null)
            {
                return;
            }

            Debug.Log($"BossPresenter: ボスが{amount}HP回復！");
            boss.Heal(amount);

            await UniTask.Yield();
        }

        /// <summary>
        /// とげマスを生成
        /// 要件: 6.4
        /// </summary>
        public async UniTask SpawnThornTiles(int count)
        {
            Debug.Log($"BossPresenter: とげマスを{count}個生成！");

            List<Vector2Int> emptyPositions = FindEmptyPositions();

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("BossPresenter: 空きマスがありません");
                return;
            }

            // ランダムに位置を選択
            int spawnCount = Mathf.Min(count, emptyPositions.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, emptyPositions.Count);
                Vector2Int pos = emptyPositions[randomIndex];
                emptyPositions.RemoveAt(randomIndex);

                // とげマスを生成（ダメージ1）
                ThornTile thornTile = TileFactory.CreateThornTile(1);
                gameState.Board.SetTile(pos, thornTile);
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// 壁マスを生成
        /// 要件: 6.5
        /// </summary>
        public async UniTask SpawnWallTiles(int count)
        {
            Debug.Log($"BossPresenter: 壁マスを{count}個生成！");

            List<Vector2Int> emptyPositions = FindEmptyPositions();

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("BossPresenter: 空きマスがありません");
                return;
            }

            // ランダムに位置を選択
            int spawnCount = Mathf.Min(count, emptyPositions.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int randomIndex = Random.Range(0, emptyPositions.Count);
                Vector2Int pos = emptyPositions[randomIndex];
                emptyPositions.RemoveAt(randomIndex);

                // 壁マスを生成
                WallTile wallTile = TileFactory.CreateWallTile();
                gameState.Board.SetTile(pos, wallTile);
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// 空いている位置（効果なしマス）を検索
        /// </summary>
        private List<Vector2Int> FindEmptyPositions()
        {
            List<Vector2Int> emptyPositions = new List<Vector2Int>();

            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = gameState.Board.GetTile(pos);

                    // 効果なしマスのみを対象
                    if (tile != null && tile.Type == TileType.Empty)
                    {
                        emptyPositions.Add(pos);
                    }
                }
            }

            return emptyPositions;
        }
    }
}
