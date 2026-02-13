using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 報酬の管理と適用を行うクラス
    /// レベルシステム対応: 各報酬はレベル1~10を持ち、取得するとレベルが上がる
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
        /// ランダムに報酬を選択（レベル・重み付き）
        /// 最大レベル到達済みの報酬は除外される
        /// 要件: 8.2, 9.3
        /// </summary>
        public List<RewardData> SelectRandomRewards(GameState gameState, int count = 3)
        {
            if (allRewards == null || allRewards.Length == 0)
            {
                Debug.LogWarning("RewardSystem: 報酬データが設定されていません");
                return new List<RewardData>();
            }

            // 利用可能な報酬とその重みを収集
            var available = new List<(RewardData data, float weight)>();
            foreach (var reward in allRewards)
            {
                int currentLevel = gameState.GetRewardLevel(reward.rewardType);

                // 最大レベル到達済みは除外
                if (currentLevel >= reward.MaxLevel) continue;

                var levelData = reward.GetLevel(currentLevel);
                if (levelData == null) continue;

                available.Add((reward, levelData.appearanceWeight));
            }

            if (available.Count == 0)
            {
                Debug.LogWarning("RewardSystem: 利用可能な報酬がありません");
                return new List<RewardData>();
            }

            // 重み付きランダム選択
            var selected = new List<RewardData>();
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                float totalWeight = 0f;
                foreach (var item in available)
                    totalWeight += item.weight;

                float random = Random.Range(0f, totalWeight);
                float cumulative = 0f;
                int selectedIndex = 0;

                for (int j = 0; j < available.Count; j++)
                {
                    cumulative += available[j].weight;
                    if (random <= cumulative)
                    {
                        selectedIndex = j;
                        break;
                    }
                }

                selected.Add(available[selectedIndex].data);
                available.RemoveAt(selectedIndex);
            }

            Debug.Log($"RewardSystem: {selected.Count}個の報酬を選択しました");
            foreach (var reward in selected)
            {
                int level = gameState.GetRewardLevel(reward.rewardType);
                var levelData = reward.GetLevel(level);
                Debug.Log($"  - {levelData.rewardName} (Lv.{level + 1}/{reward.MaxLevel})");
            }

            return selected;
        }

        /// <summary>
        /// 報酬を適用する（現在レベルの効果を適用し、レベルを上げる）
        /// 要件: 8.3, 9.2, 9.4, 9.5
        /// </summary>
        public void ApplyReward(RewardData rewardData, GameState gameState)
        {
            if (gameState == null || rewardData == null)
            {
                Debug.LogError("RewardSystem: gameStateまたはrewardDataがnullです");
                return;
            }

            int currentLevel = gameState.GetRewardLevel(rewardData.rewardType);
            var levelData = rewardData.GetLevel(currentLevel);
            if (levelData == null)
            {
                Debug.LogWarning($"RewardSystem: レベル{currentLevel}のデータがありません");
                return;
            }

            Debug.Log($"RewardSystem: 報酬を適用 - {levelData.rewardName} (Lv.{currentLevel + 1})");

            switch (rewardData.rewardType)
            {
                case RewardType.AttackBoostRateIncrease:
                case RewardType.HPRecoveryRateIncrease:
                case RewardType.EmptyRateIncrease:
                case RewardType.GoldRateIncrease:
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, levelData.spawnRateChange, 0);
                    Debug.Log($"出現率が{levelData.spawnRateChange * 100}%変化しました");
                    break;

                case RewardType.AttackBoostValueIncrease:
                case RewardType.GoldValueIncrease:
                    gameState.SpawnConfig.ApplyReward(rewardData.rewardType, 0, levelData.valueChange);
                    Debug.Log($"効果値が{levelData.valueChange}変化しました");
                    break;

                case RewardType.OneStrokeBonusIncrease:
                    gameState.Player.IncreaseOneStrokeBonus(levelData.valueChange);
                    Debug.Log($"一筆書きボーナスが{levelData.valueChange}増加しました");
                    break;
                case RewardType.GoldGet:
                    gameState.Player.AddGold(levelData.valueChange);
                    break;
                case RewardType.HPRecover:
                    gameState.Player.Heal(levelData.valueChange);
                    break;
            }

            // レベルを上げる
            gameState.IncrementRewardLevel(rewardData.rewardType);
            Debug.Log($"RewardSystem: {rewardData.rewardType}のレベルが{currentLevel + 1} → {currentLevel + 2}に上昇");
        }
    }
}
