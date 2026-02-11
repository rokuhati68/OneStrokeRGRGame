using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// コンボ状態を追跡するクラス
    /// 要件: 4.1, 4.2, 4.3, 4.4
    /// </summary>
    public class ComboTracker
    {
        private TileType? lastTileType = null;
        private int comboCount = 0;

        /// <summary>現在のコンボ数</summary>
        public int ComboCount => comboCount;

        /// <summary>
        /// 現在のタイルタイプでコンボが有効か判定
        /// 要件: 4.1
        /// </summary>
        /// <param name="currentType">現在のタイルタイプ</param>
        /// <returns>コンボ中の場合true</returns>
        public bool IsComboActive(TileType currentType)
        {
            // コンボ対象のタイルタイプか確認
            if (!IsComboEligibleType(currentType))
            {
                return false;
            }

            // 前回と同じタイプで、既にコンボが始まっている場合
            return lastTileType.HasValue &&
                   lastTileType.Value == currentType &&
                   comboCount >= 2;
        }

        /// <summary>
        /// コンボ状態を更新
        /// 要件: 4.1, 4.3
        /// </summary>
        /// <param name="currentType">現在のタイルタイプ</param>
        public void UpdateCombo(TileType currentType)
        {
            // コンボ対象のタイルタイプか確認
            if (!IsComboEligibleType(currentType))
            {
                // コンボ対象外のタイルならカウントリセット
                if (currentType != TileType.Empty)
                {
                    Reset();
                }
                return;
            }

            // 前回と同じタイプの場合、コンボ継続
            if (lastTileType.HasValue && lastTileType.Value == currentType)
            {
                comboCount++;
                Debug.Log($"ComboTracker: コンボ{comboCount}連鎖！（{currentType}）");
            }
            else
            {
                // 異なるタイプの場合、コンボリセット（要件: 4.3）
                lastTileType = currentType;
                comboCount = 1;
                Debug.Log($"ComboTracker: コンボ開始（{currentType}）");
            }
        }

        /// <summary>
        /// コンボ状態をリセット
        /// 要件: 4.4
        /// </summary>
        public void Reset()
        {
            lastTileType = null;
            comboCount = 0;
            Debug.Log("ComboTracker: コンボリセット");
        }

        /// <summary>
        /// タイルタイプがコンボ対象か判定
        /// 要件: 4.5 - 攻撃力上昇マスとHP回復マスのみコンボ対象
        /// </summary>
        private bool IsComboEligibleType(TileType type)
        {
            return type == TileType.AttackBoost || type == TileType.HPRecovery;
        }
    }
}
