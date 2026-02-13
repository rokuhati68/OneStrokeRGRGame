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
        /// <returns>選択された報酬データ</returns>
        public async UniTask<RewardData> PresentRewardSelection()
        {
            Debug.Log("RewardPresenter: 報酬選択を開始");

            // 3つのランダムな報酬を選択（要件: 8.2）
            List<RewardData> rewards = rewardSystem.SelectRandomRewards(3);

            RewardData selectedReward;

            // View層での報酬選択を待つ
            if (rewardView != null)
            {
                selectedReward = await rewardView.ShowRewardSelection(rewards);
            }
            else
            {
                // View層がない場合: 最初の報酬を自動選択
                Debug.LogWarning("RewardPresenter: RewardViewが設定されていません。最初の報酬を自動選択します。");
                selectedReward = rewards.Count > 0 ? rewards[0] : null;
                await UniTask.Delay(100);
            }

            Debug.Log($"RewardPresenter: {selectedReward?.rewardName}が選択されました");

            return selectedReward;
        }

        /// <summary>
        /// 選択された報酬を適用
        /// 要件: 8.3
        /// </summary>
        /// <param name="rewardData">適用する報酬データ</param>
        public void ApplySelectedReward(RewardData rewardData)
        {
            if (gameState == null)
            {
                Debug.LogError("RewardPresenter: gameStateがnullです");
                return;
            }

            if (rewardData == null)
            {
                Debug.LogError("RewardPresenter: rewardDataがnullです");
                return;
            }

            Debug.Log($"RewardPresenter: 報酬を適用 - {rewardData.rewardName}");

            // RewardSystemを使って報酬を適用（要件: 8.3, 9.2）
            rewardSystem.ApplyReward(rewardData, gameState);
        }

        /// <summary>
        /// 報酬選択から適用までの完全なフロー
        /// 要件: 8.2, 8.3
        /// </summary>
        public async UniTask ExecuteRewardFlow()
        {
            // 報酬を選択
            RewardData selectedReward = await PresentRewardSelection();

            // 報酬を適用
            if (selectedReward != null)
            {
                ApplySelectedReward(selectedReward);
            }

            Debug.Log("RewardPresenter: 報酬フロー完了");
        }
    }
}
