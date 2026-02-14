using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// ゲームUIの表示を管理するView
    /// 要件: 11.4
    /// </summary>
    public class UIView : MonoBehaviour
    {
        [Header("プレイヤー情報表示")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI attackPowerText;
        public TextMeshProUGUI stageText;

        [Header("ダイアログ")]
        public GameObject confirmDialogPanel;
        public TextMeshProUGUI confirmMessageText;
        public Button confirmButton;
        public Button cancelButton;

        [Header("飛ぶ光エフェクト")]
        public Sprite flyingOrbSprite;
        public float flyingDuration = 0.4f;
        public float orbSize = 30f;

        [Header("ゲームオーバー画面")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverText;
        public Button restartButton;

        private Player currentPlayer;
        private bool isWaitingForConfirmation = false;
        private bool confirmationResult = false;

        private void Awake()
        {
            // ダイアログとゲームオーバー画面を非表示に
            if (confirmDialogPanel != null)
                confirmDialogPanel.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // ボタンイベントの設定
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelButtonClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        /// <summary>
        /// プレイヤー情報を更新
        /// </summary>
        public void UpdatePlayerInfo(Player player)
        {
            currentPlayer = player;

            if (hpText != null)
            {
                UpdateTextWithAnimation(hpText, $"{player.CurrentHP}/{player.MaxHP}");
            }

            if (goldText != null)
            {
                UpdateTextWithAnimation(goldText, $"{player.Gold}");
            }

            if (attackPowerText != null)
            {
                UpdateTextWithAnimation(attackPowerText, $" {player.AttackPower}");
            }
        }

        /// <summary>
        /// ステージ番号を更新
        /// </summary>
        public void UpdateStageNumber(int stage)
        {
            if (stageText != null)
            {
                stageText.text = $"Stage　{stage}";

                // ステージ変更時のアニメーション
                stageText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
            }
        }

        /// <summary>
        /// テキストをアニメーション付きで更新
        /// </summary>
        private void UpdateTextWithAnimation(TextMeshProUGUI text, string newText)
        {
            if (text.text == newText)
                return;

            text.text = newText;

            // パンチスケールアニメーション
            text.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f);
        }

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        public async UniTask<bool> ShowConfirmDialog(string message)
        {
            if (confirmDialogPanel == null)
            {
                Debug.LogWarning("UIView: 確認ダイアログが設定されていません");
                return true;
            }

            confirmMessageText.text = message;
            confirmDialogPanel.SetActive(true);

            // ダイアログのフェードイン
            var canvasGroup = confirmDialogPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.2f);
            }

            // ボタンクリックを待機
            isWaitingForConfirmation = true;
            confirmationResult = false;

            await UniTask.WaitUntil(() => !isWaitingForConfirmation);

            // ダイアログのフェードアウト
            if (canvasGroup != null)
            {
                await canvasGroup.DOFade(0f, 0.2f).AsyncWaitForCompletion();
            }

            confirmDialogPanel.SetActive(false);

            return confirmationResult;
        }

        /// <summary>
        /// ゲームオーバー画面を表示
        /// </summary>
        public async UniTask ShowGameOver(int finalStage)
        {
            if (gameOverPanel == null)
            {
                Debug.LogWarning("UIView: ゲームオーバー画面が設定されていません");
                return;
            }

            gameOverText.text = $"Game Over\nFinal Stage: {finalStage}";
            gameOverPanel.SetActive(true);

            // フェードイン
            var canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                await canvasGroup.DOFade(1f, 0.5f).AsyncWaitForCompletion();
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// HP変化のアニメーション
        /// </summary>
        public async UniTask AnimateHPChange(int oldHP, int newHP, int maxHP)
        {
            if (hpText == null)
                return;

            // 色フラッシュ（ダメージは赤、回復は緑）
            Color flashColor = (newHP < oldHP) ? Color.red : Color.green;
            Color originalColor = hpText.color;

            hpText.DOColor(flashColor, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                hpText.color = originalColor;
            });

            // 数値アニメーション
            float duration = 0.5f;
            await DOVirtual.Float(oldHP, newHP, duration, (value) =>
            {
                hpText.text = $"{Mathf.RoundToInt(value)}/{maxHP}";
            }).AsyncWaitForCompletion();
        }

        /// <summary>
        /// ゴールド変化のアニメーション
        /// </summary>
        public async UniTask AnimateGoldChange(int oldGold, int newGold)
        {
            if (goldText == null)
                return;

            // 数値アニメーション
            float duration = 0.3f;
            await DOVirtual.Float(oldGold, newGold, duration, (value) =>
            {
                goldText.text = $"{Mathf.RoundToInt(value)}";
            }).AsyncWaitForCompletion();

            // パンチスケール
            goldText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        }

        /// <summary>
        /// 攻撃力変化のアニメーション
        /// </summary>
        public async UniTask AnimateAttackPowerChange(int oldAttack, int newAttack)
        {
            if (attackPowerText == null)
                return;

            // 数値アニメーション
            float duration = 0.3f;
            await DOVirtual.Float(oldAttack, newAttack, duration, (value) =>
            {
                attackPowerText.text = $"{Mathf.RoundToInt(value)}";
            }).AsyncWaitForCompletion();

            // 攻撃力上昇時は赤フラッシュ
            if (newAttack > oldAttack)
            {
                Color originalColor = attackPowerText.color;
                attackPowerText.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
                {
                    attackPowerText.color = originalColor;
                });
            }
        }

        /// <summary>
        /// タイル位置からUIテキストへ飛ぶ光エフェクトを再生
        /// </summary>
        public async UniTask PlayFlyingValueEffect(Vector3 sourceWorldPos, TileType tileType)
        {
            RectTransform targetRect = GetTargetRectForTileType(tileType);
            if (targetRect == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // 光のオーブを生成
            GameObject orb = new GameObject("FlyingOrb");
            orb.transform.SetParent(canvas.transform, false);

            Image orbImage = orb.AddComponent<Image>();
            if (flyingOrbSprite != null)
                orbImage.sprite = flyingOrbSprite;
            orbImage.color = GetOrbColor(tileType);
            orbImage.raycastTarget = false;

            RectTransform orbRect = orb.GetComponent<RectTransform>();
            orbRect.sizeDelta = new Vector2(orbSize, orbSize);

            // ソース位置に配置し最前面に表示
            orb.transform.position = sourceWorldPos;
            orb.transform.SetAsLastSibling();

            // ターゲットへ飛ばす + スケールダウン
            Vector3 targetPos = targetRect.position;
            Sequence seq = DOTween.Sequence();
            seq.Append(orb.transform.DOMove(targetPos, flyingDuration).SetEase(Ease.InQuad));
            seq.Join(orbRect.DOSizeDelta(new Vector2(orbSize * 0.5f, orbSize * 0.5f), flyingDuration));

            await seq.AsyncWaitForCompletion();

            if (orb != null)
                Destroy(orb);
        }

        /// <summary>
        /// タイル種別に応じたターゲットテキストのRectTransformを返す
        /// </summary>
        private RectTransform GetTargetRectForTileType(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Empty:
                case TileType.AttackBoost:
                    return attackPowerText?.rectTransform;
                case TileType.Gold:
                    return goldText?.rectTransform;
                case TileType.Thorn:
                case TileType.HPRecovery:
                    return hpText?.rectTransform;
                default:
                    return null;
            }
        }

        /// <summary>
        /// タイル種別に応じたオーブの色を返す
        /// </summary>
        private Color GetOrbColor(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Empty:
                    return new Color(1f,1f,1f,1f); // 白
                case TileType.AttackBoost:
                    return new Color(0f, 1f, 1f, 1f); // 水色
                case TileType.Gold:
                    return new Color(1f, 1f, 0f, 1f); // 黄色
                case TileType.Thorn:
                    return new Color(1f, 0f, 0f, 1f); // 赤
                case TileType.HPRecovery:
                    return new Color(1f,0f,1f,1f);//ピンク
                default:
                    return Color.white;
            }
        }

        private void OnConfirmButtonClicked()
        {
            confirmationResult = true;
            isWaitingForConfirmation = false;
        }

        private void OnCancelButtonClicked()
        {
            confirmationResult = false;
            isWaitingForConfirmation = false;
        }

        private void OnRestartButtonClicked()
        {
            // ゲーム再起動処理（後で実装）
            Debug.Log("UIView: ゲーム再起動（未実装）");
            gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // イベントリスナーの解除
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelButtonClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
    }
}
