using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// とげマス（ボスが生成）
    /// 要件: 6.4
    /// </summary>
    public class ThornTile : Tile
    {
        /// <summary>ダメージ値</summary>
        public int Damage { get; set; }

        public ThornTile(int damage)
        {
            Type = TileType.Thorn;
            Damage = damage;
        }

        /// <summary>
        /// 効果を適用する（ダメージを与える）
        /// 要件: 6.4
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            var result = new TileEffectResult();

            // プレイヤーにダメージを与える
            player.TakeDamage(Damage);
            result.DamageTaken = Damage;
            result.EffectApplied = true;

            Debug.Log($"ThornTile ({Position}): {Damage}ダメージ！");
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
