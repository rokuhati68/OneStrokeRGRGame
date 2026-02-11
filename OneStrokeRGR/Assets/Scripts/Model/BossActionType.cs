namespace OneStrokeRGR.Model
{
    /// <summary>
    /// ボスの行動タイプを定義する列挙型
    /// </summary>
    public enum BossActionType
    {
        /// <summary>攻撃力上昇マス効果無効化</summary>
        DisableAttackBoost,

        /// <summary>ボスHP回復</summary>
        HealSelf,

        /// <summary>とげマス生成</summary>
        SpawnThorns,

        /// <summary>壁マス生成</summary>
        SpawnWalls
    }
}
