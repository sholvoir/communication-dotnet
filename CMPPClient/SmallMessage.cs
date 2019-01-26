using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Vultrue.Communication
{
    /// <summary>
    /// 短信
    /// </summary>
    public class SmallMessage
    {
        /// <summary>
        /// 8 byte Unsigned Integer. 信息标识。
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 信息格式:
        ///     0:ASCII串;
        ///     3:短信写卡操作;
        ///     4:二进制信息;
        ///     8:UCS2编码;
        ///     15:含GB汉字
        /// </summary>
        public byte MsgFmt { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 信息长度(MsgFmt值为0时: 小于160个字节; 其它小于等于140个字节),取值大于或等于0。
        /// </summary>
        public byte MsgLength { get; private set; }

        /// <summary>
        /// MsgLength byte Octet String 信息内容。
        /// </summary>
        public string MsgContent { get; set; }
    }
}