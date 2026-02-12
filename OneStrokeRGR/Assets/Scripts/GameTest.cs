using UnityEngine;
using OneStrokeRGR.Model;
using OneStrokeRGR.Config;

/// <summary>
/// ゲームロジックの動作確認用テストスクリプト
/// </summary>
public class GameTest : MonoBehaviour
{
    [Header("設定")]
    public GameConfig gameConfig;

    [Header("テストオプション")]
    public bool testInitialization = true;
    public bool testTileGeneration = true;
    public bool testEnemyGeneration = true;
    public bool testPlayerOperations = true;
    public bool testRewardSystem = true;

    void Start()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameTest: GameConfig が設定されていません！");
            return;
        }

        Debug.Log("========================================");
        Debug.Log("   ゲームロジックテスト開始");
        Debug.Log("========================================\n");

        if (testInitialization)
        {
            TestInitialization();
        }

        if (testTileGeneration)
        {
            TestTileGeneration();
        }

        if (testEnemyGeneration)
        {
            TestEnemyGeneration();
        }

        if (testPlayerOperations)
        {
            TestPlayerOperations();
        }

        if (testRewardSystem)
        {
            TestRewardSystem();
        }

        Debug.Log("\n========================================");
        Debug.Log("   テスト完了");
        Debug.Log("========================================");
    }

    /// <summary>
    /// 初期化テスト
    /// </summary>
    void TestInitialization()
    {
        Debug.Log("=== 1. GameState 初期化テスト ===");

        GameState gameState = new GameState();
        gameState.Initialize(gameConfig);

        Debug.Log($"✓ ステージ: {gameState.CurrentStage}");
        Debug.Log($"✓ プレイヤーHP: {gameState.Player.CurrentHP}/{gameState.Player.MaxHP}");
        Debug.Log($"✓ ゴールド: {gameState.Player.Gold}");
        Debug.Log($"✓ 一筆書きボーナス: {gameState.Player.OneStrokeBonus}");
        Debug.Log($"✓ ボスステージ?: {gameState.IsBossStage()}");
        Debug.Log($"✓ フェーズ: {gameState.CurrentPhase}\n");
    }

    /// <summary>
    /// タイル生成テスト
    /// </summary>
    void TestTileGeneration()
    {
        Debug.Log("=== 2. タイル生成テスト ===");

        GameState gameState = new GameState();
        gameState.Initialize(gameConfig);

        // 各タイプのカウント
        int emptyCount = 0;
        int attackCount = 0;
        int hpCount = 0;
        int goldCount = 0;

        int testCount = 100;
        for (int i = 0; i < testCount; i++)
        {
            Tile tile = TileFactory.CreateRandomTile(gameState.SpawnConfig);

            switch (tile.Type)
            {
                case TileType.Empty: emptyCount++; break;
                case TileType.AttackBoost: attackCount++; break;
                case TileType.HPRecovery: hpCount++; break;
                case TileType.Gold: goldCount++; break;
            }
        }

        Debug.Log($"✓ {testCount}個のタイルを生成:");
        Debug.Log($"  - 効果なし: {emptyCount}個 ({emptyCount * 100f / testCount:F1}%)");
        Debug.Log($"  - 攻撃力上昇: {attackCount}個 ({attackCount * 100f / testCount:F1}%)");
        Debug.Log($"  - HP回復: {hpCount}個 ({hpCount * 100f / testCount:F1}%)");
        Debug.Log($"  - ゴールド: {goldCount}個 ({goldCount * 100f / testCount:F1}%)\n");
    }

    /// <summary>
    /// 敵生成テスト
    /// </summary>
    void TestEnemyGeneration()
    {
        Debug.Log("=== 3. 敵生成テスト ===");

        // ステージ1の敵
        var stage1Data = gameConfig.enemySpawnTable.GetEntryForStage(1);
        if (stage1Data != null && stage1Data.enemies.Count > 0)
        {
            var data = stage1Data.enemies[0];
            Enemy enemy1 = new Enemy(data.maxHP, data.attackPower, data.isBoss);
            Debug.Log($"✓ ステージ1の敵: HP={enemy1.MaxHP}, 攻撃={enemy1.AttackPower}, ボス={enemy1.IsBoss}");
        }

        // ステージ10の敵
        var stage10Data = gameConfig.enemySpawnTable.GetEntryForStage(10);
        if (stage10Data != null && stage10Data.enemies.Count > 0)
        {
            foreach (var data in stage10Data.enemies)
            {
                Enemy enemy = new Enemy(data.maxHP, data.attackPower, data.isBoss);
                Debug.Log($"✓ ステージ10の敵: HP={enemy.MaxHP}, 攻撃={enemy.AttackPower}, ボス={enemy.IsBoss}");
            }
            Debug.Log("");
        }
    }

    /// <summary>
    /// プレイヤー操作テスト
    /// </summary>
    void TestPlayerOperations()
    {
        Debug.Log("=== 4. プレイヤー操作テスト ===");

        Player player = new Player();
        player.Initialize(50, 5);

        // HP操作
        Debug.Log($"初期HP: {player.CurrentHP}/{player.MaxHP}");
        player.TakeDamage(2);
        Debug.Log($"✓ ダメージ後: {player.CurrentHP}/{player.MaxHP}");
        player.Heal(1);
        Debug.Log($"✓ 回復後: {player.CurrentHP}/{player.MaxHP}");

        // ゴールド操作
        Debug.Log($"\n初期ゴールド: {player.Gold}");
        player.SpendGold(10);
        Debug.Log($"✓ 消費後: {player.Gold}");
        player.AddGold(5);
        Debug.Log($"✓ 獲得後: {player.Gold}");

        // 攻撃力操作
        Debug.Log($"\n初期攻撃力: {player.AttackPower}");
        player.IncreaseAttackPower(5);
        Debug.Log($"✓ 増加後: {player.AttackPower}");
        player.ResetAttackPower();
        Debug.Log($"✓ リセット後: {player.AttackPower}\n");
    }

    /// <summary>
    /// 報酬システムテスト
    /// </summary>
    void TestRewardSystem()
    {
        Debug.Log("=== 5. 報酬システムテスト ===");

        RewardSystem rewardSystem = new RewardSystem();
        GameState gameState = new GameState();
        gameState.Initialize(gameConfig);

        // ランダム報酬選択
        var rewards = rewardSystem.SelectRandomRewards(3);
        Debug.Log("✓ ランダム報酬3つを選択:");
        foreach (var reward in rewards)
        {
            Debug.Log($"  - {reward}: {rewardSystem.GetRewardDescription(reward)}");
        }

        // 報酬適用テスト
        Debug.Log($"\n報酬適用前の一筆書きボーナス: {gameState.Player.OneStrokeBonus}");
        rewardSystem.ApplyReward(RewardType.OneStrokeBonusIncrease, gameState);
        Debug.Log($"✓ 報酬適用後: {gameState.Player.OneStrokeBonus}\n");
    }
}
