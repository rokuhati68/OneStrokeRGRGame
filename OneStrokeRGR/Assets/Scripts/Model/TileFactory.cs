using UnityEngine;

namespace OneStrokeRGR.Model
{
    /// <summary>
    /// タイル生成を担当するファクトリークラス
    /// 要件: 7.4, 10.4, 12.3, 12.4
    /// </summary>
    public static class TileFactory
    {
        /// <summary>
        /// ランダムなタイルを生成
        /// 要件: 7.4, 10.4
        /// </summary>
        /// <param name="config">タイル生成設定</param>
        /// <returns>生成されたタイル</returns>
        public static Tile CreateRandomTile(Config.TileSpawnConfig config)
        {
            if (config == null)
            {
                Debug.LogError("TileFactory: configがnullです");
                return new EmptyTile();
            }

            // 重み付きランダム選択
            float random = Random.value;
            float cumulative = 0f;

            // 効果なしマス
            cumulative += config.emptyRate;
            if (random < cumulative)
            {
                return new EmptyTile();
            }

            // 攻撃力上昇マス
            cumulative += config.attackBoostRate;
            if (random < cumulative)
            {
                int value = Random.Range(config.attackBoostRange.x, config.attackBoostRange.y + 1);
                return new AttackBoostTile(value);
            }

            // HP回復マス
            cumulative += config.hpRecoveryRate;
            if (random < cumulative)
            {
                return new HPRecoveryTile();
            }

            // ゴールドマス
            cumulative += config.goldRate;
            if (random < cumulative)
            {
                int value = Random.Range(config.goldRange.x, config.goldRange.y + 1);
                return new GoldTile(value);
            }

            // フォールバック（出現率の合計が1.0でない場合）
            Debug.LogWarning($"TileFactory: 出現率の合計が1.0ではありません（合計: {cumulative}）。EmptyTileを返します");
            return new EmptyTile();
        }

        /// <summary>
        /// 効果なしマスを生成
        /// </summary>
        /// <returns>生成された効果なしマス</returns>
        public static EmptyTile CreateEmptyTile()
        {
            return new EmptyTile();
        }

        /// <summary>
        /// 敵マスを生成
        /// 要件: 10.3, 13.1, 13.2, 13.3, 13.4, 13.5
        /// </summary>
        /// <param name="enemy">配置する敵</param>
        /// <returns>生成された敵マス</returns>
        public static EnemyTile CreateEnemyTile(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("TileFactory: enemyがnullです");
                return null;
            }

            return new EnemyTile(enemy);
        }

        /// <summary>
        /// とげマスを生成（ボス用）
        /// 要件: 6.4
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        /// <returns>生成されたとげマス</returns>
        public static ThornTile CreateThornTile(int damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning($"TileFactory: ダメージは正の値である必要があります。0に設定します");
                damage = 0;
            }

            return new ThornTile(damage);
        }

        /// <summary>
        /// 壁マスを生成（ボス用）
        /// 要件: 6.5
        /// </summary>
        /// <returns>生成された壁マス</returns>
        public static WallTile CreateWallTile()
        {
            return new WallTile();
        }

        /// <summary>
        /// 特定の種類のタイルを生成
        /// </summary>
        /// <param name="type">タイルの種類</param>
        /// <param name="config">タイル生成設定（値範囲の取得に使用）</param>
        /// <returns>生成されたタイル</returns>
        public static Tile CreateTileByType(TileType type, Config.TileSpawnConfig config)
        {
            switch (type)
            {
                case TileType.Empty:
                    return new EmptyTile();

                case TileType.AttackBoost:
                    int attackValue = config != null
                        ? Random.Range(config.attackBoostRange.x, config.attackBoostRange.y + 1)
                        : 1;
                    return new AttackBoostTile(attackValue);

                case TileType.HPRecovery:
                    return new HPRecoveryTile();

                case TileType.Gold:
                    int goldValue = config != null
                        ? Random.Range(config.goldRange.x, config.goldRange.y + 1)
                        : 1;
                    return new GoldTile(goldValue);

                case TileType.Thorn:
                    return new ThornTile(1);

                case TileType.Wall:
                    return new WallTile();

                case TileType.Enemy:
                    Debug.LogWarning("TileFactory: EnemyタイプにはCreateEnemyTileを使用してください");
                    return new EmptyTile();

                default:
                    Debug.LogWarning($"TileFactory: 未対応のタイプ {type}");
                    return new EmptyTile();
            }
        }
    }
}
