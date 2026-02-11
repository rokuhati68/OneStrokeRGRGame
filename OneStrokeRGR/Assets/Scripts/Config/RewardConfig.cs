using System;
using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 報酬システムの設定を管理するクラス
    /// </summary>
    [Serializable]
    public class RewardConfig
    {
        [Header("報酬効果の増加量")]
        [Tooltip("出現率の増加量（デフォルト: 5%）")]
        public float spawnRateIncrement = 0.05f;

        [Tooltip("値の増加量（デフォルト: 1）")]
        public int valueIncrement = 1;

        [Tooltip("一筆書きボーナスの増加量（デフォルト: 2）")]
        public int oneStrokeBonusIncrement = 2;
    }
}
