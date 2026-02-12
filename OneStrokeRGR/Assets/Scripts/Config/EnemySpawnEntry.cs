using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 個別の敵データ設定
    /// </summary>
    [Serializable]
    public class EnemyData
    {
        [Tooltip("ボスかどうか")]
        public bool isBoss;

        [Tooltip("最大HP")]
        public int maxHP = 3;

        [Range(1, 4)]
        [Tooltip("攻撃力（1-4）")]
        public int attackPower = 1;

        [Tooltip("バトルUIに表示するスプライト")]
        public Sprite sprite;
    }

    /// <summary>
    /// ステージごとの敵生成データ
    /// 要件: 13.1, 13.2, 13.3, 13.4, 13.5
    /// </summary>
    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("ステージ番号")]
        public int stageNumber;

        [Tooltip("敵データリスト（1～3体）")]
        public List<EnemyData> enemies = new List<EnemyData>();
    }
}
