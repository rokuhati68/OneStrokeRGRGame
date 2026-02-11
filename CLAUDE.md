# DO MUST
対話は日本語で行ってください。
(./.kiro)にまとめられている設計書に従いげえーむを作成する。
# OneStrokeRGRGame

一筆書きパズルとローグライク要素を組み合わせたゲームプロジェクト

## プロジェクト概要

プレイヤーが一筆書きでパスを描いてタイルの効果を発動させながら、敵を倒してステージを進めるローグライクゲーム。

### 主な機能
- 5×5のボード上で一筆書きのパス描画
- 複数のタイルタイプ（攻撃力アップ、HP回復、ゴールド、敵など）
- コンボシステム（同じタイプのタイルを連続で通過すると効果が倍増）
- ボスステージ（3ターンごとに特殊行動を実行）
- ステージクリア後の報酬選択

## 技術スタック

- **Unity**: 2022.3.x 以降
- **言語**: C#
- **パッケージ**:
  - TextMesh Pro (UI)
  - Unity Input System
  - Universal Render Pipeline (URP)
  - UniTask (非同期処理)

## アーキテクチャ


### ディレクトリ構造

```
OneStrokeRGR/
├── Assets/
│   ├── Scenes/           # シーン
│   ├── Scripts/
│   │   ├── GameSystem/       # ゲームシステム（状態管理）
│   │   │   ├── ITurnState.cs
│   │   │   ├── TurnStateContext.cs
│   │   │   ├── NewStageState.cs
│   │   │   └── PlayerInputState.cs
│   │   ├── Tiles/            # タイル関連
│   │   │   ├── ITileEffect.cs
│   │   │   ├── Tile.cs
│   │   │   ├── NormalTile.cs
│   │   │   ├── AtkAddTile.cs
│   │   │   ├── HealTile.cs
│   │   │   └── Enemy.cs
│   │   ├── Player/           # プレイヤー関連
│   │   │   └── PlayerStatus.cs
│   │   ├── TileCreateSystem/ # タイル生成システム
│   │   │   └── TileCreateManager.cs
│   │   ├── OneStrokeSystem/  # 一筆書きシステム
│   │   │   └── PathManager.cs
│   │   ├── UI/               # UI管理
│   │   │   └── UIManager.cs
│   │   └── Initializer/      # 初期化
│   │       └── GameManager.cs
│   ├── Resouse/          # リソース（スプライト、プレハブなど）
│   └── Settings/         # プロジェクト設定
└── .kiro/
    └── specs/            # 設計ドキュメント
        └── one-stroke-roguelike/
            └── class-diagram.md
```

## コーディング規約

### 命名規則
- **クラス名**: PascalCase (`PlayerStatus`, `TileCreateManager`)
- **メソッド名**: PascalCase (`ApplyEffect`, `CreateTile`)
- **フィールド**: camelCase (private), PascalCase (public)
- **インターフェース**: I + PascalCase (`ITileEffect`, `ITurnState`)
- **定数**: UPPER_SNAKE_CASE

### スクリプト構成
- 1ファイル1クラスを基本とする
- インターフェースは実装クラスと同じフォルダに配置
- MonoBehaviour継承クラスはシーン上のGameObjectにアタッチ

### 非同期処理
- `UniTask`を使用して非同期処理を実装
- アニメーション待機、順次実行などに活用



## 設計ドキュメント

詳細な設計については以下を参照：
- [クラス図](./.kiro/specs/one-stroke-roguelike/class-diagram.md)

## タスク管理
タスクは以下のファイルにまとめられている。上から順に処理を行う。
-[タスク一覧](./.kiro/specs/one-stroke-roguelike/tasks.md)

## デザイン
ゲームのデザインは以下のファイルにまとめられている。参考にすること
-[ゲームデザイン](./.kiro/specs/one-stroke-roguelike/design.md)

## 要件定義
要件定義は以下のファイルにまとめられている。遵守すること
-[要件定義書](./.kiro/specs/one-stroke-roguelike/requirements.md)

## 注意事項

- **Unity Editor**: 必ず Unity 2022.3.x 以降を使用
- **Git**: `Library/`, `Temp/`, `Logs/` フォルダは.gitignoreに含まれている
- **入力システム**: 旧Input Managerではなく、新しいInput Systemを使用
- **レンダリング**: URPを使用しているため、Built-in Render Pipelineのシェーダーは使用不可

## ビルド

現在開発中のため、ビルド設定は未確定

## 今後の実装予定

- MVP（Model-View-Presenter）パターンへのリファクタリング検討
- より詳細なタイルエフェクトの実装
- ボス敵の特殊行動の実装
- 報酬システムの実装
