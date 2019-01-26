using System;
using System.Globalization;
using System.Text;

namespace Vultrue.Communication
{
    /// <summary>
    /// 带符号BCD串类的处理
    /// </summary>
    public static class SBCD
    {
        /// <summary>
        /// 得到数字
        /// </summary>
        /// <param name="sbcd">SBCD字符串</param>
        /// <returns></returns>
        public static decimal GetDecimal(string sbcd)
        {
            if (sbcd.Length == 0) return 0;
            int initial = int.Parse(sbcd.Substring(0, 1), NumberStyles.HexNumber);
            return initial < 8 ? decimal.Parse(sbcd) :
                decimal.Parse("-" + (initial - 8).ToString() + sbcd.Substring(1, sbcd.Length - 1));
        }

        /// <summary>
        /// 得到数字的整数部分的SBCD字符串
        /// </summary>
        /// <param name="num">数字</param>
        /// <param name="digit">位数</param>
        /// <returns></returns>
        public static string GetSBCD(decimal num, int digit)
        {
            StringBuilder strb = new StringBuilder(Math.Abs(num).ToString("F0"));
            int strlen = strb.Length;
            if (strlen > digit) throw new FormatException("指定的字长太小");
            else for (int i = 0; i < digit - strlen; i++) strb.Insert(0, "0");
            int initial = int.Parse(strb.ToString(0, 1));
            if (initial > 7) throw new FormatException("指定的字长太小");
            else if (num < 0) strb[0] = (initial + 8).ToString("X1")[0];
            return strb.ToString();
        }
    }
}
