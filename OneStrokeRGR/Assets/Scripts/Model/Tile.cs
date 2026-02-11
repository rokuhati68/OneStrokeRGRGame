using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// タイルの抽象基底クラス
    /// 要件: 3.1, 3.2, 3.3, 3.4, 3.5
    /// </summary>
    public abstract class Tile
    {
        /// <summary>タイルの位置</summary>
        public Vector2Int Position { get; set; }

        /// <summary>タイルの種類</summary>
        public TileType Type { get; protected set; }

        /// <summary>
        /// タイルの効果を適用する
        /// 要件: 3.1, 3.2, 3.3, 3.4, 3.5
        /// </summary>
        /// <param name="player">効果を適用するプレイヤー</param>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>効果の適用結果</returns>
        public abstract TileEffectResult ApplyEffect(Player player, GameContext context);

        /// <summary>
        /// 効果を適用できるか判定する
        /// 要件: 3.5, 11.3
        /// </summary>
        /// <param name="player">効果を適用するプレイヤー</param>
        /// <returns>適用可能な場合true</returns>
        public abstract bool CanApplyEffect(Player player);
    }
}
