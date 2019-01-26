using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Vultrue.Communication
{
    public partial class Modem : Component
    {
        #region 变量
        private Thread unsolicitedThread;
        private bool isopen;
        /// <summary>
        /// 
        /// </summary>
        protected int free;
        /// <summary>
        /// 
        /// </summary>
        protected const int MAX_MESSAGE_LENTH = 280;
        /// <summary>
        /// 
        /// </summary>
        protected object mt = new object();
        /// <summary>
        /// 
        /// </summary>
        protected static Regex RegexOK = new Regex("^OK");
        /// <summary>
        /// 
        /// </summary>
        protected static Regex RegexError1 = new Regex("^ERROR");
        /// <summary>
        /// 
        /// </summary>
        protected static Regex RegexError2 = new Regex("^\\+CM[ES] ERROR:\\s*(?<errinfo>[\\w\\p{P}\\p{S}]+)");

        #endregion

        #region 构造

        public Modem()
        {
            InitializeComponent();
        }

        public Modem(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 读取或设置端口名称
        /// </summary>
        [DefaultValue("COM1")]
        public string PortName
        {
            get { return SerialPort.PortName; }
            set { SerialPort.PortName = value; }
        }

        /// <summary>
        /// 读取或设置波特率
        /// </summary>
        [DefaultValue(19200)]
        public int BaudRate
        {
            get { return SerialPort.BaudRate; }
            set { SerialPort.BaudRate = value; }
        }

        /// <summary>
        /// 读取或设置奇偶校验位
        /// </summary>
        [DefaultValue(System.IO.Ports.Parity.None)]
        public System.IO.Ports.Parity Parity
        {
            get { return SerialPort.Parity; }
            set { SerialPort.Parity = value; }
        }

        /// <summary>
        /// 读取或设置数据位
        /// </summary>
        [DefaultValue(8)]
        public int DataBits
        {
            get { return SerialPort.DataBits; }
            set { SerialPort.DataBits = value; }
        }

        /// <summary>
        /// 读取或设置停止位
        /// </summary>
        [DefaultValue(System.IO.Ports.StopBits.One)]
        public System.IO.Ports.StopBits StopBits
        {
            get { return SerialPort.StopBits; }
            set { SerialPort.StopBits = value; }
        }

        /// <summary>
        /// 读取或设置握手方式
        /// </summary>
        [DefaultValue(System.IO.Ports.Handshake.RequestToSend)]
        public System.IO.Ports.Handshake Handshake
        {
            get { return SerialPort.Handshake; }
            set { SerialPort.Handshake = value; }
        }

        /// <summary>
        /// Modem设置
        /// </summary>
        [Browsable(false)]
        public string SettingInfo
        {
            get
            {
                return string.Format("{0},{1},{2},{3},{4},{5}",
                    SerialPort.PortName,
                    SerialPort.BaudRate,
                    SerialPort.DataBits,
                    (int)SerialPort.Parity,
                    (int)SerialPort.StopBits,
                    (int)SerialPort.Handshake);
            }
            set
            {
                string[] setting = value.Split(',');
                if (setting.Length < 6) throw new ArgumentException();
                bool isOpen = SerialPort.IsOpen;
                if (isOpen) SerialPort.Close();
                try
                {
                    SerialPort.PortName = setting[0];
                    SerialPort.BaudRate = int.Parse(setting[1]);
                    SerialPort.DataBits = int.Parse(setting[2]);
                    SerialPort.Parity = (System.IO.Ports.Parity)int.Parse(setting[3]);
                    SerialPort.StopBits = (System.IO.Ports.StopBits)int.Parse(setting[4]);
                    SerialPort.Handshake = (System.IO.Ports.Handshake)int.Parse(setting[5]);
                }
                catch (Exception ex) { throw new ArgumentException(ex.Message, ex); }
                finally { if (isOpen) SerialPort.Open(); }
            }
        }

        /// <summary>
        /// 是否Modem已打开
        /// </summary>
        [Browsable(false)]
        public bool IsOpen { get { return isopen; } }

        /// <summary>
        /// 本机号码
        /// </summary>
        [DefaultValue("")]
        public string LocalNumber { get; set; }

        /// <summary>
        /// 日志流
        /// </summary>
        public TextWriter LogStream { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [DefaultValue("")]
        [TypeConverter(typeof(StringConverter))]
        public object Tag { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的打开事件的方法。
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的关闭事件的方法。
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// 引发连接事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnOpened(EventArgs e)
        {
            if (Opened != null) Opened(this, e);
        }

        /// <summary>
        /// 引发关闭事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnClosing(EventArgs e)
        {
            if (Closing != null) Closing(this, e);
        }

        #endregion

        #region 基本方法

        /// <summary>
        /// Modem Log
        /// </summary>
        /// <param name="log"></param>
        protected void Log(string log)
        {
            if (LogStream != null) LogStream.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss}\t{1}", DateTime.Now, log));
        }

        /// <summary>
        /// 打开
        /// </summary>
        public void Open()
        {
            SerialPort.Open();
            isopen = true;
            (unsolicitedThread = new Thread(unsolicitedDeal)).Start();
            Log("Modem is opened");
            OnOpened(EventArgs.Empty);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            isopen = false;
            OnClosing(EventArgs.Empty);
            unsolicitedThread.Join();
            System.IO.Ports.Handshake handshake = SerialPort.Handshake;
            try
            {
                SerialPort.Handshake = System.IO.Ports.Handshake.None;
                SerialPort.Close();
            }
            finally
            {
                SerialPort.Handshake = handshake;
            }
            Log("Modem is closed");
        }

        private void unsolicitedDeal()
        {
            free = 0;
            for (; isopen; )
            {
                if (free++ >= 12)
                {
                    lock (mt) ClearBuffer();
                    free = 0;
                }
                Thread.Sleep(100);
            }
        }

        #endregion

        #region 核心处理

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string ReadLine()
        {
            for (; ; )
            {
                string line = SerialPort.ReadLine();
                if (line.Length > 1)
                {
                    Log("收\t" + line);
                    return line;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ClearBuffer()
        {
            while (SerialPort.BytesToRead > 0)
            {
                string line = SerialPort.ReadLine();
                if (line.Length < 2) continue;
                Log("收\t" + line);
                UnsolicitedDeal(line);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        protected void SendCmd(string instruction)
        {
            SerialPort.Write(instruction);
            Log("发\t" + instruction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="validateRegex"></param>
        /// <returns></returns>
        protected List<Match> ExecTask(string instruction, string validateRegex)
        {
            free = 0;
            ClearBuffer();
            SendCmd(instruction);
            Regex regex = validateRegex.Length > 0 ? new Regex(validateRegex) : null;
            List<Match> matchs = new List<Match>();
            for (; ; )
            {
                string line = ReadLine();
                Match match;
                if (RegexError1.Match(line).Success)
                    throw new ModemErrorException(instruction + " 执行错误");
                if ((match = RegexError2.Match(line)).Success)
                    throw new ModemErrorException(match.Result("${errinfo}"), instruction + " 执行错误");
                if (RegexOK.Match(line).Success)
                    return matchs;
                if (regex != null && (match = regex.Match(line)).Success)
                    matchs.Add(match);
                else
                    UnsolicitedDeal(line);
            }
        }

        #endregion
    }

    #region 枚举与结构

    /// <summary>
    /// 为 Vultrue.Communication.GSMModem 对象选择活动状态
    /// </summary>
    public enum ActivityStatus
    {
        /// <summary>
        /// Ready
        /// </summary>
        Ready = 0,
        /// <summary>
        /// Unavailable
        /// </summary>
        Unavailable = 1,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 2,
        /// <summary>
        /// Ringing
        /// </summary>
        Ringing = 3,
        /// <summary>
        /// CallInProgress
        /// </summary>
        CallInProgress = 4,
        /// <summary>
        /// Asleep
        /// </summary>
        Asleep = 5
    }

    /// <summary>
    /// ME Error Report Mode
    /// </summary>
    public enum ReportMobileEquipmentErrorsMode
    {
        /// <summary>
        /// 不使用+CME ERROR:err的返回,错误时仅返回ERROR。
        /// </summary>
        DisableMEerrorReports = 0,
        /// <summary>
        /// 使用+CME ERROR:err的返回,err采用错误编号值。
        /// </summary>
        EnableMEerrorReportsWithCode = 1,
        /// <summary>
        /// 使用+CME ERROR:err的返回,err采用错误的详细字符串值。(开机默认)
        /// </summary>
        EnableMEerrorReportsWithString = 2
    }

    /// <summary>
    /// 为 Vultrue.Communication.GSMModem 对象网络注册状态
    /// </summary>
    public enum NetworkState
    {
        /// <summary>
        /// NotRegisteredNotSearching
        /// </summary>
        NotRegisteredNotSearching = 0,

        /// <summary>
        /// RegisteredHomeNetwork
        /// </summary>
        RegisteredHomeNetwork = 1,

        /// <summary>
        /// NotRegisteredSearching
        /// </summary>
        NotRegisteredSearching = 2,

        /// <summary>
        /// RegistrationDenied
        /// </summary>
        RegistrationDenied = 3,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 4,

        /// <summary>
        /// RegisteredRoaming
        /// </summary>
        RegisteredRoaming = 5
    }

    /// <summary>
    /// 电话本条目
    /// </summary>
    public struct PhonebookEntry
    {
        /// <summary>
        /// 条目的存储索引
        /// </summary>
        public int Index;

        /// <summary>
        /// 电话号码
        /// </summary>
        public string Number;

        /// <summary>
        /// 号码类型
        /// </summary>
        public int Type;

        /// <summary>
        /// 关联的文本
        /// </summary>
        public string Text;
    }

    /// <summary>
    /// 短信格式
    /// </summary>
    public enum MessageMode
    {
        /// <summary>
        /// Pdu
        /// </summary>
        Pdu = 0,

        /// <summary>
        /// Text
        /// </summary>
        Text = 1
    }

    /// <summary>
    /// 新信息指示方式
    /// </summary>
    public struct NewMessageIndication
    {
        /// <summary>
        /// 新短信提示模式
        /// </summary>
        public int Mode;

        /// <summary>
        /// 短信处理方式
        /// </summary>
        public int MessageTreat;

        /// <summary>
        /// 小区广播处理方式
        /// </summary>
        public int BroadcaseTreat;

        /// <summary>
        /// 短信状态报告
        /// </summary>
        public int SMSStatusReport;

        /// <summary>
        /// 缓冲区处理方式
        /// </summary>
        public int BufferTreat;
    }

    /// <summary>
    /// 短信状态
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// REC_UNREAD
        /// </summary>
        REC_UNREAD = 0,

        /// <summary>
        /// REC_READ
        /// </summary>
        REC_READ = 1,

        /// <summary>
        /// STO_UNSENT
        /// </summary>
        STO_UNSENT = 2,

        /// <summary>
        /// STO_SENT
        /// </summary>
        STO_SENT = 3,

        /// <summary>
        /// ALL
        /// </summary>
        ALL = 4
    }

    /// <summary>
    /// Pdu短信编码格式
    /// </summary>
    public enum DataCodingScheme
    {
        /// <summary>
        /// Data
        /// </summary>
        Data = 0x04,

        /// <summary>
        /// USC2
        /// </summary>
        USC2 = 0x08
    }

    /// <summary>
    /// 短信删除标志
    /// </summary>
    public enum DeleteFlag
    {
        /// <summary>
        /// DeleteIndex
        /// </summary>
        DeleteIndex = 0,

        /// <summary>
        /// AllRead
        /// </summary>
        AllRead = 1,

        /// <summary>
        /// AllReadSent
        /// </summary>
        AllReadSent = 2,

        /// <summary>
        /// AllReadSentUnsent
        /// </summary>
        AllReadSentUnsent = 3,

        /// <summary>
        /// All
        /// </summary>
        All = 4
    }

    /// <summary>
    /// 电话本移动模式
    /// </summary>
    public enum MoveMode
    {
        /// <summary>
        /// First
        /// </summary>
        First = 0,

        /// <summary>
        /// Last
        /// </summary>
        Last = 1,

        /// <summary>
        /// Next
        /// </summary>
        Next = 2,

        /// <summary>
        /// Previous
        /// </summary>
        Previous = 3,

        /// <summary>
        /// LastRead
        /// </summary>
        LastRead = 4,

        /// <summary>
        /// LastWriten
        /// </summary>
        LastWriten = 5
    }

    #endregion

    #region 异常

    /// <summary>
    /// 
    /// </summary>
    public class ModemErrorException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public string ErrorInfo { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ModemErrorException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorinfo"></param>
        /// <param name="message"></param>
        public ModemErrorException(string errorinfo, string message) : base(message) { ErrorInfo = errorinfo; }
    }

    #endregion
}
