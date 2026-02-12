using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// タイルタイプごとのアイコンSprite設定を管理するScriptableObject
    /// Unity Editorから各タイルの画像を設定可能
    /// </summary>
    [CreateAssetMenu(fileName = "TileIconConfig", menuName = "OneStrokeRGR/Tile Icon Config")]
    public class TileIconConfig : ScriptableObject
    {
        [Header("タイルアイコン設定")]
        [Tooltip("効果なしマスのアイコン")]
        public Sprite emptyIcon;

        [Tooltip("攻撃力上昇マスのアイコン")]
        public Sprite attackBoostIcon;

        [Tooltip("HP回復マスのアイコン")]
        public Sprite hpRecoveryIcon;

        [Tooltip("ゴールドマスのアイコン")]
        public Sprite goldIcon;

        [Tooltip("敵マスのアイコン")]
        public Sprite enemyIcon;

        [Tooltip("ボス敵マスのアイコン")]
        public Sprite bossIcon;

        [Tooltip("とげマスのアイコン")]
        public Sprite thornIcon;

        [Tooltip("壁マスのアイコン")]
        public Sprite wallIcon;

        /// <summary>
        /// タイルタイプに対応するSpriteを取得
        /// </summary>
        public Sprite GetIcon(Model.TileType tileType, bool isBoss = false)
        {
            switch (tileType)
            {
                case Model.TileType.Empty:
                    return emptyIcon;
                case Model.TileType.AttackBoost:
                    return attackBoostIcon;
                case Model.TileType.HPRecovery:
                    return hpRecoveryIcon;
                case Model.TileType.Gold:
                    return goldIcon;
                case Model.TileType.Enemy:
                    return isBoss ? bossIcon : enemyIcon;
                case Model.TileType.Thorn:
                    return thornIcon;
                case Model.TileType.Wall:
                    return wallIcon;
                default:
                    return emptyIcon;
            }
        }
    }
}
