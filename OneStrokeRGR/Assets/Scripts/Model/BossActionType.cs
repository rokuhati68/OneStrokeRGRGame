namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 敵の行動タイプを定義する列挙型
    /// </summary>
    public enum BossActionType
    {
        /// <summary>プレイヤーに攻撃</summary>
        Attack,

        /// <summary>攻撃力上昇マス効果無効化</summary>
        DisableAttackBoost,
        /// <summary>
        /// 回復マス無効化
        /// </summary>
        DisableHeal,
        /// <summary>
        /// ゴールドマス無効化
        /// </summary>
        DisableGold,

        /// <summary>自身のHP回復</summary>
        HealSelf,

        /// <summary>とげマス生成</summary>
        SpawnThorns,

        /// <summary>壁マス生成</summary>
        SpawnWalls,

        DecreaseAttack,
        DecreaseGold
    }
}
