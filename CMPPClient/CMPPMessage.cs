using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Vultrue.Communication.CMPPMessages
{
    public enum CMPP_COMMAND_ID : uint
    {
        /// <summary>
        /// 请求连接
        /// </summary>
        CMPP_CONNECT = 0x00000001,

        /// <summary>
        /// 请求连接应答
        /// </summary>
        CMPP_CONNECT_RESP = 0x80000001,

        /// <summary>
        /// 终止连接
        /// </summary>
        CMPP_TERMINATE = 0x00000002,

        /// <summary>
        /// 终止连接应答
        /// </summary>
        CMPP_TERMINATE_RESP = 0x80000002,

        /// <summary>
        /// 提交短信
        /// </summary>
        CMPP_SUBMIT = 0x00000004,

        /// <summary>
        /// 提交短信应答
        /// </summary>
        CMPP_SUBMIT_RESP = 0x80000004,

        /// <summary>
        /// 短信下发
        /// </summary>
        CMPP_DELIVER = 0x00000005,

        /// <summary>
        /// 下发短信应答
        /// </summary>
        CMPP_DELIVER_RESP = 0x80000005,

        /// <summary>
        /// 发送短信状态查询
        /// </summary>
        CMPP_QUERY = 0x0000000,

        /// <summary>
        /// 发送短信状态查询应答
        /// </summary>
        CMPP_QUERY_RESP = 0x80000006,

        /// <summary>
        /// 删除短信
        /// </summary>
        CMPP_CANCEL = 0x00000007,

        /// <summary>
        /// 删除短信应答
        /// </summary>
        CMPP_CANCEL_RESP = 0x80000007,

        /// <summary>
        /// 活动测试
        /// </summary>
        CMPP_ACTIVE_TEST = 0x00000008,

        /// <summary>
        /// 活动测试应答
        /// </summary>
        CMPP_ACTIVE_TEST_RESP = 0x80000008,

        /// <summary>
        /// 消息前转
        /// </summary>
        CMPP_FWD = 0x00000009,

        /// <summary>
        /// 消息前转应答
        /// </summary>
        CMPP_FWD_RESP = 0x80000009,

        /// <summary>
        /// MT路由请求
        /// </summary>
        CMPP_MT_ROUTE = 0x00000010,

        /// <summary>
        /// MT路由请求应答
        /// </summary>
        CMPP_MT_ROUTE_RESP = 0x80000010,

        /// <summary>
        /// MO路由请求
        /// </summary>
        CMPP_MO_ROUTE = 0x00000011,

        /// <summary>
        /// MO路由请求应答
        /// </summary>
        CMPP_MO_ROUTE_RESP = 0x80000011,

        /// <summary>
        /// 获取MT路由请求
        /// </summary>
        CMPP_GET_MT_ROUTE = 0x00000012,

        /// <summary>
        /// 获取MT路由请求应答
        /// </summary>
        CMPP_GET_MT_ROUTE_RESP = 0x80000012,

        /// <summary>
        /// MT路由更新
        /// </summary>
        CMPP_MT_ROUTE_UPDATE = 0x00000013,

        /// <summary>
        /// MT路由更新应答
        /// </summary>
        CMPP_MT_ROUTE_UPDATE_RESP = 0x80000013,

        /// <summary>
        /// MO路由更新
        /// </summary>
        CMPP_MO_ROUTE_UPDATE = 0x00000014,

        /// <summary>
        /// MO路由更新应答
        /// </summary>
        CMPP_MO_ROUTE_UPDATE_RESP = 0x80000014,

        /// <summary>
        /// MT路由更新
        /// </summary>
        CMPP_PUSH_MT_ROUTE_UPDATE = 0x00000015,

        /// <summary>
        /// MT路由更新应答
        /// </summary>
        CMPP_PUSH_MT_ROUTE_UPDATE_RESP = 0x80000015,

        /// <summary>
        /// MO路由更新
        /// </summary>
        CMPP_PUSH_MO_ROUTE_UPDATE = 0x00000016,

        /// <summary>
        /// MO路由更新应答
        /// </summary>
        CMPP_PUSH_MO_ROUTE_UPDATE_RESP = 0x80000016,

        /// <summary>
        /// 获取MO路由请求
        /// </summary>
        CMPP_GET_MO_ROUTE = 0x00000017,

        /// <summary>
        /// 获取MO路由请求应答
        /// </summary>
        CMPP_GET_MO_ROUTE_RESP = 0x80000017
    }
	
	public class MessageHeader
	{
		public const int HEAD_LENTH = 4 + 4 + 4;
		
		public MessageHeader() { }
		
		public MessageHeader(CMPP_COMMAND_ID commandId, uint sequenceId) : this(HEAD_LENTH, commandId, sequenceId) { }
		
		public MessageHeader(uint totalLength, CMPP_COMMAND_ID commandId, uint sequenceId)
		{
			TotalLength = totalLength;
            CommandId = commandId;
            SequenceId = sequenceId;
		}
		
		public MessageHeader(byte[] bytes)
		{
			int i = 0;
            Array.Reverse(bytes, i, 4); //SequenceId 4
            TotalLength = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); //CommandId 4
            CommandId = (CMPP_COMMAND_ID)BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); //SequenceId 4
            SequenceId = BitConverter.ToUInt32(bytes, i);
		}
		
        /// <summary>
        /// 4 byte Unsigned Integer. 消息总长度(含消息头及消息体)
        /// </summary>
        public uint TotalLength { get; set; }

        /// <summary>
        /// 4 byte Unsigned Integer. 命令或响应类型
        /// </summary>
        public CMPP_COMMAND_ID CommandId { get; set; }

        /// <summary>
        /// 4 byte Unsigned Integer. 消息流水号,顺序累加,步长为1,循环使用(一对请求和应答消息的流水号必须相同)
        /// </summary>
        public uint SequenceId { get; set; }
		
		public void CopyToBytes(byte[] bytes, int offset)
        {
            BitConverter.GetBytes(TotalLength).CopyTo(bytes, offset); //TotalLength 4
            Array.Reverse(bytes, offset, 4);
            BitConverter.GetBytes((uint)CommandId).CopyTo(bytes, offset += 4); //CommandId 4
            Array.Reverse(bytes, offset, 4);
            BitConverter.GetBytes(SequenceId).CopyTo(bytes, offset += 4); //SequenceId 4
            Array.Reverse(bytes, offset, 4);
        }
	}

    /// <summary>
    /// 消息基类
    /// </summary>
    public class CMPP_MESSAGE
    {
        public CMPP_MESSAGE(MessageHeader header) { Header = header; }

        public MessageHeader Header { get; set; }

        public virtual byte[] ToBytes()
        {
            byte[] bytes = new byte[MessageHeader.HEAD_LENTH];
            Header.CopyToBytes(bytes, 0);
            return bytes;
        }
    }

    /// <summary>
    /// SP请求连接到ISMG, SP的发送包
    /// </summary>
    public class CMPP_CONNECT : CMPP_MESSAGE
	{
        private const int BODY_LENTH = 6 + 16 + 1 + 4;
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceAddr"></param>
        /// <param name="password"></param>
        /// <param name="timestamp"></param>
        /// <param name="version"></param>
        /// <param name="sequenceId">消息流水号</param>
        public CMPP_CONNECT(string sourceAddr, string password, DateTime timestamp, byte version, uint sequenceId)
            : base (new MessageHeader(MessageHeader.HEAD_LENTH + BODY_LENTH, CMPP_COMMAND_ID.CMPP_CONNECT, sequenceId))
        {
            SourceAddr = sourceAddr;
            string timestr = timestamp.ToString("MMddHHmmss");
            byte[] buffer = new byte[6 + 9 + password.Length + 10];
            int i = 0;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(sourceAddr), 0, buffer, i, sourceAddr.Length > 6 ? 6 : sourceAddr.Length);
            Encoding.ASCII.GetBytes(password).CopyTo(buffer, i += 6 + 9);
			Encoding.ASCII.GetBytes(timestr).CopyTo(buffer, i += password.Length);
            AuthenticatorSource = new MD5CryptoServiceProvider().ComputeHash(buffer);
            Version = version;
            Timestamp = UInt32.Parse(timestr);
        }

        /// <summary>
        /// 6 byte Octet String. 源地址，此处为SP_Id，即SP的企业代码。
        /// </summary>
        public string SourceAddr { get; set; }

        /// <summary>
        /// 16 byte Octet String. 用于鉴别源地址。其值通过单向MD5 hash计算得出,表示如下:
        ///     AuthenticatorSource =
        ///     MD5(sourceAddr + 9字节的0 + shared secret + timestamp)
        ///     Shared secret 由中国移动与源地址实体事先商定,timestamp格式为:MMddHHmmss,即月日时分秒,10位。
        /// </summary>
        public byte[] AuthenticatorSource { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer. 双方协商的版本号(高位4bit表示主版本号,低位4bit表示次版本号),对于3.0的版本,高4bit为3,低4位为0
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// 4 byte Unsigned Integer. 时间戳的明文,由客户端产生,格式为MMddHHmmss,即月日时分秒,10位数字的整型,右对齐 。
        /// </summary>
        public uint Timestamp { get; set; }

        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[Header.TotalLength];
            int i = 0;
            Header.CopyToBytes(bytes, i); // Header 12
            Encoding.ASCII.GetBytes(SourceAddr).CopyTo(bytes, i += MessageHeader.HEAD_LENTH); // SourceAddr 6
            AuthenticatorSource.CopyTo(bytes, i += 6); // AuthenticatorSource 16
            bytes[i += 16] = (byte)Version; //Version 1
            BitConverter.GetBytes(Timestamp).CopyTo(bytes, i += 1); // timestamp 4
            Array.Reverse(bytes, i, 4);
            return bytes;
        }
    }

    /// <summary>
    /// SP请求连接到ISMG, ISMG的回应包
    /// </summary>
    public class CMPP_CONNECT_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 4 + 16 + 1;

        public CMPP_CONNECT_RESP(MessageHeader header, byte[] bytes) : base(header)
        {
            int i = 0;
            Array.Reverse(bytes, i += MessageHeader.HEAD_LENTH, 4); // Status 4
            Status = BitConverter.ToUInt32(bytes, i);
            Buffer.BlockCopy(bytes, i += 4, AuthenticatorISMG = new byte[16], 0, this.AuthenticatorISMG.Length); // AuthenticatorISMG 16
            Version = bytes[i += 16]; // Version 1
        }

        /// <summary>
        /// 4 byte Unsigned Integer 状态
        /// 0:正确 1:消息结构错 2:非法源地址 3:认证错 4:版本太高 5~:其他错误
        /// </summary>
        public uint Status { get; private set; }

        /// <summary>
        /// 16 byte Octet String ISMG认证码,用于鉴别ISMG。
        /// 其值通过单向MD5 hash计算得出,表示如下:
        /// AuthenticatorISMG = MD5(Status + AuthenticatorSource + shared secret),
        /// Shared secret 由中国移动与源地址实体事先商定,
        /// AuthenticatorSource为源地址实体发送给ISMG的对应消息CMPP_Connect中的值。
        /// 认证出错时,此项为空。
        /// </summary>
        public byte[] AuthenticatorISMG { get; private set; }

        /// <summary>
        /// 1 byte Unsigned Integer 服务器支持的最高版本号,对于3.0的版本,高4bit为3,低4位为0
        /// </summary>
        public byte Version { get; private set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// SP或ISMG请求拆除连接, 发送包
    /// </summary>
    public class CMPP_TERMINATE : CMPP_MESSAGE
    {
        public CMPP_TERMINATE(uint sequenceId) : base(new MessageHeader(CMPP_COMMAND_ID.CMPP_TERMINATE, sequenceId)) { }
        public CMPP_TERMINATE(MessageHeader header, byte[] bytes) : base(header) { }
    }

    /// <summary>
    /// SP或ISMG请求拆除连接, 回应包
    /// </summary>
    public class CMPP_TERMINATE_RESP : CMPP_MESSAGE
    {
        public CMPP_TERMINATE_RESP(uint sequenceId) : base(new MessageHeader(CMPP_COMMAND_ID.CMPP_TERMINATE_RESP, sequenceId)) { }
        public CMPP_TERMINATE_RESP(MessageHeader header, byte[] bytes) : base(header) { }
    }

    /// <summary>
    /// SP向ISMG提交短信, SP的发送包
    /// </summary>
    public class CMPP_SUBMIT : CMPP_MESSAGE
    {
        /// <summary>
        /// without DestTerminalId MsgContent
        /// </summary>
        private const int FIXED_BODY_LENGTH = 8 + 1 + 1 + 1 + 1 + 10 + 1 + 32 + 1 + 1 + 1 + 1 + 6
            + 2 + 6 + 17 + 17 + 21 + 1 /*+ 32*DestUsr_tl*/ + 1 + 1 /*+ Msg_length*/ + 20;

        public CMPP_SUBMIT(uint sequenceId) : base(new MessageHeader(CMPP_COMMAND_ID.CMPP_SUBMIT, sequenceId)) { }

        /// <summary>
        /// 8 byte Unsigned Integer. 信息标识。
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 相同MsgId的信息总条数,从1开始。
        /// </summary>
        public byte PkTotal { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 相同MsgId的信息序号,从1开始。
        /// </summary>
        public byte PkNumber { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 是否要求返回状态确认报告: 0:不需要; 1:需要。
        /// </summary>
        public byte RegisteredDelivery { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 信息级别。
        /// </summary>
        public byte MsgLevel { get; set; }

        /// <summary>
        /// 10 byte Octet String 业务标识,是数字、字母和符号的组合。
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 计费用户类型字段:
        ///     0:对目的终端MSISDN计费;
        ///     1:对源终端MSISDN计费;
        ///     2:对SP计费;
        ///     3:表示本字段无效,对谁计费参见FeeTerminalId字段。
        /// </summary>
        public byte FeeUserType { get; set; }

        /// <summary>
        /// 32 byte Octet String 被计费用户的号码,当Fee_UserType为3时该值有效,当Fee_UserType为0、1、2时该值无意义。
        /// </summary>
        public string FeeTerminalId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 被计费用户的号码类型, 0:真实号码; 1:伪码。
        /// </summary>
        public byte FeeTerminalType { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer GSM协议类型。详细是解释请参考GSM03.40中的9.2.3.9。
        /// </summary>
        public byte TPpId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer GSM协议类型。详细是解释请参考GSM03.40中的9.2.3.23,仅使用1位,右对齐。
        /// </summary>
        public byte TPudhi { get; set; }

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
        /// 6 byte Octet String 信息内容来源(SP_Id)。
        /// </summary>
        public string MsgSrc { get; set; }

        /// <summary>
        /// 2 byte Octet String 资费类别:
        ///     "01":对"计费用户号码"免费;
        ///     "02":对"计费用户号码"按条计信息费;
        ///     "03":对"计费用户号码"按包月收取信息费。
        /// </summary>
        public string FeeType { get; set; }

        /// <summary>
        /// 6 byte Octet String 资费代码(以分为单位)。
        /// </summary>
        public string FeeCode { get; set; }

        /// <summary>
        /// 17 byte Octet String 存活有效期,格式遵循SMPP3.3协议。
        /// </summary>
        public string ValIdTime { get; set; }

        /// <summary>
        /// 17 byte Octet String 定时发送时间,格式遵循SMPP3.3协议。
        /// </summary>
        public string AtTime { get; set; }

        /// <summary>
        /// 21 byte Octet String 源号码。
        /// SP的服务代码或前缀为服务代码的长号码, 网关将该号码完整的填到SMPP协议Submit_SM消息相应的source_addr字段,
        /// 该号码最终在用户手机上显示为短消息的主叫号码。
        /// </summary>
        public string SrcId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 接收信息的用户数量(小于100个用户)。
        /// </summary>
        public byte DestUsrTl { get; private set; }

        /// <summary>
        /// 32*DestUsrTl byte Octet String 接收短信的MSISDN号码。
        /// </summary>
        public string[] DestTerminalId { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 接收短信的用户的号码类型,0:真实号码;1:伪码。
        /// </summary>
        public byte DestTerminalType { get; set; }

        /// <summary>
        /// 1 byte Unsigned Integer 信息长度(MsgFmt值为0时: 小于160个字节; 其它小于等于140个字节),取值大于或等于0。
        /// </summary>
        public byte MsgLength { get; private set; }

        /// <summary>
        /// MsgLength byte Octet String 信息内容。
        /// </summary>
        public string MsgContent { get; set; }

        /// <summary>
        /// 20 Octet String 点播业务使用的LinkID, 非点播类业务的MT流程不使用该字段。
        /// </summary>
        public string LinkId { get; set; }

        public override byte[] ToBytes()
        {
            // MsgContent
            byte[] content;
            switch (this.MsgFmt)
            {
                case 4: // 二进制信息
                    content = ByteString.GetBytes(MsgContent);
                    break;
                case 8: // Unicode
                    content = Encoding.BigEndianUnicode.GetBytes(MsgContent);
                    break;
                case 15: // GB2312
                    content = Encoding.GetEncoding("gb2312").GetBytes(MsgContent);
                    break;
                case 0: // ascii
                case 3: // 短信写卡操作
                default:
                    content = Encoding.ASCII.GetBytes(MsgContent);
                    break;
            }
			// MsgLength
            MsgLength = (byte)content.Length;
			// DestUsrTl
			DestUsrTl = (byte)DestTerminalId.Length;
			if (DestUsrTl > 99) DestUsrTl = 99;

			// 开始合成
            Header.TotalLength = (uint)(MessageHeader.HEAD_LENTH + FIXED_BODY_LENGTH + 32 * DestUsrTl + MsgLength);
            byte[] bytes = new byte[Header.TotalLength];
            int i = 0;
            Header.CopyToBytes(bytes, i); // Header 12
            BitConverter.GetBytes(MsgId).CopyTo(bytes, i += MessageHeader.HEAD_LENTH); // MsgId 8 [12,19]
            Array.Reverse(bytes, i, 8);
            bytes[i += 8] = PkTotal; // PkTotal 1 [20,20]
            bytes[++i] = PkNumber; // PkNumber 1 [21,21]
            bytes[++i] = RegisteredDelivery; // RegisteredDelivery 1 [22,22]
            bytes[++i] = MsgLevel; // MsgLevel 1 [23,23]
            Encoding.ASCII.GetBytes(ServiceId).CopyTo(bytes, ++i); // ServiceId 10 [24,33]
            bytes[i += 10] = FeeUserType; // FeeUserType 1 [34,34]
            Encoding.ASCII.GetBytes(FeeTerminalId).CopyTo(bytes, ++i);// FeeTerminalId 32 [35,66]
            bytes[i += 32] = FeeTerminalType; // FeeTerminalType 1 [67,67]
            bytes[++i] = TPpId; // TPpId 1 [68,68]
            bytes[++i] = TPudhi; // TPudhi 1 [69,69]
            bytes[++i] = MsgFmt; // MsgFmt 1 [70,70]
            Encoding.ASCII.GetBytes(MsgSrc).CopyTo(bytes, ++i); // MsgSrc 6 [71,76]
            Encoding.ASCII.GetBytes(FeeType).CopyTo(bytes, i += 6); // FeeType 2 [77,78]
            Encoding.ASCII.GetBytes(FeeCode).CopyTo(bytes, i += 2); // FeeCode 6 [79,84]
			Encoding.ASCII.GetBytes(ValIdTime).CopyTo(bytes, i += 6); // ValIdTime 17 [85,101]
            Encoding.ASCII.GetBytes(AtTime).CopyTo(bytes, i += 17); // AtTime 17 [102,118]
            Encoding.ASCII.GetBytes(SrcId).CopyTo(bytes, i += 17); // SrcId 21 [119,139]
            bytes[i += 21] = DestUsrTl; // DestUsrTl 1 [140,140]
            i++; // DestTerminalId
			for (int j = 0; j < DestUsrTl; j++, i += 32)
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(DestTerminalId[j]), 0, bytes, i, DestTerminalId[j].Length);
            bytes[i] = DestTerminalType; // DestTerminalType 1
            bytes[++i] = MsgLength; // MsgLength 1
            content.CopyTo(bytes, ++i); // MsgContent
            Encoding.ASCII.GetBytes(LinkId).CopyTo(bytes, i += content.Length); // LinkID 20
			// 合成结束
            return bytes;
        }
    }
	
	/// <summary>
	/// SP向ISMG提交短信, ISMG的回应包
	/// </summary>
    public class CMPP_SUBMIT_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 8 + 4;

        public CMPP_SUBMIT_RESP(MessageHeader header, byte[] bytes) : base(header)
        {
            int i = 0;
            Array.Reverse(bytes, i += MessageHeader.HEAD_LENTH, 8); // MsgId
            MsgId = BitConverter.ToUInt64(bytes, i);
            Array.Reverse(bytes, i += 8, 4); // Result
            Result = BitConverter.ToUInt32(bytes, i);
        }

        public ulong MsgId { get; private set; }

        public uint Result { get; private set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
	
	/// <summary>
	/// SP向ISMG查询发送短信状态, SP的发送包
	/// </summary>
    public class CMPP_QUERY : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 8 + 1 + 10 + 8;

        public CMPP_QUERY(DateTime time, byte queryType, string queryCode, string reserve, uint sequenceId)
            : base(new MessageHeader(MessageHeader.HEAD_LENTH + BODY_LENTH, CMPP_COMMAND_ID.CMPP_QUERY, sequenceId))
        {
			Time = time.ToString("yyyyMMdd");
            QueryType = queryType;
            QueryCode = queryCode;
            Reserve = reserve;
        }

		/// <summary>
		/// 8 byte Octet String 时间YYYYMMDD(精确至日)。
		/// </summary>
        public string Time { get; private set; }
		
		/// <summary>
		/// 1 byte Unsigned Integer 查询类别: 0:总数查询; 1:按业务类型查询。
		/// </summary>
        public byte QueryType { get; private set; }
		
		/// <summary>
		/// 10 byte Octet String 查询码, 当QueryType为0时, 此项无效; 当QueryType为1时, 此项填写业务类型Service_Id。
		/// </summary>
        public string QueryCode { get; private set; }

		/// <summary>
		/// 8 byte Octet String 保留。
		/// </summary>
        public string Reserve { get; private set; }

        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[Header.TotalLength];
            int i = 0;
			Header.CopyToBytes(bytes, i); // Header 12
            Encoding.ASCII.GetBytes(Time).CopyTo(bytes, i += MessageHeader.HEAD_LENTH); // Time 8
            bytes[i += 8] = QueryType; // QueryType 1
			Encoding.ASCII.GetBytes(QueryCode).CopyTo(bytes, ++i); // QueryCode 10
            Encoding.ASCII.GetBytes(Reserve).CopyTo(bytes, i += 10); // Reserve 8
            return bytes;
        }
    }
	
	/// <summary>
	/// SP向ISMG查询发送短信状态, ISMG的回应包
	/// </summary>
    public class CMPP_QUERY_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 8 + 1 + 10 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4;

        public CMPP_QUERY_RESP(MessageHeader header, byte[] bytes) : base(header)
        {
            int i = 0;
            Time = Encoding.ASCII.GetString(bytes, i += MessageHeader.HEAD_LENTH, 8); // Time 8
            QueryType = bytes[i += 8]; // QueryType 1
            QueryCode = Encoding.ASCII.GetString(bytes, ++i, 10); // QueryCode 10
            Array.Reverse(bytes, i += 10, 4); // MtTlMsg 4
            MtTlMsg = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MtTlUsr 4
            MtTlUsr = BitConverter.ToUInt32(bytes, 0);
            Array.Reverse(bytes, i += 4, 4); // MTScs 4
            MtScs = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MtWt 4
            MtWt = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MtFl 4
            MtFl = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MoScs 4
            MoScs = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MoWt 4
            MoWt = BitConverter.ToUInt32(bytes, i);
            Array.Reverse(bytes, i += 4, 4); // MoFl 4
            MoFl = BitConverter.ToUInt32(bytes, i);
        }

		/// <summary>
		/// 8 byte Octet String 时间(精确至日)。
		/// </summary>
        public string Time { get; private set; }
		
		/// <summary>
		/// 1 byte Unsigned Integer 查询类别: 0:总数查询; 1:按业务类型查询。
		/// </summary>
        public byte QueryType { get; private set; }
		
		/// <summary>
		/// 10 byte Octet String 查询码。
		/// </summary>
        public string QueryCode { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 从SP接收信息总数。
		/// </summary>
        public uint MtTlMsg { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 从SP接收用户总数。
		/// </summary>
        public uint MtTlUsr { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 成功转发数量。
		/// </summary>
        public uint MtScs { get; private set; }

		/// <summary>
		/// 4 byte Unsigned Integer 待转发数量。
		/// </summary>
        public uint MtWt { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 转发失败数量。
		/// </summary>
        public uint MtFl { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 向SP成功送达数量。
		/// </summary>
        public uint MoScs { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 向SP待送达数量。
		/// </summary>
        public uint MoWt { get; private set; }
		
		/// <summary>
		/// 4 byte Unsigned Integer 向SP送达失败数量。
		/// </summary>
        public uint MoFl { get; private set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ISMG向SP送交短信, ISMG的发送包
    /// </summary>
    public class CMPP_DELIVER : CMPP_MESSAGE
    {
        private const int FIXED_BODY_LENTH = 8 + 21 + 10 + 1 + 1 + 1 + 32 + 1 + 1 + 1 + 20;

        public CMPP_DELIVER(MessageHeader header, byte[] bytes) : base(header)
        {
            int i = 0;
            Array.Reverse(bytes, i += MessageHeader.HEAD_LENTH, 8); // MsgId 8
            MsgId = BitConverter.ToUInt64(bytes, i);
            DestId = Encoding.ASCII.GetString(bytes, i += 8, 21); // DestId 21
            ServiceId = Encoding.ASCII.GetString(bytes, i += 21, 10); // ServiceId 10
            TpPid = bytes[i += 10]; // TpPid 1
            TpUdhi = bytes[++i]; // TpUdhi 1
            MsgFmt = bytes[++i]; // MsgFmt 1
            SrcTerminalId = Encoding.ASCII.GetString(bytes, ++i, 32); // SrcTerminalId 32
            SrcTerminalType = bytes[i += 32]; // SrcTerminalType 1
            RegisteredDelivery = bytes[++i]; // RegisteredDelivery 1
            MsgLenth = bytes[++i]; // MsgLenth 1
            i++; // MsgContent
            switch (MsgFmt)
            {
                case 4: // 二进制信息
                    MsgContent = ByteString.GetByteString(bytes, i, MsgLenth);
                    break;
                case 8: // Unicode
                    this.MsgContent = Encoding.BigEndianUnicode.GetString(bytes, i, MsgLenth).Trim();
                    break;
                case 15: //gb2312
                    this.MsgContent = Encoding.GetEncoding("gb2312").GetString(bytes, i, MsgLenth).Trim();
                    break;
                case 0: //ascii
                case 3: //短信写卡操作
                default:
                    this.MsgContent = Encoding.ASCII.GetString(bytes, i, MsgLenth).ToString();
                    break;
            }
            LinkId = Encoding.ASCII.GetString(bytes, i += MsgLenth, 20); // Linkid 20
        }

        /// <summary>
        /// 8 Unsigned Integer 信息标识。
        ///     生成算法如下:
        ///     采用64位(8字节)的整数:
        ///     (1)????????? 时间(格式为MMDDHHMMSS,即月日时分秒):bit64~bit39,其中
        ///     bit64~bit61:月份的二进制表示;
        ///     bit60~bit56:日的二进制表示;
        ///     bit55~bit51:小时的二进制表示;
        ///     bit50~bit45:分的二进制表示;
        ///     bit44~bit39:秒的二进制表示;
        ///     (2)????????? 短信网关代码:bit38~bit17,把短信网关的代码转换为整数填写到该字段中;
        ///     (3)????????? 序列号:bit16~bit1,顺序增加,步长为1,循环使用。
        ///     各部分如不能填满,左补零,右对齐。
        /// </summary>
        public ulong MsgId { get; private set; }

        /// <summary>
        /// 21 Octet String 目的号码。
        /// SP的服务代码,一般4--6位,或者是前缀为服务代码的长号码;该号码是手机用户短消息的被叫号码。
        /// </summary>
        public string DestId { get; private set; }

        /// <summary>
        /// 10 Octet String 业务标识,是数字、字母和符号的组合。
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer GSM协议类型。详细解释请参考GSM03.40中的9.2.3.9。
        /// </summary>
        public byte TpPid { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer GSM协议类型。详细解释请参考GSM03.40中的9.2.3.23,仅使用1位,右对齐。
        /// </summary>
        public byte TpUdhi { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer 信息格式: 0:ASCII串; 3:短信写卡操作; 4:二进制信息; 8:UCS2编码; 15:含GB汉字。
        /// </summary>
        public byte MsgFmt { get; private set; }

        /// <summary>
        /// 32 Octet String 源终端MSISDN号码(状态报告时填为CMPP_SUBMIT消息的目的终端号码)。
        /// </summary>
        public string SrcTerminalId { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer 源终端号码类型,0:真实号码;1:伪码。
        /// </summary>
        public byte SrcTerminalType { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer 是否为状态报告: 0:非状态报告; 1:状态报告。
        /// </summary>
        public byte RegisteredDelivery { get; private set; }

        /// <summary>
        /// 1 Unsigned Integer 消息长度,取值大于或等于0。
        /// </summary>
        public byte MsgLenth { get; private set; }

        /// <summary>
        /// MsgLenth byte Octet String 消息内容。
        /// </summary>
        public string MsgContent { get; private set; }

        /// <summary>
        /// 20 Octet String 点播业务使用的LinkID,非点播类业务的MT流程不使用该字段。
        /// </summary>
        public string LinkId { get; private set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ISMG向SP送交短信, SP的回应包
    /// </summary>
    public class CMPP_DELIVER_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 8 + 4;

        public CMPP_DELIVER_RESP(ulong msgId, uint result, uint sequenceId)
            : base(new MessageHeader(MessageHeader.HEAD_LENTH + BODY_LENTH, CMPP_COMMAND_ID.CMPP_DELIVER_RESP, sequenceId))
        {
            MsgId = msgId;
            Result = result;
        }
        
        public ulong MsgId { get; set; }
        public uint Result { get; set; }

        public override byte[] ToBytes()
        {
            int i = 0;
            byte[] bytes = new byte[Header.TotalLength];
            Header.CopyToBytes(bytes, i);
            BitConverter.GetBytes(MsgId).CopyTo(bytes, i += MessageHeader.HEAD_LENTH); // MsgId 8
            Array.Reverse(bytes, i, 8);
            BitConverter.GetBytes(Result).CopyTo(bytes, i += 8); // Result 4
            Array.Reverse(bytes, i, 4);
            return bytes;
        }
    }

    /// <summary>
    /// 状态报告
    /// </summary>
    public class CMPP_MSG_CONTENT : CMPP_MESSAGE
    {
        private const int BodyLength = 8 + 7 + 10 + 10 + 32 + 4;

        public CMPP_MSG_CONTENT(MessageHeader header, byte[] bytes)
            : base(header)
        {
            if (bytes.Length != BodyLength) throw new ArgumentException("长度错误", "bytes");
            int i = 0;
            //_Msg_Id 8
            byte[] buffer = new byte[8];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            Array.Reverse(buffer);
            this.MsgId = BitConverter.ToUInt32(buffer, 0);

            //_Stat 7
            i += 8;
            buffer = new byte[7];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            this.Stat = Encoding.ASCII.GetString(buffer);

            //_Submit_time 10
            i += 7;
            buffer = new byte[10];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            this.SubmitTime = Encoding.ASCII.GetString(buffer);

            //_Done_time 10
            i += 10;
            buffer = new byte[10];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            this.SubmitTime = Encoding.ASCII.GetString(buffer);

            //Dest_terminal_Id 32
            i += 10;
            buffer = new byte[32];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            this.DestTerminalId = Encoding.ASCII.GetString(buffer);

            //SMSC_sequence 4
            i += 32;
            buffer = new byte[4];
            Buffer.BlockCopy(bytes, i, buffer, 0, buffer.Length);
            Array.Reverse(buffer);
            this.SMSCSequence = BitConverter.ToUInt32(buffer, 0);
        }

        /// <summary>
        /// 8 byte Unsigned Integer 信息标识。SP提交短信(CMPP_SUBMIT)操作时,与SP相连的ISMG产生的Msg_Id。
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        ///  7 byte Octet String 发送短信的应答结果,含义详见表一。SP根据该字段确定CMPP_SUBMIT消息的处理状态。
        /// </summary>
        public string Stat { get; set; }

        /// <summary>
        /// 10 byte Octet String YYMMDDHHMM(YY为年的后两位00-99,MM:01-12,DD:01-31,HH:00-23,MM:00-59)。
        /// </summary>
        public string SubmitTime { get; set; }

        /// <summary>
        /// 10 byte Octet String YYMMDDHHMM。
        /// </summary>
        public string DoneTime { get; set; }

        /// <summary>
        /// 32 byte Octet String 目的终端MSISDN号码(SP发送CMPP_SUBMIT消息的目标终端)。
        /// </summary>
        public string DestTerminalId { get; set; }

        /// <summary>
        /// 4 byte Unsigned Integer 取自SMSC发送状态报告的消息体中的消息标识。
        /// </summary>
        public uint SMSCSequence { get; set; }
    }

    /// <summary>
    /// SP向ISMG发起删除短信, SP的发送包
    /// </summary>
    public class CMPP_CANCEL : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 8;

        public CMPP_CANCEL(ulong msgId, uint sequenceId)
            : base(new MessageHeader(BODY_LENTH + MessageHeader.HEAD_LENTH, CMPP_COMMAND_ID.CMPP_CANCEL, sequenceId))
        {
            MsgId = msgId;
        }

        public ulong MsgId { get; private set; }

        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[BODY_LENTH + MessageHeader.HEAD_LENTH];
            int i = 0;
            Header.CopyToBytes(bytes, i);
            BitConverter.GetBytes(MsgId).CopyTo(bytes, i += MessageHeader.HEAD_LENTH);
            Array.Reverse(bytes, i, 8);
            return bytes;
        }
    }

    /// <summary>
    /// SP向ISMG发起删除短信, ISMG的回应包
    /// </summary>
    public class CMPP_CANCEL_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 4;

        public CMPP_CANCEL_RESP(MessageHeader header, byte[] bytes) : base(header)
        {
            Array.Reverse(bytes, MessageHeader.HEAD_LENTH, 4);
            SuccessID = BitConverter.ToUInt32(bytes, MessageHeader.HEAD_LENTH);
        }

        public uint SuccessID { get; private set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 链路检测 发送包
    /// </summary>
    public class CMPP_ACTIVE_TEST : CMPP_MESSAGE
    {
        public CMPP_ACTIVE_TEST(uint sequenceId) : base(new MessageHeader(CMPP_COMMAND_ID.CMPP_ACTIVE_TEST, sequenceId)) { }
        public CMPP_ACTIVE_TEST(MessageHeader header, byte[] bytes) : base(header) { }
    }

    /// <summary>
    /// 链路检测 回应包
    /// </summary>
    public class CMPP_ACTIVE_TEST_RESP : CMPP_MESSAGE
    {
        private const int BODY_LENTH = 1;

        public CMPP_ACTIVE_TEST_RESP(uint sequenceId, byte reserved)
            : base(new MessageHeader(BODY_LENTH + MessageHeader.HEAD_LENTH, CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP, sequenceId))
        {
            Reserved = reserved;
        }

        public CMPP_ACTIVE_TEST_RESP(MessageHeader header, byte[] bytes) : base(header)
        {
            Reserved = bytes[MessageHeader.HEAD_LENTH];
        }

        public byte Reserved { get; private set; }

        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[Header.TotalLength];
            Header.CopyToBytes(bytes, 0);
            bytes[MessageHeader.HEAD_LENTH] = Reserved;
            return bytes;
        }
    }

}
