using System.Collections.Generic;

namespace OneStrokeRGR.Presenter
{
    /// <summary>
    /// パス実行前の予測情報を保持するクラス
    /// </summary>
    public class PathPreview
    {
        /// <summary>予測される攻撃力</summary>
        public int PredictedAttackPower { get; set; }

        /// <summary>予測されるゴールド消費量</summary>
        public int PredictedGoldSpent { get; set; }

        /// <summary>予測されるゴールド獲得量</summary>
        public int PredictedGoldGained { get; set; }

        /// <summary>予測されるHP変化量（正=回復、負=ダメージ）</summary>
        public int PredictedHPChange { get; set; }

        /// <summary>パス上のタイル種類のシーケンス</summary>
        public List<Model.TileType> TileSequence { get; set; }

        public PathPreview()
        {
            PredictedAttackPower = 0;
            PredictedGoldSpent = 0;
            PredictedGoldGained = 0;
            PredictedHPChange = 0;
            TileSequence = new List<Model.TileType>();
        }
    }
}
