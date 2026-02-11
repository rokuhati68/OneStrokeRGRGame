using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// プレイヤーの状態を管理するクラス
    /// 要件: 11.1, 11.2, 11.3, 11.5, 11.6
    /// </summary>
    public class Player
    {
        private int maxHP = 5;
        private int currentHP;
        private int gold;
        private int attackPower;
        private Vector2Int position;
        private int oneStrokeBonus;

        /// <summary>最大HP（要件: 11.1）</summary>
        public int MaxHP => maxHP;

        /// <summary>現在のHP</summary>
        public int CurrentHP => currentHP;

        /// <summary>所持ゴールド</summary>
        public int Gold => gold;

        /// <summary>攻撃力</summary>
        public int AttackPower => attackPower;

        /// <summary>プレイヤーの位置</summary>
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>一筆書きボーナス値（要件: 11.6）</summary>
        public int OneStrokeBonus => oneStrokeBonus;

        /// <summary>
        /// プレイヤーの初期化
        /// 要件: 11.1, 11.2, 11.6
        /// </summary>
        /// <param name="startingGold">開始時のゴールド（デフォルト: 50）</param>
        /// <param name="bonusValue">一筆書きボーナス値（デフォルト: 5）</param>
        public void Initialize(int startingGold = 50, int bonusValue = 5)
        {
            currentHP = maxHP;
            gold = startingGold;
            attackPower = 0;
            oneStrokeBonus = bonusValue;
            position = Vector2Int.zero;
        }

        /// <summary>
        /// ダメージを受ける
        /// 要件: 5.3
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning("Player.TakeDamage: ダメージは正の値である必要があります");
                return;
            }

            currentHP -= damage;

            // HPが負の値にならないようにする
            if (currentHP < 0)
            {
                currentHP = 0;
            }

            Debug.Log($"Player: {damage}ダメージを受けた。HP: {currentHP}/{maxHP}");
        }

        /// <summary>
        /// HPを回復する
        /// 要件: 3.3, 11.5
        /// </summary>
        /// <param name="amount">回復量</param>
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Player.Heal: 回復量は正の値である必要があります");
                return;
            }

            currentHP += amount;

            // 最大HPを超えないようにする（要件: 11.5）
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }

            Debug.Log($"Player: {amount}回復した。HP: {currentHP}/{maxHP}");
        }

        /// <summary>
        /// ゴールドを追加する
        /// 要件: 3.4
        /// </summary>
        /// <param name="amount">追加するゴールド量</param>
        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Player.AddGold: ゴールド量は正の値である必要があります");
                return;
            }

            gold += amount;
            Debug.Log($"Player: {amount}ゴールド獲得。所持金: {gold}");
        }

        /// <summary>
        /// ゴールドを消費する
        /// 要件: 3.2, 3.3, 3.5, 11.3
        /// </summary>
        /// <param name="amount">消費するゴールド量</param>
        /// <returns>消費に成功したかどうか</returns>
        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Player.SpendGold: ゴールド量は正の値である必要があります");
                return false;
            }

            if (gold < amount)
            {
                Debug.Log($"Player: ゴールド不足。必要: {amount}, 所持: {gold}");
                return false;
            }

            gold -= amount;
            Debug.Log($"Player: {amount}ゴールド消費。残り: {gold}");
            return true;
        }

        /// <summary>
        /// 攻撃力を増加させる
        /// 要件: 2.1, 3.2
        /// </summary>
        /// <param name="amount">増加量</param>
        public void IncreaseAttackPower(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Player.IncreaseAttackPower: 増加量は正の値である必要があります");
                return;
            }

            attackPower += amount;
            Debug.Log($"Player: 攻撃力+{amount}。現在の攻撃力: {attackPower}");
        }

        /// <summary>
        /// 攻撃力をリセットする（ターン開始時）
        /// 要件: 2.3
        /// </summary>
        public void ResetAttackPower()
        {
            attackPower = 0;
            Debug.Log("Player: 攻撃力をリセット");
        }

        /// <summary>
        /// 一筆書きボーナスを増加させる
        /// 要件: 9.2
        /// </summary>
        /// <param name="amount">増加量</param>
        public void IncreaseOneStrokeBonus(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Player.IncreaseOneStrokeBonus: 増加量は正の値である必要があります");
                return;
            }

            oneStrokeBonus += amount;
            Debug.Log($"Player: 一筆書きボーナス+{amount}。現在: {oneStrokeBonus}");
        }

        /// <summary>
        /// プレイヤーが生存しているか判定
        /// 要件: 5.4
        /// </summary>
        /// <returns>生存している場合true</returns>
        public bool IsAlive()
        {
            return currentHP > 0;
        }
    }
}
