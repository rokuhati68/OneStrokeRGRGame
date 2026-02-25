using UnityEngine;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 遊び方パネルを表示するスクリプト
    /// Button の OnClick に ShowPanel() を登録して使用する
    /// </summary>
    public class ShowHowToPlayView : MonoBehaviour
    {
        [SerializeField] private GameObject howToPlayPanel;

        public void ShowPanel()
        {
            howToPlayPanel.SetActive(true);
        }
    }
}
