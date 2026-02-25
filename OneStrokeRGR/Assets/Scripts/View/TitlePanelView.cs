using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Presenter;

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

        

        public void OnStartClicked()
        {
            titlePanel.SetActive(false);
            gamePresenter.InitializeGame().Forget();
        }
    }
}
