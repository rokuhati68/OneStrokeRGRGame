using System;
using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// BossActionType ごとのアイコン Sprite 設定を管理する ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "BossActionIconConfig", menuName = "OneStrokeRGR/Boss Action Icon Config")]
    public class BossActionIconConfig : ScriptableObject
    {
        [Serializable]
        public class ActionIconEntry
        {
            public BossActionType actionType;
            public Sprite icon;
        }

        [Header("行動タイプ別アイコン設定")]
        public List<ActionIconEntry> entries = new List<ActionIconEntry>();

        /// <summary>
        /// 指定した BossActionType に対応する Sprite を返す。
        /// 見つからない場合は null を返す。
        /// </summary>
        public Sprite GetIcon(BossActionType actionType)
        {
            foreach (var entry in entries)
            {
                if (entry.actionType == actionType)
                    return entry.icon;
            }
            return null;
        }
    }
}
