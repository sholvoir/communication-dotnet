using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Vultrue.Communication
{
    /// <summary>
    /// GSM 03.40 地址域
    /// </summary>
    public static class GSMAddress
    {
        /// <summary>
        /// 将电话号码转换成GSM地址域
        /// </summary>
        /// <param name="phoneNumber">电话号码</param>
        /// <returns>GSM地址域</returns>
        public static string GetGSMAddress(string phoneNumber)
        {
            if (phoneNumber.Length == 0) return "00";
            Match match = new Regex("^(\\+|(00))86").Match(phoneNumber);
            string phoneNum = phoneNumber.TrimStart('+', '0');
            if (!match.Success) phoneNum = "86" + phoneNum;
            StringBuilder gsmAddress = new StringBuilder(phoneNum.Length.ToString("X2")).Append("91").Append(phoneNum);
            if (phoneNum.Length % 2 != 0) gsmAddress.Append("F");
            for (int i = 4; i < gsmAddress.Length; i+=2)
            {
                char tmp = gsmAddress[i];
                gsmAddress[i] = gsmAddress[i + 1];
                gsmAddress[i + 1] = tmp;
            }
            return gsmAddress.ToString();
        }

        /// <summary>
        /// 将GSM地址域解码为电话号码
        /// </summary>
        /// <param name="gsmAddress">GSM地址域</param>
        /// <returns>电话号码</returns>
        public static string GetPhoneNumber(string gsmAddress)
        {
            int lenth = int.Parse(gsmAddress.Substring(0, 2), NumberStyles.HexNumber);
            int numtype = int.Parse(gsmAddress.Substring(2, 1), NumberStyles.HexNumber);
            StringBuilder phoneNum = new StringBuilder(gsmAddress);
            for (int i = 4; i < phoneNum.Length; i += 2)
            {
                char tmp = phoneNum[i];
                phoneNum[i] = gsmAddress[i + 1];
                phoneNum[i + 1] = tmp;
            }
            StringBuilder phoneNumber = new StringBuilder(phoneNum.ToString(4, lenth));
            if (numtype == 9)
            {
                phoneNumber.Remove(0, 2);
                if (phoneNumber[0] != '1') phoneNumber.Insert(0, '0');
            }
            return phoneNumber.ToString();
        }
    }
}
