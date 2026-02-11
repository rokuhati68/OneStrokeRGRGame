using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 壁マス（ボスが生成、通行不可）
    /// 要件: 6.5
    /// </summary>
    public class WallTile : Tile
    {
        public WallTile()
        {
            Type = TileType.Wall;
        }

        /// <summary>
        /// 効果を適用する（通行不可のため、通常は呼ばれない）
        /// 要件: 6.5
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            Debug.LogWarning($"WallTile ({Position}): 壁マスは通行できません");
            return new TileEffectResult { EffectApplied = false };
        }

        /// <summary>
        /// 壁マスは通行不可のため、常に適用不可
        /// 要件: 6.5
        /// </summary>
        public override bool CanApplyEffect(Player player)
        {
            return false;
        }
    }
}
