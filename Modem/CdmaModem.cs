using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Vultrue.Communication
{
    /// <summary>
    /// CdmaModem
    /// </summary>
    public class CdmaModem : Modem
    {
        #region SHORT MESSAGES COMMANDS

        /// <summary>
        /// 获取短信存储空间列表
        /// </summary>
        /// <returns>Item1:读取,删除操作的可选目标空间; Item2:写入,发送的可选目标空间; Item3:收到短信的可选目标空间</returns>
        public Tuple<string[], string[], string[]> GetMessageStorageList()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPMS=?\r", "^\\+CPMS:\\s*\\((?<rld>[\",\\w]+)\\),\\((?<ws>[\",\\w]+)\\),\\((?<rv>[\",\\w]+)\\)")[0];
                return new Tuple<string[], string[], string[]>(match.Result("${rld}").Replace("\"", "").Split(','),
                    match.Result("${ws}").Replace("\"", "").Split(','), match.Result("${rv}").Replace("\"", "").Split(','));
            }
        }

        /// <summary>
        /// 获取CdmaModem默认的短信存储空间
        /// </summary>
        /// <returns>[0]:读取,删除操作目标; [1]写入,发送操作目标; [2]收到短信目标
        /// Item1:目标存储空间; Item2:已使用的短信条数; Item3:总存储的空间大小</returns>
        public Tuple<string, int, int>[] GetPreferredMessageStorage()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPMS?\r",
                    "^\\+CPMS:\\s*\"(?<rld>\\w+)\",(?<rldu>\\d+),(?<rldt>\\d+),\"(?<ws>\\w+)\",(?<wsu>\\d+),(?<wst>\\d+),\"(?<rv>\\w+)\",(?<rvu>\\d+),(?<rvt>\\d+)")[0];
                return new Tuple<string, int, int>[]{
                    new Tuple<string,int,int>(match.Result("${rld}"), int.Parse(match.Result("${rldu}")), int.Parse(match.Result("${rldt}"))),
                    new Tuple<string,int,int>(match.Result("${ws}"), int.Parse(match.Result("${wsu}")), int.Parse(match.Result("${wst}"))),
                    new Tuple<string,int,int>(match.Result("${rv}"), int.Parse(match.Result("${rvu}")), int.Parse(match.Result("${rvt}")))
                };
            }
        }

        /// <summary>
        /// 设置CdmaModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,删除操作目标存储空间</param>
        /// <returns>[0]:读取,删除操作目标; [1]写入,发送操作目标; [2]收到短信目标
        /// Item1:已使用的短信条数; Item2:总存储的空间大小</returns>
        public Tuple<int, int>[] SetPreferredMessageStorage(string rldStorage)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPMS=\"{0}\"\r", rldStorage),
                   "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+),(?<rvu>\\d+),(?<rvt>\\d+)")[0];
                return new Tuple<int, int>[] {
                    new Tuple<int, int>(int.Parse(match.Result("${rldu}")), int.Parse(match.Result("${rldt}"))),
                    new Tuple<int, int>(int.Parse(match.Result("${wsu}")), int.Parse(match.Result("${wst}"))),
                    new Tuple<int, int>(int.Parse(match.Result("${rvu}")), int.Parse(match.Result("${rvt}")))
                };
            }
        }

        /// <summary>
        /// 设置CdmaModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="wsStorage">写入与发送操作目标存储空间</param>
        /// <returns>[0]:读取,删除操作目标; [1]写入,发送操作目标; [2]收到短信目标
        /// Item1:已使用的短信条数; Item2:总存储的空间大小</returns>
        public Tuple<int, int>[] SetPreferredMessageStorage(string rldStorage, string wsStorage)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPMS=\"{0}\",\"{1}\"\r", rldStorage, wsStorage),
                    "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+),(?<rvu>\\d+),(?<rvt>\\d+)")[0];
                return new Tuple<int, int>[] {
                    new Tuple<int, int>(int.Parse(match.Result("${rldu}")), int.Parse(match.Result("${rldt}"))),
                    new Tuple<int, int>(int.Parse(match.Result("${wsu}")), int.Parse(match.Result("${wst}"))),
                    new Tuple<int, int>(int.Parse(match.Result("${rvu}")), int.Parse(match.Result("${rvt}")))
                };
            }
        }

        #endregion
    }

    /// <summary>
    /// ACK
    /// </summary>
    public enum MessageACK
    {
        /// <summary>
        /// 
        /// </summary>
        NoNeed = 0,
        /// <summary>
        /// 
        /// </summary>
        Need = 1
    }

    /// <summary>
    /// MessagePriority
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>
        /// 
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 
        /// </summary>
        Interactive = 1,
        /// <summary>
        /// 
        /// </summary>
        Urgent = 2,
        /// <summary>
        /// 
        /// </summary>
        Emergency = 3
    }

    /// <summary>
    /// MessageFormat
    /// </summary>
    public enum MessageFormat
    {
        /// <summary>
        /// 
        /// </summary>
        GSM7BIT = 0,
        /// <summary>
        /// 
        /// </summary>
        ASCII = 1,
        /// <summary>
        /// 
        /// </summary>
        IA5 = 2,
        /// <summary>
        /// 
        /// </summary>
        OCTET = 3,
        /// <summary>
        /// 
        /// </summary>
        LATIN = 4,
        /// <summary>
        /// 
        /// </summary>
        LATIN_HEBREW = 5,
        /// <summary>
        /// 
        /// </summary>
        UNICODE = 6
    }

    /// <summary>
    /// MessagePreserve
    /// </summary>
    public enum MessagePreserve
    {
        /// <summary>
        /// 
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 
        /// </summary>
        Restricted = 1,
        /// <summary>
        /// 
        /// </summary>
        Confidential = 2,
        /// <summary>
        /// 
        /// </summary>
        Secret = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MessageParameter
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageACK ACK;
        /// <summary>
        /// 
        /// </summary>
        public MessagePriority Priority;
        /// <summary>
        /// 
        /// </summary>
        public MessageFormat Format;
        /// <summary>
        /// 
        /// </summary>
        public MessagePreserve Preserve;
    }
}
