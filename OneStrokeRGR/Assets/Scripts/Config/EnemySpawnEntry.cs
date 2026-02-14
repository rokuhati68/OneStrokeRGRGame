using System;
using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 敵の個別行動データ（行動タイプ、値、ターン数）
    /// </summary>
    [Serializable]
    public class EnemyActionEntry
    {
        [Tooltip("行動タイプ")]
        public BossActionType actionType;

        [Tooltip("行動の値（攻撃ダメージ、回復量、生成数など）")]
        public int value = 1;

        [Tooltip("この行動が発動するまでのターン数")]
        public int turnCount = 3;
    }

    /// <summary>
    /// 個別の敵データ設定
    /// </summary>
    [Serializable]
    public class EnemyData
    {
        [Tooltip("ボスかどうか（真ん中に配置）")]
        public bool isBoss;

        [Tooltip("最大HP")]
        public int maxHP = 3;

        [Range(1, 4)]
        [Tooltip("攻撃力（1-4）")]
        public int attackPower = 1;

        [Tooltip("バトルUIに表示するスプライト")]
        public Sprite sprite;

        [Tooltip("行動パターン（順番に実行し、最後まで行ったら最初に戻る）")]
        public List<EnemyActionEntry> actionPattern = new List<EnemyActionEntry>();
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
