using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 報酬の管理と適用を行うクラス
    /// 要件: 8.2, 9.1, 9.2, 9.3, 9.4, 9.5
    /// </summary>
    public class RewardSystem
    {
        /// <summary>
        /// ランダムに報酬を選択
        /// 要件: 8.2, 9.3
        /// </summary>
        /// <param name="count">選択する報酬の数（デフォルト: 3）</param>
        /// <returns>選択された報酬タイプのリスト</returns>
        public List<RewardType> SelectRandomRewards(int count = 3)
        {
            // すべての報酬タイプを取得
            var allRewards = System.Enum.GetValues(typeof(RewardType))
                .Cast<RewardType>()
                .ToList();

            // シャッフル
            var shuffled = allRewards.OrderBy(x => Random.value).ToList();

            // 指定数だけ選択（要件: 9.3 - 3つの異なる報酬タイプ）
            var selected = shuffled.Take(count).ToList();

            Debug.Log($"RewardSystem: {count}個の報酬を選択しました");
            foreach (var reward in selected)
            {
                Debug.Log($"  - {reward}");
            }

            return selected;
        }

        /// <summary>
        /// 報酬を適用する
        /// 要件: 8.3, 9.2, 9.4, 9.5
        /// </summary>
        /// <param name="reward">適用する報酬タイプ</param>
        /// <param name="gameState">ゲーム状態</param>
        public void ApplyReward(RewardType reward, GameState gameState)
        {
            if (gameState == null)
            {
                Debug.LogError("RewardSystem: gameStateがnullです");
                return;
            }

            Debug.Log($"RewardSystem: 報酬を適用 - {reward}");

            // 報酬設定から増加量を取得
            float rateIncrement = gameState.RewardConfig.spawnRateIncrement;
            int valueIncrement = gameState.RewardConfig.valueIncrement;
            int bonusIncrement = gameState.RewardConfig.oneStrokeBonusIncrement;

            switch (reward)
            {
                case RewardType.AttackBoostRateIncrease:
                    // 攻撃力上昇マス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(reward, rateIncrement, 0);
                    Debug.Log($"攻撃力上昇マス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.AttackBoostValueIncrease:
                    // 攻撃力上昇マス値増加（要件: 9.5）
                    gameState.SpawnConfig.ApplyReward(reward, 0, valueIncrement);
                    Debug.Log($"攻撃力上昇マスの値範囲が拡大しました");
                    break;

                case RewardType.HPRecoveryRateIncrease:
                    // HP回復マス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(reward, rateIncrement, 0);
                    Debug.Log($"HP回復マス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.EmptyRateIncrease:
                    // 効果なしマス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(reward, rateIncrement, 0);
                    Debug.Log($"効果なしマス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.GoldRateIncrease:
                    // ゴールドマス出現率増加（要件: 9.4）
                    gameState.SpawnConfig.ApplyReward(reward, rateIncrement, 0);
                    Debug.Log($"ゴールドマス出現率が{rateIncrement * 100}%増加しました");
                    break;

                case RewardType.GoldValueIncrease:
                    // ゴールドマス値増加（要件: 9.5）
                    gameState.SpawnConfig.ApplyReward(reward, 0, valueIncrement);
                    Debug.Log($"ゴールドマスの値範囲が拡大しました");
                    break;

                case RewardType.OneStrokeBonusIncrease:
                    // 一筆書きボーナス値増加（要件: 9.2）
                    gameState.Player.IncreaseOneStrokeBonus(bonusIncrement);
                    Debug.Log($"一筆書きボーナスが{bonusIncrement}増加しました");
                    break;
            }
        }

        /// <summary>
        /// 報酬の説明テキストを取得
        /// </summary>
        /// <param name="reward">報酬タイプ</param>
        /// <returns>説明テキスト</returns>
        public string GetRewardDescription(RewardType reward)
        {
            switch (reward)
            {
                case RewardType.AttackBoostRateIncrease:
                    return "攻撃力上昇マスの出現率が上がる";

                case RewardType.AttackBoostValueIncrease:
                    return "攻撃力上昇マスの効果が強化される";

                case RewardType.HPRecoveryRateIncrease:
                    return "HP回復マスの出現率が上がる";

                case RewardType.EmptyRateIncrease:
                    return "効果なしマスの出現率が上がる";

                case RewardType.GoldRateIncrease:
                    return "ゴールドマスの出現率が上がる";

                case RewardType.GoldValueIncrease:
                    return "ゴールドマスの獲得量が増える";

                case RewardType.OneStrokeBonusIncrease:
                    return "一筆書きボーナスが増加する";

                default:
                    return "不明な報酬";
            }
        }
    }
}
