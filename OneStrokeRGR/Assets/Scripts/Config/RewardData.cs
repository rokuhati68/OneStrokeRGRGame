using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 報酬データを定義するScriptableObject
    /// Inspector上でName, Icon, Descriptionを設定可能
    /// </summary>
    [CreateAssetMenu(fileName = "RewardData", menuName = "OneStrokeRGR/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("報酬の基本情報")]
        [Tooltip("報酬の表示名")]
        public string rewardName;

        [Tooltip("報酬のアイコン")]
        public Sprite icon;

        [TextArea(2, 4)]
        [Tooltip("報酬の説明文")]
        public string description;

        [Header("報酬の効果")]
        [Tooltip("報酬の種類（適用する効果を決定）")]
        public RewardType rewardType;
    }
}
