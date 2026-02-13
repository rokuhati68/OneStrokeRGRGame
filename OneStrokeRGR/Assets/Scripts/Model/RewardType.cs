namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 報酬の種類を定義する列挙型
    /// </summary>
    public enum RewardType
    {
        /// <summary>攻撃力上昇マス出現率増加</summary>
        AttackBoostRateIncrease,

        /// <summary>攻撃力上昇マス値増加</summary>
        AttackBoostValueIncrease,

        /// <summary>HP回復マス出現率増加</summary>
        HPRecoveryRateIncrease,

        /// <summary>効果なしマス出現率増加</summary>
        EmptyRateIncrease,

        /// <summary>ゴールドマス出現率増加</summary>
        GoldRateIncrease,

        /// <summary>ゴールドマス値増加</summary>
        GoldValueIncrease,

        /// <summary>一筆書きボーナス値増加</summary>
        OneStrokeBonusIncrease,
        /// <summary>
        /// ゴールドを手に入れる
        /// </summary>
        GoldGet,
        /// <summary>
        /// HPが回復する
        /// </summary>
        HPRecover
    }
}
