using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;
using OneStrokeRGR.View;
using OneStrokeRGR.Sound;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// ゲーム全体のフローを制御するPresenter
    /// 要件: すべての要件を統合
    /// </summary>
    public class GamePresenter : MonoBehaviour
    {
        [Header("設定")]
        public GameConfig gameConfig;

        [Header("View層")]
        public BoardView boardView;
        public UIView uiView;
        public PathDrawingView pathDrawingView;
        public RewardView rewardView;
        public BattleUIView battleUIView;

        private GameState gameState;
        private PathPresenter pathPresenter;
        private CombatPresenter combatPresenter;
        private BossPresenter bossPresenter;
        private RewardPresenter rewardPresenter;
        private int pathMoveCount;
        private List<Sprite> currentEnemySprites = new List<Sprite>();

        private void Awake()
        {
            if (gameConfig == null)
            {
                Debug.LogError("GamePresenter: GameConfigが設定されていません！");
                return;
            }

            // GameStateの初期化
            gameState = new GameState();

            // 各Presenterの初期化
            pathPresenter = new PathPresenter(gameState);
            combatPresenter = new CombatPresenter(gameState);
            bossPresenter = new BossPresenter(gameState);
            rewardPresenter = new RewardPresenter(gameState, gameConfig.rewardDataList);

            // CombatPresenterのコールバック設定
            combatPresenter.OnPlayerMoveTo = async (pos) =>
            {
                // 移動SE
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayMoveSE();
                }

                if (boardView != null)
                {
                    await boardView.MovePlayerTo(pos);
                }
                // 2番目以降の移動で、通過済み区間の線を削除
                if (pathMoveCount > 0 && pathDrawingView != null)
                {
                    pathDrawingView.RemoveFirstLineSegment();
                }
                pathMoveCount++;
            };
            combatPresenter.OnEffectApplied = async () =>
            {
                if (uiView != null)
                {
                    uiView.UpdatePlayerInfo(gameState.Player);
                }
                if (boardView != null)
                {
                    // プレイヤーが通過したタイルを使用済み表示に変更
                    Vector2Int pos = gameState.Player.Position;
                    boardView.MarkTileAsVisited(pos);
                }
                await UniTask.Yield();
            };

            // バトルUI用コールバック（斬撃エフェクト → HPバー更新）
            combatPresenter.OnEnemyDamaged = async (enemy) =>
            {
                if (battleUIView != null)
                {
                    await battleUIView.AnimateEnemyDamage(enemy);
                }
            };
            combatPresenter.OnEnemyDefeated = (enemy) =>
            {
                if (battleUIView != null)
                {
                    battleUIView.PlayEnemyDefeatAnimation(enemy);
                }
            };

            // タイル種別SE
            combatPresenter.OnTileProcessed = (tileType) =>
            {
                if (SoundManager.Instance == null) return;

                switch (tileType)
                {
                    case TileType.Enemy:
                        SoundManager.Instance.PlayAttackSE();
                        break;
                    case TileType.AttackBoost:
                        SoundManager.Instance.PlayGetPowerSE();
                        break;
                    case TileType.Gold:
                        SoundManager.Instance.PlayGetGoldSE();
                        break;
                    case TileType.HPRecovery:
                        SoundManager.Instance.PlayGetHealSE();
                        break;
                }
            };
        }

        private async void Start()
        {
            // View層の初期化
            if (pathDrawingView != null && pathPresenter != null && gameState != null)
            {
                pathDrawingView.Initialize(pathPresenter, gameState);
            }

            if (rewardView != null && rewardPresenter != null)
            {
                rewardPresenter.SetRewardView(rewardView);
            }

            await InitializeGame();
        }

        /// <summary>
        /// ゲーム初期化
        /// 要件: 11.1, 11.2, 11.6
        /// </summary>
        public async UniTask InitializeGame()
        {
            Debug.Log("GamePresenter: ゲーム初期化開始");

            // GameStateの初期化
            gameState.Initialize(gameConfig);

            // 最初のステージを開始
            await StartNewStage();

            Debug.Log("GamePresenter: ゲーム初期化完了");
        }

        /// <summary>
        /// 新しいステージを開始
        /// 要件: 10.1, 10.2, 10.3
        /// </summary>
        public async UniTask StartNewStage()
        {
            Debug.Log($"GamePresenter: ステージ{gameState.CurrentStage}開始");

            // ステージ1の場合のみボードをクリア
            if (gameState.CurrentStage == 1)
            {
                gameState.Board.Clear();
                gameState.Player.Position = new Vector2Int(0, 0);
                Debug.Log($"GamePresenter: プレイヤー位置を初期位置(0, 0)に設定");
            }
            else
            {
                Debug.Log($"GamePresenter: プレイヤー位置を前ステージの終了位置{gameState.Player.Position}から継続");
            }

            // 新しいマス生成システムでボードを初期化
            await InitializeBoardWithNewSystem();

            // フェーズをパス描画に設定
            gameState.CurrentPhase = GamePhase.PathDrawing;

            // View層の更新
            if (boardView != null)
            {
                await boardView.InitializeBoard(gameState.Board);

                // プレイヤーアイコンを初期化・配置
                boardView.InitializePlayerIcon(gameState.Player.Position);
            }

            if (uiView != null)
            {
                uiView.UpdateStageNumber(gameState.CurrentStage);
                uiView.UpdatePlayerInfo(gameState.Player);
            }

            // バトルUIの初期化
            if (battleUIView != null)
            {
                var enemies = gameState.Board.GetEnemies();
                battleUIView.InitializeBattleUI(enemies, currentEnemySprites);
            }

            // BGM切り替え（ボスがいるステージはBossBGM）
            if (SoundManager.Instance != null)
            {
                if (gameState.IsBossStage())
                {
                    SoundManager.Instance.PlayBossBGM();
                }
                else
                {
                    SoundManager.Instance.PlayNormalBGM();
                }
            }

            Debug.Log($"GamePresenter: ステージ{gameState.CurrentStage}の準備完了");

            // パス描画フェーズを開始
            await HandlePathDrawingPhase();
        }

        /// <summary>
        /// 新しいマス生成システムでボードを初期化
        /// </summary>
        private async UniTask InitializeBoardWithNewSystem()
        {
            // ステージに応じた敵データを取得
            var spawnEntry = gameConfig.enemySpawnTable.GetEntryForStage(gameState.CurrentStage);
            if (spawnEntry == null || spawnEntry.enemies == null || spawnEntry.enemies.Count == 0)
            {
                Debug.LogError($"GamePresenter: ステージ{gameState.CurrentStage}の敵データが見つかりません");
                return;
            }

            int enemyCount = Mathf.Min(spawnEntry.enemies.Count, 3);
            currentEnemySprites.Clear();

            // ① 未決定のマスリストを作成
            List<Vector2Int> undecidedPositions = new List<Vector2Int>();
            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // ステージ2以降の場合、訪問済みマスのみ処理対象
                    if (gameState.CurrentStage > 1)
                    {
                        if (gameState.IsPositionVisited(pos))
                        {
                            undecidedPositions.Add(pos);
                        }
                    }
                    else
                    {
                        // ステージ1は全マスが対象
                        undecidedPositions.Add(pos);
                    }
                }
            }

            // ① プレイヤーの現在位置を効果なしマスに確定し、未決定リストから削除
            Vector2Int playerPos = gameState.Player.Position;
            undecidedPositions.Remove(playerPos);
            Tile playerTile = TileFactory.CreateEmptyTile();
            gameState.Board.SetTile(playerPos, playerTile);
            Debug.Log($"GamePresenter: プレイヤー位置{playerPos}を効果なしマスに確定（残り未決定: {undecidedPositions.Count}）");

            // ② 各EnemyDataから敵を生成してランダムに配置
            for (int i = 0; i < enemyCount; i++)
            {
                if (undecidedPositions.Count == 0)
                {
                    Debug.LogWarning("GamePresenter: 敵を配置する未決定マスがありません");
                    break;
                }

                EnemyData data = spawnEntry.enemies[i];

                // ランダムでインデックスを取得
                int randomIndex = Random.Range(0, undecidedPositions.Count);
                Vector2Int enemyPos = undecidedPositions[randomIndex];

                // EnemyDataの設定から敵を生成
                Enemy enemy = new Enemy(data.maxHP, data.attackPower, data.isBoss, data.actionPattern);
                enemy.Position = enemyPos;
                gameState.Board.AddEnemy(enemy);

                EnemyTile enemyTile = TileFactory.CreateEnemyTile(enemy);
                gameState.Board.SetTile(enemyPos, enemyTile);

                // バトルUI用のスプライトを記録
                currentEnemySprites.Add(data.sprite);

                // 未決定リストから削除
                undecidedPositions.RemoveAt(randomIndex);

                string typeLabel = data.isBoss ? "ボス" : "通常敵";
                Debug.Log($"GamePresenter: {typeLabel}生成 {enemyPos} (HP={enemy.MaxHP}, 攻撃={enemy.AttackPower})");
            }

            // ③ 残りの未決定マスを出現率からランダム生成
            foreach (var pos in undecidedPositions)
            {
                Tile tile = TileFactory.CreateRandomTile(gameState.SpawnConfig);
                gameState.Board.SetTile(pos, tile);
            }

            Debug.Log($"GamePresenter: ボード初期化完了（未決定マス{undecidedPositions.Count}個をランダム生成）");

            await UniTask.Yield();
        }

        // SpawnEnemies は InitializeBoardWithNewSystem に統合されたため削除

        /// <summary>
        /// ボードを初期化（ランダムタイルで埋める）
        /// 要件: 10.1, 10.4
        /// </summary>
        private void InitializeBoard()
        {
            // ステージ1の場合のみプレイヤー位置を左下に設定
            // ステージ2以降は前のステージの終了位置を保持
            if (gameState.CurrentStage == 1)
            {
                gameState.Player.Position = new Vector2Int(0, 0);
                Debug.Log($"GamePresenter: プレイヤー位置を初期位置(0, 0)に設定");
            }
            else
            {
                Debug.Log($"GamePresenter: プレイヤー位置を前ステージの終了位置{gameState.Player.Position}から継続");
            }

            // プレイヤーの現在位置を最優先で効果なしマスにする（既にタイルがあっても強制的に置き換え）
            Vector2Int playerPos = gameState.Player.Position;
            Tile playerTile = TileFactory.CreateEmptyTile();
            gameState.Board.SetTile(playerPos, playerTile);
            Debug.Log($"GamePresenter: プレイヤー位置{playerPos}を効果なしマスに強制設定");

            // すべてのマスをランダムタイルで埋める
            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // 既にタイルがある場合はスキップ（プレイヤー位置、敵マスなど）
                    if (gameState.Board.GetTile(pos) != null)
                    {
                        continue;
                    }

                    // ランダムタイルを生成
                    Tile tile = TileFactory.CreateRandomTile(gameState.SpawnConfig);
                    gameState.Board.SetTile(pos, tile);
                }
            }

            Debug.Log("GamePresenter: ボード初期化完了");
        }

        /// <summary>
        /// 空いているランダムな位置を検索（プレイヤー位置を除く）
        /// </summary>
        private Vector2Int FindRandomEmptyPosition()
        {
            List<Vector2Int> emptyPositions = new List<Vector2Int>();
            Vector2Int playerPos = gameState.Player.Position;

            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // プレイヤー位置は除外
                    if (pos == playerPos)
                    {
                        continue;
                    }

                    // 空いているマスのみ追加
                    if (gameState.Board.GetTile(pos) == null)
                    {
                        emptyPositions.Add(pos);
                    }
                }
            }

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("GamePresenter: 空き位置が見つかりません（プレイヤー位置を除く）");
                return Vector2Int.zero;
            }

            // リストをシャッフルして完全にランダムな位置を取得
            ShuffleList(emptyPositions);
            Vector2Int selectedPos = emptyPositions[0];
            Debug.Log($"GamePresenter: ランダム位置を選択 - {selectedPos} (候補数: {emptyPositions.Count})");

            return selectedPos;
        }

        /// <summary>
        /// Fisher-Yatesアルゴリズムでリストをシャッフル
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// パス描画フェーズ
        /// 要件: 1.1, 1.2, 1.3, 1.4
        /// </summary>
        public async UniTask HandlePathDrawingPhase()
        {
            Debug.Log("GamePresenter: パス描画フェーズ");
            gameState.CurrentPhase = GamePhase.PathDrawing;

            // プレイヤーの開始位置をハイライト
            if (boardView != null)
            {
                boardView.HighlightTile(gameState.Player.Position, true);
                Debug.Log($"GamePresenter: プレイヤー開始位置{gameState.Player.Position}をハイライト");
            }

            List<Vector2Int> path;

            // View層での入力待機
            if (pathDrawingView != null)
            {
                path = await pathDrawingView.WaitForPathInput(gameState.Player.Position);
            }
            else
            {
                // View層がない場合: テスト用の簡単なパスを作成
                Debug.LogWarning("GamePresenter: PathDrawingViewが設定されていません。テストパスを使用します。");
                path = CreateTestPath();
            }

            if (path != null && path.Count > 0)
            {
                await HandlePathExecutionPhase(path);
            }
            else
            {
                Debug.Log("GamePresenter: パスが空です。再度パス描画フェーズへ");
                await HandlePathDrawingPhase();
            }
        }

        /// <summary>
        /// テスト用のパスを作成（仮実装）
        /// </summary>
        private List<Vector2Int> CreateTestPath()
        {
            List<Vector2Int> path = new List<Vector2Int>();

            // プレイヤー位置から開始
            path.Add(gameState.Player.Position);

            // 敵を探す
            var enemies = gameState.Board.GetEnemies();
            if (enemies.Count > 0)
            {
                Enemy targetEnemy = enemies[0];
                Debug.Log($"テストパス: {gameState.Player.Position} -> {targetEnemy.Position}");
                path.Add(targetEnemy.Position);
            }

            return path;
        }

        /// <summary>
        /// パス実行フェーズ
        /// 要件: 1.5, 2.1, 2.2, 2.3, 2.4
        /// </summary>
        public async UniTask HandlePathExecutionPhase(List<Vector2Int> path)
        {
            Debug.Log("GamePresenter: パス実行フェーズ");
            gameState.CurrentPhase = GamePhase.PathExecution;

            // 移動カウンターをリセット
            pathMoveCount = 0;

            // プレイヤー開始位置のハイライトをクリア
            if (boardView != null)
            {
                boardView.HighlightTile(gameState.Player.Position, false);
            }

            // パス検証
            if (!pathPresenter.ValidatePath(path, gameState.Player.Position))
            {
                Debug.LogWarning("GamePresenter: パスが無効です");
                // 線分もクリア
                if (pathDrawingView != null)
                    pathDrawingView.ClearRemainingLineSegments();
                return;
            }

            // パスを実行
            await combatPresenter.ExecutePath(path);

            // 残りの線分をクリア
            if (pathDrawingView != null)
            {
                pathDrawingView.ClearRemainingLineSegments();
            }

            // View層を更新（敵HP、プレイヤー情報、ボード）
            if (boardView != null)
            {
                // ボード全体を更新（敵HPの変更を反映）
                foreach (var pos in path)
                {
                    Tile tile = gameState.Board.GetTile(pos);
                    if (tile != null)
                    {
                        boardView.UpdateTile(pos, tile);
                    }
                }
            }

            if (uiView != null)
            {
                uiView.UpdatePlayerInfo(gameState.Player);
            }

            // バトルUIの敵情報を更新
            if (battleUIView != null)
            {
                battleUIView.UpdateAllEnemyDisplays();
            }

            // ゲームオーバーチェック
            if (!gameState.Player.IsAlive())
            {
                await HandleGameOverPhase();
                return;
            }

            // 敵行動フェーズ（行動パターンを持つ敵がいる場合）
            await HandleEnemyActionPhase();

            // ステージクリアチェック
            if (gameState.Board.GetEnemies().Count == 0)
            {
                await HandleStageClearedPhase();
            }
            else
            {
                // 次のターンへ
                await HandlePathDrawingPhase();
            }
        }

        /// <summary>
        /// 敵行動フェーズ
        /// 行動パターンを持つ全ての敵のターンを進行し、行動を実行する
        /// Board描画完了後、1秒待機してからアニメーション付きでタイルを更新する
        /// </summary>
        public async UniTask HandleEnemyActionPhase()
        {
            var enemies = gameState.Board.GetEnemies();
            var actionEnemies = enemies.FindAll(e => e.HasActionPattern && e.IsAlive());

            if (actionEnemies.Count == 0) return;

            Debug.Log("GamePresenter: 敵行動フェーズ");
            gameState.CurrentPhase = GamePhase.BossAction;

            foreach (var enemy in actionEnemies)
            {
                // ターン数を増加
                enemy.TurnsSinceLastAction++;

                // 敵行動を実行（データのみ変更、UIはまだ更新しない）
                List<Vector2Int> changedPositions = await bossPresenter.ExecuteEnemyAction(enemy);

                if (changedPositions != null)
                {
                    // 1秒待機（新しいBoardが描画された後に演出開始）
                    await UniTask.Delay(1000);

                    // SE再生
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayEnemyActionSE();
                    }

                    // 変更されたタイルをアニメーション付きで更新
                    if (boardView != null && changedPositions.Count > 0)
                    {
                        await boardView.UpdateTiles(changedPositions, gameState.Board);
                    }

                    // プレイヤー情報を更新（攻撃でHP変化の可能性）
                    if (uiView != null)
                    {
                        uiView.UpdatePlayerInfo(gameState.Player);
                    }
                }

                // バトルUIの敵情報を更新（ターン表示含む）
                if (battleUIView != null)
                {
                    battleUIView.UpdateAllEnemyDisplays();
                }

                // 行動後にプレイヤーが死亡した場合は中断
                if (!gameState.Player.IsAlive())
                {
                    break;
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// ステージクリアフェーズ
        /// 要件: 8.1, 8.2, 8.3, 8.4
        /// </summary>
        public async UniTask HandleStageClearedPhase()
        {
            Debug.Log("GamePresenter: ステージクリアフェーズ");
            gameState.SetStageCleared();

            // 報酬選択フロー
            await rewardPresenter.ExecuteRewardFlow();

            // 次のステージへ
            gameState.AdvanceToNextStage();

            await StartNewStage();
        }

        /// <summary>
        /// ゲームオーバーフェーズ
        /// 要件: 5.4
        /// </summary>
        public async UniTask HandleGameOverPhase()
        {
            Debug.Log("GamePresenter: ゲームオーバーフェーズ");
            gameState.SetGameOver();

            // View層でゲームオーバー表示
            if (uiView != null)
            {
                await uiView.ShowGameOver(gameState.CurrentStage);
            }

            await UniTask.Yield();
        }
    }
}
