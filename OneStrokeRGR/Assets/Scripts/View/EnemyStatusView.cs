using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 個別の敵ステータス表示を管理するView
    /// HPバー、攻撃までのターン数、敵の全体像を表示
    /// </summary>
    public class EnemyStatusView : MonoBehaviour
    {
        [Header("敵の画像")]
        public Image enemyImage;

        [Header("HPバー")]
        public Image hpBarFill;
        public Image hpBarBackground;
        public TextMeshProUGUI hpText;

        [Header("攻撃ターン表示")]
        public TextMeshProUGUI turnText;

        [Header("ボス表示")]
        public GameObject bossIndicator;

        private Enemy currentEnemy;
        private float maxBarWidth;

        private void Awake()
        {
            if (hpBarFill != null)
            {
                maxBarWidth = hpBarFill.rectTransform.sizeDelta.x;
            }

            if (bossIndicator != null)
            {
                bossIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// 敵データをセットして表示を初期化
        /// </summary>
        public void SetEnemy(Enemy enemy, Sprite enemySprite = null)
        {
            currentEnemy = enemy;

            // CanvasGroupのalphaをリセット（PlayDefeatAnimationで0にされるため）
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // 敵画像の設定
            if (enemyImage != null)
            {
                // スケールとalphaをリセット（PlayDefeatAnimationで変更されるため）
                enemyImage.transform.localScale = Vector3.one;
                var color = enemyImage.color;
                color.a = 1f;
                enemyImage.color = color;

                if (enemySprite != null)
                {
                    enemyImage.sprite = enemySprite;
                    enemyImage.enabled = true;
                }
            }

            // ボスインジケーター
            if (bossIndicator != null)
            {
                bossIndicator.SetActive(enemy.IsBoss);
            }

            // 初期表示を更新
            UpdateDisplay();

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        public void UpdateDisplay()
        {
            if (currentEnemy == null) return;

            UpdateHPBar();
            UpdateTurnDisplay();
        }

        /// <summary>
        /// HPバーを更新
        /// </summary>
        private void UpdateHPBar()
        {
            if (currentEnemy == null) return;

            float hpRatio = (float)currentEnemy.CurrentHP / currentEnemy.MaxHP;

            // HPバーのFillを更新
            if (hpBarFill != null)
            {
                hpBarFill.fillAmount = hpRatio;

                // HP割合に応じて色を変更
                if (hpRatio > 0.5f)
                    hpBarFill.color = Color.green;
                else if (hpRatio > 0.25f)
                    hpBarFill.color = Color.yellow;
                else
                    hpBarFill.color = Color.red;
            }

            // HPテキストを更新
            if (hpText != null)
            {
                hpText.text = $"{currentEnemy.CurrentHP}/{currentEnemy.MaxHP}";
            }
        }

        /// <summary>
        /// 攻撃ターン表示を更新
        /// </summary>
        private void UpdateTurnDisplay()
        {
            if (currentEnemy == null || turnText == null) return;

            int turnsUntilAttack = currentEnemy.BossActionInterval - currentEnemy.TurnsSinceLastAction;
            if (turnsUntilAttack < 0) turnsUntilAttack = 0;

            turnText.text = $"{turnsUntilAttack}";
        }

        /// <summary>
        /// HPバーのアニメーション付き更新
        /// </summary>
        public void AnimateHPChange()
        {
            if (currentEnemy == null || hpBarFill == null) return;

            float targetRatio = (float)currentEnemy.CurrentHP / currentEnemy.MaxHP;

            // HPバーをアニメーション
            hpBarFill.DOFillAmount(targetRatio, 0.3f).SetEase(Ease.OutQuad);

            // HP割合に応じて色を変更
            Color targetColor;
            if (targetRatio > 0.5f)
                targetColor = Color.green;
            else if (targetRatio > 0.25f)
                targetColor = Color.yellow;
            else
                targetColor = Color.red;

            hpBarFill.DOColor(targetColor, 0.3f);

            // HPテキストを更新
            if (hpText != null)
            {
                hpText.text = $"{currentEnemy.CurrentHP}/{currentEnemy.MaxHP}";
                hpText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f);
            }

            // ダメージ演出（シェイク）
            if (enemyImage != null)
            {
                enemyImage.transform.DOShakePosition(0.3f, 5f, 20);
            }
        }

        /// <summary>
        /// 敵撃破演出
        /// </summary>
        public void PlayDefeatAnimation()
        {
            if (enemyImage != null)
            {
                // フェードアウト + 縮小
                enemyImage.DOFade(0f, 0.5f);
                enemyImage.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack);
            }

            // HPバーもフェードアウト
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 表示をクリア
        /// </summary>
        public void Clear()
        {
            currentEnemy = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 現在追跡中の敵を取得
        /// </summary>
        public Enemy GetEnemy()
        {
            return currentEnemy;
        }
    }
}
