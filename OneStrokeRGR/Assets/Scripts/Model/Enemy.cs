using System.Collections.Generic;
using UnityEngine;
using OneStrokeRGR.Config;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// 敵の状態を管理するクラス
    /// 要件: 5.1, 5.2, 5.5, 6.1, 6.2, 13.1, 13.4
    /// </summary>
    public class Enemy
    {
        private int maxHP;
        private int currentHP;
        private int attackPower;
        private Vector2Int position;
        private bool isBoss;
        private List<EnemyActionEntry> actionPattern;
        private int currentActionIndex = 0;
        private int turnsSinceLastAction = 0;

        /// <summary>最大HP</summary>
        public int MaxHP => maxHP;

        /// <summary>現在のHP</summary>
        public int CurrentHP => currentHP;

        /// <summary>攻撃力（1-4の範囲、要件: 5.5, 13.4）</summary>
        public int AttackPower => attackPower;

        /// <summary>敵の位置</summary>
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>ボスかどうか（要件: 6.1、真ん中に配置用）</summary>
        public bool IsBoss => isBoss;

        /// <summary>行動パターンを持っているか</summary>
        public bool HasActionPattern => actionPattern != null && actionPattern.Count > 0;

        /// <summary>現在の行動インデックス</summary>
        public int CurrentActionIndex => currentActionIndex;

        /// <summary>前回の行動からのターン数</summary>
        public int TurnsSinceLastAction
        {
            get => turnsSinceLastAction;
            set => turnsSinceLastAction = value;
        }

        /// <summary>現在の行動の発動までの残りターン数</summary>
        public int TurnsUntilAction
        {
            get
            {
                if (!HasActionPattern) return 0;
                var current = actionPattern[currentActionIndex];
                int remaining = current.turnCount - turnsSinceLastAction;
                return remaining > 0 ? remaining : 0;
            }
        }

        /// <summary>
        /// Enemyのコンストラクタ
        /// 要件: 13.1, 13.4, 13.5
        /// </summary>
        /// <param name="hp">HP</param>
        /// <param name="attack">攻撃力（1-4に制限される）</param>
        /// <param name="boss">ボスフラグ（真ん中配置用）</param>
        /// <param name="actions">行動パターンリスト</param>
        public Enemy(int hp, int attack, bool boss = false, List<EnemyActionEntry> actions = null)
        {
            maxHP = hp;
            currentHP = hp;

            // 攻撃力を1-4に制限（要件: 5.5, 13.4）
            attackPower = Mathf.Clamp(attack, 1, 4);
            if (attack != attackPower)
            {
                Debug.LogWarning($"Enemy: 攻撃力が範囲外でした。{attack} -> {attackPower}に調整されました");
            }

            isBoss = boss;
            actionPattern = actions;
            currentActionIndex = 0;
            turnsSinceLastAction = 0;
            position = Vector2Int.zero;

            string typeLabel = boss ? "ボス" : "通常敵";
            string actionInfo = HasActionPattern ? $", 行動パターン={actionPattern.Count}個" : "";
            Debug.Log($"Enemy生成: {typeLabel} HP={maxHP}, 攻撃={attackPower}{actionInfo}");
        }

        /// <summary>
        /// ダメージを受ける
        /// 要件: 5.1, 5.2
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning("Enemy.TakeDamage: ダメージは正の値である必要があります");
                return;
            }

            int previousHP = currentHP;
            currentHP -= damage;

            // HPが負の値にならないようにする
            if (currentHP < 0)
            {
                currentHP = 0;
            }

            Debug.Log($"Enemy: {damage}ダメージを受けた。HP: {currentHP}/{maxHP} (前回: {previousHP})");
        }

        /// <summary>
        /// HPを回復する
        /// 要件: 6.3
        /// </summary>
        /// <param name="amount">回復量</param>
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Enemy.Heal: 回復量は正の値である必要があります");
                return;
            }

            currentHP += amount;

            // 最大HPを超えないようにする
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }

            Debug.Log($"Enemy: {amount}回復した。HP: {currentHP}/{maxHP}");
        }

        /// <summary>
        /// 敵が生存しているか判定
        /// 要件: 5.2
        /// </summary>
        /// <returns>生存している場合true</returns>
        public bool IsAlive()
        {
            return currentHP > 0;
        }

        /// <summary>
        /// 敵が行動を実行すべきか判定
        /// 行動パターンがない場合はfalseを返す
        /// </summary>
        /// <returns>行動を実行すべき場合true</returns>
        public bool ShouldPerformAction()
        {
            if (!HasActionPattern) return false;

            var currentAction = actionPattern[currentActionIndex];
            return turnsSinceLastAction >= currentAction.turnCount;
        }

        /// <summary>
        /// 現在の行動エントリを取得
        /// </summary>
        /// <returns>現在の行動エントリ、パターンがない場合はnull</returns>
        public EnemyActionEntry GetCurrentAction()
        {
            if (!HasActionPattern) return null;
            return actionPattern[currentActionIndex];
        }

        /// <summary>
        /// 次の行動に進める（サイクル：最後まで行ったら最初に戻る）
        /// </summary>
        public void AdvanceToNextAction()
        {
            if (!HasActionPattern) return;

            turnsSinceLastAction = 0;
            currentActionIndex = (currentActionIndex + 1) % actionPattern.Count;

            Debug.Log($"Enemy: 次の行動へ（{currentActionIndex + 1}/{actionPattern.Count}）");
        }
    }
}
