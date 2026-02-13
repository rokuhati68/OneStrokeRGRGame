using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;
using OneStrokeRGR.View;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// 報酬選択を制御するPresenter
    /// 要件: 8.2, 8.3
    /// </summary>
    public class RewardPresenter
    {
        private RewardSystem rewardSystem;
        private GameState gameState;
        private RewardView rewardView;

        public RewardPresenter(GameState state, RewardData[] rewardDataList)
        {
            gameState = state;
            rewardSystem = new RewardSystem(rewardDataList);
        }

        /// <summary>
        /// RewardViewを設定
        /// </summary>
        public void SetRewardView(RewardView view)
        {
            rewardView = view;
            if (rewardView != null)
            {
                rewardView.Initialize(this);
            }
        }

        /// <summary>
        /// 報酬選択フローを提示
        /// 要件: 8.2
        /// </summary>
        public async UniTask<RewardData> PresentRewardSelection()
        {
            Debug.Log("RewardPresenter: 報酬選択を開始");

            // レベル・重みを考慮して3つの報酬を選択（要件: 8.2）
            List<RewardData> rewards = rewardSystem.SelectRandomRewards(gameState, 3);

            if (rewards.Count == 0)
            {
                Debug.LogWarning("RewardPresenter: 利用可能な報酬がありません");
                return null;
            }

            RewardData selectedReward;

            // View層での報酬選択を待つ
            if (rewardView != null)
            {
                // 各報酬の現在レベルを取得してViewに渡す
                List<int> levels = new List<int>();
                foreach (var reward in rewards)
                {
                    levels.Add(gameState.GetRewardLevel(reward.rewardType));
                }

                selectedReward = await rewardView.ShowRewardSelection(rewards, levels);
            }
            else
            {
                Debug.LogWarning("RewardPresenter: RewardViewが設定されていません。最初の報酬を自動選択します。");
                selectedReward = rewards[0];
                await UniTask.Delay(100);
            }

            return selectedReward;
        }

        /// <summary>
        /// 選択された報酬を適用（効果適用＋レベルアップ）
        /// 要件: 8.3
        /// </summary>
        public void ApplySelectedReward(RewardData rewardData)
        {
            if (gameState == null || rewardData == null)
            {
                Debug.LogError("RewardPresenter: gameStateまたはrewardDataがnullです");
                return;
            }

            // RewardSystemを使って報酬を適用＋レベルアップ（要件: 8.3, 9.2）
            rewardSystem.ApplyReward(rewardData, gameState);
        }

        /// <summary>
        /// 報酬選択から適用までの完全なフロー
        /// 要件: 8.2, 8.3
        /// </summary>
        public async UniTask ExecuteRewardFlow()
        {
            RewardData selectedReward = await PresentRewardSelection();

            if (selectedReward != null)
            {
                ApplySelectedReward(selectedReward);
            }

            Debug.Log("RewardPresenter: 報酬フロー完了");
        }
    }
}
