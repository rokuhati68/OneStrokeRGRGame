using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 攻撃力上昇マス
    /// 要件: 3.2, 3.5, 4.1, 4.2, 12.3
    /// </summary>
    public class AttackBoostTile : Tile
    {
        /// <summary>攻撃力上昇値</summary>
        public int BoostValue { get; set; }

        public AttackBoostTile(int value)
        {
            Type = TileType.AttackBoost;
            BoostValue = value;
        }

        /// <summary>
        /// 効果を適用する
        /// 要件: 3.2, 4.2
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            var result = new TileEffectResult();

            // ゴールドが不足している場合は効果を適用しない（要件: 3.5）
            if (!CanApplyEffect(player))
            {
                Debug.Log($"AttackBoostTile ({Position}): ゴールド不足のため効果なし");
                return result;
            }

            // コンボ中はゴールド消費なし（要件: 4.2）
            if (!context.IsComboActive)
            {
                player.SpendGold(1);
                result.GoldSpent = 1;
            }

            // 攻撃力を増加
            player.IncreaseAttackPower(BoostValue);
            result.AttackGained = BoostValue;
            result.EffectApplied = true;

            Debug.Log($"AttackBoostTile ({Position}): 攻撃力+{BoostValue}（ゴールド消費: {result.GoldSpent}）");
            return result;
        }

        /// <summary>
        /// ゴールドが1以上ある、またはコンボ中の場合に適用可能
        /// 要件: 3.5, 4.2
        /// </summary>
        public override bool CanApplyEffect(Player player)
        {
            return player.Gold >= 1;
        }
    }
}
