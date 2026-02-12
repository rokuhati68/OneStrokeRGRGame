using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// 5×5ボードグリッドの視覚表現を管理するView
    /// 要件: 10.1, 14.4
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("プレハブ")]
        public GameObject tileViewPrefab;

        [Header("アイコン設定")]
        public TileIconConfig tileIconConfig;

        [Header("プレイヤーアイコン")]
        public Sprite playerIconSprite;
        public float playerMoveSpeed = 0.25f;

        [Header("レイアウト設定")]
        public GridLayoutGroup gridLayoutGroup;
        public float tileSpacing = 10f;
        public float tileSize = 100f;

        private TileView[,] tileViews = new TileView[Board.BoardSize, Board.BoardSize];
        private GameObject playerIconObject;

        private void Awake()
        {
            if (gridLayoutGroup == null)
            {
                gridLayoutGroup = GetComponent<GridLayoutGroup>();
            }

            // GridLayoutGroupの設定
            if (gridLayoutGroup != null)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = Board.BoardSize;
                gridLayoutGroup.spacing = new Vector2(tileSpacing, tileSpacing);
                gridLayoutGroup.cellSize = new Vector2(tileSize, tileSize);
            }
        }

        /// <summary>
        /// ボードを初期化
        /// </summary>
        public async UniTask InitializeBoard(Board board)
        {
            Debug.Log("BoardView: ボード初期化開始");

            // 既存のタイルビューをクリア
            ClearBoard();

            // タイルビューを生成
            for (int y = Board.BoardSize - 1; y >= 0; y--) // 上から下へ
            {
                for (int x = 0; x < Board.BoardSize; x++) // 左から右へ
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = board.GetTile(pos);

                    // TileViewを生成
                    GameObject tileObj = Instantiate(tileViewPrefab, transform);
                    TileView tileView = tileObj.GetComponent<TileView>();

                    if (tileView != null)
                    {
                        tileView.Setup(tile, pos, tileIconConfig);
                        tileViews[x, y] = tileView;

                        // 出現アニメーション
                        tileView.PlayAppearAnimation();
                    }
                    else
                    {
                        Debug.LogError($"BoardView: TileViewコンポーネントが見つかりません at {pos}");
                    }

                    // 少し遅延を入れて順次表示
                    await UniTask.Delay(20);
                }
            }

            Debug.Log("BoardView: ボード初期化完了");
        }

        /// <summary>
        /// プレイヤーアイコンを初期化して指定位置に配置
        /// </summary>
        public void InitializePlayerIcon(Vector2Int position)
        {
            if (playerIconObject == null)
            {
                playerIconObject = new GameObject("PlayerIcon");
                playerIconObject.transform.SetParent(transform.parent, false);

                var image = playerIconObject.AddComponent<Image>();
                if (playerIconSprite != null)
                {
                    image.sprite = playerIconSprite;
                }
                image.raycastTarget = false;

                // タイルと同じサイズに設定
                var rectTransform = playerIconObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(tileSize, tileSize);
            }

            SetPlayerIconPosition(position);
            playerIconObject.SetActive(true);
        }

        /// <summary>
        /// プレイヤーアイコンの位置を即座に設定
        /// </summary>
        public void SetPlayerIconPosition(Vector2Int gridPos)
        {
            if (playerIconObject == null || !IsValidPosition(gridPos)) return;

            TileView tileView = tileViews[gridPos.x, gridPos.y];
            if (tileView != null)
            {
                playerIconObject.transform.position = tileView.transform.position;
            }
        }

        /// <summary>
        /// プレイヤーアイコンをアニメーション付きで移動
        /// </summary>
        public async UniTask MovePlayerTo(Vector2Int gridPos)
        {
            if (playerIconObject == null || !IsValidPosition(gridPos)) return;

            TileView tileView = tileViews[gridPos.x, gridPos.y];
            if (tileView == null) return;

            Vector3 targetPos = tileView.transform.position;
            await playerIconObject.transform.DOMove(targetPos, playerMoveSpeed)
                .SetEase(Ease.InOutQuad)
                .AsyncWaitForCompletion();
        }

        /// <summary>
        /// プレイヤーアイコンの表示・非表示
        /// </summary>
        public void SetPlayerIconVisible(bool visible)
        {
            if (playerIconObject != null)
            {
                playerIconObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 特定のタイルを更新
        /// </summary>
        public void UpdateTile(Vector2Int position, Tile newTile)
        {
            if (!IsValidPosition(position))
            {
                Debug.LogWarning($"BoardView: 無効な位置 {position}");
                return;
            }

            TileView tileView = tileViews[position.x, position.y];
            if (tileView != null)
            {
                tileView.Setup(newTile, position, tileIconConfig);
                tileView.UpdateVisuals();
            }
        }

        /// <summary>
        /// 複数のタイルを更新（アニメーション付き）
        /// </summary>
        public async UniTask UpdateTiles(List<Vector2Int> positions, Board board)
        {
            foreach (var pos in positions)
            {
                if (!IsValidPosition(pos))
                    continue;

                TileView tileView = tileViews[pos.x, pos.y];
                if (tileView != null)
                {
                    // 消失アニメーション
                    await tileView.PlayDisappearAnimation();

                    // 新しいタイルデータをセット
                    Tile newTile = board.GetTile(pos);
                    tileView.Setup(newTile, pos, tileIconConfig);

                    // 出現アニメーション
                    tileView.PlayAppearAnimation();

                    await UniTask.Delay(50);
                }
            }
        }

        /// <summary>
        /// タイルのハイライトを設定
        /// </summary>
        public void HighlightTile(Vector2Int position, bool highlight)
        {
            if (!IsValidPosition(position))
                return;

            TileView tileView = tileViews[position.x, position.y];
            if (tileView != null)
            {
                tileView.SetHighlight(highlight);
            }
        }

        /// <summary>
        /// 複数タイルのハイライトを設定
        /// </summary>
        public void HighlightPath(List<Vector2Int> path, bool highlight)
        {
            foreach (var pos in path)
            {
                HighlightTile(pos, highlight);
            }
        }

        /// <summary>
        /// タイルの効果アニメーションを再生
        /// </summary>
        public async UniTask PlayTileEffectAnimation(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return;

            TileView tileView = tileViews[position.x, position.y];
            if (tileView != null)
            {
                tileView.PlayEffectAnimation();
                await UniTask.Delay(300);
            }
        }

        /// <summary>
        /// 敵のダメージアニメーションを再生
        /// </summary>
        public async UniTask PlayEnemyDamageAnimation(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return;

            TileView tileView = tileViews[position.x, position.y];
            if (tileView != null)
            {
                tileView.PlayDamageAnimation();
                await UniTask.Delay(300);
            }
        }

        /// <summary>
        /// 特定位置のTileViewを取得
        /// </summary>
        public TileView GetTileView(Vector2Int position)
        {
            if (!IsValidPosition(position))
                return null;

            return tileViews[position.x, position.y];
        }

        /// <summary>
        /// ボードをクリア
        /// </summary>
        private void ClearBoard()
        {
            for (int x = 0; x < Board.BoardSize; x++)
            {
                for (int y = 0; y < Board.BoardSize; y++)
                {
                    if (tileViews[x, y] != null)
                    {
                        Destroy(tileViews[x, y].gameObject);
                        tileViews[x, y] = null;
                    }
                }
            }
        }

        /// <summary>
        /// 位置が有効かチェック
        /// </summary>
        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < Board.BoardSize &&
                   position.y >= 0 && position.y < Board.BoardSize;
        }

        private void OnDestroy()
        {
            ClearBoard();
            if (playerIconObject != null)
            {
                Destroy(playerIconObject);
                playerIconObject = null;
            }
        }
    }
}
