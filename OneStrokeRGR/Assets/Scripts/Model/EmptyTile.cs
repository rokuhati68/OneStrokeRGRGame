using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 効果なしマス
    /// 要件: 3.1
    /// </summary>
    public class EmptyTile : Tile
    {
        public EmptyTile()
        {
            Type = TileType.Empty;
        }

        /// <summary>
        /// 効果を適用する（何もしない）
        /// 要件: 3.1
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            Debug.Log($"EmptyTile ({Position}): 効果なし");
            return new TileEffectResult { EffectApplied = false };
        }

        /// <summary>
        /// 常に適用可能
        /// </summary>
        public override bool CanApplyEffect(Player player)
        {
            return true;
        }
    }
}
