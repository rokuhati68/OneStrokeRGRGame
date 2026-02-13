using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 報酬の管理と適用を行うクラス
    /// 要件: 8.2, 9.1, 9.2, 9.3, 9.4, 9.5
    /// </summary>
    public class RewardSystem
    {
        private RewardData[] allRewards;

        public RewardSystem(RewardData[] rewards)
        {
            allRewards = rewards;
        }

        /// <summary>
        /// ランダムに報酬を選択
        /// 要件: 8.2, 9.3
        /// </summary>
        /// <param name="count">選択する報酬の数（デフォルト: 3）</param>
        /// <returns>選択された報酬データのリスト</returns>
        public List<RewardData> SelectRandomRewards(int count = 3)
        {
            if (allRewards == null || allRewards.Length == 0)
            {
                Debug.LogWarning("RewardSystem: 報酬データが設定されていません");
                return new List<RewardData>();
            }

            // シャッフルして指定数だけ選択（要件: 9.3 - 3つの異なる報酬）
            var shuffled = allRewards.OrderBy(x => Random.value).ToList();
            var selected = shuffled.Take(count).ToList();

            Debug.Log($"RewardSystem: {selected.Count}個の報酬を選択しました");
            foreach (var reward in selected)
            {
                Debug.Log($"  - {reward.rewardName}");
            }

            return selected;
        }

        /// <summary>
        /// 報酬を適用する
        /// 要件: 8.3, 9.2, 9.4, 9.5
        /// </summary>
        /// <param name="rewardData">適用する報酬データ</param>
        /// <param name="gameState">ゲーム状態</param>
        public void ApplyReward(RewardData rewardData, GameState gameState)
        {
            if (gameState == null)
            {
                Debug.LogError("RewardSystem: gameStateがnullです");
                return;
            }

            if (rewardData == null)
            {
                Debug.LogError("RewardSystem: rewardDataがnullです");
                return;
            }

            Debug.Log($"RewardSystem: 報酬を適用 - {rewardData.rewardName}");

            // 報酬設定から増加量を取得
            float rateIncrement = gameState.RewardConfig.spawnRateIncrement;
            int valueIncrement = gameState.RewardConfig.valueIncrement;
            int bonusIncrement = gameState.RewardConfig.oneStrokeBonusIncrement;

            switch (rewardData.rewardType)
            {
                case RewardType.AttackBoostRateIncrease:
                    // 攻撃力上昇マス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, rateIncrement, 0);
                    Debug.Log($"攻撃力上昇マス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.AttackBoostValueIncrease:
                    // 攻撃力上昇マス値増加（要件: 9.5）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, 0, valueIncrement);
                    Debug.Log($"攻撃力上昇マスの値範囲が拡大しました");
                    break;

                case RewardType.HPRecoveryRateIncrease:
                    // HP回復マス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, rateIncrement, 0);
                    Debug.Log($"HP回復マス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.EmptyRateIncrease:
                    // 効果なしマス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, rateIncrement, 0);
                    Debug.Log($"効果なしマス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.GoldRateIncrease:
                    // ゴールドマス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, rateIncrement, 0);
                    Debug.Log($"ゴールドマス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.GoldValueIncrease:
                    // ゴールドマス値増加（要件: 9.5）
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, 0, valueIncrement);
                    Debug.Log($"ゴールドマスの値範囲が拡大しました");
                    break;

                case RewardType.OneStrokeBonusIncrease:
                    // 一筆書きボーナス値増加（要件: 9.2）
                    gameState.Player.IncreaseOneStrokeBonus(bonusIncrement);
                    Debug.Log($"一筆書きボーナスが{bonusIncrement}増加しました");
                    break;
            }
        }
    }
}
