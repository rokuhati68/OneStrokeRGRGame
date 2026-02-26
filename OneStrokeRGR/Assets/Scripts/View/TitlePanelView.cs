using UnityEngine;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Presenter;
using OneStrokeRGR.Sound;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// タイトルパネルのスタートボタンに付けるスクリプト
    /// クリックでパネルを非表示にしゲームを初期化する
    /// </summary>
    public class TitlePanelView : MonoBehaviour
    {
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GamePresenter gamePresenter;

        private void Start()
        {
            SoundManager.Instance?.PlayTitleBGM();
        }

        public void OnStartClicked()
        {
            titlePanel.SetActive(false);
            gamePresenter.InitializeGame().Forget();
        }
    }
}
