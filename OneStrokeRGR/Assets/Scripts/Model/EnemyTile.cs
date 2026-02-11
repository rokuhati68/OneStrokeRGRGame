using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 敵マス
    /// 要件: 5.1, 5.2, 5.3
    /// </summary>
    public class EnemyTile : Tile
    {
        /// <summary>このマスに配置されている敵</summary>
        public Enemy Enemy { get; set; }

        public EnemyTile(Enemy enemy)
        {
            Type = TileType.Enemy;
            Enemy = enemy;
        }

        /// <summary>
        /// 効果を適用する（敵への攻撃と反撃）
        /// 要件: 5.1, 5.2, 5.3
        /// </summary>
        public override TileEffectResult ApplyEffect(Player player, GameContext context)
        {
            var result = new TileEffectResult();

            if (Enemy == null)
            {
                Debug.LogWarning($"EnemyTile ({Position}): 敵が存在しません");
                return result;
            }

            // 敵にダメージを与える（要件: 5.1）
            int attackPower = player.AttackPower;
            Enemy.TakeDamage(attackPower);

            Debug.Log($"EnemyTile ({Position}): 敵に{attackPower}ダメージ。敵HP: {Enemy.CurrentHP}/{Enemy.MaxHP}");

            // 敵が倒されたか判定（要件: 5.2）
            if (!Enemy.IsAlive())
            {
                result.DefeatedEnemy = Enemy;
                Debug.Log($"EnemyTile ({Position}): 敵を倒した！");
            }
            else
            {
                // 敵が生き残った場合、反撃ダメージを受ける（要件: 5.3）
                int counterAttack = Enemy.AttackPower;
                player.TakeDamage(counterAttack);
                result.DamageTaken = counterAttack;

                Debug.Log($"EnemyTile ({Position}): 敵の反撃！{counterAttack}ダメージ受けた");
            }

            result.EffectApplied = true;
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
