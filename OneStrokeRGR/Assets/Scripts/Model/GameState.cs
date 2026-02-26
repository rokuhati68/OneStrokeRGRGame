using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>タイル生成設定</summary>
        public Config.TileSpawnConfig SpawnConfig { get; set; }

        /// <summary>現在のゲームフェーズ</summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>ゲーム全体で訪問済みのマス位置（ステージ跨ぎで保持）</summary>
        public HashSet<Vector2Int> VisitedPositions { get; private set; } = new HashSet<Vector2Int>();

        /// <summary>報酬タイプごとの現在レベル（0ベース）</summary>
        private Dictionary<RewardType, int> rewardLevels = new Dictionary<RewardType, int>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameState()
        {
            CurrentStage = 1;
            Player = new Player();
            Board = new Board();
            SpawnConfig = new Config.TileSpawnConfig();
            CurrentPhase = GamePhase.PathDrawing;
        }

        /// <summary>
        /// 現在のステージがボスステージか判定
        /// ボード上にボス敵が存在する場合にtrue
        /// 要件: 6.1
        /// </summary>
        /// <returns>ボスステージの場合true</returns>
        public bool IsBossStage()
        {
            return Board.GetEnemies().Exists(e => e.IsBoss);
        }

        /// <summary>
        /// ゲーム状態を初期化
        /// </summary>
        /// <param name="config">ゲーム設定</param>
        public void Initialize(Config.GameConfig config)
        {
            CurrentStage = 1;
            CurrentPhase = GamePhase.PathDrawing;

            // 訪問済みマスをクリア（ステージ跨ぎ管理のリセット）
            VisitedPositions.Clear();

            // HP・ゴールドのみリセット（報酬強化・oneStrokeBonusは引き継ぐ）
            Player.ResetHPAndGold(config.initialGold);

            // ボードのクリア
            Board.Clear();

            // SpawnConfig・rewardLevels は引き継ぐ（強くてニューゲーム）
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

        /// <summary>
        /// マスを訪問済みとしてマーク
        /// </summary>
        public void MarkPositionAsVisited(Vector2Int position)
        {
            VisitedPositions.Add(position);
        }

        /// <summary>
        /// 複数のマスを訪問済みとしてマーク
        /// </summary>
        public void MarkPositionsAsVisited(List<Vector2Int> positions)
        {
            foreach (var pos in positions)
            {
                VisitedPositions.Add(pos);
            }
        }

        /// <summary>
        /// マスが訪問済みか判定
        /// </summary>
        public bool IsPositionVisited(Vector2Int position)
        {
            return VisitedPositions.Contains(position);
        }

        /// <summary>
        /// 報酬の現在レベルを取得（0ベース）
        /// </summary>
        public int GetRewardLevel(RewardType type)
        {
            return rewardLevels.TryGetValue(type, out int level) ? level : 0;
        }

        /// <summary>
        /// 報酬のレベルを1つ上げる
        /// </summary>
        public void IncrementRewardLevel(RewardType type)
        {
            if (!rewardLevels.ContainsKey(type))
                rewardLevels[type] = 0;
            rewardLevels[type]++;
        }
    }
}
