using System;
using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// ステージごとの敵生成データ
    /// 要件: 13.1, 13.2, 13.3, 13.4, 13.5
    /// </summary>
    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("ステージ番号")]
        public int stageNumber;

        [Tooltip("通常敵の数")]
        public int enemyCount;

        [Tooltip("通常敵のHP")]
        public int enemyHP;

        [Range(1, 4)]
        [Tooltip("通常敵の攻撃力（1-4）")]
        public int enemyAttack;

        [Tooltip("ボスのHP")]
        public int bossHP;

        [Range(1, 4)]
        [Tooltip("ボスの攻撃力（1-4）")]
        public int bossAttack;
    }
}
