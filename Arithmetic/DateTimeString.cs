using System;

namespace Vultrue.Communication
{
    /// <summary>
    /// 用来处理 "yyyyMMddHHmmss" 格式的时间字符串的静态类
    /// </summary>
    public static class DateTimeString
    {
        /// <summary>
        /// 得到时间字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static string GetDateTimeString(DateTime time)
        {
            return time.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// 得到时间
        /// </summary>
        /// <param name="str">时间字符串</param>
        /// <returns></returns>
        public static DateTime GetDateTime(string str)
        {
            return DateTime.Parse(string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                str.Substring(0, 4),
                str.Substring(4, 2),
                str.Substring(6, 2),
                str.Substring(8, 2),
                str.Substring(10, 2),
                str.Substring(12, 2)));
        }
    }
}
