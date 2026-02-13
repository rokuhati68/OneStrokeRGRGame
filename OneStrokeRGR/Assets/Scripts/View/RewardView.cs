using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Config;
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
        private RewardData selectedReward;
        private bool isWaitingForSelection = false;
        private bool selectionComplete = false;

        private void Awake()
        {
            if (rewardPanel != null)
                rewardPanel.SetActive(false);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(RewardPresenter presenter)
        {
            rewardPresenter = presenter;

            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null)
                {
                    int index = i;
                    rewardCards[i].SetButtonClickListener(() => OnRewardCardClicked(index));
                }
            }
        }

        /// <summary>
        /// 報酬選択を表示し、ユーザーの選択を待つ
        /// 要件: 8.2
        /// </summary>
        /// <param name="rewards">報酬データリスト</param>
        /// <param name="levels">各報酬の現在レベル（0ベース）</param>
        public async UniTask<RewardData> ShowRewardSelection(List<RewardData> rewards, List<int> levels)
        {
            if (rewards == null || rewards.Count == 0)
            {
                Debug.LogError("RewardView: 報酬リストが空です");
                return null;
            }

            Debug.Log("RewardView: 報酬選択画面を表示");

            // パネルを表示
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);

                var canvasGroup = rewardPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }

            if (titleText != null)
                titleText.text = "報酬を選択してください";

            // 各カードに報酬データとレベルを設定
            int cardCount = Mathf.Min(rewards.Count, rewardCards.Length);
            for (int i = 0; i < cardCount; i++)
            {
                if (rewardCards[i] != null)
                {
                    int level = (i < levels.Count) ? levels[i] : 0;
                    rewardCards[i].SetReward(rewards[i], level);
                }
            }

            // 使用しないカードを非表示
            for (int i = cardCount; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null && rewardCards[i].cardObject != null)
                {
                    rewardCards[i].cardObject.SetActive(false);
                }
            }

            await PlayCardAppearAnimation(cardCount);

            isWaitingForSelection = true;
            selectionComplete = false;

            await UniTask.WaitUntil(() => selectionComplete);

            await PlayPanelDisappearAnimation();

            if (rewardPanel != null)
                rewardPanel.SetActive(false);

            return selectedReward;
        }

        private async UniTask PlayCardAppearAnimation(int count = 3)
        {
            int cardCount = Mathf.Min(count, rewardCards.Length);
            for (int i = 0; i < cardCount; i++)
            {
                if (rewardCards[i] != null)
                {
                    rewardCards[i].PlayAppearAnimation(cardAppearDuration);
                    await UniTask.Delay((int)(cardAppearDelay * 1000));
                }
            }
        }

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

        private void OnRewardCardClicked(int cardIndex)
        {
            if (!isWaitingForSelection)
                return;

            if (cardIndex < 0 || cardIndex >= rewardCards.Length || rewardCards[cardIndex] == null)
                return;

            selectedReward = rewardCards[cardIndex].GetRewardData();

            rewardCards[cardIndex].PlaySelectionAnimation();

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
    }

    /// <summary>
    /// 個別の報酬カードを表すView
    /// </summary>
    [System.Serializable]
    public class RewardCardView
    {
        public GameObject cardObject;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Button selectButton;

        private RewardData rewardData;
        private Vector3 originalScale;

        /// <summary>
        /// RewardDataとレベルを設定して表示を更新
        /// </summary>
        public void SetReward(RewardData data, int level)
        {
            rewardData = data;

            var levelData = data.GetLevel(level);
            if (levelData == null)
            {
                Debug.LogWarning($"RewardCardView: レベル{level}のデータがありません");
                return;
            }

            if (titleText != null)
                titleText.text = levelData.rewardName;

            if (descriptionText != null)
                descriptionText.text = levelData.description;

            if (iconImage != null)
            {
                if (levelData.icon != null)
                {
                    iconImage.sprite = levelData.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (cardObject != null)
            {
                originalScale = Vector3.one;
                cardObject.transform.localScale = Vector3.zero;
                cardObject.SetActive(true);

                var canvasGroup = cardObject.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        public void SetButtonClickListener(System.Action onClick)
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void PlayAppearAnimation(float duration)
        {
            if (cardObject != null)
            {
                cardObject.transform.localScale = Vector3.zero;
                cardObject.transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
            }
        }

        public void PlaySelectionAnimation()
        {
            if (cardObject != null)
            {
                cardObject.transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad);

                if (selectButton != null)
                {
                    selectButton.image.DOColor(Color.green, 0.2f);
                }
            }
        }

        public void PlayDeselectionAnimation()
        {
            if (cardObject != null)
            {
                var canvasGroup = cardObject.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0.3f, 0.2f);
                }
            }
        }

        public RewardData GetRewardData()
        {
            return rewardData;
        }
    }
}
