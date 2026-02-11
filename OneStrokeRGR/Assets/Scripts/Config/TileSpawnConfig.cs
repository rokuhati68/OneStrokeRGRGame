using System;
using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// タイル生成の確率設定を管理するクラス
    /// 要件: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6
    /// </summary>
    [Serializable]
    public class TileSpawnConfig
    {
        [Header("出現率設定")]
        [Range(0f, 1f)]
        [Tooltip("効果なしマスの出現率（初期値: 70%）")]
        public float emptyRate = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("攻撃力上昇マスの出現率（初期値: 10%）")]
        public float attackBoostRate = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("HP回復マスの出現率（初期値: 10%）")]
        public float hpRecoveryRate = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("ゴールドマスの出現率（初期値: 10%）")]
        public float goldRate = 0.1f;

        [Header("値範囲設定")]
        [Tooltip("攻撃力上昇マスの値範囲（初期値: 1-2）")]
        public Vector2Int attackBoostRange = new Vector2Int(1, 2);

        [Tooltip("ゴールドマスの値範囲（初期値: 1-2）")]
        public Vector2Int goldRange = new Vector2Int(1, 2);

        /// <summary>
        /// 出現率の合計が1.0になっているか検証
        /// </summary>
        public bool IsValid()
        {
            float total = emptyRate + attackBoostRate + hpRecoveryRate + goldRate;
            return Mathf.Approximately(total, 1.0f);
        }

        /// <summary>
        /// 報酬を適用してタイル生成設定を更新
        /// 要件: 9.4, 9.5, 12.5, 12.6
        /// </summary>
        public void ApplyReward(Model.RewardType rewardType, float rateIncrement = 0.05f, int valueIncrement = 1)
        {
            switch (rewardType)
            {
                case Model.RewardType.AttackBoostRateIncrease:
                    attackBoostRate += rateIncrement;
                    break;

                case Model.RewardType.AttackBoostValueIncrease:
                    attackBoostRange.y += valueIncrement;
                    break;

                case Model.RewardType.HPRecoveryRateIncrease:
                    hpRecoveryRate += rateIncrement;
                    break;

                case Model.RewardType.EmptyRateIncrease:
                    emptyRate += rateIncrement;
                    break;

                case Model.RewardType.GoldRateIncrease:
                    goldRate += rateIncrement;
                    break;

                case Model.RewardType.GoldValueIncrease:
                    goldRange.y += valueIncrement;
                    break;
            }
        }
    }
}
