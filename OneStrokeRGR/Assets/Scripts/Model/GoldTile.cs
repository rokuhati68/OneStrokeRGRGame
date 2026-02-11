using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// ゴールドマス
    /// 要件: 3.4, 12.4
    /// </summary>
    public class GoldTile : Tile
    {
        /// <summary>ゴールド値</summary>
        public int GoldValue { get; set; }

        public GoldTile(int value)
        {
            Type = TileType.Gold;
            GoldValue = value;
        }

        /// <summary>
        /// 効果を適用する
        /// 要件: 3.4
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            var result = new TileEffectResult();

            // ゴールドを追加
            player.AddGold(GoldValue);
            result.GoldGained = GoldValue;
            result.EffectApplied = true;

            Debug.Log($"GoldTile ({Position}): ゴールド+{GoldValue}");
            return result;
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
