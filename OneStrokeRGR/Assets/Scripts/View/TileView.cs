using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// タイルの視覚表現を管理するView
    /// 要件: 14.1, 14.2, 14.3
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [Header("UI要素")]
        public Image backgroundImage;
        public TextMeshProUGUI valueText;
        public Image iconImage;

        [Header("色設定")]
        public Color emptyColor = Color.gray;
        public Color attackBoostColor = Color.red;
        public Color hpRecoveryColor = Color.green;
        public Color goldColor = Color.yellow;
        public Color enemyColor = Color.magenta;
        public Color thornColor = new Color(0.5f, 0f, 0.5f); // 紫
        public Color wallColor = Color.black;
        public Color highlightColor = Color.cyan;
        public Color visitedColor = new Color(0.4f, 0.4f, 0.4f); // 使用済みマスの色

        private Tile tileData;
        private Vector2Int gridPosition;
        private Color originalColor;
        private bool isHighlighted = false;
        private TileIconConfig iconConfig;

        /// <summary>
        /// タイルのセットアップ
        /// </summary>
        public void Setup(Tile tile, Vector2Int position)
        {
            tileData = tile;
            gridPosition = position;
            UpdateVisuals();
        }

        /// <summary>
        /// アイコン設定付きのタイルセットアップ
        /// </summary>
        public void Setup(Tile tile, Vector2Int position, TileIconConfig config)
        {
            tileData = tile;
            gridPosition = position;
            iconConfig = config;
            UpdateVisuals();
        }

        /// <summary>
        /// タイルの視覚表現を更新
        /// </summary>
        public void UpdateVisuals()
        {
            if (tileData == null)
            {
                SetEmpty();
                return;
            }

            // タイプに応じた色を設定
            switch (tileData.Type)
            {
                case TileType.Empty:
                    originalColor = emptyColor;
                    valueText.text = "";
                    break;

                case TileType.AttackBoost:
                    originalColor = attackBoostColor;
                    valueText.text = $"+{((AttackBoostTile)tileData).BoostValue}";
                    break;

                case TileType.HPRecovery:
                    originalColor = hpRecoveryColor;
                    valueText.text = "+1HP"; // HP回復は固定で1
                    break;

                case TileType.Gold:
                    originalColor = goldColor;
                    valueText.text = $"{((GoldTile)tileData).GoldValue}G";
                    break;

                case TileType.Enemy:
                    originalColor = enemyColor;
                    var enemyTile = (EnemyTile)tileData;
                    if (enemyTile.Enemy.IsBoss)
                    {
                        valueText.text = $"BOSS\nHP:{enemyTile.Enemy.CurrentHP}/{enemyTile.Enemy.MaxHP}";
                    }
                    else
                    {
                        valueText.text = $"HP:{enemyTile.Enemy.CurrentHP}/{enemyTile.Enemy.MaxHP}";
                    }
                    break;

                case TileType.Thorn:
                    originalColor = thornColor;
                    valueText.text = $"-{((ThornTile)tileData).Damage}HP";
                    break;

                case TileType.Wall:
                    originalColor = wallColor;
                    valueText.text = "■";
                    break;

                default:
                    originalColor = emptyColor;
                    valueText.text = "";
                    break;
            }

            if (!isHighlighted)
            {
                backgroundImage.color = originalColor;
            }

            // アイコン画像の更新
            UpdateIcon();
        }

        /// <summary>
        /// 空のタイルとして設定
        /// </summary>
        private void SetEmpty()
        {
            originalColor = emptyColor;
            backgroundImage.color = originalColor;
            valueText.text = "";
            UpdateIcon();
        }

        /// <summary>
        /// アイコン画像を更新
        /// </summary>
        private void UpdateIcon()
        {
            if (iconImage == null) return;

            if (iconConfig == null || tileData == null)
            {
                iconImage.enabled = false;
                return;
            }

            bool isBoss = tileData.Type == TileType.Enemy && tileData is EnemyTile et && et.Enemy.IsBoss;
            Sprite icon = iconConfig.GetIcon(tileData.Type, isBoss);

            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        /// <summary>
        /// ハイライト表示を設定
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            isHighlighted = highlight;
            backgroundImage.color = highlight ? highlightColor : originalColor;
        }

        /// <summary>
        /// 使用済み表示に変更（プレイヤー通過後）
        /// BackgroundImageの色とIconImageを使用済み用に変更
        /// </summary>
        public void SetVisited(Sprite visitedSprite)
        {
            // 背景色を使用済み色に変更
            originalColor = visitedColor;
            backgroundImage.color = visitedColor;
            isHighlighted = false;

            // テキストをクリア
            valueText.text = "";

            // アイコンを使用済みスプライトに変更
            if (iconImage != null)
            {
                if (visitedSprite != null)
                {
                    iconImage.sprite = visitedSprite;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }

        /// <summary>
        /// 出現アニメーション
        /// 要件: 14.1
        /// </summary>
        public void PlayAppearAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.3f);
            }
        }

        /// <summary>
        /// 消失アニメーション
        /// 要件: 14.2
        /// </summary>
        public async UniTask PlayDisappearAnimation()
        {
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                await canvasGroup.DOFade(0f, 0.2f).AsyncWaitForCompletion();
            }
            else
            {
                await UniTask.Delay(200);
            }
        }

        /// <summary>
        /// 効果発動アニメーション
        /// 要件: 14.3
        /// </summary>
        public void PlayEffectAnimation()
        {
            // パンチスケールアニメーション
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

            // 色のフラッシュ
            var originalColor = backgroundImage.color;
            backgroundImage.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                backgroundImage.color = originalColor;
            });
        }

        /// <summary>
        /// ダメージアニメーション（敵用）
        /// </summary>
        public void PlayDamageAnimation()
        {
            // 揺れアニメーション
            transform.DOShakePosition(0.3f, strength: 10f, vibrato: 10, randomness: 90f);

            // 赤フラッシュ
            var originalColor = backgroundImage.color;
            backgroundImage.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                backgroundImage.color = originalColor;
            });
        }

        public Tile GetTileData()
        {
            return tileData;
        }

        public Vector2Int GetGridPosition()
        {
            return gridPosition;
        }
    }
}
