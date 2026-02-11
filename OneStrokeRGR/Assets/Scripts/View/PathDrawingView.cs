using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// パス描画の視覚フィードバックを管理するView（スケルトン）
    /// 詳細はタスク15で実装
    /// </summary>
    public class PathDrawingView : MonoBehaviour
    {
        [Header("参照")]
        public BoardView boardView;
        public LineRenderer pathLineRenderer;

        [Header("設定")]
        public Color validPathColor = Color.cyan;
        public Color invalidPathColor = Color.red;
        public float lineWidth = 5f;

        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private bool isDrawing = false;
        private bool isPathValid = false;

        private void Awake()
        {
            // LineRendererの初期設定
            if (pathLineRenderer != null)
            {
                pathLineRenderer.startWidth = lineWidth;
                pathLineRenderer.endWidth = lineWidth;
                pathLineRenderer.positionCount = 0;
                pathLineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// パス入力を待機（仮実装）
        /// タスク15で詳細実装
        /// </summary>
        public async UniTask<List<Vector2Int>> WaitForPathInput(Vector2Int playerPosition)
        {
            Debug.Log("PathDrawingView: パス入力待機（仮実装）");

            // TODO: タスク15でマウス入力検出を実装
            // TODO: リアルタイムのパス描画を実装
            // TODO: パス検証とフィードバックを実装

            // 仮実装: 空のパスを返す
            await UniTask.Yield();
            return new List<Vector2Int>();
        }

        /// <summary>
        /// パスを描画
        /// </summary>
        public void DrawPath(List<Vector2Int> path, bool isValid)
        {
            if (pathLineRenderer == null)
                return;

            currentPath = path;
            isPathValid = isValid;

            if (path == null || path.Count == 0)
            {
                pathLineRenderer.enabled = false;
                return;
            }

            pathLineRenderer.enabled = true;
            pathLineRenderer.positionCount = path.Count;
            pathLineRenderer.material.color = isValid ? validPathColor : invalidPathColor;

            // TODO: タスク15で実際の座標変換を実装
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = GetWorldPositionFromGrid(path[i]);
                pathLineRenderer.SetPosition(i, worldPos);
            }
        }

        /// <summary>
        /// パスをクリア
        /// </summary>
        public void ClearPath()
        {
            currentPath.Clear();
            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
                pathLineRenderer.enabled = false;
            }

            // ハイライトをクリア
            if (boardView != null)
            {
                boardView.HighlightPath(currentPath, false);
            }
        }

        /// <summary>
        /// グリッド座標からワールド座標に変換（仮実装）
        /// </summary>
        private Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
        {
            // TODO: タスク15で実際の座標変換を実装
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
        /// パスプレビュー情報を表示（仮実装）
        /// </summary>
        public void ShowPathPreview(int predictedAttack, int predictedGold, int predictedHP)
        {
            // TODO: タスク15で実装
            Debug.Log($"PathDrawingView: プレビュー - ATK:{predictedAttack}, Gold:{predictedGold}, HP:{predictedHP}");
        }

        /// <summary>
        /// パスプレビューを非表示
        /// </summary>
        public void HidePathPreview()
        {
            // TODO: タスク15で実装
        }
    }
}
