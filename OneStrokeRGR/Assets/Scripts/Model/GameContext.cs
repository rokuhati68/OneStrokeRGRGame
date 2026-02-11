namespace OneStrokeRGR.Model
{
    /// <summary>
    /// タイル効果適用時に必要なゲームコンテキスト情報
    /// </summary>
    public class GameContext
    {
        /// <summary>コンボが有効かどうか</summary>
        public bool IsComboActive { get; set; }

        /// <summary>現在のステージ番号</summary>
        public int CurrentStage { get; set; }

        /// <summary>ボスステージかどうか</summary>
        public bool IsBossStage { get; set; }

        public GameContext()
        {
            IsComboActive = false;
            CurrentStage = 1;
            IsBossStage = false;
        }
    }
}
