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
    /// ボタン種別
    /// 命名はDualShock4と順番は下記サイトを参考
    /// https://nanmo.hateblo.jp/entry/2014/02/23/000000
    /// </summary>
    public enum BUttonType
    {
        /// <summary>方向キー上</summary>
	    Up = 0,

        /// <summary>方向キー下</summary>
	    Down = 1,

        /// <summary>方向キー左</summary>
	    Left = 2,

        /// <summary>方向キー右</summary>
	    Right = 3,

        /// <summary>□ボタン</summary>
	    Square = 4,

        /// <summary>×ボタン</summary>
	    Cross = 5,

        /// <summary>○ボタン</summary>
	    Circle = 6,

        /// <summary>△ボタン</summary>
	    Triangle = 7,

        /// <summary>L1</summary>
	    L1 = 8,

        /// <summary>R1</summary>
	    R1 = 9,

        /// <summary>L2</summary>
	    L2 = 10,

        /// <summary>R2</summary>
	    R2 = 11,

        /// <summary>Share</summary>
        Share = 12,

        /// <summary>Option</summary>
	    Option = 13,

        /// <summary>左スティック押し込み</summary>
	    L3 = 14,

        /// <summary>右スティック押し込み</summary>
	    R3 = 15,

        /// <summary>PSボタン</summary>
	    PsButton = 16,

        /// <summary>トラックパッドクリック</summary>
	    TrackPad = 17
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
