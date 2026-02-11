using UnityEngine;

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
        private int bossActionInterval = 3;
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

        /// <summary>ボスかどうか（要件: 6.1）</summary>
        public bool IsBoss => isBoss;

        /// <summary>ボスの行動間隔（要件: 6.2）</summary>
        public int BossActionInterval => bossActionInterval;

        /// <summary>前回の行動からのターン数</summary>
        public int TurnsSinceLastAction
        {
            get => turnsSinceLastAction;
            set => turnsSinceLastAction = value;
        }

        /// <summary>
        /// Enemyのコンストラクタ
        /// 要件: 13.1, 13.4, 13.5
        /// </summary>
        /// <param name="hp">HP</param>
        /// <param name="attack">攻撃力（1-4に制限される）</param>
        /// <param name="boss">ボスフラグ</param>
        /// <param name="actionInterval">ボスの行動間隔（デフォルト: 3）</param>
        public Enemy(int hp, int attack, bool boss = false, int actionInterval = 3)
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
            bossActionInterval = actionInterval;
            turnsSinceLastAction = 0;
            position = Vector2Int.zero;

            Debug.Log($"Enemy生成: HP={maxHP}, 攻撃={attackPower}, ボス={isBoss}");
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
        /// HPを回復する（ボスの自己回復用）
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
        /// ボスが特殊行動を実行すべきか判定
        /// 要件: 6.2
        /// </summary>
        /// <returns>特殊行動を実行すべき場合true</returns>
        public bool ShouldPerformBossAction()
        {
            if (!isBoss)
            {
                return false;
            }

            return turnsSinceLastAction >= bossActionInterval;
        }

        /// <summary>
        /// ボスの行動カウンターをリセット
        /// </summary>
        public void ResetBossActionCounter()
        {
            turnsSinceLastAction = 0;
            Debug.Log("Enemy: ボス行動カウンターをリセット");
        }
    }
}
