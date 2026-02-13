using System;
using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 報酬の各レベルのデータ
    /// </summary>
    [Serializable]
    public class RewardLevelData
    {
        [Header("基本情報")]
        [Tooltip("報酬の表示名")]
        public string rewardName;

        [Tooltip("報酬のアイコン")]
        public Sprite icon;

        [TextArea(2, 4)]
        [Tooltip("報酬の説明文")]
        public string description;

        [Header("出現設定１固定")]
        [Tooltip("選択肢に出現する確率の重み")]
        public float appearanceWeight = 1f;

        [Header("効果設定")]
        [Tooltip("出現率の増加量")]
        public float spawnRateChange;

        [Tooltip("効果値の変化量")]
        public int valueChange;
    }

    /// <summary>
    /// 報酬データを定義するScriptableObject
    /// 各報酬はレベル1~10を持ち、レベルごとに名前・アイコン・説明・効果を設定可能
    /// </summary>
    [CreateAssetMenu(fileName = "RewardData", menuName = "OneStrokeRGR/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("報酬の種類")]
        [Tooltip("報酬の種類（適用する効果を決定）")]
        public RewardType rewardType;

        [Header("レベルデータ（1~10）")]
        public List<RewardLevelData> levels = new List<RewardLevelData>();

        public int MaxLevel => levels.Count;

        /// <summary>
        /// 指定レベルのデータを取得（0ベースインデックス）
        /// </summary>
        public RewardLevelData GetLevel(int level)
        {
            if (level < 0 || level >= levels.Count) return null;
            return levels[level];
        }
    }
}
