using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// リプレイに必要なデータ周りの管理クラス(Singleton)
    /// </summary>
    public class DataManager
    {
        /// <summary>
        /// 落としていくノーツのリスト
        /// </summary>
        public List<INote> NoteList;

        /// <summary>
        /// 自身の実体
        /// </summary>
        static DataManager instance;

        /// <summary>
        /// 実体のアクセサ
        /// </summary>
        public static DataManager Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataManager();
                }

                return instance;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataManager()
        {

        }
    }
}
