using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using OneStrokeRGR.Model;

namespace OneStrokeRGR.View
{
    /// <summary>
    /// バトルUIパネルの管理View
    /// 画面右側に敵の全体像、HPバー、攻撃ターンを表示
    /// ドラクエスタイルで最大3体を横並びに表示
    ///
    /// スロット配置:
    ///   [0]=左  [1]=中央  [2]=右
    ///
    /// 配置ルール:
    ///   1体 → 中央
    ///   2体（ボスなし） → 左 + 右
    ///   2体（ボスあり） → ボス=中央, 他=左
    ///   3体 → ボス=中央（いなければ順に左→中央→右）
    /// </summary>
    public class BattleUIView : MonoBehaviour
    {
        [Header("パネル設定")]
        public GameObject battlePanel;

        [Header("敵ステータス表示（左・中央・右の3スロット）")]
        public EnemyStatusView[] enemyStatusViews = new EnemyStatusView[3];

        private List<Enemy> trackedEnemies = new List<Enemy>();

        private void Awake()
        {
            foreach (var statusView in enemyStatusViews)
            {
                if (statusView != null)
                {
                    statusView.Clear();
                }
            }
        }

        /// <summary>
        /// バトルUIを初期化（ステージ開始時に呼ばれる）
        /// 敵リストとそれに対応するスプライトリストを受け取る
        /// </summary>
        public void InitializeBattleUI(List<Enemy> enemies, List<Sprite> sprites)
        {
            trackedEnemies.Clear();

            // 全スロットをクリア
            foreach (var statusView in enemyStatusViews)
            {
                if (statusView != null)
                {
                    statusView.Clear();
                }
            }

            int count = Mathf.Min(enemies.Count, 3);
            if (count == 0) return;

            // スロット割り当てを計算
            int[] slotAssignment = CalculateSlotAssignment(enemies, count);

            // 割り当てに従って配置
            for (int i = 0; i < count; i++)
            {
                int slot = slotAssignment[i];
                if (slot < 0 || slot >= enemyStatusViews.Length) continue;
                if (enemyStatusViews[slot] == null) continue;

                Sprite sprite = (i < sprites.Count) ? sprites[i] : null;
                enemyStatusViews[slot].SetEnemy(enemies[i], sprite);
                trackedEnemies.Add(enemies[i]);
            }

            if (battlePanel != null)
            {
                battlePanel.SetActive(true);
            }

            Debug.Log($"BattleUIView: {count}体の敵を表示");
        }

        /// <summary>
        /// 敵数とボスの有無からスロット割り当てを計算
        /// 戻り値: enemies[i] → スロット番号の配列
        /// </summary>
        private int[] CalculateSlotAssignment(List<Enemy> enemies, int count)
        {
            int[] slots = new int[count];
            int bossIndex = -1;

            // ボスのインデックスを検索
            for (int i = 0; i < count; i++)
            {
                if (enemies[i].IsBoss)
                {
                    bossIndex = i;
                    break;
                }
            }

            if (count == 1)
            {
                // 1体 → 中央
                slots[0] = 1;
            }
            else if (count == 2)
            {
                if (bossIndex >= 0)
                {
                    // 2体（ボスあり） → ボス=中央, 他=左
                    int otherIndex = (bossIndex == 0) ? 1 : 0;
                    slots[bossIndex] = 1;   // ボス → 中央
                    slots[otherIndex] = 0;  // 他 → 左
                }
                else
                {
                    // 2体（ボスなし） → 左 + 右
                    slots[0] = 0;
                    slots[1] = 2;
                }
            }
            else // count == 3
            {
                if (bossIndex >= 0)
                {
                    // 3体（ボスあり） → ボス=中央, 他を左と右に配置
                    slots[bossIndex] = 1; // ボス → 中央
                    int sideSlot = 0;
                    for (int i = 0; i < count; i++)
                    {
                        if (i == bossIndex) continue;
                        slots[i] = sideSlot; // 左(0) → 右(2)
                        sideSlot = 2;
                    }
                }
                else
                {
                    // 3体（ボスなし） → 左・中央・右
                    slots[0] = 0;
                    slots[1] = 1;
                    slots[2] = 2;
                }
            }

            return slots;
        }

        /// <summary>
        /// 全敵の表示を更新
        /// </summary>
        public void UpdateAllEnemyDisplays()
        {
            for (int i = 0; i < enemyStatusViews.Length; i++)
            {
                if (enemyStatusViews[i] != null && enemyStatusViews[i].gameObject.activeSelf)
                {
                    Enemy enemy = enemyStatusViews[i].GetEnemy();
                    if (enemy != null && enemy.IsAlive())
                    {
                        enemyStatusViews[i].UpdateDisplay();
                    }
                }
            }
        }

        /// <summary>
        /// 特定の敵のHP変化をアニメーション表示
        /// </summary>
        public void AnimateEnemyDamage(Enemy enemy)
        {
            if (enemy == null) return;

            for (int i = 0; i < enemyStatusViews.Length; i++)
            {
                if (enemyStatusViews[i] != null && enemyStatusViews[i].GetEnemy() == enemy)
                {
                    enemyStatusViews[i].AnimateHPChange();
                    return;
                }
            }
        }

        /// <summary>
        /// 敵撃破時の演出
        /// </summary>
        public void PlayEnemyDefeatAnimation(Enemy enemy)
        {
            if (enemy == null) return;

            for (int i = 0; i < enemyStatusViews.Length; i++)
            {
                if (enemyStatusViews[i] != null && enemyStatusViews[i].GetEnemy() == enemy)
                {
                    enemyStatusViews[i].PlayDefeatAnimation();
                    trackedEnemies.Remove(enemy);
                    Debug.Log($"BattleUIView: 敵撃破演出（残り{trackedEnemies.Count}体）");
                    return;
                }
            }
        }

        /// <summary>
        /// バトルUIを非表示にする
        /// </summary>
        public void HideBattleUI()
        {
            if (battlePanel != null)
            {
                battlePanel.SetActive(false);
            }

            trackedEnemies.Clear();
        }

        /// <summary>
        /// パネルの表示/非表示を切り替え
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (battlePanel != null)
            {
                battlePanel.SetActive(visible);
            }
        }
    }
}
