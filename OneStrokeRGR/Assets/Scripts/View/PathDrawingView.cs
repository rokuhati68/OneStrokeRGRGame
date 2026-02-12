using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;
using OneStrokeRGR.Presenter;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// パス描画の視覚フィードバックを管理するView
    /// 要件: 14.5, UI仕様
    /// </summary>
    public class PathDrawingView : MonoBehaviour
    {
        [Header("参照")]
        public BoardView boardView;
        public LineRenderer pathLineRenderer;
        public Camera uiCamera; // UI用カメラ（なければMainCameraを使用）

        [Header("プレビューUI")]
        public GameObject previewPanel;
        public TextMeshProUGUI previewAttackText;
        public TextMeshProUGUI previewGoldText;
        public TextMeshProUGUI previewHPText;

        [Header("確認ボタン")]
        public Button confirmButton;
        public Button cancelButton;

        [Header("線描画設定")]
        public Color validPathColor = Color.cyan;
        public Color invalidPathColor = Color.red;
        public float lineWidth = 8f;

        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private List<GameObject> lineSegments = new List<GameObject>();
        private Transform lineContainer;
        private bool isDrawing = false;
        private bool isPathValid = false;
        private bool isWaitingForConfirmation = false;
        private bool pathConfirmed = false;

        private PathPresenter pathPresenter;
        private GameState gameState;
        private Vector2Int playerPosition;

        private void Awake()
        {
            // LineRendererの初期設定（互換性のため残す）
            if (pathLineRenderer != null)
            {
                pathLineRenderer.startWidth = lineWidth;
                pathLineRenderer.endWidth = lineWidth;
                pathLineRenderer.positionCount = 0;
                pathLineRenderer.enabled = false;
            }

            // UI線描画用コンテナの作成
            var containerObj = new GameObject("PathLineContainer");
            containerObj.transform.SetParent(boardView != null ? boardView.transform.parent : transform, false);
            var containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            lineContainer = containerObj.transform;

            // プレビューパネルを非表示
            if (previewPanel != null)
                previewPanel.SetActive(false);

            // ボタンイベントの設定
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
                confirmButton.gameObject.SetActive(false);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
                cancelButton.gameObject.SetActive(false);
            }

            // カメラの設定
            if (uiCamera == null)
            {
                uiCamera = Camera.main;
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(PathPresenter presenter, GameState state)
        {
            pathPresenter = presenter;
            gameState = state;
        }

        /// <summary>
        /// パス入力を待機
        /// 要件: 15.1, 15.2, 15.3, 15.4
        /// </summary>
        public async UniTask<List<Vector2Int>> WaitForPathInput(Vector2Int startPosition)
        {
            playerPosition = startPosition;
            currentPath.Clear();
            isDrawing = false;
            isWaitingForConfirmation = false;
            pathConfirmed = false;

            Debug.Log("PathDrawingView: パス入力待機開始");

            // 入力ループ
            while (!pathConfirmed && !isWaitingForConfirmation)
            {
                await UniTask.Yield();
            }

            // 確認待ち
            if (isWaitingForConfirmation)
            {
                await UniTask.WaitUntil(() => pathConfirmed || currentPath.Count == 0);
            }

            // パスのコピーを作成（クリーンアップ前に！）
            List<Vector2Int> resultPath = new List<Vector2Int>(currentPath);

            // クリーンアップ（線は残す — プレイヤー移動中に順次消す）
            if (boardView != null && currentPath.Count > 0)
            {
                boardView.HighlightPath(currentPath, false);
            }
            currentPath.Clear();
            HideConfirmButtons();
            HidePathPreview();

            Debug.Log($"PathDrawingView: パス入力完了 - {resultPath.Count}マス");
            return resultPath;
        }

        private void Update()
        {
            if (isWaitingForConfirmation)
                return;

            // マウス入力検出
            HandleMouseInput();
        }

        /// <summary>
        /// マウス入力処理
        /// 要件: 15.1
        /// </summary>
        private void HandleMouseInput()
        {
            // マウスボタンが押された
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"PathDrawingView: マウスクリック検出 - IsPointerOverUI: {IsPointerOverUI()}");
                if (!IsPointerOverUI())
                {
                    StartDrawing();
                }
                else
                {
                    // UI要素の上なので、タイルをチェック
                    Vector2Int? tilePos = GetTileAtMousePosition();
                    Debug.Log($"PathDrawingView: タイル位置検出 - {tilePos}");
                    if (tilePos.HasValue)
                    {
                        // タイルの上なので描画開始
                        StartDrawing();
                    }
                }
            }

            // マウスドラッグ中
            if (Input.GetMouseButton(0) && isDrawing)
            {
                ContinueDrawing();
            }

            // マウスボタンが離された
            if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                EndDrawing();
            }
        }

        /// <summary>
        /// 描画開始
        /// </summary>
        private void StartDrawing()
        {
            currentPath.Clear();
            isDrawing = true;

            // 最初のタイルを追加
            Vector2Int? tilePos = GetTileAtMousePosition();
            Debug.Log($"PathDrawingView: StartDrawing - タイル位置: {tilePos}, プレイヤー位置: {playerPosition}");

            if (tilePos.HasValue)
            {
                // プレイヤー位置から開始する必要がある
                if (tilePos.Value == playerPosition)
                {
                    currentPath.Add(tilePos.Value);
                    UpdatePathVisualization();
                    Debug.Log($"PathDrawingView: パス描画開始 - {tilePos.Value}");
                }
                else
                {
                    isDrawing = false;
                    Debug.Log($"PathDrawingView: パスはプレイヤー位置から開始してください (クリック: {tilePos.Value})");
                }
            }
            else
            {
                isDrawing = false;
                Debug.Log("PathDrawingView: タイルが検出されませんでした");
            }
        }

        /// <summary>
        /// 描画継続
        /// </summary>
        private void ContinueDrawing()
        {
            Vector2Int? tilePos = GetTileAtMousePosition();
            if (tilePos.HasValue)
            {
                // 既にパスに含まれている場合はスキップ
                if (currentPath.Contains(tilePos.Value))
                    return;

                // パスの最後のタイルと隣接しているかチェック
                if (currentPath.Count > 0)
                {
                    Vector2Int lastPos = currentPath[currentPath.Count - 1];
                    if (IsAdjacent(lastPos, tilePos.Value))
                    {
                        currentPath.Add(tilePos.Value);
                        UpdatePathVisualization();
                        Debug.Log($"PathDrawingView: タイル追加 - {tilePos.Value}, パス長: {currentPath.Count}");
                    }
                }
            }
        }

        /// <summary>
        /// 描画終了
        /// </summary>
        private void EndDrawing()
        {
            isDrawing = false;

            if (currentPath.Count > 1)
            {
                // パスが有効か検証
                ValidateAndShowConfirmation();
            }
            else
            {
                // パスが短すぎる場合はクリア
                ClearPath();
                Debug.Log("PathDrawingView: パスが短すぎます");
            }
        }

        /// <summary>
        /// パスを検証して確認ボタンを表示
        /// </summary>
        private void ValidateAndShowConfirmation()
        {
            if (pathPresenter == null || gameState == null)
            {
                Debug.LogWarning("PathDrawingView: PathPresenterまたはGameStateが設定されていません");
                return;
            }

            // パス検証
            isPathValid = pathPresenter.ValidatePath(currentPath, playerPosition);

            // パスを再描画（色を更新）
            DrawPath(currentPath, isPathValid);

            if (isPathValid)
            {
                // プレビュー情報を計算して表示
                var preview = pathPresenter.CalculatePathPreview(currentPath, gameState.Player, gameState.Board);

                // 最終的な値を計算
                int finalGold = gameState.Player.Gold + preview.PredictedGoldGained - preview.PredictedGoldSpent;
                int finalHP = gameState.Player.CurrentHP + preview.PredictedHPChange;

                ShowPathPreview(preview.PredictedAttackPower, finalGold, finalHP);

                // 確認ボタンを表示
                ShowConfirmButtons();
                isWaitingForConfirmation = true;
            }
            else
            {
                Debug.Log("PathDrawingView: 無効なパスです");
                // 少し待ってからクリア
                ClearPathDelayed().Forget();
            }
        }

        /// <summary>
        /// 遅延してパスをクリア
        /// </summary>
        private async UniTaskVoid ClearPathDelayed()
        {
            await UniTask.Delay(1000);
            ClearPath();
        }

        /// <summary>
        /// マウス位置のタイルを取得
        /// </summary>
        private Vector2Int? GetTileAtMousePosition()
        {
            if (boardView == null)
                return null;

            // マウス位置からRayを発射
            Vector2 mousePos = Input.mousePosition;

            // UI要素のヒットテスト
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // TileViewコンポーネントを持つオブジェクトを探す
            foreach (var result in results)
            {
                TileView tileView = result.gameObject.GetComponentInParent<TileView>();
                if (tileView != null)
                {
                    return tileView.GetGridPosition();
                }
            }

            return null;
        }

        /// <summary>
        /// 2つの位置が隣接しているかチェック
        /// </summary>
        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            int diffX = Mathf.Abs(pos1.x - pos2.x);
            int diffY = Mathf.Abs(pos1.y - pos2.y);
            return (diffX + diffY == 1); // 上下左右のみ（斜めは不可）
        }

        /// <summary>
        /// UI上にポインタがあるかチェック
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// パス視覚化を更新
        /// 要件: 15.2
        /// </summary>
        private void UpdatePathVisualization()
        {
            // パスを描画
            DrawPath(currentPath, true);

            // タイルのハイライト
            if (boardView != null)
            {
                boardView.HighlightPath(currentPath, true);
            }
        }

        /// <summary>
        /// パスを描画（UI Imageベース）
        /// 要件: 15.2
        /// </summary>
        public void DrawPath(List<Vector2Int> path, bool isValid)
        {
            currentPath = path;
            isPathValid = isValid;

            // 既存の線分をクリア
            ClearLineSegments();

            if (path == null || path.Count < 2)
                return;

            Color lineColor = isValid ? validPathColor : invalidPathColor;

            // 連続するタイル間に線分を作成
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPos = GetWorldPositionFromGrid(path[i]);
                Vector3 endPos = GetWorldPositionFromGrid(path[i + 1]);
                CreateLineSegment(startPos, endPos, lineColor);
            }
        }

        /// <summary>
        /// 2点間にUI Imageで線分を作成
        /// </summary>
        private void CreateLineSegment(Vector3 start, Vector3 end, Color color)
        {
            if (lineContainer == null) return;

            var lineObj = new GameObject("LineSegment");
            lineObj.transform.SetParent(lineContainer, false);

            var image = lineObj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var rect = lineObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 0.5f);

            // 位置と回転を設定
            rect.position = start;
            float distance = Vector3.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            rect.sizeDelta = new Vector2(distance, lineWidth);
            rect.localRotation = Quaternion.Euler(0, 0, angle);

            lineSegments.Add(lineObj);
        }

        /// <summary>
        /// 線分をクリア
        /// </summary>
        private void ClearLineSegments()
        {
            foreach (var seg in lineSegments)
            {
                if (seg != null) Destroy(seg);
            }
            lineSegments.Clear();
        }

        /// <summary>
        /// 先頭の線分を1本削除（プレイヤーが通過した区間を消す）
        /// </summary>
        public void RemoveFirstLineSegment()
        {
            if (lineSegments.Count > 0)
            {
                if (lineSegments[0] != null)
                    Destroy(lineSegments[0]);
                lineSegments.RemoveAt(0);
            }
        }

        /// <summary>
        /// 残りの線分をすべて削除
        /// </summary>
        public void ClearRemainingLineSegments()
        {
            ClearLineSegments();
        }

        /// <summary>
        /// パスをクリア
        /// </summary>
        public void ClearPath()
        {
            // ハイライトをクリア
            if (boardView != null && currentPath.Count > 0)
            {
                boardView.HighlightPath(currentPath, false);
            }

            currentPath.Clear();

            // UI線分をクリア
            ClearLineSegments();

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
                pathLineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// グリッド座標からワールド座標に変換
        /// </summary>
        private Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
        {
            if (boardView != null)
            {
                TileView tileView = boardView.GetTileView(gridPos);
                if (tileView != null)
                {
                    return tileView.transform.position;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// パスプレビュー情報を表示
        /// 要件: 15.3
        /// </summary>
        public void ShowPathPreview(int predictedAttack, int predictedGold, int predictedHP)
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(true);

                if (previewAttackText != null)
                    previewAttackText.text = $"攻撃力: {predictedAttack}";

                if (previewGoldText != null)
                    previewGoldText.text = $"ゴールド: {predictedGold}";

                if (previewHPText != null)
                    previewHPText.text = $"HP: {predictedHP}";
            }
        }

        /// <summary>
        /// パスプレビューを非表示
        /// </summary>
        public void HidePathPreview()
        {
            if (previewPanel != null)
                previewPanel.SetActive(false);
        }

        /// <summary>
        /// 確認ボタンを表示
        /// 要件: 15.4
        /// </summary>
        private void ShowConfirmButtons()
        {
            if (confirmButton != null)
                confirmButton.gameObject.SetActive(true);

            if (cancelButton != null)
                cancelButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// 確認ボタンを非表示
        /// </summary>
        private void HideConfirmButtons()
        {
            if (confirmButton != null)
                confirmButton.gameObject.SetActive(false);

            if (cancelButton != null)
                cancelButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// 確認ボタンクリック
        /// </summary>
        private void OnConfirmButtonClicked()
        {
            pathConfirmed = true;
            isWaitingForConfirmation = false;
            Debug.Log("PathDrawingView: パスが確認されました");
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void OnCancelButtonClicked()
        {
            ClearPath();
            HideConfirmButtons();
            HidePathPreview();
            isWaitingForConfirmation = false;
            Debug.Log("PathDrawingView: パスがキャンセルされました");
        }

        private void OnDestroy()
        {
            // イベントリスナーの解除
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelButtonClicked);

            ClearLineSegments();
            if (lineContainer != null)
                Destroy(lineContainer.gameObject);
        }
    }
}
