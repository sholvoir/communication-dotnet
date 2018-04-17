using System.Globalization;
using System.Text;

namespace Vultrue.Communication
{
    /// <summary>
    /// Pdu串编解码静态类
    /// </summary>
    public static class PduString
    {
        /// <summary>
        /// 将Unicode字符串编码为PDU串
        /// </summary>
        /// <param name="unicode">Unicode字符串</param>
        /// <returns>PDU串</returns>
        public static string GetPdustr(string unicode)
        {
            StringBuilder pdu = new StringBuilder();
            byte[] bytes = Encoding.Unicode.GetBytes(unicode);
            for (int i = 0; i < bytes.Length; i += 2)
            {
                pdu.Append(bytes[i + 1].ToString("X2"));
                pdu.Append(bytes[i].ToString("X2"));
            }
            return pdu.ToString();
        }

        /// <summary>
        /// 将PDU串解码为Unicode字符串
        /// </summary>
        /// <param name="pdustr"></param>
        /// <returns></returns>
        public static string GetUnicode(string pdustr)
        {
            byte[] bytes = new byte[pdustr.Length / 2];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                bytes[i] = byte.Parse(pdustr.Substring(2 * i + 2, 2), NumberStyles.HexNumber);
                bytes[i + 1] = byte.Parse(pdustr.Substring(2 * i, 2), NumberStyles.HexNumber);
            }
            return Encoding.Unicode.GetString(bytes);
        }
    }
}
