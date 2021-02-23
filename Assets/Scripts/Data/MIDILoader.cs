using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class MIDILoader
{
    ///<summary>
    /// MIDIロード後の成果物をまとめたクラス
    ///</summary>
    public class MIDILoadData
    {
        public short Division;             // 分能値
        public NoteData[] NoteArray;       // ノーツ
        public SoflanData[] SoflanArray;   // テンポ変更
        public BeatData[] BeatArray;       // 拍子
    }

    ///<summary>
    /// ノーツデータ
    ///</summary>
    public class NoteData
    {
        public int EventTime;   // イベント時間
        public byte LaneIndex;  // レーン番号
        public int SoflanNo;    // 自分が何番目のソフランの番号か(ソフラン依存の座標にするときに必要)
        public bool IsPress;      // 押下か（falseなら終端）
    }

    ///<summary>
    /// テンポ変更データ
    ///</summary>
    public class SoflanData
    {
        public int EventTime;         // イベント時間
        public float Bpm;             // BPM値(小数点込)
        public float Tick;            // tick値(60 / BPM / 分能値 * 1000)
    }

    ///<summary>
    /// 拍子変更データ
    ///</summary>
    public class BeatData
    {
        public int EventTime;    // イベント時間
        public byte Numerator;   // 分子
        public byte Denominator; // 分母
    }

    /// <summary>
    /// ヘッダーチャンク情報
    /// </summary>
    public class HeaderChunkData
    {
        public byte[] ChunkType;    // チャンクのIDを示す(4byte)
        public int DataLength;      // チャンクのデータ長(4byte)
        public short Format;        // MIDIファイルフォーマットで、(2byte)
        public short Tracks;        // トラック数(2byte)
        public short Division;      // タイムベース MIDI独自の時間の最小単位をtickと呼び、4分音符あたりのtick数がタイムベース 大体480(2byte)
    };

    /// <summary>
    /// トラックチャンク情報
    /// </summary>
    public class TrackChunkData
    {
        public byte[] ChunkType;    // チャンクのIDを示す(4byte)
        public int DataLength;      // チャンクのデータ長(4byte)
        public byte[] Data;         // 演奏情報が入っているデータ
    };

    /// <summary>
    /// MIDIファイルをロードし必要な情報を返す
    /// </summary>
    /// <returns>ロードに成功したらそのデータ、失敗したらnull</returns>
    public MIDILoadData Load(string fileName)
    {
        var ret = new MIDILoadData();

        try
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                /* ヘッダチャンク侵入 */
                var headerChunk = new HeaderChunkData();

                // チャンクID読み込み
                headerChunk.ChunkType = reader.ReadBytes(4);
                // お前は本当にヘッダチャンクか？
                if (
                    headerChunk.ChunkType[0] != 'M' ||
                    headerChunk.ChunkType[1] != 'T' ||
                    headerChunk.ChunkType[2] != 'h' ||
                    headerChunk.ChunkType[3] != 'd')
                {
                    throw new FormatException("head chunk != MThd.");
                }
                // 自分のPCがリトルエンディアンなら変換する
                if (BitConverter.IsLittleEndian)
                {
                    // ヘッダ部のデータ長(値は6固定)
                    var byteArray = reader.ReadBytes(4);
                    Array.Reverse(byteArray);
                    headerChunk.DataLength = BitConverter.ToInt32(byteArray, 0);
                    // フォーマット(2byte)
                    byteArray = reader.ReadBytes(2);
                    Array.Reverse(byteArray);
                    headerChunk.Format = BitConverter.ToInt16(byteArray, 0);
                    // トラック数(2byte)
                    byteArray = reader.ReadBytes(2);
                    Array.Reverse(byteArray);
                    headerChunk.Tracks = BitConverter.ToInt16(byteArray, 0);
                    // タイムベース(2byte)
                    byteArray = reader.ReadBytes(2);
                    Array.Reverse(byteArray);
                    headerChunk.Division = BitConverter.ToInt16(byteArray, 0);
                }
                else
                {
                    // ヘッダ部のデータ長(値は6固定)
                    headerChunk.DataLength = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    // フォーマット(2byte)
                    headerChunk.Format = BitConverter.ToInt16(reader.ReadBytes(2), 0);
                    // トラック数(2byte)
                    headerChunk.Tracks = BitConverter.ToInt16(reader.ReadBytes(2), 0);
                    // タイムベース(2byte)
                    headerChunk.Division = BitConverter.ToInt16(reader.ReadBytes(2), 0);
                }

                // 分能値保存
                ret.Division = headerChunk.Division;

                // トラックが何もなかったら出ていけぇ！
                if (headerChunk.Tracks <= 0)
                {
                    throw new Exception("not exsist tracks.");
                }

                /* トラックチャンク侵入 */
                var trackChunks = new TrackChunkData[headerChunk.Tracks];

                // トラック数ぶん回す
                for (int i = 0; i < headerChunk.Tracks; i++)
                {
                    // チャンクID読み込み
                    trackChunks[i].ChunkType = reader.ReadBytes(4);
                    // お前は本当にトラックチャンクか？
                    if (
                        trackChunks[i].ChunkType[0] != 'M' ||
                        trackChunks[i].ChunkType[1] != 'T' ||
                        trackChunks[i].ChunkType[2] != 'r' ||
                        trackChunks[i].ChunkType[3] != 'k')
                    {
                        throw new FormatException("track chunk != MTrk.");
                    }

                    // 自分のPCがリトルエンディアンなら変換する
                    if (BitConverter.IsLittleEndian)
                    {
                        // トラックのデータ長読み込み(値は6固定)
                        var byteArray = reader.ReadBytes(4);
                        Array.Reverse(byteArray);
                        trackChunks[i].DataLength = BitConverter.ToInt32(byteArray, 0);
                    }
                    else
                    {
                        trackChunks[i].DataLength = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    }

                    // データ部読み込み
                    trackChunks[i].Data = reader.ReadBytes(trackChunks[i].DataLength);

                    // データ部解析
                    TrackDataAnalysis(
                        trackChunks[i].Data,
                        headerChunk.Division,
                        (n, s, b) =>
                        {
                            ret.NoteArray = n;
                            ret.SoflanArray = s;
                            ret.BeatArray = b;
                        },
                        () =>
                        {
                            throw new Exception("track data analysis failure.");
                        });
                }
            }
        }
        catch (Exception e)
        {
            // エラーメッセージ処理
            Debug.LogError("LoadMIDI Error: " + e);
            return null;
        }

        return ret;
    }

    /// <summary>
    /// トラックデータ解析
    /// </summary>
    public void TrackDataAnalysis(byte[] data, short division, Action<NoteData[], SoflanData[], BeatData[]> success, Action failure)
    {
        // リスト初期化
        var noteList = new List<NoteData>();
        var soflanList = new List<SoflanData>();
        var beatList = new List<BeatData>();

        try
        {
            uint deltaTime = 0;                                 // デルタタイム格納用
            uint currentTime = 0;                               // デルタタイムを足していく、つまり現在の時間（ノーツやソフランのイベントタイムはこれを使う）
            byte tmp = 0;                                       // 値保存用
            byte statusByte = 0;                                // ステータスバイト
            byte dataByte0, dataByte1, dataByte2, dataByte3;    // メッセージの後に付いてるデータ

            // データぶん回す
            for (int i = 0; i < data.Length;)
            {
                // メモ データ部は<デルタタイム><イベント>の繰り返しで構成されている

                /* デルタタイム(可変長数値表現) */

                deltaTime = 0;

                // 可変長数値表現をint型に戻す
                while (true)
                {
                    // 無限ループは怖いので例外を書いておく
                    if (i >= data.Length)
                    {
                        throw new Exception("delta time infinity loop.");
                    }
                    tmp = data[i++];

                    // 下位7bitを格納
                    deltaTime |= tmp & (uint)0x7f;

                    // 最上位1bitが0ならデータ終了
                    if ((tmp & 0x80) == 0) break;

                    // 次の下位7bit用に移動
                    deltaTime = deltaTime << 7;
                }
                // 現在の時間にデルタタイムを足す
                currentTime += deltaTime;

                System.Diagnostics.Debug.WriteLine("delta_time:" + deltaTime);
                System.Diagnostics.Debug.WriteLine("current_time:" + currentTime);

                /* ランニングステータス(前回のステータスバイトを使いまわす) */
                if (data[i] < 0x80)
                {
                    // ランニングステータス適応
                    //throw new Exception("not status byte. must be statusByte < 0x80 || statusByte > 0xff" + statusByte);
                    System.Diagnostics.Debug.WriteLine("Running Status.");
                }
                else
                {
                    // ステータスバイト保存
                    statusByte = data[i++];
                }

                /* イベント */

                /* MIDIイベント(ステータスバイト0x80-0xEF) */
                if (statusByte >= 0x80 && statusByte <= 0xef)
                {
                    // MIDIチャンネル番号
                    System.Diagnostics.Debug.WriteLine("channel:" + (statusByte & 0x0f));

                    switch (statusByte & 0xf0)
                    {
                        /* チャンネルメッセージ */

                        case 0x80:  // ノートオフ

                            // どのキーが離されたか
                            dataByte0 = data[i++];
                            // ベロシティ値
                            dataByte1 = data[i++];

                            // ノート情報生成
                            {
                                var note = new NoteData();
                                note.EventTime = (int)currentTime;
                                note.LaneIndex = dataByte0;
                                noteList.Add(note);
                            }

                            System.Diagnostics.Debug.WriteLine("NoteOFF. kk:" + dataByte0 + " vv:" + dataByte1);
                            break;

                        case 0x90:  // ノートオン(ノートオフが呼ばれるまでは押しっぱなし扱い)

                            // どのキーが押されたか
                            dataByte0 = data[i++];
                            // ベロシティ値(鍵盤押す強さ機能がないキーボードは0x40固定) キーボードによってはノートオフメッセージの代わりにここで0を送ってくる
                            dataByte1 = data[i++];

                            // ノート情報生成
                            {
                                var note = new NoteData();
                                note.EventTime = (int)currentTime;
                                note.LaneIndex = dataByte0;
                                note.IsPress = dataByte1 != 0;
                                noteList.Add(note);
                            }

                            System.Diagnostics.Debug.WriteLine("NoteON. kk:" + dataByte0 + " vv:" + dataByte1);
                            break;

                        case 0xa0:  // ポリフォニック キープレッシャー(鍵盤楽器で、キーを押した状態でさらに押し込んだ際に、その圧力に応じて送信される)

                            // どのキーが押されたか
                            dataByte0 = data[i++];
                            // 押されている圧力
                            dataByte1 = data[i++];

                            System.Diagnostics.Debug.WriteLine("Polyphonic Key Pressure. kk:" + dataByte0 + " vv:" + dataByte1);
                            break;

                        case 0xb0:  // コントロールチェンジ(音量、音質など様々な要素を制御するための命令)

                            // コントロールする番号
                            dataByte0 = data[i++];
                            // 設定する値
                            dataByte1 = data[i++];

                            // ※0x00-0x77までがコントロールチェンジで、それ以上はチャンネルモードメッセージとして処理する
                            if (dataByte0 < 0x78)
                            {
                                // コントロールチェンジ
                                System.Diagnostics.Debug.WriteLine("Controller Change. cc:" + dataByte0 + " nn:" + dataByte1);
                            }
                            else
                            {
                                // チャンネルモードメッセージは一律データバイトを2つ使用している

                                // チャンネルモードメッセージ
                                switch (dataByte0)
                                {
                                    case 0x78:  // オールサウンドオフ

                                        // 該当するチャンネルの発音中の音を直ちに消音する。後述のオールノートオフより強制力が強い。
                                        System.Diagnostics.Debug.WriteLine("All Sound Off.");
                                        break;

                                    case 0x79:  // リセットオールコントローラ

                                        // 該当するチャンネルの全種類のコントロール値を初期化する。
                                        System.Diagnostics.Debug.WriteLine("Reset All Controllers.");
                                        break;

                                    case 0x7a:  // ローカルコントロール(ピアノ音ゲーをやる時に電子ピアノに設定するやつ)

                                        // オフ:鍵盤を弾くとMIDIメッセージは送信されるがピアノ自体から音は出ない
                                        // オン:鍵盤を弾くと音源から音が出る(基本こっち)
                                        System.Diagnostics.Debug.WriteLine("Local Control. xx:" + (dataByte1 == 0 ? "Local Off" : "Local On"));
                                        break;

                                    case 0x7b:  // オールノートオフ

                                        // 該当するチャンネルの発音中の音すべてに対してノートオフ命令を出す
                                        System.Diagnostics.Debug.WriteLine("All Notes Off.");
                                        break;

                                    /* MIDIモード設定 */
                                    // オムニのオン・オフとモノ・ポリモードを組み合わせて4種類のモードがある

                                    case 0x7c:  // オムニモードオフ

                                        System.Diagnostics.Debug.WriteLine("Omni Mode Off.");
                                        break;

                                    case 0x7d:  // オムニモードオン

                                        System.Diagnostics.Debug.WriteLine("Omni Mode On.");
                                        break;

                                    case 0x7e:  // モノモードオン

                                        /*
                                            dataByte2
                                            00 = use n...16 
                                            01 = use 1 channel 
                                            0x10 = use 16 channels (provided n=0)
                                         */
                                        System.Diagnostics.Debug.WriteLine("Mono Mode On.");
                                        break;

                                    case 0x7f:  // モノモードオン

                                        System.Diagnostics.Debug.WriteLine("Poly Mode On.");
                                        break;

                                    default:
                                        throw new Exception("channel mode message unintended value.");
                                }
                            }
                            break;

                        case 0xc0:  // プログラムチェンジ(音色を変える命令)

                            // 音色の番号
                            dataByte0 = data[i++];

                            System.Diagnostics.Debug.WriteLine("Program Change. pp:" + dataByte0);
                            break;

                        case 0xd0:  // チャンネルプレッシャー(概ねポリフォニック キープレッシャーと同じだが、違いはそのチャンネルの全ノートナンバーに対して有効となる)

                            // 圧力の値
                            dataByte0 = data[i++];

                            System.Diagnostics.Debug.WriteLine("Channel Key Pressure. ww:" + dataByte0);
                            break;

                        case 0xe0:  // ピッチベンド(ウォェーンウェューンの表現で使う)

                            // Least Signiflcant Byte
                            dataByte0 = data[i++];
                            // Most Signiflcant Byte
                            dataByte1 = data[i++];

                            // メモ 日本語wikiではMSB,LSBの順だと記載されているが、別の英語のサイトではLSB,MSBの順と記載されており正解が分からない。試す機会があったら確認
                            System.Diagnostics.Debug.WriteLine("Pitch Bend. LSB:" + dataByte0 + " MSB:" + dataByte1);
                            break;

                        default:
                            throw new Exception("channel voice message unintended value.");
                    }
                }

                /* システムエクスクルーシブ (SysEx) イベント　よくわからんから適当にあしらう */
                else if(statusByte == 0x70 || statusByte == 0x7f)
                {
                    // データ長
                    byte dataLength = data[i++];
                    // スルーするー
                    i += dataLength;
                }

                /* メタイベント*/
                else if(statusByte == 0xff)
                {
                    tmp = data[i++];
                    byte[] dataByteArray;
                    // データ長
                    byte dataLength = data[i++];

                    switch (tmp)
                    {
                        case 0x00:  // シーケンスメッセージ

                            if (dataLength != 0x02) throw new Exception("Sequence Number not value 0x02.");
                            // 
                            dataByte0 = data[i++];
                            // 
                            dataByte1 = data[i++];

                            System.Diagnostics.Debug.WriteLine("Sequence. Number. ss:" + dataByte0 + " ss:" + dataByte1);
                            break;

                        case 0x01:  // テキストイベント

                            System.Diagnostics.Debug.WriteLine("Text Event.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x02:  // 著作権表示

                            System.Diagnostics.Debug.WriteLine("Copyright Notice.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x03:  // シーケンス/トラック名

                            System.Diagnostics.Debug.WriteLine("Sequence/Track Name.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x04:  // 楽器名

                            System.Diagnostics.Debug.WriteLine("Instrument Name.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x05:  // 歌詞

                            System.Diagnostics.Debug.WriteLine("Lyric.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x06:  // マーカー

                            System.Diagnostics.Debug.WriteLine("Marker.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine(" Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x07:  // キューポイント

                            System.Diagnostics.Debug.WriteLine("Cue Point.");
                            if (dataLength == 0) continue;
                            // テキスト
                            dataByteArray = new byte[dataLength];
                            Array.Copy(data, i, dataByteArray, 0, dataLength);
                            i += dataLength;

                            System.Diagnostics.Debug.WriteLine("Length:" + dataLength + " Text:" + System.Text.Encoding.ASCII.GetString(dataByteArray));
                            break;

                        case 0x20:  // MIDIチャンネルプリフィクス

                            if (dataLength != 0x01) throw new Exception("MIDI Channel Prefix not value 0x01.");
                            // 
                            dataByte0 = data[i++];

                            System.Diagnostics.Debug.WriteLine("MIDI Channel Prefix. cc:" + dataByte0);
                            break;

                        case 0x21:  // MIDIポートプリフィックス

                            if (dataLength != 0x01) throw new Exception("MIDI Port Prefix not value 0x01.");
                            // 
                            dataByte0 = data[i++];

                            System.Diagnostics.Debug.WriteLine("MIDI Port Prefix. cc:" + dataByte0);
                            break;

                        case 0x2f:  // トラック終了

                            if (dataLength != 0x00) throw new Exception("End of Track not value 0x00.");
                            System.Diagnostics.Debug.WriteLine("End of Track.");

                            break;

                        case 0x51:  // テンポ

                            if (dataLength != 0x03) throw new Exception("Set Tempo not value 0x03.");
                            // ４分音符の長さをマイクロ秒単位で格納されている
                            //dataByteArray = new byte[4];
                            //Array.Copy(data, i, dataByteArray, 0, 3);
                            //i += 3;
                            //// byte配列をintに変換
                            //var tempo = BitConverter.ToInt32(dataByteArray, 0);
                            var tempo = 0;
                            tempo |= data[i++];
                            tempo <<= 8;
                            tempo |= data[i++];
                            tempo <<= 8;
                            tempo |= data[i++];

                            {
                                // ソフラン情報生成
                                var soflan = new SoflanData();
                                soflan.EventTime = (int)currentTime;

                                // BPM割り出し
                                soflan.Bpm = 60000000 / (float)tempo;

                                // 小数点第1で切り捨て処理(10にすると第一位、100にすると第2位まで切り捨てられる)
                                soflan.Bpm = Mathf.Floor(soflan.Bpm * 10) / 10;

                                // tick値割り出し
                                soflan.Tick = (60 / soflan.Bpm / division * 1000);

                                // リストにつっこむ
                                soflanList.Add(soflan);
                            }

                            System.Diagnostics.Debug.WriteLine("Set Tempo. tt:" + tempo);
                            break;

                        case 0x54:  // SMTPEオフセット

                            if (dataLength != 0x05) throw new Exception("SMTPE Offset not value 0x05.");
                            // hour
                            dataByte0 = data[i++];
                            // minutes
                            dataByte1 = data[i++];
                            // seconds
                            dataByte2 = data[i++];
                            // frames
                            dataByte3 = data[i++];
                            // fractional frame.
                            tmp = data[i++];

                            System.Diagnostics.Debug.WriteLine("SMTPE Offset. hh:" + dataByte0 + ", mm:" + dataByte1 + ", ss:" + dataByte2 + ", fr:" + dataByte3 + " , ff:" + tmp);
                            break;

                        case 0x58:  // 拍子

                            if (dataLength != 0x04) throw new Exception("Time Signature not value 0x04.");
                            // Time signature numerator
                            dataByte0 = data[i++];
                            // Time signeture denominator expressed as a power of 2
                            dataByte1 = data[i++];
                            // MIDI Clocks per metronome tick
                            dataByte2 = data[i++];
                            // Number of 1/32 notes per 24 MIDI clocks(8 is standard)
                            dataByte3 = data[i++];

                            {
                                // 拍子情報生成
                                var beat = new BeatData();
                                beat.EventTime = (int)currentTime;
                                // 分子
                                beat.Numerator = dataByte0;
                                // 分母
                                beat.Denominator = 1;
                                for (int j = 0; j < dataByte1; j++)
                                    beat.Denominator *= 2;  // 2の累乗

                                // リストにつっこむ
                                beatList.Add(beat);
                            }

                            System.Diagnostics.Debug.WriteLine("Time Signature. nn:" + dataByte0 + ", dd:" + dataByte1 + ", cc:" + dataByte2 + ", bb:" + dataByte3);
                            break;

                        case 0x59:  // 調号

                            if (dataLength != 0x02) throw new Exception("Key Signature not value 0x02.");
                            // number of sharps or flats -7, 0(key of C), +7
                            dataByte0 = data[i++];
                            // 0: major key.    1: minor key
                            dataByte1 = data[i++];

                            System.Diagnostics.Debug.WriteLine("Key Signature. sf:" + dataByte0 + ", mi:" + (dataByte1 == 0 ? "major key" : "minor key"));
                            break;

                        case 0x7f:  // シーケンサ固有メタイベント

                            // length of <id>+<data>
                            dataLength = data[i++];
                            // 1 or 3 bytes repressenting the Manufacture's ID
                            if (dataLength - 8 == 1) dataByte1 = data[i++];
                            else if (dataLength - 8 == 3)
                            {
                                dataByteArray = new byte[3];
                                Array.Copy(data, i, dataByteArray, 0, 3);
                                i += 3;
                            }
                            else throw new Exception("Seuencer-Specific Meta-event length not 1 or 3");
                            // 8-bit binary data
                            dataByteArray = new byte[8];
                            Array.Copy(data, i, dataByteArray, 0, 8);
                            i += 8;

                            System.Diagnostics.Debug.WriteLine("Seuencer-Specific Meta-event");
                            break;

                        default:
                            throw new Exception("meta event unintended value:" + tmp);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // エラーメッセージ処理
            Debug.LogWarning("MIDIDataAnalysisError: " + e);
            failure();
        }

        // テンポを元にイベント時刻を補正
        ModificationEventTimes(noteList, soflanList, beatList);
        success(noteList.ToArray(), soflanList.ToArray(), beatList.ToArray());
    }

    /// <summary>
    /// テンポを元にイベント時刻を補正
    /// </summary>
    void ModificationEventTimes(List<NoteData> noteList, List<SoflanData> soflanList, List<BeatData> beatList)
    {
        var tempNoteList = new List<NoteData>(noteList);
        var tempSoflanList = new List<SoflanData>(soflanList);
        var tempBeatList = new List<BeatData>(beatList);

        // ソフラン座標調整
        for (int i = 1; i < soflanList.Count; i++)
        {
            SoflanData soflan = soflanList[i];

            int timeDifference = tempSoflanList[i].EventTime - tempSoflanList[i - 1].EventTime;
            soflan.EventTime = (int)(timeDifference * soflanList[i - 1].Tick) + soflanList[i - 1].EventTime;

            soflanList[i] = soflan;
        }

        // ノーツ座標調整
        for (int i = 0; i < noteList.Count; i++)
        {
            for (int j = soflanList.Count - 1; j >= 0; j--)
            {
                if (tempNoteList[i].EventTime >= tempSoflanList[j].EventTime)   // >ではなく、>=にしないとソフランがずれる
                {
                    NoteData note = noteList[i];

                    int timeDifference = noteList[i].EventTime - tempSoflanList[j].EventTime;
                    note.EventTime = (int)(timeDifference * tempSoflanList[j].Tick) + soflanList[j].EventTime;   // ソフランのイベント時間+そっからの自分の時間的な何か
                    note.SoflanNo = j;
                    noteList[i] = note;
                    break;
                }
            }
        }

        // 拍子座標調整
        for (int i = 0; i < beatList.Count; i++)
        {
            for (int j = soflanList.Count - 1; j >= 0; j--)
            {
                if (tempBeatList[i].EventTime >= tempSoflanList[j].EventTime)   // >ではなく、>=にしないとソフランがずれる
                {
                    BeatData beat = beatList[i];

                    int timeDifference = beatList[i].EventTime - tempSoflanList[j].EventTime;
                    beat.EventTime = (int)(timeDifference * tempSoflanList[j].Tick) + soflanList[j].EventTime;   // ソフランのイベント時間+そっからの自分の時間的な何か
                    beatList[i] = beat;
                    break;
                }
            }
        }
    }
}
