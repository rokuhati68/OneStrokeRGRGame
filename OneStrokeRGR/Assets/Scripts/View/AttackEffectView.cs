using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 敵攻撃時の斬撃エフェクトを再生するView
    /// BattlePanel上に配置し、対象の敵画像の上に斬撃アニメーションを表示する
    /// </summary>
    public class AttackEffectView : MonoBehaviour
    {
        [Header("斬撃スプライト")]
        [Tooltip("剣で切るエフェクト画像")]
        public Sprite slashSprite;

        [Header("アニメーション設定")]
        [Tooltip("斬撃画像のサイズ")]
        public Vector2 slashSize = new Vector2(200f, 200f);

        [Tooltip("斬撃の回転角度（開始）")]
        public float startAngle = 45f;

        [Tooltip("斬撃の回転角度（終了）")]
        public float endAngle = -30f;

        [Tooltip("斬り込みの速さ（秒）")]
        public float slashInDuration = 0.08f;

        [Tooltip("斬撃の表示維持（秒）")]
        public float slashHoldDuration = 0.1f;

        [Tooltip("斬撃のフェードアウト（秒）")]
        public float slashOutDuration = 0.15f;

        [Header("ヒットフラッシュ")]
        [Tooltip("白フラッシュの色")]
        public Color flashColor = new Color(1f, 1f, 1f, 0.6f);

        [Tooltip("フラッシュの時間（秒）")]
        public float flashDuration = 0.08f;

        /// <summary>
        /// 対象のRectTransform上に斬撃エフェクトを再生する
        /// </summary>
        public async UniTask PlaySlashEffect(RectTransform target)
        {
            if (target == null || slashSprite == null) return;

            // --- 斬撃画像を生成 ---
            GameObject slashObj = new GameObject("SlashEffect");
            slashObj.transform.SetParent(target, false);

            Image slashImage = slashObj.AddComponent<Image>();
            slashImage.sprite = slashSprite;
            slashImage.raycastTarget = false;
            slashImage.color = new Color(1f, 1f, 1f, 0f); // 透明から開始

            RectTransform slashRect = slashObj.GetComponent<RectTransform>();
            slashRect.anchoredPosition = Vector2.zero;
            slashRect.sizeDelta = slashSize;
            slashRect.localScale = Vector3.zero;
            slashRect.localRotation = Quaternion.Euler(0f, 0f, startAngle);

            // --- 白フラッシュ用オーバーレイ ---
            GameObject flashObj = new GameObject("FlashOverlay");
            flashObj.transform.SetParent(target, false);

            Image flashImage = flashObj.AddComponent<Image>();
            flashImage.raycastTarget = false;
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

            RectTransform flashRect = flashObj.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            // --- アニメーションシーケンス ---
            Sequence sequence = DOTween.Sequence();

            // ① 斬り込み: スケール0→1.2、透明→不透明、回転
            sequence.Append(slashImage.DOFade(1f, slashInDuration));
            sequence.Join(slashRect.DOScale(1.2f, slashInDuration).SetEase(Ease.OutQuad));
            sequence.Join(slashRect.DOLocalRotate(new Vector3(0f, 0f, endAngle), slashInDuration, RotateMode.Fast).SetEase(Ease.OutQuad));

            // ② ヒットフラッシュ（斬り込みと同時に発光）
            sequence.Join(flashImage.DOFade(flashColor.a, flashDuration).SetEase(Ease.OutQuad));

            // ③ フラッシュ消灯
            sequence.Append(flashImage.DOFade(0f, flashDuration).SetEase(Ease.InQuad));

            // ④ 保持
            sequence.AppendInterval(slashHoldDuration);

            // ⑤ 斬撃フェードアウト
            sequence.Append(slashImage.DOFade(0f, slashOutDuration));
            sequence.Join(slashRect.DOScale(1.5f, slashOutDuration).SetEase(Ease.InQuad));

            // ⑥ 完了後にオブジェクト削除
            sequence.OnComplete(() =>
            {
                Destroy(flashObj);
                Destroy(slashObj);
            });

            await sequence.AsyncWaitForCompletion();
        }
    }
}
