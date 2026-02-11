namespace OneStrokeRGR.Model
{
    /// <summary>
    /// ゲームのフェーズを定義する列挙型
    /// </summary>
    public enum GamePhase
    {
        /// <summary>パス描画フェーズ</summary>
        PathDrawing,

        /// <summary>パス確認フェーズ</summary>
        PathConfirmation,

        /// <summary>パス実行フェーズ</summary>
        PathExecution,

        /// <summary>ボス行動フェーズ</summary>
        BossAction,

        /// <summary>ステージクリアフェーズ</summary>
        StageCleared,

        /// <summary>報酬選択フェーズ</summary>
        RewardSelection,

        /// <summary>ゲームオーバーフェーズ</summary>
        GameOver
    }
}
