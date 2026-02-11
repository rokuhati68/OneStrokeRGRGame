# 設計書：一筆書きローグライクゲーム

## 概要

本ドキュメントは、Unity、DOTween、UniTaskを使用した一筆書きパズルローグライクゲームの技術設計を定義します。プレイヤーはマウスドラッグで5×5グリッド上にパスを描き、マス効果を活用して敵を倒し、ステージを進行します。

### 技術スタック

- **ゲームエンジン**: Unity
- **アニメーションライブラリ**: DOTween
- **非同期処理**: UniTask
- **言語**: C#

### 設計原則

- MVPパターンによる関心の分離
- イベント駆動アーキテクチャによる疎結合
- UniTaskによる非同期フロー制御
- DOTweenによる滑らかなアニメーション

## アーキテクチャ

### 全体構造

システムはModel-View-Presenterパターンに従います：

```
┌─────────────────────────────────────────────┐
│                   View Layer                 │
│  (UI表示、入力検出、アニメーション)           │
│  - BoardView                                 │
│  - TileView                                  │
│  - PathDrawingView                           │
│  - UIView (HP, Gold, etc.)                   │
└─────────────────┬───────────────────────────┘
                  │ Events / Commands
┌─────────────────▼───────────────────────────┐
│              Presenter Layer                 │
│  (ゲームロジック制御、状態遷移)               │
│  - GamePresenter                             │
│  - PathPresenter                             │
│  - CombatPresenter                           │
│  - RewardPresenter                           │
└─────────────────┬───────────────────────────┘
                  │ Data Access
┌─────────────────▼───────────────────────────┐
│                Model Layer                   │
│  (ゲーム状態、ビジネスロジック)               │
│  - GameState                                 │
│  - Board                                     │
│  - Tile (各種タイプ)                         │
│  - Player                                    │
│  - Enemy                                     │
│  - RewardSystem                              │
└─────────────────────────────────────────────┘
```

### コアシステム

1. **入力システム**: マウスドラッグによる一筆書きパス検出
2. **ボード管理システム**: 5×5グリッドの状態管理とタイル生成
3. **戦闘システム**: 攻撃力計算、ダメージ処理、敵AI
4. **コンボシステム**: 連続同効果タイルのゴールド免除
5. **報酬システム**: ステージクリア後の永続的アップグレード
6. **ボスシステム**: 特殊行動を持つボス敵
7. **アニメーションシステム**: DOTweenによる視覚効果
8. **状態管理システム**: ゲーム全体の状態遷移

## コンポーネントとインターフェース

### Model層

#### GameState
ゲーム全体の状態を保持します。

```csharp
public class GameState
{
    public int CurrentStage { get; set; }
    public Player Player { get; set; }
    public Board Board { get; set; }
    public RewardConfig RewardConfig { get; set; }
    public TileSpawnConfig SpawnConfig { get; set; }
    public GamePhase CurrentPhase { get; set; }
    
    public bool IsBossStage() => CurrentStage % 10 == 0;
}

public enum GamePhase
{
    PathDrawing,
    PathConfirmation,
    PathExecution,
    BossAction,
    StageCleared,
    RewardSelection,
    GameOver
}
```

#### Player
プレイヤーの状態を管理します。

```csharp
public class Player
{
    public int MaxHP { get; private set; } = 5;
    public int CurrentHP { get; private set; }
    public int Gold { get; private set; }
    public int AttackPower { get; private set; }
    public Vector2Int Position { get; set; }
    public int OneStrokeBonus { get; private set; }
    
    public void Initialize(int startingGold = 50, int oneStrokeBonus = 5)
    public void TakeDamage(int damage)
    public void Heal(int amount)
    public void AddGold(int amount)
    public bool SpendGold(int amount)
    public void IncreaseAttackPower(int amount)
    public void ResetAttackPower()
    public bool IsAlive() => CurrentHP > 0;
}
```

#### Board
5×5グリッドとタイルを管理します。

```csharp
public class Board
{
    private Tile[,] tiles = new Tile[5, 5];
    private List<Enemy> enemies = new List<Enemy>();
    
    public Tile GetTile(Vector2Int position)
    public void SetTile(Vector2Int position, Tile tile)
    public List<Enemy> GetEnemies()
    public void RemoveEnemy(Enemy enemy)
    public void AddEnemy(Enemy enemy)
    public bool IsValidPosition(Vector2Int position)
    public void RegenerateTiles(List<Vector2Int> positions, TileSpawnConfig config)
}
```

#### Tile (抽象基底クラス)
各タイプのタイルの基底クラスです。

```csharp
public abstract class Tile
{
    public Vector2Int Position { get; set; }
    public TileType Type { get; protected set; }
    
    public abstract TileEffectResult ApplyEffect(Player player, GameContext context);
    public abstract bool CanApplyEffect(Player player);
}

public enum TileType
{
    Empty,
    AttackBoost,
    HPRecovery,
    Gold,
    Enemy,
    Thorn,  // ボスが生成
    Wall    // ボスが生成
}

public class TileEffectResult
{
    public bool EffectApplied { get; set; }
    public int GoldSpent { get; set; }
    public int AttackGained { get; set; }
    public int HPGained { get; set; }
    public int GoldGained { get; set; }
    public int DamageTaken { get; set; }
    public Enemy DefeatedEnemy { get; set; }
}
```

#### 具体的なタイルクラス

```csharp
public class EmptyTile : Tile
{
    public EmptyTile() { Type = TileType.Empty; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => true;
}

public class AttackBoostTile : Tile
{
    public int BoostValue { get; set; }
    public AttackBoostTile(int value) { Type = TileType.AttackBoost; BoostValue = value; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => player.Gold >= 1;
}

public class HPRecoveryTile : Tile
{
    public HPRecoveryTile() { Type = TileType.HPRecovery; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => player.Gold >= 1;
}

public class GoldTile : Tile
{
    public int GoldValue { get; set; }
    public GoldTile(int value) { Type = TileType.Gold; GoldValue = value; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => true;
}

public class EnemyTile : Tile
{
    public Enemy Enemy { get; set; }
    public EnemyTile(Enemy enemy) { Type = TileType.Enemy; Enemy = enemy; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => true;
}

public class ThornTile : Tile
{
    public int Damage { get; set; }
    public ThornTile(int damage) { Type = TileType.Thorn; Damage = damage; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context)
    public override bool CanApplyEffect(Player player) => true;
}

public class WallTile : Tile
{
    public WallTile() { Type = TileType.Wall; }
    public override TileEffectResult ApplyEffect(Player player, GameContext context) => null;
    public override bool CanApplyEffect(Player player) => false;
}
```

#### Enemy
敵の状態を管理します。

```csharp
public class Enemy
{
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int AttackPower { get; private set; }
    public Vector2Int Position { get; set; }
    public bool IsBoss { get; private set; }
    public int BossActionInterval { get; private set; } = 3;
    public int TurnsSinceLastAction { get; set; } = 0;
    
    public Enemy(int hp, int attack, bool isBoss = false)
    public void TakeDamage(int damage)
    public bool IsAlive() => CurrentHP > 0;
    public bool ShouldPerformBossAction()
}
```

#### TileSpawnConfig
タイル生成の確率設定を管理します。

```csharp
public class TileSpawnConfig
{
    public float EmptyRate { get; set; } = 0.7f;
    public float AttackBoostRate { get; set; } = 0.1f;
    public float HPRecoveryRate { get; set; } = 0.1f;
    public float GoldRate { get; set; } = 0.1f;
    
    public (int min, int max) AttackBoostRange { get; set; } = (1, 2);
    public (int min, int max) GoldRange { get; set; } = (1, 2);
    
    public void ApplyReward(RewardType rewardType)
}
```

#### RewardSystem
報酬の管理と適用を行います。

```csharp
public enum RewardType
{
    AttackBoostRateIncrease,
    AttackBoostValueIncrease,
    HPRecoveryRateIncrease,
    EmptyRateIncrease,
    GoldRateIncrease,
    GoldValueIncrease,
    OneStrokeBonusIncrease
}

public class RewardConfig
{
    public float SpawnRateIncrement { get; set; } = 0.05f;
    public int ValueIncrement { get; set; } = 1;
    public int OneStrokeBonusIncrement { get; set; } = 2;
}

public class RewardSystem
{
    public List<RewardType> SelectRandomRewards(int count = 3)
    public void ApplyReward(RewardType reward, GameState gameState)
}
```

### Presenter層

#### GamePresenter
ゲーム全体のフローを制御します。

```csharp
public class GamePresenter
{
    private GameState gameState;
    private PathPresenter pathPresenter;
    private CombatPresenter combatPresenter;
    private RewardPresenter rewardPresenter;
    private BoardView boardView;
    private UIView uiView;
    
    public async UniTask InitializeGame()
    public async UniTask StartNewStage()
    public async UniTask HandlePathDrawingPhase()
    public async UniTask HandlePathExecutionPhase(List<Vector2Int> path)
    public async UniTask HandleBossActionPhase()
    public async UniTask HandleStageClearedPhase()
    public async UniTask HandleGameOverPhase()
}
```

#### PathPresenter
パス描画と検証を制御します。

```csharp
public class PathPresenter
{
    private GameState gameState;
    private PathDrawingView pathView;
    
    public bool ValidatePath(List<Vector2Int> path, Vector2Int playerPosition)
    public bool IsPathStartingFromPlayer(List<Vector2Int> path, Vector2Int playerPosition)
    public bool IsPathEndingOnEnemy(List<Vector2Int> path, Board board)
    public bool IsPathContinuous(List<Vector2Int> path)
    public PathPreview CalculatePathPreview(List<Vector2Int> path, Player player, Board board)
}

public class PathPreview
{
    public int PredictedAttackPower { get; set; }
    public int PredictedGoldSpent { get; set; }
    public int PredictedGoldGained { get; set; }
    public int PredictedHPChange { get; set; }
    public List<TileType> TileSequence { get; set; }
}
```

#### CombatPresenter
戦闘ロジックを制御します。

```csharp
public class CombatPresenter
{
    private GameState gameState;
    
    public async UniTask ExecutePath(List<Vector2Int> path)
    public async UniTask ProcessTileEffect(Tile tile, ComboTracker comboTracker)
    public async UniTask ProcessCombat(Enemy enemy, int attackPower)
    public void ApplyOneStrokeBonus(int pathLength, Player player)
}

public class ComboTracker
{
    private TileType? lastTileType = null;
    private int comboCount = 0;
    
    public bool IsComboActive(TileType currentType)
    public void UpdateCombo(TileType currentType)
    public void Reset()
}
```

#### BossPresenter
ボスの特殊行動を制御します。

```csharp
public class BossPresenter
{
    private GameState gameState;
    
    public async UniTask ExecuteBossAction(Enemy boss)
    public BossActionType SelectRandomBossAction()
    public async UniTask DisableAttackBoostTiles()
    public async UniTask HealBoss(Enemy boss, int amount)
    public async UniTask SpawnThornTiles(int count)
    public async UniTask SpawnWallTiles(int count)
}

public enum BossActionType
{
    DisableAttackBoost,
    HealSelf,
    SpawnThorns,
    SpawnWalls
}
```

#### RewardPresenter
報酬選択を制御します。

```csharp
public class RewardPresenter
{
    private RewardSystem rewardSystem;
    private RewardView rewardView;
    
    public async UniTask<RewardType> PresentRewardSelection(GameState gameState)
    public void ApplySelectedReward(RewardType reward, GameState gameState)
}
```

### View層

#### BoardView
ゲームボードの視覚表現を管理します。

```csharp
public class BoardView : MonoBehaviour
{
    private TileView[,] tileViews = new TileView[5, 5];
    
    public void InitializeBoard(Board board)
    public void UpdateTileView(Vector2Int position, Tile tile)
    public async UniTask AnimateTileRemoval(Vector2Int position)
    public async UniTask AnimateTileSpawn(Vector2Int position, Tile tile)
    public TileView GetTileView(Vector2Int position)
}
```

#### TileView
個々のタイルの視覚表現を管理します。

```csharp
public class TileView : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public TileType Type { get; set; }
    
    public void SetTileData(Tile tile)
    public void SetHighlight(bool highlighted)
    public async UniTask PlayEffectAnimation()
    public void UpdateDisplay(Tile tile)
}
```

#### PathDrawingView
パス描画の入力と視覚フィードバックを管理します。

```csharp
public class PathDrawingView : MonoBehaviour
{
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private bool isDrawing = false;
    private LineRenderer pathLine;
    
    public event Action<List<Vector2Int>> OnPathCompleted;
    public event Action<List<Vector2Int>> OnPathUpdated;
    
    private void Update()
    public void StartDrawing(Vector2Int startPosition)
    public void UpdateDrawing(Vector2Int currentPosition)
    public void EndDrawing()
    public void ClearPath()
    public void HighlightPath(List<Vector2Int> path)
    public void ShowPathPreview(PathPreview preview)
}
```

#### UIView
ゲームUIを管理します。

```csharp
public class UIView : MonoBehaviour
{
    public void UpdateHP(int current, int max)
    public void UpdateGold(int gold)
    public void UpdateAttackPower(int attackPower)
    public void UpdateStageNumber(int stage)
    public async UniTask ShowConfirmationDialog()
    public async UniTask<bool> WaitForConfirmation()
    public void ShowGameOver()
    public async UniTask AnimateValueChange(int oldValue, int newValue, string label)
}
```

#### RewardView
報酬選択UIを管理します。

```csharp
public class RewardView : MonoBehaviour
{
    public async UniTask<RewardType> ShowRewardSelection(List<RewardType> rewards)
    public void DisplayRewardOption(RewardType reward, string description)
}
```

## データモデル

### ゲーム状態の永続化

```csharp
[Serializable]
public class GameSaveData
{
    public int currentStage;
    public int playerHP;
    public int playerGold;
    public int oneStrokeBonus;
    public TileSpawnConfig spawnConfig;
    // 必要に応じて追加
}
```

### 設定データ

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    [Header("初期値")]
    public int initialHP = 5;
    public int initialGold = 50;
    public int initialOneStrokeBonus = 5;
    
    [Header("タイル生成")]
    public TileSpawnConfig defaultSpawnConfig;
    
    [Header("敵設定")]
    public EnemySpawnTable enemySpawnTable;
    
    [Header("報酬設定")]
    public RewardConfig rewardConfig;
    
    [Header("ボス設定")]
    public int bossStageInterval = 10;
    public int bossActionInterval = 3;
}

[Serializable]
public class EnemySpawnTable
{
    public List<EnemySpawnEntry> entries;
    
    public EnemySpawnEntry GetEntryForStage(int stage)
}

[Serializable]
public class EnemySpawnEntry
{
    public int stageNumber;
    public int enemyCount;
    public int enemyHP;
    public int enemyAttack;
    public int bossHP;
    public int bossAttack;
}
```


## 正確性プロパティ

プロパティとは、システムのすべての有効な実行において真であるべき特性または動作です。本質的には、システムが何をすべきかについての形式的な記述です。プロパティは、人間が読める仕様と機械で検証可能な正確性保証との橋渡しとなります。

### パス検証プロパティ

**プロパティ1: パスはプレイヤー位置から開始**
*任意の*パスとプレイヤー位置について、パスが有効と判定されるためには、パスの最初の位置がプレイヤーの現在位置と一致しなければならない
**検証: 要件 1.1**

**プロパティ2: パスは敵マスで終了**
*任意の*パスとボード状態について、パスが有効と判定されるためには、パスの最後の位置が敵マスでなければならない
**検証: 要件 1.2**

**プロパティ3: パスの連続性**
*任意の*パスについて、パスが有効と判定されるためには、各連続する位置が隣接（上下左右）しており、同じ位置が重複して現れないこととする
**検証: 要件 1.3**

**プロパティ4: 中間敵マス通過許可**
*任意の*複数の敵を含むボードとパスについて、パスが最終位置以外で敵マスを通過する場合でも、他の検証条件を満たしていれば有効と判定されなければならない
**検証: 要件 1.4**

**プロパティ5: マス効果の実行順序**
*任意の*有効なパスについて、マス効果はパスが訪れた順序で正確に実行されなければならない
**検証: 要件 1.5**

### 攻撃力プロパティ

**プロパティ6: 移動による攻撃力増加**
*任意の*パスについて、パス実行後の攻撃力は、初期攻撃力 + パスの長さ + 適用された攻撃力ブーストの合計に等しくなければならない
**検証: 要件 2.1**

**プロパティ7: 敵へのダメージ適用**
*任意の*攻撃力値と敵について、プレイヤーが敵マスに乗った際、敵のHPは現在の攻撃力分だけ減少しなければならない
**検証: 要件 2.2**

**プロパティ8: ターン開始時の攻撃力リセット**
*任意の*ゲーム状態について、新しいターンが開始される際、プレイヤーの攻撃力は0にリセットされなければならない
**検証: 要件 2.3**

**プロパティ9: 一筆書きボーナス適用**
*任意の*25マスすべてを使用するパスについて、最終敵への攻撃前に一筆書きボーナス値が攻撃力に加算されなければならない
**検証: 要件 2.4**

### タイル効果プロパティ

**プロパティ10: 効果なしマスの無効果性**
*任意の*プレイヤー状態について、効果なしマスに乗った際、プレイヤーの状態（HP、ゴールド、攻撃力）は変化しないこととする
**検証: 要件 3.1**

**プロパティ11: 攻撃力上昇マスの効果**
*任意の*十分なゴールドを持つプレイヤーと攻撃力上昇マスについて、マスに乗った際、ゴールドが1減少し、攻撃力がマスの値だけ増加しなければならない
**検証: 要件 3.2**

**プロパティ12: HP回復マスの効果**
*任意の*十分なゴールドを持つプレイヤーとHP回復マスについて、マスに乗った際、ゴールドが1減少し、HPが1回復（最大5まで）しなければならない
**検証: 要件 3.3, 11.5**

**プロパティ13: ゴールドマスの効果**
*任意の*プレイヤーとゴールドマスについて、マスに乗った際、プレイヤーのゴールドがマスの値だけ増加しなければならない
**検証: 要件 3.4**

**プロパティ14: ゴールド不足時の効果無視**
*任意の*ゴールドが0のプレイヤーとゴールドコスト付きマスについて、マスに乗った際、マスの効果は適用されず、プレイヤー状態は変化しないこととする
**検証: 要件 3.5, 11.3**

### コンボシステムプロパティ

**プロパティ15: コンボ開始条件**
*任意の*パスについて、同じ効果タイプのマスに連続して2回乗った際、コンボ状態が開始されなければならない
**検証: 要件 4.1**

**プロパティ16: コンボ中のゴールド免除**
*任意の*コンボ状態が有効なパスについて、2回目以降の同じ効果タイプのマスでゴールド消費が発生してはならない
**検証: 要件 4.2**

**プロパティ17: コンボ終了条件**
*任意の*コンボ状態が有効なパスについて、異なる効果タイプのマスに乗った際、コンボ状態が終了しなければならない
**検証: 要件 4.3**

**プロパティ18: ターン終了時のコンボリセット**
*任意の*コンボ状態を持つゲーム状態について、ターンが終了した際、すべてのコンボ状態がリセットされなければならない
**検証: 要件 4.4**

### 戦闘プロパティ

**プロパティ19: 敵HP減少**
*任意の*攻撃力と敵について、プレイヤーが敵マスに乗った際、敵のHPが攻撃力分だけ減少しなければならない
**検証: 要件 5.1**

**プロパティ20: 敵撃破判定**
*任意の*敵について、HPが0以下になった際、その敵は倒された状態としてマークされなければならない
**検証: 要件 5.2**

**プロパティ21: 敵の反撃ダメージ**
*任意の*敵マスについて、攻撃後に敵が生存している場合、プレイヤーのHPが敵の攻撃力分だけ減少しなければならない
**検証: 要件 5.3**

**プロパティ22: ゲームオーバー条件**
*任意の*プレイヤー状態について、HPが0以下になった際、ゲームオーバー状態に遷移しなければならない
**検証: 要件 5.4**

**プロパティ23: 敵攻撃力の上限**
*任意の*生成される敵について、その攻撃力は1から4の範囲内でなければならない
**検証: 要件 5.5, 13.4**

### ボスシステムプロパティ

**プロパティ24: ボス出現条件**
*任意の*ステージ番号について、ステージ番号が10の倍数である場合、そのステージにボスが配置されなければならない
**検証: 要件 6.1**

**プロパティ25: ボス行動間隔**
*任意の*ボス戦について、ボスは指定された間隔（デフォルト3ターン）ごとに特殊行動を実行しなければならない
**検証: 要件 6.2**

**プロパティ26: とげマスのダメージ**
*任意の*とげマスについて、プレイヤーがそのマスに乗った際、指定されたダメージ値だけHPが減少しなければならない
**検証: 要件 6.4**

**プロパティ27: 壁マスの通行不可**
*任意の*壁マスを含むパスについて、そのパスは無効と判定されなければならない
**検証: 要件 6.5**

### ボード管理プロパティ

**プロパティ28: 訪問マスの削除**
*任意の*パスについて、ターン終了時にパスが訪れたすべてのマス位置が削除され、新しいマスで置き換えられなければならない
**検証: 要件 7.1, 7.2**

**プロパティ29: 敵撃破後のマス再生成**
*任意の*敵について、撃破された際、その敵マスの位置がランダムな新しいマスで置き換えられなければならない
**検証: 要件 7.3**

**プロパティ30: マス生成の確率分布**
*任意の*大量のマス生成について、各タイプの出現頻度は設定された出現率に統計的に近似しなければならない（許容誤差±5%）
**検証: 要件 7.4, 10.4**

**プロパティ31: 壁マスの永続性**
*任意の*ボスが生成した壁マスについて、プレイヤーが訪れていない限り、ターン終了時に削除されてはならない
**検証: 要件 7.5**

### ステージ進行プロパティ

**プロパティ32: ステージクリア条件**
*任意の*ステージについて、すべての敵が倒された際、ステージがクリア済み状態に遷移しなければならない
**検証: 要件 8.1**

**プロパティ33: 報酬提示**
*任意の*ステージクリア時について、3つの異なる報酬タイプがランダムに選択され、プレイヤーに提示されなければならない
**検証: 要件 8.2, 9.3**

**プロパティ34: 報酬効果の適用**
*任意の*報酬タイプについて、選択された際、対応するゲームパラメータが永続的に変更されなければならない
**検証: 要件 8.3, 9.2**

**プロパティ35: 報酬後のステージ進行**
*任意の*報酬選択について、適用後にステージ番号が1増加しなければならない
**検証: 要件 8.4**

### 報酬システムプロパティ

**プロパティ36: 出現率報酬の効果**
*任意の*出現率増加報酬について、適用後に生成されるマスは、変更された出現率（+5%）を反映しなければならない
**検証: 要件 9.4, 12.5**

**プロパティ37: 値報酬の効果**
*任意の*値増加報酬について、適用後に生成されるマスの値は、拡張された範囲から選択されなければならない
**検証: 要件 9.5, 12.6**

### ボード初期化プロパティ

**プロパティ38: ボードサイズ**
*任意の*新しいステージについて、初期化時に5×5の25マスのボードが作成されなければならない
**検証: 要件 10.1**

**プロパティ39: プレイヤー配置**
*任意の*ボード初期化について、プレイヤーは有効なマス位置に配置されなければならない
**検証: 要件 10.2**

**プロパティ40: 敵配置数**
*任意の*ステージについて、初期化時にステージ設定で指定された固定数の敵が配置されなければならない
**検証: 要件 10.3**

**プロパティ41: ステージ進行による敵数増加**
*任意の*2つの連続するステージについて、後のステージの敵数は前のステージ以上でなければならない
**検証: 要件 10.5, 13.3**

### マス生成パラメータプロパティ

**プロパティ42: 攻撃力上昇マスの値範囲**
*任意の*攻撃力上昇マス生成について、割り当てられる値は現在の値範囲（初期: 1-2）内でなければならない
**検証: 要件 12.3**

**プロパティ43: ゴールドマスの値範囲**
*任意の*ゴールドマス生成について、割り当てられる値は現在の値範囲（初期: 1-2）内でなければならない
**検証: 要件 12.4**

### 敵生成プロパティ

**プロパティ44: ステージ進行による敵HP増加**
*任意の*2つの異なるステージについて、ステージ番号が大きい方の敵HPは小さい方以上でなければならない
**検証: 要件 13.2**

**プロパティ45: ボスの高HP**
*任意の*ボスステージについて、ボスのHPは同ステージの通常敵のHPより高くなければならない
**検証: 要件 13.5**


## エラーハンドリング

### 入力検証エラー

**無効なパス開始位置**
- 検出: パスの最初の位置がプレイヤー位置と一致しない
- 処理: パスを拒否し、視覚的フィードバックを表示（赤いハイライトなど）
- ユーザーへの通知: 「現在の位置から開始してください」

**無効なパス終了位置**
- 検出: パスの最後の位置が敵マスでない
- 処理: パスを拒否し、視覚的フィードバックを表示
- ユーザーへの通知: 「敵マスで終了してください」

**非連続パス**
- 検出: パス内の連続する位置が隣接していない、または同じ位置が重複
- 処理: パスを拒否し、問題のある部分をハイライト
- ユーザーへの通知: 「連続したパスを描いてください」

**壁マスを含むパス**
- 検出: パスに壁マスが含まれる
- 処理: パスを拒否し、壁マスを特別な色でハイライト
- ユーザーへの通知: 「壁マスは通過できません」

### リソース不足エラー

**ゴールド不足**
- 検出: マス効果適用時にゴールドが不足
- 処理: 効果を適用せず、マスをスキップ
- ユーザーへの通知: マス上に「ゴールド不足」アイコンを一時表示

**HP上限超過**
- 検出: HP回復時に最大HPを超える
- 処理: HPを最大値5に制限
- ユーザーへの通知: なし（自然な動作）

### ゲーム状態エラー

**ゲームオーバー状態での操作**
- 検出: プレイヤーHP ≤ 0の状態で操作を試みる
- 処理: すべての入力を無効化
- ユーザーへの通知: ゲームオーバー画面を表示

**無効なステージ遷移**
- 検出: 敵が残っている状態でステージクリアを試みる
- 処理: ステージ遷移を拒否
- ログ: エラーログを記録（デバッグ用）

### データ整合性エラー

**ボード範囲外アクセス**
- 検出: 0-4の範囲外の座標にアクセス
- 処理: 例外をスローし、デフォルト値を返す
- ログ: エラーログを記録

**null参照エラー**
- 検出: タイルやエンティティがnull
- 処理: 安全なデフォルト動作を実行
- ログ: 警告ログを記録

### 非同期処理エラー

**アニメーション中断**
- 検出: アニメーション実行中にキャンセルトークンが発火
- 処理: アニメーションを即座に完了状態にスキップ
- ログ: デバッグログを記録

**UniTaskキャンセル**
- 検出: UniTask実行中にOperationCanceledException
- 処理: 適切にクリーンアップし、安全な状態に戻る
- ログ: 情報ログを記録

### エラーリカバリー戦略

1. **グレースフルデグラデーション**: 重大でないエラーは警告を表示して続行
2. **状態ロールバック**: 重大なエラーは前の安全な状態に戻る
3. **ユーザーフィードバック**: すべてのエラーに対して明確な視覚的フィードバック
4. **ログ記録**: すべてのエラーを適切なレベルでログに記録

## テスト戦略

### デュアルテストアプローチ

本プロジェクトでは、ユニットテストとプロパティベーステストの両方を使用します：

- **ユニットテスト**: 特定の例、エッジケース、エラー条件を検証
- **プロパティベーステスト**: すべての入力にわたる普遍的なプロパティを検証
- 両者は補完的であり、包括的なカバレッジに必要

### ユニットテストのバランス

- ユニットテストは特定の例とエッジケースに有用
- 多数のユニットテストを書きすぎない - プロパティベーステストが多くの入力をカバー
- ユニットテストの焦点：
  - 正しい動作を示す特定の例
  - コンポーネント間の統合ポイント
  - エッジケースとエラー条件
- プロパティテストの焦点：
  - すべての入力に対して成り立つ普遍的なプロパティ
  - ランダム化による包括的な入力カバレッジ

### プロパティベーステスト設定

**テストライブラリ**: C#用のFsCheck

**設定**:
- 各プロパティテストは最小100回の反復を実行
- 各プロパティテストは設計書のプロパティを参照
- タグ形式: `// Feature: one-stroke-roguelike, Property {番号}: {プロパティテキスト}`
- 各正確性プロパティは単一のプロパティベーステストで実装

### テスト対象コンポーネント

#### Model層のテスト

**Player**
- ユニットテスト: 初期化、HP/ゴールド境界値
- プロパティテスト: リソース操作の不変条件（HP ≤ MaxHP、Gold ≥ 0）

**Board**
- ユニットテスト: ボード初期化、特定の配置パターン
- プロパティテスト: 位置の有効性、タイル操作の一貫性

**Tile（各種）**
- ユニットテスト: 各タイプの基本動作、エッジケース
- プロパティテスト: 効果適用の正確性、ゴールド消費

**Enemy**
- ユニットテスト: 初期化、撃破判定
- プロパティテスト: ダメージ計算、攻撃力範囲

**TileSpawnConfig**
- ユニットテスト: 初期設定値、報酬適用
- プロパティテスト: 確率分布の統計的検証

#### Presenter層のテスト

**PathPresenter**
- ユニットテスト: 特定の有効/無効パターン
- プロパティテスト: パス検証ルール（プロパティ1-5）

**CombatPresenter**
- ユニットテスト: 特定の戦闘シナリオ
- プロパティテスト: ダメージ計算、コンボロジック

**BossPresenter**
- ユニットテスト: 各ボス行動の個別テスト
- プロパティテスト: ボス行動間隔、特殊マス生成

**RewardPresenter**
- ユニットテスト: 各報酬タイプの適用
- プロパティテスト: 報酬選択のランダム性、効果の永続性

#### 統合テスト

**完全なターンフロー**
- パス描画 → 検証 → 実行 → ボード更新の一連の流れ
- 複数ターンにわたるゲーム状態の一貫性

**ステージ進行フロー**
- ステージ開始 → 敵撃破 → ステージクリア → 報酬選択 → 次ステージ

**ボス戦フロー**
- ボス出現 → 複数ターン戦闘 → ボス行動 → ボス撃破

### テストデータ生成

**FsCheckジェネレータ**

```csharp
// パス生成
public static Arbitrary<List<Vector2Int>> ValidPathGenerator()
{
    // 連続した有効なパスを生成
}

// ボード状態生成
public static Arbitrary<Board> BoardGenerator()
{
    // ランダムなタイル配置のボードを生成
}

// プレイヤー状態生成
public static Arbitrary<Player> PlayerGenerator()
{
    // 有効な範囲内のHP/ゴールドを持つプレイヤーを生成
}

// 敵生成
public static Arbitrary<Enemy> EnemyGenerator()
{
    // 有効な範囲内のHP/攻撃力を持つ敵を生成
}
```

### テスト実行戦略

1. **開発中**: 変更したコンポーネントの関連テストを実行
2. **コミット前**: すべてのユニットテストとプロパティテストを実行
3. **CI/CD**: すべてのテストと統合テストを実行
4. **リリース前**: 長時間実行のプロパティテスト（1000回反復）を実行

### カバレッジ目標

- コードカバレッジ: 80%以上（Model層とPresenter層）
- プロパティカバレッジ: 設計書の全45プロパティを実装
- エッジケースカバレッジ: 各エラーハンドリングパスをテスト

### モックとスタブ

**View層のモック**
- テスト時にView層をモック化し、Presenter層を独立してテスト
- DOTweenアニメーションは即座完了モードで実行

**非同期処理のテスト**
- UniTaskはテストモードで同期実行
- タイムアウトは短縮値を使用

### パフォーマンステスト

**ボード生成パフォーマンス**
- 1000回のボード生成を1秒以内に完了

**パス検証パフォーマンス**
- 最大長（25マス）のパス検証を10ms以内に完了

**アニメーション実行**
- 60FPSを維持しながらアニメーション実行

## 実装上の注意事項

### Unity固有の考慮事項

1. **ScriptableObjectの活用**: GameConfigなどの設定はScriptableObjectで管理
2. **Prefabシステム**: タイルビューはPrefabとして作成し、動的に生成
3. **オブジェクトプーリング**: タイルビューの生成/破棄を最適化
4. **レイヤーとタグ**: 入力検出にPhysics2Dレイヤーを活用

### DOTween使用ガイドライン

1. **Sequenceの活用**: 複数のアニメーションを順序実行
2. **Easing関数**: 適切なイージングで滑らかな動き
3. **Kill処理**: シーン遷移時に適切にTweenをKill
4. **SetAutoKill**: 完了後の自動クリーンアップを設定

### UniTask使用ガイドライン

1. **CancellationToken**: すべての非同期メソッドでキャンセルをサポート
2. **例外処理**: UniTask内の例外を適切にキャッチ
3. **await vs Forget**: 結果が不要な場合はForgetを使用
4. **PlayerLoopTiming**: 適切なタイミング（Update、LateUpdateなど）を選択

### パフォーマンス最適化

1. **ガベージコレクション削減**: 
   - Listの事前確保
   - 構造体の活用
   - オブジェクトプーリング

2. **描画最適化**:
   - スプライトアトラスの使用
   - バッチング可能なマテリアル
   - 不要なカメラレンダリングの削減

3. **メモリ管理**:
   - 大きなオブジェクトの適切な破棄
   - テクスチャの圧縮
   - オーディオクリップの最適化

### 拡張性の考慮

1. **新しいタイルタイプの追加**: Tileクラスを継承して簡単に追加可能
2. **新しいボス行動の追加**: BossActionTypeとBossPresenterを拡張
3. **新しい報酬タイプの追加**: RewardTypeとRewardSystemを拡張
4. **難易度調整**: GameConfigのScriptableObjectで簡単に調整

### デバッグ支援

1. **ログレベル**: Debug、Info、Warning、Errorを適切に使い分け
2. **ビジュアルデバッグ**: Gizmosでパスや範囲を可視化
3. **チートコマンド**: 開発ビルドでのテスト用コマンド実装
4. **状態ダンプ**: ゲーム状態をJSON形式で出力可能に
