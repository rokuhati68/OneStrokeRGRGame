using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// ゲーム全体の設定を管理するScriptableObject
    /// 要件: 11.1, 11.2, 11.6, 12.1, 12.2, 13.1
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "OneStrokeRGR/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("初期値設定")]
        [Tooltip("プレイヤーの最大HP（要件: 11.1）")]
        public int initialHP = 5;

        [Tooltip("各ステージ開始時のゴールド（要件: 11.2）")]
        public int initialGold = 50;

        [Tooltip("一筆書きボーナスの初期値（要件: 11.6）")]
        public int initialOneStrokeBonus = 5;

        [Header("タイル生成設定")]
        [Tooltip("デフォルトのタイル出現率設定（要件: 12.1, 12.2）")]
        public TileSpawnConfig defaultSpawnConfig = new TileSpawnConfig();

        [Header("敵生成設定")]
        [Tooltip("ステージごとの敵生成テーブル（要件: 13.1）")]
        public EnemySpawnTable enemySpawnTable = new EnemySpawnTable();

        [Header("報酬設定")]
        [Tooltip("報酬効果の設定")]
        public RewardConfig rewardConfig = new RewardConfig();

        [Header("ボス設定")]
        [Tooltip("ボスが出現するステージ間隔（デフォルト: 10）")]
        public int bossStageInterval = 10;

        [Tooltip("ボスの行動間隔（デフォルト: 3ターン）")]
        public int bossActionInterval = 3;

        private void OnValidate()
        {
            // 出現率の合計が1.0になっているか警告
            if (!defaultSpawnConfig.IsValid())
            {
                Debug.LogWarning("GameConfig: タイル出現率の合計が1.0になっていません。");
            }

            // 初期値の検証
            if (initialHP <= 0)
            {
                Debug.LogWarning("GameConfig: initialHPは正の値である必要があります。");
            }

            if (initialGold < 0)
            {
                Debug.LogWarning("GameConfig: initialGoldは0以上である必要があります。");
            }

            if (initialOneStrokeBonus < 0)
            {
                Debug.LogWarning("GameConfig: initialOneStrokeBonusは0以上である必要があります。");
            }
        }
    }
}
