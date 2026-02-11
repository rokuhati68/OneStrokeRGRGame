using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Presenter;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 報酬選択UIを管理するView
    /// 要件: 8.2
    /// </summary>
    public class RewardView : MonoBehaviour
    {
        [Header("UI要素")]
        public GameObject rewardPanel;
        public TextMeshProUGUI titleText;

        [Header("報酬カード")]
        public RewardCardView[] rewardCards = new RewardCardView[3];

        [Header("アニメーション設定")]
        public float cardAppearDelay = 0.2f;
        public float cardAppearDuration = 0.5f;

        private RewardPresenter rewardPresenter;
        private RewardType selectedReward;
        private bool isWaitingForSelection = false;
        private bool selectionComplete = false;

        private void Awake()
        {
            // パネルを非表示に
            if (rewardPanel != null)
                rewardPanel.SetActive(false);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(RewardPresenter presenter)
        {
            rewardPresenter = presenter;

            // 各カードのボタンイベントを設定
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null)
                {
                    int index = i; // クロージャのためにローカル変数にコピー
                    rewardCards[i].SetButtonClickListener(() => OnRewardCardClicked(index));
                }
            }
        }

        /// <summary>
        /// 報酬選択を表示し、ユーザーの選択を待つ
        /// 要件: 8.2
        /// </summary>
        public async UniTask<RewardType> ShowRewardSelection(List<RewardType> rewards)
        {
            if (rewards == null || rewards.Count < 3)
            {
                Debug.LogError("RewardView: 報酬リストが不正です");
                return RewardType.OneStrokeBonusIncrease;
            }

            Debug.Log("RewardView: 報酬選択画面を表示");

            // パネルを表示
            if (rewardPanel != null)
                rewardPanel.SetActive(true);

            // タイトルを設定
            if (titleText != null)
                titleText.text = "報酬を選択してください";

            // 各カードに報酬を設定
            for (int i = 0; i < 3 && i < rewards.Count && i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null && rewardPresenter != null)
                {
                    string description = rewardPresenter.GetRewardDescription(rewards[i]);
                    rewardCards[i].SetReward(rewards[i], GetRewardTitle(rewards[i]), description);
                }
            }

            // カード出現アニメーション
            await PlayCardAppearAnimation();

            // ユーザーの選択を待つ
            isWaitingForSelection = true;
            selectionComplete = false;

            await UniTask.WaitUntil(() => selectionComplete);

            // パネルを非表示
            await PlayPanelDisappearAnimation();

            if (rewardPanel != null)
                rewardPanel.SetActive(false);

            Debug.Log($"RewardView: {selectedReward}が選択されました");
            return selectedReward;
        }

        /// <summary>
        /// カード出現アニメーション
        /// 要件: 8.2（アニメーション）
        /// </summary>
        private async UniTask PlayCardAppearAnimation()
        {
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null)
                {
                    rewardCards[i].PlayAppearAnimation(cardAppearDuration);
                    await UniTask.Delay((int)(cardAppearDelay * 1000));
                }
            }
        }

        /// <summary>
        /// パネル消失アニメーション
        /// </summary>
        private async UniTask PlayPanelDisappearAnimation()
        {
            if (rewardPanel != null)
            {
                var canvasGroup = rewardPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    await canvasGroup.DOFade(0f, 0.3f).AsyncWaitForCompletion();
                }
                else
                {
                    await UniTask.Delay(300);
                }
            }
        }

        /// <summary>
        /// カードクリック時の処理
        /// </summary>
        private void OnRewardCardClicked(int cardIndex)
        {
            if (!isWaitingForSelection)
                return;

            if (cardIndex < 0 || cardIndex >= rewardCards.Length || rewardCards[cardIndex] == null)
                return;

            // 選択された報酬を記録
            selectedReward = rewardCards[cardIndex].GetRewardType();

            // 選択アニメーション
            rewardCards[cardIndex].PlaySelectionAnimation();

            // 他のカードを非選択状態に
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (i != cardIndex && rewardCards[i] != null)
                {
                    rewardCards[i].PlayDeselectionAnimation();
                }
            }

            isWaitingForSelection = false;
            selectionComplete = true;
        }

        /// <summary>
        /// 報酬タイプからタイトルを取得
        /// </summary>
        private string GetRewardTitle(RewardType reward)
        {
            switch (reward)
            {
                case RewardType.OneStrokeBonusIncrease:
                    return "一筆書きボーナス増加";
                case RewardType.AttackBoostRateIncrease:
                    return "攻撃力上昇マス増加";
                case RewardType.HPRecoveryRateIncrease:
                    return "HP回復マス増加";
                case RewardType.GoldRateIncrease:
                    return "ゴールドマス増加";
                case RewardType.AttackBoostValueIncrease:
                    return "攻撃力上昇値増加";
                case RewardType.GoldValueIncrease:
                    return "ゴールド獲得量増加";
                case RewardType.EmptyRateIncrease:
                    return "効果なしマス増加";
                default:
                    return "不明な報酬";
            }
        }
    }

    /// <summary>
    /// 個別の報酬カードを表すView
    /// </summary>
    [System.Serializable]
    public class RewardCardView
    {
        public GameObject cardObject;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Button selectButton;

        private RewardType rewardType;
        private Vector3 originalScale;

        /// <summary>
        /// 報酬を設定
        /// </summary>
        public void SetReward(RewardType type, string title, string description)
        {
            rewardType = type;

            if (titleText != null)
                titleText.text = title;

            if (descriptionText != null)
                descriptionText.text = description;

            if (cardObject != null)
            {
                originalScale = cardObject.transform.localScale;
                cardObject.transform.localScale = Vector3.zero;
            }
        }

        /// <summary>
        /// ボタンクリックリスナーを設定
        /// </summary>
        public void SetButtonClickListener(System.Action onClick)
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        /// <summary>
        /// 出現アニメーション
        /// </summary>
        public void PlayAppearAnimation(float duration)
        {
            if (cardObject != null)
            {
                cardObject.transform.localScale = Vector3.zero;
                cardObject.transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
            }
        }

        /// <summary>
        /// 選択アニメーション
        /// </summary>
        public void PlaySelectionAnimation()
        {
            if (cardObject != null)
            {
                // スケールアップ
                cardObject.transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad);

                // 色を変更（ボタンの色を変える）
                if (selectButton != null)
                {
                    var colors = selectButton.colors;
                    selectButton.image.DOColor(Color.green, 0.2f);
                }
            }
        }

        /// <summary>
        /// 非選択アニメーション
        /// </summary>
        public void PlayDeselectionAnimation()
        {
            if (cardObject != null)
            {
                // フェードアウト
                var canvasGroup = cardObject.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0.3f, 0.2f);
                }
            }
        }

        /// <summary>
        /// 報酬タイプを取得
        /// </summary>
        public RewardType GetRewardType()
        {
            return rewardType;
        }
    }
}
