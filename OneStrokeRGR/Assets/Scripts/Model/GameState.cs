namespace OneStrokeRGR.Model
{
    /// <summary>
    /// ゲーム全体の状態を保持するクラス
    /// 要件: 6.1, 8.1
    /// </summary>
    public class GameState
    {
        /// <summary>現在のステージ番号</summary>
        public int CurrentStage { get; set; }

        /// <summary>プレイヤー</summary>
        public Player Player { get; set; }

        /// <summary>ゲームボード</summary>
        public Board Board { get; set; }

        /// <summary>報酬設定</summary>
        public Config.RewardConfig RewardConfig { get; set; }

        /// <summary>タイル生成設定</summary>
        public Config.TileSpawnConfig SpawnConfig { get; set; }

        /// <summary>現在のゲームフェーズ</summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>ボスステージ間隔（デフォルト: 10）</summary>
        public int BossStageInterval { get; set; } = 10;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameState()
        {
            CurrentStage = 1;
            Player = new Player();
            Board = new Board();
            RewardConfig = new Config.RewardConfig();
            SpawnConfig = new Config.TileSpawnConfig();
            CurrentPhase = GamePhase.PathDrawing;
        }

        /// <summary>
        /// 現在のステージがボスステージか判定
        /// 要件: 6.1
        /// </summary>
        /// <returns>ボスステージの場合true</returns>
        public bool IsBossStage()
        {
            return CurrentStage % BossStageInterval == 0;
        }

        /// <summary>
        /// ゲーム状態を初期化
        /// </summary>
        /// <param name="config">ゲーム設定</param>
        public void Initialize(Config.GameConfig config)
        {
            CurrentStage = 1;
            CurrentPhase = GamePhase.PathDrawing;

            // プレイヤーの初期化
            Player.Initialize(config.initialGold, config.initialOneStrokeBonus);

            // 設定のコピー
            BossStageInterval = config.bossStageInterval;

            // タイル生成設定のコピー
            SpawnConfig.emptyRate = config.defaultSpawnConfig.emptyRate;
            SpawnConfig.attackBoostRate = config.defaultSpawnConfig.attackBoostRate;
            SpawnConfig.hpRecoveryRate = config.defaultSpawnConfig.hpRecoveryRate;
            SpawnConfig.goldRate = config.defaultSpawnConfig.goldRate;
            SpawnConfig.attackBoostRange = config.defaultSpawnConfig.attackBoostRange;
            SpawnConfig.goldRange = config.defaultSpawnConfig.goldRange;

            // 報酬設定のコピー
            RewardConfig.spawnRateIncrement = config.rewardConfig.spawnRateIncrement;
            RewardConfig.valueIncrement = config.rewardConfig.valueIncrement;
            RewardConfig.oneStrokeBonusIncrement = config.rewardConfig.oneStrokeBonusIncrement;

            // ボードのクリア
            Board.Clear();

            UnityEngine.Debug.Log($"GameState: ゲームを初期化（ステージ{CurrentStage}）");
        }

        /// <summary>
        /// 次のステージに進む
        /// 要件: 8.4
        /// </summary>
        public void AdvanceToNextStage()
        {
            CurrentStage++;
            CurrentPhase = GamePhase.PathDrawing;

            // プレイヤーの状態をリセット
            Player.ResetAttackPower();

            UnityEngine.Debug.Log($"GameState: ステージ{CurrentStage}に進行");
        }

        /// <summary>
        /// ゲームオーバー状態にする
        /// 要件: 5.4
        /// </summary>
        public void SetGameOver()
        {
            CurrentPhase = GamePhase.GameOver;
            UnityEngine.Debug.Log("GameState: ゲームオーバー");
        }

        /// <summary>
        /// ステージクリア状態にする
        /// 要件: 8.1
        /// </summary>
        public void SetStageCleared()
        {
            CurrentPhase = GamePhase.StageCleared;
            UnityEngine.Debug.Log($"GameState: ステージ{CurrentStage}クリア");
        }
    }
}
