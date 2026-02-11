namespace OneStrokeRGR.Model
{
    /// <summary>
    /// タイル効果の適用結果を保持する構造体
    /// </summary>
    public class TileEffectResult
    {
        /// <summary>効果が適用されたかどうか</summary>
        public bool EffectApplied { get; set; }

        /// <summary>消費したゴールド量</summary>
        public int GoldSpent { get; set; }

        /// <summary>獲得した攻撃力</summary>
        public int AttackGained { get; set; }

        /// <summary>回復したHP量</summary>
        public int HPGained { get; set; }

        /// <summary>獲得したゴールド量</summary>
        public int GoldGained { get; set; }

        /// <summary>受けたダメージ量</summary>
        public int DamageTaken { get; set; }

        /// <summary>倒した敵（nullの場合は倒していない）</summary>
        public Enemy DefeatedEnemy { get; set; }

        public TileEffectResult()
        {
            EffectApplied = false;
            GoldSpent = 0;
            AttackGained = 0;
            HPGained = 0;
            GoldGained = 0;
            DamageTaken = 0;
            DefeatedEnemy = null;
        }
    }
}
