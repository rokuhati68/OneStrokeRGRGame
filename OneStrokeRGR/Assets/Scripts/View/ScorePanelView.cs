using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Presenter;
using OneStrokeRGR.Sound;
using unityroom.Api;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// スコアパネルの表示を管理するView
    /// HP=0 または最終ステージクリア時に表示される
    /// </summary>
    public class ScorePanelView : MonoBehaviour
    {
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private TextMeshProUGUI clearedStageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private GamePresenter gamePresenter;

        private void Awake()
        {
            scorePanel.SetActive(false);
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        /// <summary>
        /// スコアパネルを表示する
        /// </summary>
        /// <param name="clearedStage">クリアしたステージ番号</param>
        public void Show(int clearedStage)
        {
            clearedStageText.text = $"{clearedStage}";
            scorePanel.SetActive(true);
            SoundManager.Instance?.PlayResultBGM();
            UnityroomApiClient.Instance.SendScore(1, clearedStage, ScoreboardWriteMode.HighScoreDesc);
        }

        /// <summary>
        /// スコアパネルを非表示にする
        /// </summary>
        public void Hide()
        {
            scorePanel.SetActive(false);
        }

        private void OnRetryClicked()
        {
            Hide();
            gamePresenter.InitializeGame().Forget();
        }
    }
}
