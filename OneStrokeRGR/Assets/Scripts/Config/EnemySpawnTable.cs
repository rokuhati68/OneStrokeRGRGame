using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OneStrokeRGR.Config
{
    /// <summary>
    /// 敵の生成テーブルを管理するクラス
    /// </summary>
    [Serializable]
    public class EnemySpawnTable
    {
        [Tooltip("ステージごとの敵生成データリスト")]
        public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();

        /// <summary>
        /// 指定されたステージに対応する敵生成データを取得
        /// ステージ番号に完全一致するエントリーがない場合は、
        /// ステージ番号以下の最大のエントリーを返す
        /// </summary>
        public EnemySpawnEntry GetEntryForStage(int stage)
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogError("EnemySpawnTable: エントリーが設定されていません");
                return null;
            }

            // 完全一致するエントリーを探す
            var exactMatch = entries.FirstOrDefault(e => e.stageNumber == stage);
            if (exactMatch != null)
            {
                return exactMatch;
            }

            // 一致しない場合は、ステージ番号以下の最大のエントリーを取得
            var fallback = entries
                .Where(e => e.stageNumber <= stage)
                .OrderByDescending(e => e.stageNumber)
                .FirstOrDefault();

            if (fallback != null)
            {
                return fallback;
            }

            // それでもない場合は最初のエントリーを返す
            Debug.LogWarning($"EnemySpawnTable: ステージ{stage}に適したエントリーが見つかりません。最初のエントリーを使用します。");
            return entries[0];
        }
    }
}
