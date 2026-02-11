# クラス図：一筆書きローグライクゲーム

## 全体構造図

```mermaid
classDiagram
    %% View Layer
    class BoardView {
        -TileView[5,5] tileViews
        +InitializeBoard(Board)
        +UpdateTileView(Vector2Int, Tile)
        +AnimateTileRemoval(Vector2Int) UniTask
        +AnimateTileSpawn(Vector2Int, Tile) UniTask
        +GetTileView(Vector2Int) TileView
    }

    class TileView {
        +Vector2Int Position
        +TileType Type
        +SetTileData(Tile)
        +SetHighlight(bool)
        +PlayEffectAnimation() UniTask
        +UpdateDisplay(Tile)
    }

    class PathDrawingView {
        -List~Vector2Int~ currentPath
        -bool isDrawing
        -LineRenderer pathLine
        +OnPathCompleted Event
        +OnPathUpdated Event
        +StartDrawing(Vector2Int)
        +UpdateDrawing(Vector2Int)
        +EndDrawing()
        +ClearPath()
        +HighlightPath(List~Vector2Int~)
        +ShowPathPreview(PathPreview)
    }

    class UIView {
        +UpdateHP(int, int)
        +UpdateGold(int)
        +UpdateAttackPower(int)
        +UpdateStageNumber(int)
        +ShowConfirmationDialog() UniTask
        +WaitForConfirmation() UniTask~bool~
        +ShowGameOver()
        +AnimateValueChange(int, int, string) UniTask
    }

    class RewardView {
        +ShowRewardSelection(List~RewardType~) UniTask~RewardType~
        +DisplayRewardOption(RewardType, string)
    }

    %% Presenter Layer
    class GamePresenter {
        -GameState gameState
        -PathPresenter pathPresenter
        -CombatPresenter combatPresenter
        -RewardPresenter rewardPresenter
        -BossPresenter bossPresenter
        -BoardView boardView
        -UIView uiView
        +InitializeGame() UniTask
        +StartNewStage() UniTask
        +HandlePathDrawingPhase() UniTask
        +HandlePathExecutionPhase(List~Vector2Int~) UniTask
        +HandleBossActionPhase() UniTask
        +HandleStageClearedPhase() UniTask
        +HandleGameOverPhase() UniTask
    }

    class PathPresenter {
        -GameState gameState
        -PathDrawingView pathView
        +ValidatePath(List~Vector2Int~, Vector2Int) bool
        +IsPathStartingFromPlayer(List~Vector2Int~, Vector2Int) bool
        +IsPathEndingOnEnemy(List~Vector2Int~, Board) bool
        +IsPathContinuous(List~Vector2Int~) bool
        +CalculatePathPreview(List~Vector2Int~, Player, Board) PathPreview
    }

    class CombatPresenter {
        -GameState gameState
        +ExecutePath(List~Vector2Int~) UniTask
        +ProcessTileEffect(Tile, ComboTracker) UniTask
        +ProcessCombat(Enemy, int) UniTask
        +ApplyOneStrokeBonus(int, Player)
    }

    class BossPresenter {
        -GameState gameState
        +ExecuteBossAction(Enemy) UniTask
        +SelectRandomBossAction() BossActionType
        +DisableAttackBoostTiles() UniTask
        +HealBoss(Enemy, int) UniTask
        +SpawnThornTiles(int) UniTask
        +SpawnWallTiles(int) UniTask
    }

    class RewardPresenter {
        -RewardSystem rewardSystem
        -RewardView rewardView
        +PresentRewardSelection(GameState) UniTask~RewardType~
        +ApplySelectedReward(RewardType, GameState)
    }

    class ComboTracker {
        -TileType? lastTileType
        -int comboCount
        +IsComboActive(TileType) bool
        +UpdateCombo(TileType)
        +Reset()
    }

    class PathPreview {
        +int PredictedAttackPower
        +int PredictedGoldSpent
        +int PredictedGoldGained
        +int PredictedHPChange
        +List~TileType~ TileSequence
    }

    %% Model Layer
    class GameState {
        +int CurrentStage
        +Player Player
        +Board Board
        +RewardConfig RewardConfig
        +TileSpawnConfig SpawnConfig
        +GamePhase CurrentPhase
        +IsBossStage() bool
    }

    class Player {
        +int MaxHP
        +int CurrentHP
        +int Gold
        +int AttackPower
        +Vector2Int Position
        +int OneStrokeBonus
        +Initialize(int, int)
        +TakeDamage(int)
        +Heal(int)
        +AddGold(int)
        +SpendGold(int) bool
        +IncreaseAttackPower(int)
        +ResetAttackPower()
        +IsAlive() bool
    }

    class Board {
        -Tile[5,5] tiles
        -List~Enemy~ enemies
        +GetTile(Vector2Int) Tile
        +SetTile(Vector2Int, Tile)
        +GetEnemies() List~Enemy~
        +RemoveEnemy(Enemy)
        +AddEnemy(Enemy)
        +IsValidPosition(Vector2Int) bool
        +RegenerateTiles(List~Vector2Int~, TileSpawnConfig)
    }

    class Tile {
        <<abstract>>
        +Vector2Int Position
        +TileType Type
        +ApplyEffect(Player, GameContext)* TileEffectResult
        +CanApplyEffect(Player)* bool
    }

    class EmptyTile {
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class AttackBoostTile {
        +int BoostValue
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class HPRecoveryTile {
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class GoldTile {
        +int GoldValue
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class EnemyTile {
        +Enemy Enemy
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class ThornTile {
        +int Damage
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class WallTile {
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class Enemy {
        +int MaxHP
        +int CurrentHP
        +int AttackPower
        +Vector2Int Position
        +bool IsBoss
        +int BossActionInterval
        +int TurnsSinceLastAction
        +TakeDamage(int)
        +IsAlive() bool
        +ShouldPerformBossAction() bool
    }

    class TileSpawnConfig {
        +float EmptyRate
        +float AttackBoostRate
        +float HPRecoveryRate
        +float GoldRate
        +(int, int) AttackBoostRange
        +(int, int) GoldRange
        +ApplyReward(RewardType)
    }

    class RewardSystem {
        +SelectRandomRewards(int) List~RewardType~
        +ApplyReward(RewardType, GameState)
    }

    class RewardConfig {
        +float SpawnRateIncrement
        +int ValueIncrement
        +int OneStrokeBonusIncrement
    }

    class TileEffectResult {
        +bool EffectApplied
        +int GoldSpent
        +int AttackGained
        +int HPGained
        +int GoldGained
        +int DamageTaken
        +Enemy DefeatedEnemy
    }

    class GameConfig {
        <<ScriptableObject>>
        +int initialHP
        +int initialGold
        +int initialOneStrokeBonus
        +TileSpawnConfig defaultSpawnConfig
        +EnemySpawnTable enemySpawnTable
        +RewardConfig rewardConfig
        +int bossStageInterval
        +int bossActionInterval
    }

    class EnemySpawnTable {
        +List~EnemySpawnEntry~ entries
        +GetEntryForStage(int) EnemySpawnEntry
    }

    class EnemySpawnEntry {
        +int stageNumber
        +int enemyCount
        +int enemyHP
        +int enemyAttack
        +int bossHP
        +int bossAttack
    }

    %% Enums
    class GamePhase {
        <<enumeration>>
        PathDrawing
        PathConfirmation
        PathExecution
        BossAction
        StageCleared
        RewardSelection
        GameOver
    }

    class TileType {
        <<enumeration>>
        Empty
        AttackBoost
        HPRecovery
        Gold
        Enemy
        Thorn
        Wall
    }

    class RewardType {
        <<enumeration>>
        AttackBoostRateIncrease
        AttackBoostValueIncrease
        HPRecoveryRateIncrease
        EmptyRateIncrease
        GoldRateIncrease
        GoldValueIncrease
        OneStrokeBonusIncrease
    }

    class BossActionType {
        <<enumeration>>
        DisableAttackBoost
        HealSelf
        SpawnThorns
        SpawnWalls
    }

    %% Relationships - View Layer
    BoardView "1" --> "*" TileView : contains
    GamePresenter --> BoardView : uses
    GamePresenter --> UIView : uses
    GamePresenter --> PathDrawingView : uses
    RewardPresenter --> RewardView : uses
    PathPresenter --> PathDrawingView : uses

    %% Relationships - Presenter Layer
    GamePresenter --> PathPresenter : uses
    GamePresenter --> CombatPresenter : uses
    GamePresenter --> RewardPresenter : uses
    GamePresenter --> BossPresenter : uses
    CombatPresenter --> ComboTracker : uses
    PathPresenter --> PathPreview : creates

    %% Relationships - Model Layer
    GamePresenter --> GameState : manages
    GameState --> Player : contains
    GameState --> Board : contains
    GameState --> TileSpawnConfig : contains
    GameState --> RewardConfig : contains
    GameState --> GamePhase : uses
    Board "1" --> "25" Tile : contains
    Board "1" --> "*" Enemy : contains
    Tile <|-- EmptyTile : extends
    Tile <|-- AttackBoostTile : extends
    Tile <|-- HPRecoveryTile : extends
    Tile <|-- GoldTile : extends
    Tile <|-- EnemyTile : extends
    Tile <|-- ThornTile : extends
    Tile <|-- WallTile : extends
    EnemyTile --> Enemy : references
    Tile --> TileType : uses
    Tile --> TileEffectResult : returns
    RewardSystem --> RewardType : uses
    RewardSystem --> RewardConfig : uses
    TileSpawnConfig --> RewardType : uses
    GameConfig --> TileSpawnConfig : contains
    GameConfig --> EnemySpawnTable : contains
    GameConfig --> RewardConfig : contains
    EnemySpawnTable --> EnemySpawnEntry : contains
    BossPresenter --> BossActionType : uses
```

## レイヤー別詳細図

### Model層の詳細

```mermaid
classDiagram
    class GameState {
        +int CurrentStage
        +Player Player
        +Board Board
        +RewardConfig RewardConfig
        +TileSpawnConfig SpawnConfig
        +GamePhase CurrentPhase
        +IsBossStage() bool
    }

    class Player {
        -int maxHP = 5
        -int currentHP
        -int gold
        -int attackPower
        -Vector2Int position
        -int oneStrokeBonus
        +int MaxHP
        +int CurrentHP
        +int Gold
        +int AttackPower
        +Vector2Int Position
        +int OneStrokeBonus
        +Initialize(int startingGold, int oneStrokeBonus)
        +TakeDamage(int damage)
        +Heal(int amount)
        +AddGold(int amount)
        +SpendGold(int amount) bool
        +IncreaseAttackPower(int amount)
        +ResetAttackPower()
        +IsAlive() bool
    }

    class Board {
        -Tile[5,5] tiles
        -List~Enemy~ enemies
        +GetTile(Vector2Int position) Tile
        +SetTile(Vector2Int position, Tile tile)
        +GetEnemies() List~Enemy~
        +RemoveEnemy(Enemy enemy)
        +AddEnemy(Enemy enemy)
        +IsValidPosition(Vector2Int position) bool
        +RegenerateTiles(List~Vector2Int~ positions, TileSpawnConfig config)
    }

    class Enemy {
        -int maxHP
        -int currentHP
        -int attackPower
        -Vector2Int position
        -bool isBoss
        -int bossActionInterval = 3
        -int turnsSinceLastAction = 0
        +int MaxHP
        +int CurrentHP
        +int AttackPower
        +Vector2Int Position
        +bool IsBoss
        +int BossActionInterval
        +int TurnsSinceLastAction
        +Enemy(int hp, int attack, bool isBoss)
        +TakeDamage(int damage)
        +IsAlive() bool
        +ShouldPerformBossAction() bool
    }

    GameState --> Player
    GameState --> Board
    Board --> Enemy
```

### Tile階層の詳細

```mermaid
classDiagram
    class Tile {
        <<abstract>>
        +Vector2Int Position
        +TileType Type
        +ApplyEffect(Player player, GameContext context)* TileEffectResult
        +CanApplyEffect(Player player)* bool
    }

    class EmptyTile {
        +EmptyTile()
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class AttackBoostTile {
        +int BoostValue
        +AttackBoostTile(int value)
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class HPRecoveryTile {
        +HPRecoveryTile()
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class GoldTile {
        +int GoldValue
        +GoldTile(int value)
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class EnemyTile {
        +Enemy Enemy
        +EnemyTile(Enemy enemy)
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class ThornTile {
        +int Damage
        +ThornTile(int damage)
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class WallTile {
        +WallTile()
        +ApplyEffect(Player, GameContext) TileEffectResult
        +CanApplyEffect(Player) bool
    }

    class TileEffectResult {
        +bool EffectApplied
        +int GoldSpent
        +int AttackGained
        +int HPGained
        +int GoldGained
        +int DamageTaken
        +Enemy DefeatedEnemy
    }

    Tile <|-- EmptyTile
    Tile <|-- AttackBoostTile
    Tile <|-- HPRecoveryTile
    Tile <|-- GoldTile
    Tile <|-- EnemyTile
    Tile <|-- ThornTile
    Tile <|-- WallTile
    Tile ..> TileEffectResult : returns
    EnemyTile --> Enemy : references
```

### Presenter層の詳細

```mermaid
classDiagram
    class GamePresenter {
        -GameState gameState
        -PathPresenter pathPresenter
        -CombatPresenter combatPresenter
        -RewardPresenter rewardPresenter
        -BossPresenter bossPresenter
        -BoardView boardView
        -UIView uiView
        +InitializeGame() UniTask
        +StartNewStage() UniTask
        +HandlePathDrawingPhase() UniTask
        +HandlePathExecutionPhase(List~Vector2Int~ path) UniTask
        +HandleBossActionPhase() UniTask
        +HandleStageClearedPhase() UniTask
        +HandleGameOverPhase() UniTask
    }

    class PathPresenter {
        -GameState gameState
        -PathDrawingView pathView
        +ValidatePath(List~Vector2Int~ path, Vector2Int playerPosition) bool
        +IsPathStartingFromPlayer(List~Vector2Int~ path, Vector2Int playerPosition) bool
        +IsPathEndingOnEnemy(List~Vector2Int~ path, Board board) bool
        +IsPathContinuous(List~Vector2Int~ path) bool
        +CalculatePathPreview(List~Vector2Int~ path, Player player, Board board) PathPreview
    }

    class CombatPresenter {
        -GameState gameState
        +ExecutePath(List~Vector2Int~ path) UniTask
        +ProcessTileEffect(Tile tile, ComboTracker comboTracker) UniTask
        +ProcessCombat(Enemy enemy, int attackPower) UniTask
        +ApplyOneStrokeBonus(int pathLength, Player player)
    }

    class BossPresenter {
        -GameState gameState
        +ExecuteBossAction(Enemy boss) UniTask
        +SelectRandomBossAction() BossActionType
        +DisableAttackBoostTiles() UniTask
        +HealBoss(Enemy boss, int amount) UniTask
        +SpawnThornTiles(int count) UniTask
        +SpawnWallTiles(int count) UniTask
    }

    class RewardPresenter {
        -RewardSystem rewardSystem
        -RewardView rewardView
        +PresentRewardSelection(GameState gameState) UniTask~RewardType~
        +ApplySelectedReward(RewardType reward, GameState gameState)
    }

    class ComboTracker {
        -TileType? lastTileType
        -int comboCount
        +IsComboActive(TileType currentType) bool
        +UpdateCombo(TileType currentType)
        +Reset()
    }

    class PathPreview {
        +int PredictedAttackPower
        +int PredictedGoldSpent
        +int PredictedGoldGained
        +int PredictedHPChange
        +List~TileType~ TileSequence
    }

    GamePresenter --> PathPresenter
    GamePresenter --> CombatPresenter
    GamePresenter --> RewardPresenter
    GamePresenter --> BossPresenter
    CombatPresenter --> ComboTracker
    PathPresenter ..> PathPreview : creates
```

### View層の詳細

```mermaid
classDiagram
    class BoardView {
        -TileView[5,5] tileViews
        +InitializeBoard(Board board)
        +UpdateTileView(Vector2Int position, Tile tile)
        +AnimateTileRemoval(Vector2Int position) UniTask
        +AnimateTileSpawn(Vector2Int position, Tile tile) UniTask
        +GetTileView(Vector2Int position) TileView
    }

    class TileView {
        -SpriteRenderer spriteRenderer
        -TextMeshPro valueText
        +Vector2Int Position
        +TileType Type
        +SetTileData(Tile tile)
        +SetHighlight(bool highlighted)
        +PlayEffectAnimation() UniTask
        +UpdateDisplay(Tile tile)
    }

    class PathDrawingView {
        -List~Vector2Int~ currentPath
        -bool isDrawing
        -LineRenderer pathLine
        +event Action~List~Vector2Int~~ OnPathCompleted
        +event Action~List~Vector2Int~~ OnPathUpdated
        -Update()
        +StartDrawing(Vector2Int startPosition)
        +UpdateDrawing(Vector2Int currentPosition)
        +EndDrawing()
        +ClearPath()
        +HighlightPath(List~Vector2Int~ path)
        +ShowPathPreview(PathPreview preview)
    }

    class UIView {
        -TextMeshProUGUI hpText
        -TextMeshProUGUI goldText
        -TextMeshProUGUI attackPowerText
        -TextMeshProUGUI stageText
        -GameObject confirmationDialog
        -GameObject gameOverPanel
        +UpdateHP(int current, int max)
        +UpdateGold(int gold)
        +UpdateAttackPower(int attackPower)
        +UpdateStageNumber(int stage)
        +ShowConfirmationDialog() UniTask
        +WaitForConfirmation() UniTask~bool~
        +ShowGameOver()
        +AnimateValueChange(int oldValue, int newValue, string label) UniTask
    }

    class RewardView {
        -List~RewardButton~ rewardButtons
        +ShowRewardSelection(List~RewardType~ rewards) UniTask~RewardType~
        +DisplayRewardOption(RewardType reward, string description)
    }

    BoardView "1" --> "25" TileView : manages
```

### 設定とデータ構造

```mermaid
classDiagram
    class GameConfig {
        <<ScriptableObject>>
        +int initialHP
        +int initialGold
        +int initialOneStrokeBonus
        +TileSpawnConfig defaultSpawnConfig
        +EnemySpawnTable enemySpawnTable
        +RewardConfig rewardConfig
        +int bossStageInterval
        +int bossActionInterval
    }

    class TileSpawnConfig {
        +float EmptyRate
        +float AttackBoostRate
        +float HPRecoveryRate
        +float GoldRate
        +(int min, int max) AttackBoostRange
        +(int min, int max) GoldRange
        +ApplyReward(RewardType rewardType)
    }

    class RewardConfig {
        +float SpawnRateIncrement
        +int ValueIncrement
        +int OneStrokeBonusIncrement
    }

    class RewardSystem {
        +SelectRandomRewards(int count) List~RewardType~
        +ApplyReward(RewardType reward, GameState gameState)
    }

    class EnemySpawnTable {
        +List~EnemySpawnEntry~ entries
        +GetEntryForStage(int stage) EnemySpawnEntry
    }

    class EnemySpawnEntry {
        +int stageNumber
        +int enemyCount
        +int enemyHP
        +int enemyAttack
        +int bossHP
        +int bossAttack
    }

    GameConfig --> TileSpawnConfig
    GameConfig --> RewardConfig
    GameConfig --> EnemySpawnTable
    EnemySpawnTable --> EnemySpawnEntry
    RewardSystem --> RewardConfig
```

## シーケンス図：ターン実行フロー

```mermaid
sequenceDiagram
    participant Player
    participant PathDrawingView
    participant PathPresenter
    participant GamePresenter
    participant CombatPresenter
    participant Board
    participant UIView

    Player->>PathDrawingView: マウスドラッグでパス描画
    PathDrawingView->>PathPresenter: パス検証要求
    PathPresenter->>PathPresenter: ValidatePath()
    PathPresenter-->>PathDrawingView: 検証結果
    PathDrawingView->>Player: 視覚的フィードバック
    
    Player->>PathDrawingView: パス確定
    PathDrawingView->>GamePresenter: OnPathCompleted
    GamePresenter->>UIView: 確認ダイアログ表示
    Player->>UIView: 決定ボタンクリック
    
    GamePresenter->>CombatPresenter: ExecutePath(path)
    loop パス上の各マス
        CombatPresenter->>Board: GetTile(position)
        Board-->>CombatPresenter: tile
        CombatPresenter->>CombatPresenter: ProcessTileEffect(tile)
        CombatPresenter->>UIView: 状態更新
    end
    
    CombatPresenter->>Board: RegenerateTiles()
    Board-->>CombatPresenter: 完了
    CombatPresenter-->>GamePresenter: ターン完了
```

## 主要な設計パターン

### 1. Model-View-Presenter (MVP)
- **Model**: ゲームロジックとデータ（GameState, Player, Board, Tile等）
- **View**: UI表示と入力検出（BoardView, UIView, PathDrawingView等）
- **Presenter**: ロジック制御と状態遷移（GamePresenter, CombatPresenter等）

### 2. Strategy Pattern
- Tileクラス階層で各タイプの効果を実装
- ApplyEffect()メソッドで異なる動作を実現

### 3. Observer Pattern
- PathDrawingViewのイベント（OnPathCompleted, OnPathUpdated）
- PresenterがViewのイベントを購読

### 4. State Pattern
- GamePhase列挙型でゲーム状態を管理
- GamePresenterが各フェーズのハンドラを実装

### 5. Factory Pattern
- タイル生成でTileSpawnConfigを使用
- 確率に基づいて適切なTileサブクラスを生成
