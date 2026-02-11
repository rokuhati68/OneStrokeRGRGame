using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;
using OneStrokeRGR.View;

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

        private GameState gameState;
        private PathPresenter pathPresenter;
        private CombatPresenter combatPresenter;
        private BossPresenter bossPresenter;
        private RewardPresenter rewardPresenter;

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
            rewardPresenter = new RewardPresenter(gameState);
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

            // ボードをクリア
            gameState.Board.Clear();

            // 敵を生成して配置（要件: 10.3, 13.1）
            await SpawnEnemies();

            // ボードの残りのマスをランダムタイルで埋める
            InitializeBoard();

            // フェーズをパス描画に設定
            gameState.CurrentPhase = GamePhase.PathDrawing;

            // View層の更新
            if (boardView != null)
            {
                await boardView.InitializeBoard(gameState.Board);
            }

            if (uiView != null)
            {
                uiView.UpdateStageNumber(gameState.CurrentStage);
                uiView.UpdatePlayerInfo(gameState.Player);
            }

            Debug.Log($"GamePresenter: ステージ{gameState.CurrentStage}の準備完了");

            // パス描画フェーズを開始
            await HandlePathDrawingPhase();
        }

        /// <summary>
        /// 敵を生成して配置
        /// 要件: 10.3, 13.1, 13.2, 13.3
        /// </summary>
        private async UniTask SpawnEnemies()
        {
            // ステージに応じた敵データを取得
            var enemyData = gameConfig.enemySpawnTable.GetEntryForStage(gameState.CurrentStage);
            if (enemyData == null)
            {
                Debug.LogError($"GamePresenter: ステージ{gameState.CurrentStage}の敵データが見つかりません");
                return;
            }

            bool isBossStage = gameState.IsBossStage();
            int enemyCount = enemyData.enemyCount;

            Debug.Log($"敵を{enemyCount}体生成（ボスステージ: {isBossStage}）");

            // 敵を生成
            for (int i = 0; i < enemyCount; i++)
            {
                // ボスステージの場合
                if (isBossStage && i == 0)
                {
                    Enemy boss = new Enemy(enemyData.bossHP, enemyData.bossAttack, true, gameConfig.bossActionInterval);
                    Vector2Int bossPos = FindRandomEmptyPosition();
                    boss.Position = bossPos;

                    gameState.Board.AddEnemy(boss);

                    EnemyTile bossTile = TileFactory.CreateEnemyTile(boss);
                    gameState.Board.SetTile(bossPos, bossTile);

                    Debug.Log($"ボス生成: {bossPos}, HP={boss.MaxHP}, 攻撃={boss.AttackPower}");
                }
                else
                {
                    // 通常敵
                    Enemy enemy = new Enemy(enemyData.enemyHP, enemyData.enemyAttack, false);
                    Vector2Int enemyPos = FindRandomEmptyPosition();
                    enemy.Position = enemyPos;

                    gameState.Board.AddEnemy(enemy);

                    EnemyTile enemyTile = TileFactory.CreateEnemyTile(enemy);
                    gameState.Board.SetTile(enemyPos, enemyTile);

                    Debug.Log($"敵生成: {enemyPos}, HP={enemy.MaxHP}, 攻撃={enemy.AttackPower}");
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// ボードを初期化（ランダムタイルで埋める）
        /// 要件: 10.1, 10.4
        /// </summary>
        private void InitializeBoard()
        {
            // プレイヤー位置を設定（左下）
            gameState.Player.Position = new Vector2Int(0, 0);

            // すべてのマスをランダムタイルで埋める
            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // 既にタイルがある場合はスキップ（敵マスなど）
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
        /// 空いているランダムな位置を検索
        /// </summary>
        private Vector2Int FindRandomEmptyPosition()
        {
            List<Vector2Int> emptyPositions = new List<Vector2Int>();

            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (gameState.Board.GetTile(pos) == null)
                    {
                        emptyPositions.Add(pos);
                    }
                }
            }

            if (emptyPositions.Count == 0)
            {
                Debug.LogWarning("GamePresenter: 空き位置が見つかりません");
                return Vector2Int.zero;
            }

            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }

        /// <summary>
        /// パス描画フェーズ
        /// 要件: 1.1, 1.2, 1.3, 1.4
        /// </summary>
        public async UniTask HandlePathDrawingPhase()
        {
            Debug.Log("GamePresenter: パス描画フェーズ");
            gameState.CurrentPhase = GamePhase.PathDrawing;

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

            // パス検証
            if (!pathPresenter.ValidatePath(path, gameState.Player.Position))
            {
                Debug.LogWarning("GamePresenter: パスが無効です");
                return;
            }

            // パスを実行
            await combatPresenter.ExecutePath(path);

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

            // ゲームオーバーチェック
            if (!gameState.Player.IsAlive())
            {
                await HandleGameOverPhase();
                return;
            }

            // ボス行動フェーズ
            if (gameState.IsBossStage())
            {
                await HandleBossActionPhase();
            }

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
        /// ボス行動フェーズ
        /// 要件: 6.2, 6.3
        /// </summary>
        public async UniTask HandleBossActionPhase()
        {
            Debug.Log("GamePresenter: ボス行動フェーズ");
            gameState.CurrentPhase = GamePhase.BossAction;

            // ボスを探す
            var enemies = gameState.Board.GetEnemies();
            Enemy boss = enemies.Find(e => e.IsBoss);

            if (boss != null)
            {
                // ターン数を増加
                boss.TurnsSinceLastAction++;

                // ボス行動を実行
                await bossPresenter.ExecuteBossAction(boss);
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
