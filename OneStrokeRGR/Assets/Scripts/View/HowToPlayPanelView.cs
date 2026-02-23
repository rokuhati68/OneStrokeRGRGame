using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 遊び方パネル
    /// 複数のページパネルを前・次ボタンで切り替える
    /// </summary>
    public class HowToPlayPanelView : MonoBehaviour
    {
        [Header("ページ（表示順にドロップ）")]
        [SerializeField] private List<GameObject> pages;

        [Header("ボタン")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        private int currentIndex;

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackClicked);
            prevButton.onClick.AddListener(OnPrevClicked);
            nextButton.onClick.AddListener(OnNextClicked);
        }

        private void OnEnable()
        {
            currentIndex = 0;
            ShowPage(currentIndex);
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnPrevClicked()
        {
            if (currentIndex <= 0) return;
            currentIndex--;
            ShowPage(currentIndex);
        }

        private void OnNextClicked()
        {
            if (currentIndex >= pages.Count - 1) return;
            currentIndex++;
            ShowPage(currentIndex);
        }

        private void ShowPage(int index)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(i == index);
            }

            prevButton.interactable = index > 0;
            nextButton.interactable = index < pages.Count - 1;
        }
    }
}
