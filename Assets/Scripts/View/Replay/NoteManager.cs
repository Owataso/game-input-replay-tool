using System.Collections.Generic;

namespace View.Replay
{
    /// <summary>
    /// ノーツ種別
    /// </summary>
    public enum NoteType
    {
        /// <summary>通常ノーツ</summary>
        Normal = 0,

        /// <summary>ロング開始</summary>
        LongStart = 1,

        /// <summary>ロング終了</summary>
        LongEnd = 2
    }

    /// <summary>
    /// 判定
    /// </summary>
    public enum JudgeType
    {
        Perfect,
        Great,
        Good,
        Bad,
        Miss
    }

    /// <summary>
    /// 画面に表示するキー入力Viewのインターフェースとなるもの
    /// ここでは見た目上の観点からノーツと定義する
    /// </summary>
    public interface INote
    {
        /// <summary>自身のノーツ種別定義</summary>
        NoteType GetNoteType();

        /// <summary>ボタン入力時に発火するイベント</summary>
        /// <param name="time">現在の再生時間</param>
        /// <param name="press">ボタン押下か。falseなら離した</param>
        void OnButton(int time, bool press);

        /// <summary>ノーツ処理時に必ず呼び出す</summary>
        /// <param name="judge">処理時の判定</param>
        void Process(JudgeType judge);

        /// <summary>発火時間</summary>
        int EventTime { get; }

        /// <summary>レーン番号</summary>
        int LaneIndex { get; }

        /// <summary>処理されたか(消えてもいいか)</summary>
        bool IsProcessed { get; }
    }

    /// <summary>
    /// ノーツ管理クラス
    /// </summary>
    public class NoteManager
    {
        /// <summary>
        /// まだ落としていないノーツ一覧
        /// </summary>
        List<Data.NoteData> noteDataList;

        /// <summary>
        /// 画面中に表示しているノーツ一覧
        /// </summary>
        List<INote> activeNoteList;

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="time">現在の再生時間</param>
        public void Update(int time)
        {
            for (int i = activeNoteList.Count; i >= 0; i--)
            {
                if (activeNoteList[i].IsProcessed)
                {

                }
            }
        }
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
            IsProcessed = false;
        }

        /// <summary>ボタン入力時に発火するイベント</summary>
        /// <param name="time">現在の再生時間</param>
        /// <param name="press">ボタン押下か。falseなら離した</param>
        public void OnButton(int time, bool press)
        {
            // ボタン離しなら判定不要
            if (!press) return;

            Process(JudgeType.Perfect);
        }

        /// <summary>ノーツ処理時に必ず呼び出す</summary>
        /// <param name="judge">処理時の判定</param>
        public void Process(JudgeType judge)
        {
            // 二重処理防止
            if (IsProcessed) return;

            IsProcessed = true;
        }

        /// <summary>入力種別定義</summary>
        public NoteType GetNoteType() { return NoteType.Normal; }

        /// <summary>発火時間</summary>
        public int EventTime { get; private set; }

        /// <summary>レーン番号</summary>
        public int LaneIndex { get; private set; }

        /// <summary>処理されたか(消えてもいいか)</summary>
        public bool IsProcessed { get; private set; }
    }

    /// <summary>
    /// ロングノーツ
    /// </summary>
    public class LongNote : INote
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LongNote()
        {
            IsProcessed = false;
        }

        /// <summary>ボタン入力時に発火するイベント</summary>
        /// <param name="time">現在の再生時間</param>
        /// <param name="press">ボタン押下か。falseなら離した</param>
        public void OnButton(int time, bool press)
        {
            if (IsStart)
            {
                // ロング開始ノーツだったら
                // ボタン離しなら判定不要
                if (!press) return;
            }
            else
            {
                // ロング終了ノーツだったら
            }

            Process(JudgeType.Perfect);
        }

        /// <summary>ノーツ処理時に必ず呼び出す</summary>
        /// <param name="judge">処理時の判定</param>
        public void Process(JudgeType judge)
        {
            // 二重処理防止
            if (IsProcessed) return;

            IsProcessed = true;
        }

        /// <summary>入力種別定義</summary>
        public NoteType GetNoteType() { return IsStart ? NoteType.LongStart : NoteType.LongEnd; }

        /// <summary>発火時間</summary>
        public int EventTime { get; private set; }

        /// <summary>レーン番号</summary>
        public int LaneIndex { get; private set; }

        /// <summary>処理されたか(消えてもいいか)</summary>
        public bool IsProcessed { get; private set; }

        /// <summary>開始か（false:ロング終了ノーツ）</summary>
        public bool IsStart { get; private set; }
    }
}
