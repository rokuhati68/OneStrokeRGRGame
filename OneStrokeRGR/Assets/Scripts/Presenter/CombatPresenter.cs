using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// 戦闘ロジックを制御するPresenter
    /// 要件: 1.5, 2.1, 2.2, 2.3, 2.4, 5.1, 5.2, 5.3, 5.4
    /// </summary>
    public class CombatPresenter
    {
        private GameState gameState;

        public CombatPresenter(GameState state)
        {
            gameState = state;
        }

        /// <summary>
        /// パスを実行する（マス効果を順次処理）
        /// 要件: 1.5, 2.1
        /// </summary>
        /// <param name="path">実行するパス</param>
        public async UniTask ExecutePath(List<Vector2Int> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("CombatPresenter: パスが空です");
                return;
            }

            Debug.Log($"CombatPresenter: パス実行開始（長さ: {path.Count}）");

            // 攻撃力をリセット（要件: 2.3）
            gameState.Player.ResetAttackPower();

            // コンボトラッカーの初期化
            ComboTracker comboTracker = new ComboTracker();

            // 移動による攻撃力増加（要件: 2.1）
            gameState.Player.IncreaseAttackPower(path.Count);

            // 一筆書きボーナスの適用（要件: 2.4）
            if (path.Count == Board.BoardSize * Board.BoardSize)
            {
                ApplyOneStrokeBonus(path.Count, gameState.Player);
            }

            // パス上の各マスを順次処理（要件: 1.5）
            List<Vector2Int> visitedPositions = new List<Vector2Int>();
            List<Enemy> survivingEnemies = new List<Enemy>(); // 生き残った敵を記録

            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int pos = path[i];
                Tile tile = gameState.Board.GetTile(pos);

                if (tile == null)
                {
                    Debug.LogWarning($"CombatPresenter: 位置{pos}にタイルがありません");
                    continue;
                }

                // 敵タイルの場合、処理前に敵を記録
                if (tile.Type == TileType.Enemy && tile is EnemyTile enemyTile)
                {
                    Enemy enemy = enemyTile.Enemy;
                    int enemyHPBefore = enemy.CurrentHP;

                    // プレイヤー位置を更新
                    gameState.Player.Position = pos;

                    // タイル効果を処理
                    await ProcessTileEffect(tile, comboTracker);

                    // 敵が生き残った場合、リストに追加
                    if (enemy.IsAlive())
                    {
                        survivingEnemies.Add(enemy);
                        Debug.Log($"CombatPresenter: 敵が生き残りました (HP: {enemyHPBefore} -> {enemy.CurrentHP})");
                    }
                }
                else
                {
                    // プレイヤー位置を更新
                    gameState.Player.Position = pos;

                    // タイル効果を処理
                    await ProcessTileEffect(tile, comboTracker);
                }

                // 訪問済み位置を記録
                visitedPositions.Add(pos);

                // プレイヤーが死亡した場合、処理を中断（要件: 5.4）
                if (!gameState.Player.IsAlive())
                {
                    Debug.Log("CombatPresenter: プレイヤーが死亡しました");
                    gameState.SetGameOver();
                    break;
                }

                // アニメーション待機（実装時に調整）
                await UniTask.Delay(100);
            }

            // ターン終了時の処理
            comboTracker.Reset(); // コンボリセット（要件: 4.4）

            // 訪問したマスをGameStateに記録
            if (visitedPositions.Count > 0)
            {
                gameState.MarkPositionsAsVisited(visitedPositions);
            }

            // 訪問したマスを再生成（要件: 7.1, 7.2）
            if (visitedPositions.Count > 0)
            {
                gameState.Board.RegenerateTiles(visitedPositions, gameState.SpawnConfig);

                // プレイヤーの現在位置を必ず効果なしマスにする
                Vector2Int playerPos = gameState.Player.Position;
                if (visitedPositions.Contains(playerPos))
                {
                    Tile emptyTile = TileFactory.CreateEmptyTile();
                    gameState.Board.SetTile(playerPos, emptyTile);
                    Debug.Log($"CombatPresenter: プレイヤー位置{playerPos}を効果なしマスに設定");
                }

                // 生き残った敵をランダムな位置に再配置
                if (survivingEnemies.Count > 0)
                {
                    // プレイヤーの最終位置を除外した再配置候補を作成
                    Vector2Int playerFinalPos = gameState.Player.Position;
                    List<Vector2Int> availablePositions = visitedPositions.FindAll(pos => pos != playerFinalPos);

                    if (availablePositions.Count == 0)
                    {
                        Debug.LogWarning("CombatPresenter: 敵を再配置できる位置がありません");
                    }
                    else
                    {
                        Debug.Log($"CombatPresenter: {survivingEnemies.Count}体の敵を再配置");
                        foreach (var enemy in survivingEnemies)
                        {
                            // ランダムな再生成マスを選択（プレイヤー位置を除く）
                            Vector2Int newPos = availablePositions[Random.Range(0, availablePositions.Count)];

                            // 敵を再配置
                            enemy.Position = newPos;
                            EnemyTile newEnemyTile = TileFactory.CreateEnemyTile(enemy);
                            gameState.Board.SetTile(newPos, newEnemyTile);

                            Debug.Log($"CombatPresenter: 敵を再配置 {newPos} (HP: {enemy.CurrentHP}/{enemy.MaxHP})");
                        }
                    }
                }
            }

            Debug.Log("CombatPresenter: パス実行完了");
        }

        /// <summary>
        /// タイル効果を処理する
        /// 要件: 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.2
        /// </summary>
        public async UniTask ProcessTileEffect(Tile tile, ComboTracker comboTracker)
        {
            if (tile == null)
            {
                return;
            }

            // ゲームコンテキストの作成
            GameContext context = new GameContext
            {
                IsComboActive = comboTracker.IsComboActive(tile.Type),
                CurrentStage = gameState.CurrentStage,
                IsBossStage = gameState.IsBossStage()
            };

            // コンボトラッカーの更新（要件: 4.1）
            comboTracker.UpdateCombo(tile.Type);

            // タイル効果を適用
            TileEffectResult result = tile.ApplyEffect(gameState.Player, context);

            if (result == null || !result.EffectApplied)
            {
                return;
            }

            // 敵を倒した場合の処理（要件: 5.2, 7.3）
            if (result.DefeatedEnemy != null)
            {
                await ProcessEnemyDefeated(result.DefeatedEnemy);
            }

            // アニメーション待機
            await UniTask.Delay(50);
        }

        /// <summary>
        /// 戦闘処理（ダメージ計算と反撃）
        /// 要件: 5.1, 5.2, 5.3
        /// </summary>
        public async UniTask ProcessCombat(Enemy enemy, int attackPower)
        {
            if (enemy == null)
            {
                return;
            }

            Debug.Log($"CombatPresenter: 敵に攻撃（攻撃力: {attackPower}）");

            // 敵にダメージを与える（要件: 5.1）
            enemy.TakeDamage(attackPower);

            // 敵が倒されたか判定（要件: 5.2）
            if (!enemy.IsAlive())
            {
                await ProcessEnemyDefeated(enemy);
            }
            else
            {
                // 敵が生き残った場合、反撃ダメージ（要件: 5.3）
                gameState.Player.TakeDamage(enemy.AttackPower);

                // プレイヤーが死亡した場合（要件: 5.4）
                if (!gameState.Player.IsAlive())
                {
                    gameState.SetGameOver();
                }
            }
        }

        /// <summary>
        /// 敵撃破時の処理
        /// 要件: 7.3, 8.1
        /// </summary>
        private async UniTask ProcessEnemyDefeated(Enemy enemy)
        {
            Debug.Log($"CombatPresenter: 敵を撃破！");

            // ボードから敵を削除
            gameState.Board.RemoveEnemy(enemy);

            // 敵マスをランダムなマスで置き換え（要件: 7.3）
            Vector2Int enemyPos = enemy.Position;
            Tile newTile = TileFactory.CreateRandomTile(gameState.SpawnConfig);
            gameState.Board.SetTile(enemyPos, newTile);

            // すべての敵が倒された場合、ステージクリア（要件: 8.1）
            if (gameState.Board.GetEnemies().Count == 0)
            {
                gameState.SetStageCleared();
                Debug.Log("CombatPresenter: ステージクリア！");
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// 一筆書きボーナスを適用
        /// 要件: 2.4
        /// </summary>
        public void ApplyOneStrokeBonus(int pathLength, Player player)
        {
            if (pathLength == Board.BoardSize * Board.BoardSize)
            {
                int bonus = player.OneStrokeBonus;
                player.IncreaseAttackPower(bonus);
                Debug.Log($"CombatPresenter: 一筆書きボーナス適用！攻撃力+{bonus}");
            }
        }
    }
}
