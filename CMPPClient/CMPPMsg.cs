using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Vultrue.Communication.CmppMsg
{
    #region 枚举类型

    public enum CMPP_COMMAND_ID : uint
    {
        CMPP_CONNECT = 1,
        CMPP_CONNECT_RESP = 0x80000001,
        CMPP_TERMINATE = 0x00000002,  // 终止连接
        CMPP_TERMINATE_RESP = 0x80000002,  // 终止连接应答
        CMPP_SUBMIT = 0x00000004,   //提交短信
        CMPP_SUBMIT_RESP = 0x80000004,   // 提交短信应答
        CMPP_DELIVER = 0x00000005,   //短信下发
        CMPP_DELIVER_RESP = 0x80000005,   // 下发短信应答
        CMPP_QUERY = 0x00000006, //发送短信状态查询
        CMPP_QUERY_RESP = 0x80000006, // 发送短信状态查询应答
        CMPP_CANCEL = 0x00000007, // 删除短信
        CMPP_CANCEL_RESP = 0x80000007, // 删除短信应答
        CMPP_ACTIVE_TEST = 0x00000008, //激活测试
        CMPP_ACTIVE_TEST_RESP = 0x80000008 // 激活测试应答
    }

    public enum FeeUserType
    {
        FEE_TERMINAL_ID = 0,    //
        FEE_SOURCE_ID = 1,
        FEE_SP = 2,
        FEE_NULL = 3
    }
    public enum Msg_Format
    {
        ASCII = 0,
        WRITECARD = 1,
        BINARY = 2,
        UCS2 = 8,
        GB2312 = 15
    }
    public enum SUBMIT_RESULT
    {
        SUCC = 0,
        MSG_STRUCTURE_ERR = 1,
        COMMANID_ERR = 2,
        MSG_SEQUENCE_ERR = 3,
        MSG_LENGTH_ERR = 4,
        FEE_CODE_ERR = 5,
        OUT_OF_MSG_LEN_ERR = 6,
        SVC_CODE_ERR = 7,
        FLUX_ERR = 8,
        OTHER_ERR = 9
    }

    #endregion

    #region 结构类型

    public struct FeeType
    {
        public static readonly string FEE_TERMINAL_FREE = "01";
        public static readonly string FEE_TERMINAL_PERITEM = "02";
        public static readonly string FEE_TERMINAL_MONTH = "03";
        public static readonly string FEE_TERMINAL_TOP = "04";
        public static readonly string FEE_TERMINAL_SP = "05";
    }

    public struct DELIVER_STATE
    {
        public static readonly string DELIVERED = "DELIVRD";
        public static readonly string EXPIRED = "EXPIRED";
        public static readonly string DELETED = "DELETED";
        public static readonly string UNDELIVERABLE = "UNDELIV";
        public static readonly string ACCEPTED = "ACCEPTD";
        public static readonly string UNKNOWN = "UNKNOWN";
        public static readonly string REJECTED = "REJECTD";
    }
    //*************结构类型结束***********************************

    #endregion

    #region 消息类

    public class CMPP_MSG_Header  //消息头
    {
        private byte[] initValue = new byte[CMPP_MSG_Header.HeaderLength];

        public CMPP_MSG_Header(CMPP_COMMAND_ID Command_ID) //发送前
        {
            BIConvert.Int2Bytes((uint)Command_ID).CopyTo(initValue, 4);
        }

        public CMPP_MSG_Header(byte[] bs) //根据受到的字节进行构造 字节序列
        {
            int l = CMPP_MSG_Header.HeaderLength;
            for (int i = 0; i < l; i++)
            {
                initValue[i] = bs[i];
            }
        }
        public CMPP_MSG_Header(byte[] bs, int baseIndex) //根据受到的字节进行构造 字节序列
        {
            int l = CMPP_MSG_Header.HeaderLength;
            for (int i = 0; i < l; i++)
            {
                initValue[i] = bs[baseIndex + i];
            }
        }



        public uint MSGLength  //获取此消息头代表的消息的整个长度
        {
            get
            {
                return (BIConvert.Bytes2UInt(initValue, 0));
            }
            set
            {
                byte[] t = BIConvert.Int2Bytes(value);
                for (int i = 0; i < 4; i++)
                {
                    initValue[i] = t[i];
                }
            }
        }

        public uint Command_ID
        {
            get
            {
                return (BIConvert.Bytes2UInt(initValue, 4));
            }
            set
            {
                byte[] t = BIConvert.Int2Bytes(value);
                for (int i = 0; i < 4; i++)
                {
                    initValue[i + 4] = t[i];
                }
            }
        }

        public uint SequenceId
        {
            get
            {
                return (BIConvert.Bytes2UInt(initValue, 8));
            }
            set
            {
                byte[] t = BIConvert.Int2Bytes(value);
                for (int i = 0; i < 4; i++)
                {
                    initValue[i + 4 + 4] = t[i];
                }
            }
        }

        public byte[] toBytes()
        {
            return (initValue); //将字段转化为字节
        }

        public void fromBytes(byte[] bs)
        {
            for (int i = 0; i < CMPP_MSG_Header.HeaderLength; i++)
            {
                initValue[i] = bs[i];
            }
        }

        public static int HeaderLength
        {
            get
            {
                return (4 + 4 + 4);
            }
        }
    }

    public class CMPP_MSG_CONNECT
    {
        CMPP_MSG_Header header;
        byte[] initValue;
        byte[] body;
        int baseIndex = CMPP_MSG_Header.HeaderLength; //消息的起始
        byte ver = 0x20;
        byte[] AuthenticatorSource = new Byte[16];   //发送的验证信息
        string _SystemID = "000000";
        string _Password = "00000000";
        string _timestamp = "0000000000";

        public CMPP_MSG_CONNECT(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_CONNECT);
            header.SequenceId = sequence;
            header.MSGLength = (uint)(this.BodyLength + CMPP_MSG_Header.HeaderLength);
            body = new byte[this.BodyLength];
        }

        public CMPP_MSG_CONNECT(byte[] bs)
        {
            initValue = new byte[bs.Length];
            bs.CopyTo(initValue, 0); //进行初始化行为  
            byte[] temp = new byte[CMPP_MSG_Header.HeaderLength];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bs[i];
            }
            byte[] body = new Byte[bs.Length - CMPP_MSG_Header.HeaderLength];
            for (int i = 0; i < body.Length; i++)
            {
                body[i] = bs[CMPP_MSG_Header.HeaderLength + i]; //将消息的字节存储
            }
            header = new CMPP_MSG_Header(temp);  //构造 消息头
            header.MSGLength = (uint)(this.BodyLength + CMPP_MSG_Header.HeaderLength);
            header.Command_ID = (uint)CMPP_COMMAND_ID.CMPP_CONNECT;
        }
        public int BodyLength
        {
            get
            {
                return (6 + 16 + 4 + 1);
            }
        }

        public string SourceAdd
        {
            set
            {
                _SystemID = value;
                byte[] t = Encoding.ASCII.GetBytes(_SystemID); //转换为字节数组
                t.CopyTo(body, 0);
            }
        }


        public string Password
        {
            set
            {
                _Password = value;
            }
        }

        public string Version
        {
            set
            {
                ver = Convert.ToByte("0x" + value, 16);
            }
        }


        private static string getTimestamp() //返回一个时间戳 4 字节
        {
            DateTime msgtime = DateTime.Now;
            string u = msgtime.Month.ToString().PadLeft(2, '0');
            u = u + msgtime.Day.ToString().PadLeft(2, '0');
            u = u + msgtime.Hour.ToString().PadLeft(2, '0');
            u = u + msgtime.Minute.ToString().PadLeft(2, '0');
            u = u + msgtime.Second.ToString().PadLeft(2, '0');
            return (u);
        }
        private byte[] getMd5Code()
        {
            MD5 md5 = new MD5CryptoServiceProvider(); //创建MD5类别
            byte[] buf = new byte[6 + 9 + _Password.Length + 10];
            byte[] s_a = Encoding.ASCII.GetBytes(_SystemID); //Source_ADD 就是企业代码
            byte[] s_0 = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };     //9字节的0,此处当作右补0
            byte[] s_p = Encoding.ASCII.GetBytes(_Password); //密码
            this._timestamp = getTimestamp();    //取得认证码时赋值字符串
            byte[] s_t = Encoding.ASCII.GetBytes(_timestamp); //10位的字符串字节数组
            s_a.CopyTo(buf, 0);    //base 0
            s_0.CopyTo(buf, 6);    //base 6
            s_p.CopyTo(buf, 6 + 9);   //base 6+9
            s_t.CopyTo(buf, 6 + 9 + _Password.Length);  //base 6+9+password.length   

            return (md5.ComputeHash(buf, 0, buf.Length));
        }
        private byte[] getSourceAdd()
        {
            return (Encoding.ASCII.GetBytes(this._SystemID));
        }
        public byte[] ToBytes()   //返回当前对象的字节数组印象
        {
            byte[] reVal = new Byte[CMPP_MSG_Header.HeaderLength + this.BodyLength];
            header.toBytes().CopyTo(reVal, 0);       //消息头
            getSourceAdd().CopyTo(reVal, CMPP_MSG_Header.HeaderLength);  //源地址 企业代码
            getMd5Code().CopyTo(reVal, CMPP_MSG_Header.HeaderLength + 6);   //认证md5
            reVal[CMPP_MSG_Header.HeaderLength + 6 + 16] = this.ver;    //版本字节
            BIConvert.Int2Bytes(Convert.ToUInt32(this._timestamp)).CopyTo(reVal, CMPP_MSG_Header.HeaderLength + 6 + 16 + 1);
            return (reVal);
        }
    }

    public class CMPP_MSG_CONNECT_RESP
    {
        CMPP_MSG_Header header;
        byte Status;
        byte[] AuthenticatorISMG;
        byte _Version;

        public CMPP_MSG_CONNECT_RESP(byte[] bs)
        {
            byte[] temp_head = new Byte[CMPP_MSG_Header.HeaderLength];
            int index = 0;
            for (int i = 0; i < CMPP_MSG_Header.HeaderLength; i++)
            {
                temp_head[i] = bs[index++];
            }
            header = new CMPP_MSG_Header(temp_head);
            Status = bs[index++];      //状态字节
            AuthenticatorISMG = new byte[16];   //回应摘要
            for (int i = 0; i < AuthenticatorISMG.Length; i++)
            {
                AuthenticatorISMG[i] = bs[index++];
            }
            _Version = bs[index++];
        }

        public bool isOk
        {
            get
            {
                return (true);
            }
        }

        public string Ver
        {
            get
            {
                return (Convert.ToString(this._Version, 16));
            }
        }

        public string ISMGREturnAuthCode
        {
            get
            {
                return (Encoding.ASCII.GetString(AuthenticatorISMG));
            }
        }

        public uint Command_ID
        {
            get
            {
                return (header.Command_ID);
            }
            set
            {
                header.Command_ID = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }
    }

    public class CMPP_MSG_TERMINATE
    {
        CMPP_MSG_Header header;
        public CMPP_MSG_TERMINATE(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_TERMINATE);
            header.MSGLength = (uint)CMPP_MSG_Header.HeaderLength;
            header.SequenceId = sequence;
        }
        public CMPP_MSG_TERMINATE(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
        }

        public byte[] toBytes()
        {
            return (header.toBytes());
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }
    }

    public class CMPP_MSG_TERMINATE_RESP
    {
        CMPP_MSG_Header header;
        public CMPP_MSG_TERMINATE_RESP(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
        }
        public CMPP_MSG_TERMINATE_RESP(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_TERMINATE_RESP);
            header.MSGLength = (uint)CMPP_MSG_Header.HeaderLength;
            header.SequenceId = sequence;
        }

        public byte[] toBytes()
        {
            return (header.toBytes());
        }
        public uint Command_ID
        {
            get
            {
                return (header.Command_ID);
            }
            set
            {
                header.Command_ID = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }
    }

    public class CMPP_MSG_SUBMIT
    {
        CMPP_MSG_Header header;
        int _isReportOrSMC = 1;   //是否需要状态报告
        int _msgTotal = 1;    //相同消息的条数
        int _msgNumber = 1;    //
        int _msgLevel = 0;    //消息级别,却胜0
        string _svcCode = ""; //业务类型
        int _feeUserType = 0;    //计费用户类型字段0：对目的终端MSISDN计费；1：对源终端MSISDN计费；2：对SP计费;3：表示本字段无效，对谁计费参见Fee_terminal_Id字段
        string _feeTerminalId = "";  //被计费终端
        int _tpPid = 0;
        int _tpUDHI = 0;
        uint _msgFmt = 0;  //消息格式
        string _msgsrc = "";    //消息来源 即spid
        string _feeType = CmppMsg.FeeType.FEE_TERMINAL_PERITEM;  //
        string _feeCode = "";    //资费
        string _valIdTime = "";   //存活期
        string _atTime = "";    //调度时间
        string _srcId = "";    //源号码，就是在手机上显示的号码
        int _destUsrNum = 0;   //接受消息的手机数
        string[] _destTerminalIds = new string[100]; //至多100个号码
        int _msgLengt = (int)Msg_Format.GB2312;
        string _MsgContent = "";
        UInt64 _MsgID;    //返回的消息ID


        public CMPP_MSG_SUBMIT(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_SUBMIT);
            header.SequenceId = sequence;
        }

        //属性////////////////////////////////////////////////////
        public string SMS_Content
        {
            set
            {
                this._MsgContent = value;
                byte[] t = Encoding.ASCII.GetBytes(value);
                this._msgLengt = t.Length;
            }
        }

        public int SMS_Delivery_Type  //是否要求返回状态确认报告：0：不需要1：需要2：产生SMC话单 （该类型短信仅供网关计费使用，不发送给目的终端)
        {
            set
            {
                _isReportOrSMC = value;
            }
        }

        public int Msg_Level
        {
            set
            {
                this._msgLevel = value;
            }
        }

        public string Svc_Code
        {
            set
            {
                this._svcCode = value;
            }
        }

        public int FeeUserType
        {
            set
            {
                this._feeUserType = value;
            }
        }

        public string FeeTerminalId
        {
            set
            {
                this._feeTerminalId = value;
            }
        }

        public int UDHI
        {
            set
            {
                this._tpUDHI = value;
            }
        }

        public uint MSGFormat
        {
            set
            {
                this._msgFmt = value;
            }
        }

        public string SPID
        {
            set
            {
                this._msgsrc = value;
            }
        }

        public string SrcID   //可以此处确定长号码
        {
            set
            {
                this._srcId = value;
            }
        }

        public string FeeType
        {
            set
            {
                this._feeType = value.PadLeft(2, '0');
            }
        }

        public string FeeCode
        {
            set
            {
                this._feeCode = value;
            }
        }

        public string ValIdTime
        {
            set
            {
                this._valIdTime = value;
            }
        }

        public string AtTime
        {
            set
            {
                this._atTime = value;
            }
        }

        public UInt64 MsgID
        {
            set
            {
                this._MsgID = value;
            }
            get
            {
                return (this._MsgID);
            }
        }
        //属性结束//////////////////////////////////////////////

        public void addTerminalID(string id)
        {
            if (this._destUsrNum < 100)
            {
                this._destTerminalIds[this._destUsrNum++] = id;
            }
        }

        public byte[] toBytes() //返回字节数印象
        {
            byte[] submitData = new byte[400];
            int index = CMPP_MSG_Header.HeaderLength;     //当前包的填充指针
            {//进入填充包的过程
                index = index + 8;      //msgid跳过
                submitData[index++] = (byte)this._msgTotal;
                submitData[index++] = (byte)this._msgNumber;
                submitData[index++] = (byte)this._isReportOrSMC;
                submitData[index++] = (byte)this._msgLevel;
                byte[] svccode = Encoding.ASCII.GetBytes(this._svcCode);
                svccode.CopyTo(submitData, index);   //拷贝到目标
                index = index + 10;   //index增加
                submitData[index++] = (byte)this._feeUserType;
                byte[] feetid = Encoding.ASCII.GetBytes(this._feeTerminalId);
                feetid.CopyTo(submitData, index);
                index = index + 21;
                submitData[index++] = (byte)this._tpPid;
                submitData[index++] = (byte)this._tpUDHI;
                submitData[index++] = (byte)this._msgFmt;
                byte[] spid = Encoding.ASCII.GetBytes(this._msgsrc);
                spid.CopyTo(submitData, index);
                index = index + 6;
                byte[] feetype = Encoding.ASCII.GetBytes(this._feeType);
                feetype.CopyTo(submitData, index);
                index = index + 2;
                byte[] feecode = Encoding.ASCII.GetBytes(this._feeCode);
                feecode.CopyTo(submitData, index);
                index = index + 6;
                //byte[] validtime=Encoding.ASCII.GetBytes(this._valIdTime);
                //validtime.CopyTo (submitData,index);
                index = index + 17;
                //byte[] attime=Encoding.ASCII.GetBytes(this._valIdTime);
                //attime.CopyTo (submitData,index);
                index = index + 17;
                byte[] srcid = Encoding.ASCII.GetBytes(this._srcId);
                srcid.CopyTo(submitData, index);
                index = index + 21;
                submitData[index++] = (byte)this._destUsrNum;
                for (int i = 0; i < this._destUsrNum; i++)
                {
                    byte[] temp = Encoding.ASCII.GetBytes(this._destTerminalIds[i]);
                    temp.CopyTo(submitData, index);
                    index = index + 21;
                }
                submitData[index++] = (byte)this._msgLengt;
                byte[] msg = null;
                switch (this._msgFmt)
                {//根据编码类型确定转换字节
                    case (uint)Msg_Format.ASCII:
                        msg = Encoding.ASCII.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;

                    case (uint)Msg_Format.BINARY:
                        msg = Encoding.ASCII.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;

                    case (uint)Msg_Format.GB2312:
                        msg = Encoding.Default.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;

                    case (uint)Msg_Format.UCS2:
                        msg = Encoding.BigEndianUnicode.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;

                    case (uint)Msg_Format.WRITECARD:   //写卡操作
                        msg = Encoding.ASCII.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;
                    default:
                        msg = Encoding.ASCII.GetBytes(this._MsgContent);
                        msg.CopyTo(submitData, index);
                        submitData[index - 1] = (byte)msg.Length;   //重新设定长度
                        index = index + msg.Length;
                        break;
                }
                index = index + 8;   //8个保留字节
            }
            header.MSGLength = (uint)index;//根据index的长度决定传输数据字节长度
            byte[] reVal = new byte[index];
            header.toBytes().CopyTo(reVal, 0);
            for (int i = CMPP_MSG_Header.HeaderLength; i < reVal.Length; i++)
            {
                reVal[i] = submitData[i];
            }
            return (reVal);
        }
    }

    public class CMPP_MSG_SUBMIT_RESP
    {
        CMPP_MSG_Header header;
        byte[] Msg_Id = new byte[8];
        byte[] initValue;

        public CMPP_MSG_SUBMIT_RESP(byte[] bs)
        {
            initValue = new byte[bs.Length];
            for (int i = 0; i < bs.Length; i++)
            {
                initValue[i] = bs[i];
            }
            init();
        }

        private void init()
        {
            int index = 0;
            byte[] temp = new byte[CMPP_MSG_Header.HeaderLength];
            for (int i = 0; i < CMPP_MSG_Header.HeaderLength; i++)
            {
                temp[i] = initValue[i];
                index = i;
            }
            index += 1;//指到正确位置
            header = new CMPP_MSG_Header(temp);
            for (int i = 0; i < 8; i++)
            {
                Msg_Id[i] = initValue[index + i];
            }
            BIConvert.DumpBytes(Msg_Id, "C:\\Submit_resp_MsgID.txt");
        }

        public UInt64 Msg_ID
        {
            get
            {

                UInt64 t = BitConverter.ToUInt64(this.Msg_Id, 0);
                return (t);
            }
        }

        public string MsgID
        {
            get
            {
                return BitConverter.ToUInt64(this.Msg_Id, 0).ToString();
            }
        }

        public uint Command_ID
        {
            get
            {
                return (header.Command_ID);
            }
            set
            {
                header.Command_ID = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }

        public bool isOK
        {
            get
            {
                byte b = initValue[CMPP_MSG_Header.HeaderLength + 8];
                if ((int)b == 0)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
        }
    }

    public class CMPP_MSG_QUERY
    {
        CMPP_MSG_Header header;
        string _time;
        int _queryType = 0;
        string _queryCode;

        public string Time
        {
            set
            {
                _time = value;
            }
        }

        public int Query_Type
        {
            set
            {
                _queryType = value;
            }
        }

        public string Query_Code
        {
            set
            {
                _queryCode = value;
            }
        }

        public CMPP_MSG_QUERY(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_QUERY);
            header.SequenceId = sequence;
        }

        public byte[] toBytes()
        {
            byte[] reVal = new byte[CMPP_MSG_Header.HeaderLength + 8 + 1 + 10 + 8];
            int index = 0;
            header.toBytes().CopyTo(reVal, index);
            index = index + CMPP_MSG_Header.HeaderLength;
            Encoding.ASCII.GetBytes(this._time).CopyTo(reVal, index);//8 Octet String 时间YYYYMMDD(精确至日)
            index = index + 8;
            reVal[index++] = Convert.ToByte(this._queryType);
            Encoding.ASCII.GetBytes(this._queryCode).CopyTo(reVal, index);
            return (reVal);
        }
    }

    public class CMPP_MSG_QUERY_RESP
    {
        CMPP_MSG_Header header;
        string _time;
        byte _queryType;
        string _queryCode;
        System.UInt32 _MT_TLMsg;
        System.UInt32 _MT_Tlusr;
        System.UInt32 _MT_Scs;
        System.UInt32 _MT_WT;
        System.UInt32 _MT_FL;
        System.UInt32 _MO_Scs;
        System.UInt32 _MO_WT;
        System.UInt32 _MO_FL;

        public CMPP_MSG_QUERY_RESP(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
            int index = CMPP_MSG_Header.HeaderLength;
            _time = BitConverter.ToString(bs, index, 8);
            index = index + 8;
            this._queryType = bs[index++];
            this._queryCode = BitConverter.ToString(bs, index, 10);
            index = index + 10;
            this._MT_TLMsg = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MT_Tlusr = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MT_Scs = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MT_WT = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MT_FL = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MO_Scs = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MO_WT = BIConvert.Bytes2UInt(bs, index);
            index = index + 4;
            this._MO_FL = BIConvert.Bytes2UInt(bs, index);
        }

        public string Time
        {
            get
            {
                return (this._time);
            }
        }

        public int Qery_Type
        {
            get
            {
                return (this._queryType);
            }
        }

        public string QueryCode
        {
            get
            {
                return (this._queryCode);
            }
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }

        public uint MT_TLMsg
        {
            get
            {
                return (this._MT_TLMsg);
            }
        }

        public uint MT_TLUsr
        {
            get
            {
                return (this._MT_Tlusr);
            }
        }

        public uint MT_Src
        {
            get
            {
                return (this._MT_Scs);
            }
        }

        public uint MT_WT
        {
            get
            {
                return (this._MT_WT);
            }
        }

        public uint MT_FL
        {
            get
            {
                return (this._MT_FL);
            }
        }

        public uint MO_Src
        {
            get
            {
                return (this._MO_Scs);
            }
        }

        public uint MO_WT
        {
            get
            {
                return (this._MO_WT);
            }
        }

        public uint MO_FL
        {
            get
            {
                return (this._MO_FL);
            }
        }


    }

    public class CMPP_MSG_DELIVER
    {
        CMPP_MSG_Header header;
        System.UInt64 _msgid;
        string _destid;
        string _svccode;
        int _tpid;
        int _udhi;
        int _msgfmt;
        string _srctid;
        bool _isReport;
        int _msglength;
        string _msg;

        System.UInt64 _reportForMsgid;
        string _reportState;
        string _submitTime;
        string _doneTime;
        string _reportDesttid;
        int _smscSequence;

        public CMPP_MSG_DELIVER(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
            int index = CMPP_MSG_Header.HeaderLength;
            this._msgid = BitConverter.ToUInt64(bs, index);
            index += 8;
            this._destid = Encoding.ASCII.GetString(bs, index, 21);
            index = index + 21;
            this._svccode = Encoding.ASCII.GetString(bs, index, 10);
            index = index + 10;
            this._tpid = (int)bs[index++];
            this._udhi = (int)bs[index++];
            this._msgfmt = (int)bs[index++];
            this._srctid = Encoding.ASCII.GetString(bs, index, 21);
            index += 21;
            if (bs[index++] == 1)
            {
                this._isReport = true;
            }
            else
            {
                this._isReport = false;
            }
            this._msglength = (int)bs[index++];
            if (!this._isReport)
            {
                switch (this._msgfmt)
                {
                    case (int)Msg_Format.ASCII:
                        this._msg = Encoding.ASCII.GetString(bs, index, this._msglength);
                        index += this._msglength;
                        break;
                    case (int)Msg_Format.BINARY:
                        this._msg = Encoding.Default.GetString(bs, index, this._msglength);
                        index += this._msglength;
                        break;

                    case (int)Msg_Format.GB2312:
                        this._msg = Encoding.Default.GetString(bs, index, this._msglength);
                        index += this._msglength;
                        break;

                    case (int)Msg_Format.UCS2:
                        this._msg = Encoding.BigEndianUnicode.GetString(bs, index, this._msglength);
                        index += this._msglength;
                        break;

                    default:
                        break;
                }
            }
            else
            {//状态报告
                this._reportForMsgid = BitConverter.ToUInt64(bs, index);
                index += 8;
                this._reportState = BitConverter.ToString(bs, index, 7);
                index += 7;
                this._submitTime = BitConverter.ToString(bs, index, 10);
                index += 10;
                this._doneTime = BitConverter.ToString(bs, index, 10);
                index += 10;
                this._reportDesttid = BitConverter.ToString(bs, index, 21);
                index += 21;
                this._smscSequence = (int)BIConvert.Bytes2UInt(bs, index);
            }
        }

        public bool isReport
        {
            get
            {
                return (_isReport);
            }
        }

        public string Msg
        {
            get
            {
                return (this._msg);
            }
        }

        public string SrcID
        {
            get
            {
                return (this._srctid);
            }
        }

        public string SvcCode
        {
            get
            {
                return (this._svccode);
            }
        }

        public string DestID
        {
            get
            {
                return (this._destid);
            }
        }

        public UInt64 MsgID   //给应用程序提供序号
        {
            get
            {
                return (this._msgid);
            }
        }

        public string StateReport
        {
            get
            {
                {
                    return (this._reportState);
                }
            }
        }

        public UInt64 ReportMsgID
        {
            get
            {
                {
                    return (this._reportForMsgid);
                }
            }
        }

        public string SubmitTime
        {
            get
            {
                {
                    return (this._submitTime);
                }
            }
        }
        public string DoneTime
        {
            get
            {
                {
                    return (this._doneTime);
                }
            }
        }

        public string ReportbyDestID
        {
            get
            {
                {
                    return (this._reportDesttid);
                }
            }
        }

        public int SMSCSequence
        {
            get
            {
                return (this._smscSequence);
            }
        }

        public int ISMGSequence
        {
            get
            {
                return ((int)this.header.SequenceId);
            }
        }

        public int MsgBytelen
        {
            get
            {
                return (this._msglength);    //返回deliver包的报告正文长度
            }
        }
    }

    public class CMPP_MSG_DELIVER_RESP
    {
        CMPP_MSG_Header header;
        int _result;
        //byte[] _msgidbytes=new byte[8];
        System.UInt64 _msgid;

        public CMPP_MSG_DELIVER_RESP(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_DELIVER_RESP);
            header.SequenceId = sequence;
            header.MSGLength = (uint)CMPP_MSG_Header.HeaderLength + 8 + 1;
        }

        public int Result
        {
            set
            {
                this._result = value;
            }
        }

        public UInt64 MsgID
        {
            set
            {
                this._msgid = value;
            }
        }

        public byte[] toBytes()
        {
            byte[] reVal = new byte[CMPP_MSG_Header.HeaderLength + 9];
            int index = 0;
            header.toBytes().CopyTo(reVal, index);
            index = index + CMPP_MSG_Header.HeaderLength;
            BitConverter.GetBytes(this._msgid).CopyTo(reVal, index);
            index = index + 8;
            reVal[index++] = Convert.ToByte(this._result);
            return (reVal);
        }

    }

    public class CMPP_MSG_CANCEL
    {
        CMPP_MSG_Header header;
        byte[] Msg_Id = new byte[8];
        string _msgid;

        public CMPP_MSG_CANCEL(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_CANCEL);
            header.SequenceId = sequence;
            header.MSGLength = (uint)(CMPP_MSG_Header.HeaderLength + 8);
        }

        public string MsgID
        {
            set
            {
                this._msgid = value;
            }
        }

        public byte[] toBytes()
        {
            byte[] reVal = new byte[CMPP_MSG_Header.HeaderLength + 8];
            int index = 0;
            header.toBytes().CopyTo(reVal, index);
            index = index + CMPP_MSG_Header.HeaderLength;
            Encoding.ASCII.GetBytes(this._msgid).CopyTo(reVal, index);
            return (reVal);
        }
    }

    public class CMPP_MSG_CANCEL_RESP
    {
        CMPP_MSG_Header header;
        bool _Suceeid;

        public CMPP_MSG_CANCEL_RESP(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
            if (bs[CMPP_MSG_Header.HeaderLength] == 0)
            {
                this._Suceeid = false;
            }
            else
            {
                this._Suceeid = true;
            }
        }

        public bool isSucc
        {
            get
            {
                return (this._Suceeid);
            }
        }

        public uint Sequence
        {
            get
            {
                return (this.header.SequenceId);
            }
            set
            {
                header.SequenceId = value;
            }
        }
    }

    public class CMPP_MSG_TEST
    {
        CMPP_MSG_Header header;

        public CMPP_MSG_TEST(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_ACTIVE_TEST);
            header.MSGLength = (uint)CMPP_MSG_Header.HeaderLength;
            header.SequenceId = sequence;
        }

        public CMPP_MSG_TEST(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
        }

        public byte[] toBytes()
        {
            return (header.toBytes());
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }
    }

    public class CMPP_MSG_TEST_RESP
    {
        CMPP_MSG_Header header;

        public CMPP_MSG_TEST_RESP(uint sequence)
        {
            header = new CMPP_MSG_Header(CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP);
            header.MSGLength = (uint)(CMPP_MSG_Header.HeaderLength + 1);
            header.SequenceId = sequence;
        }

        public CMPP_MSG_TEST_RESP(byte[] bs)
        {
            header = new CMPP_MSG_Header(bs);
        }

        public uint Sequence
        {
            get
            {
                return (header.SequenceId);
            }
        }

        public byte[] toBytes()
        {
            byte[] reVal = new byte[CMPP_MSG_Header.HeaderLength + 1];
            header.toBytes().CopyTo(reVal, 0);
            return (reVal);
        }

    }

    #endregion
}
