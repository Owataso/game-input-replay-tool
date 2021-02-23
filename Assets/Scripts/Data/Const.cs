using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// どこからでも使いそうな定数系を雑に置いとく
    /// </summary>
    public static class Const
    {
        /// <summary>
        /// レーン番号とボタン種別のマッピング
        /// 左からアサインしていく
        /// </summary>
        public static readonly Dictionary<int, BUttonType> LaneIndexToButtonMap = new Dictionary<int, BUttonType>()
        {
            {  0, BUttonType.L2 },
            {  1, BUttonType.L1 },
            {  2, BUttonType.Left },
            {  3, BUttonType.Down },
            {  4, BUttonType.Up },
            {  5, BUttonType.Right },
            {  6, BUttonType.Share },
            {  7, BUttonType.L3 },
            {  8, BUttonType.PsButton },
            {  9, BUttonType.R3 },
            { 10, BUttonType.Option },
            { 11, BUttonType.Square },
            { 12, BUttonType.Cross },
            { 13, BUttonType.Triangle },
            { 14, BUttonType.Circle },
            { 15, BUttonType.R1 },
            { 16, BUttonType.R2 }
        };
    }
}
