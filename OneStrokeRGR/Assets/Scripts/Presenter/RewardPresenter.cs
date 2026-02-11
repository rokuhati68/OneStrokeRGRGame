using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;

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

        public RewardPresenter(GameState state)
        {
            gameState = state;
            rewardSystem = new RewardSystem();
        }

        /// <summary>
        /// 報酬選択フローを提示
        /// 要件: 8.2
        /// </summary>
        /// <returns>選択された報酬タイプ</returns>
        public async UniTask<RewardType> PresentRewardSelection()
        {
            Debug.Log("RewardPresenter: 報酬選択を開始");

            // 3つのランダムな報酬を選択（要件: 8.2）
            List<RewardType> rewards = rewardSystem.SelectRandomRewards(3);

            // View層での報酬選択を待つ（実装時にRewardViewと連携）
            // 仮実装: 最初の報酬を自動選択
            RewardType selectedReward = rewards[0];

            Debug.Log($"RewardPresenter: {selectedReward}が選択されました");

            // TODO: 実際のUI実装時に、ユーザー選択を待つ処理に置き換える
            // selectedReward = await rewardView.ShowRewardSelection(rewards);

            await UniTask.Delay(100);

            return selectedReward;
        }

        /// <summary>
        /// 選択された報酬を適用
        /// 要件: 8.3
        /// </summary>
        /// <param name="reward">適用する報酬タイプ</param>
        public void ApplySelectedReward(RewardType reward)
        {
            if (gameState == null)
            {
                Debug.LogError("RewardPresenter: gameStateがnullです");
                return;
            }

            Debug.Log($"RewardPresenter: 報酬を適用 - {reward}");

            // RewardSystemを使って報酬を適用（要件: 8.3, 9.2）
            rewardSystem.ApplyReward(reward, gameState);
        }

        /// <summary>
        /// 報酬の説明を取得
        /// </summary>
        /// <param name="reward">報酬タイプ</param>
        /// <returns>説明テキスト</returns>
        public string GetRewardDescription(RewardType reward)
        {
            return rewardSystem.GetRewardDescription(reward);
        }

        /// <summary>
        /// 報酬選択から適用までの完全なフロー
        /// 要件: 8.2, 8.3
        /// </summary>
        public async UniTask ExecuteRewardFlow()
        {
            // 報酬を選択
            RewardType selectedReward = await PresentRewardSelection();

            // 報酬を適用
            ApplySelectedReward(selectedReward);

            Debug.Log("RewardPresenter: 報酬フロー完了");
        }
    }
}
