namespace Data
{
    /// <summary>
    /// キー入力種別
    /// ここでは見た目上の観点からノーツと定義する
    /// </summary>
    public enum NoteType
    {
        /// <summary>通常ノーツ</summary>
        Normal,

        /// <summary>ロングノーツ(開始)</summary>
        LongStart,

        /// <summary>ロングノーツ(終了)</summary>
        LongEnd
    }

    /// <summary>
    /// キー入力情報インターフェース
    /// </summary>
    public interface INote
    {
        /// <summary>発火時間</summary>
        int EventTime { get; }

        /// <summary>レーン番号</summary>
        int LaneIndex { get; }
    }

    /// <summary>
    /// 通常ノーツ
    /// </summary>
    public class NormalNote : INote
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NormalNote()
        {
        }

        /// <summary>発火時間</summary>
        public int EventTime { get; private set; }

        /// <summary>レーン番号</summary>
        public int LaneIndex { get; private set; }
    }

    /// <summary>
    /// ロングノーツ(開始)
    /// </summary>
    public class LongNoteStart : INote
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LongNoteStart()
        {
        }

        /// <summary>発火時間</summary>
        public int EventTime { get; private set; }

        /// <summary>レーン番号</summary>
        public int LaneIndex { get; private set; }
    }

    /// <summary>
    /// ロングノーツ(終了)
    /// </summary>
    public class LongNoteEnd : INote
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LongNoteEnd()
        {
        }

        /// <summary>発火時間</summary>
        public int EventTime { get; private set; }

        /// <summary>レーン番号</summary>
        public int LaneIndex { get; private set; }
    }
}
