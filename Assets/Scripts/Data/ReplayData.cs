namespace Data
{
    /// <summary>
    /// 入力種別
    /// </summary>
    public enum GameInputType
    {
        /// <summary>各種ボタン（方向キー含）</summary>
        Button = 0,

        /// <summary>左スティック</summary>
        LeftStick = 1,

        /// <summary>右スティック</summary>
        RightStick = 2
    }

    /// <summary>
    /// 画面に表示するキー入力としての情報
    /// ここでは見た目上の観点からノーツと定義する
    /// </summary>
    public class NoteData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NoteData(int eventTime, GameInputType inputType, int laneIndex, int pressRate)
        {
            EventTime = eventTime;
            InputType = inputType;
            LaneIndex = laneIndex;
            PressRate = pressRate;
        }

        /// <summary>
        /// 発火時間
        /// </summary>
        public int EventTime { get; private set; }

        /// <summary>
        /// 入力種別（ボタン or スティック）
        /// </summary>
        public GameInputType InputType {  get; private set; }

        /// <summary>
        /// 降ってくるレーン番号
        /// ボタン系の場合は各ボタンの種別ごとに割り振られたIDとして扱う
        /// スティック系の場合、0～10000として時計回りで管理（例：2500…右, 5000…下, 7500…左）
        /// </summary>
        public int LaneIndex { get; private set; }

        /// <summary>
        /// 押下の割合（万分率）
        /// 主に0,1による押し離しやスティック系の倒し度合を表現するために用いる
        /// </summary>
        public int PressRate { get; private set; }
    }
}
