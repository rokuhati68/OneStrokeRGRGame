using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// HP回復マス
    /// 要件: 3.3, 3.5, 4.1, 4.2, 11.5
    /// </summary>
    public class HPRecoveryTile : Tile
    {
        public HPRecoveryTile()
        {
            Type = TileType.HPRecovery;
        }

        /// <summary>
        /// 効果を適用する
        /// 要件: 3.3, 4.2, 11.5
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            var result = new TileEffectResult();

            // ゴールドが不足している場合は効果を適用しない（要件: 3.5）
            if (!CanApplyEffect(player))
            {
                Debug.Log($"HPRecoveryTile ({Position}): ゴールド不足のため効果なし");
                return result;
            }

            // コンボ中はゴールド消費なし（要件: 4.2）
            if (!context.IsComboActive)
            {
                player.SpendGold(1);
                result.GoldSpent = 1;
            }

            // HPを1回復（最大HPを超えない、要件: 11.5）
            player.Heal(1);
            result.HPGained = 1;
            result.EffectApplied = true;

            Debug.Log($"HPRecoveryTile ({Position}): HP+1（ゴールド消費: {result.GoldSpent}）");
            return result;
        }

        /// <summary>
        /// ゴールドが1以上ある場合に適用可能
        /// 要件: 3.5
        /// </summary>
        public override bool CanApplyEffect(Player player)
        {
            return player.Gold >= 1;
        }
    }
}
