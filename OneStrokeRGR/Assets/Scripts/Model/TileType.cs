namespace OneStrokeRGR.Model
{
    /// <summary>
    /// タイルの種類を定義する列挙型
    /// </summary>
    public enum TileType
    {
        /// <summary>効果なしマス</summary>
        Empty,

        /// <summary>攻撃力上昇マス</summary>
        AttackBoost,

        /// <summary>HP回復マス</summary>
        HPRecovery,

        /// <summary>ゴールドマス</summary>
        Gold,

        /// <summary>敵マス</summary>
        Enemy,

        /// <summary>とげマス（ボスが生成）</summary>
        Thorn,

        /// <summary>壁マス（ボスが生成）</summary>
        Wall
    }
}
