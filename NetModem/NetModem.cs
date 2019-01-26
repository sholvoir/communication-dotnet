using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;

namespace Vultrue.Communication
{
    /// <summary>
    /// GSMModem驱动程序
    /// </summary>
    [DefaultProperty("Setting")]
    [DefaultEvent("MessageReceived")]
    public partial class NetModem : Component
    {
        #region 变量
        private const int MAXNOTELETLENTH = 280;
        private const int BUFFERSIZE = 1024;
        private TcpClient tcp = new TcpClient();
        private NetworkStream stream;
        private Thread readStream;
        private byte[] data = new byte[BUFFERSIZE];
        private List<byte> buffer = new List<byte>(127);
        private Queue<string> lines = new Queue<string>();
        private object modemLock = new object();
        #endregion

        #region 构造

        /// <summary>
        /// 构造GSMModem对象
        /// </summary>
        public NetModem()
        {
            InitializeComponent();
            initializeComponent();
        }

        /// <summary>
        /// 构造GSMModem对象, 并加入容器
        /// </summary>
        /// <param name="container"></param>
        public NetModem(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            Timeout = 5000;
            Setting = "127.0.0.1:4375";
        }

        #endregion

        #region 属性

        /// <summary>
        /// Modem设置
        /// </summary>
        [DefaultValue("127.0.0.1:4375")]
        public string Setting { get; set; }

        /// <summary>
        /// 读取或设置写入超时毫秒数
        /// </summary>
        [DefaultValue(5000)]
        public int Timeout { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [DefaultValue(null)]
        public object Tag { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的连接事件的方法。
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的关闭事件的方法。
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的通信错误事件的方法。
        /// </summary>
        public event EventHandler<ModemErrorEventArgs> ModemError;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的发送数据事件的方法。
        /// </summary>
        public event EventHandler<SerialDataEventArgs> DataSended;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的接收数据事件的方法。
        /// </summary>
        public event EventHandler<SerialDataEventArgs> DataReceived;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的来电事件的方法。
        /// </summary>
        public event EventHandler Ringing;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的接收到短信事件的方法。
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的网络注册信息已改变的方法。
        /// </summary>
        public event EventHandler<NetworkRegistrationChangedEventArgs> NetworkRegistrationChanged;

        #endregion

        #region 事件引发

        /// <summary>
        /// 异步引发连接事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnConnected(EventArgs e)
        {
            if (Connected != null) new Thread(() => { Connected(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发关闭事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnClosed(EventArgs e)
        {
            if (Closed != null) new Thread(() => { Closed(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发通信错误事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnModemError(ModemErrorEventArgs e)
        {
            if (ModemError != null) new Thread(() => { ModemError(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发数据发送事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnDataSended(SerialDataEventArgs e)
        {
            if (DataSended != null) new Thread(() => { DataSended(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发数据接收事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnDataReceived(SerialDataEventArgs e)
        {
            if (DataReceived != null) new Thread(() => { DataReceived(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发来电事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnRinging(EventArgs e)
        {
            if (Ringing != null) new Thread(() => { Ringing(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发接收到短信事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (MessageReceived != null) new Thread(() => { MessageReceived(this, e); }).Start();
        }

        /// <summary>
        /// 异步引发网络注册信息已改变事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnNetworkRegistrationChanged(NetworkRegistrationChangedEventArgs e)
        {
            if (NetworkRegistrationChanged != null) new Thread(() => { NetworkRegistrationChanged(this, e); }).Start();
        }

        #endregion

        #region 基本方法

        /// <summary>
        /// 连接到网络串口
        /// </summary>
        public void Connect()
        {
            string[] iport = Setting.Split(':');
            tcp.Connect(iport[0], int.Parse(iport[1]));
            stream = tcp.GetStream();
            (readStream = new Thread(readData)).Start();
            OnConnected(EventArgs.Empty);
        }

        /// <summary>
        /// 连接到网络串口
        /// </summary>
        /// <param name="setting">连接设置</param>
        public void Connect(string setting)
        {
            Setting = setting;
            Connect();
        }

        /// <summary>
        /// 关闭Modem连接
        /// </summary>
        /// <exception cref="System.InvalidOperationException">指定的端口未打开</exception>
        public void Close()
        {
            readStream.Abort();
            stream.Close();
            tcp.Close();
            stream = null;
            OnClosed(EventArgs.Empty);
        }

        #endregion

        #region 核心处理

        private void readData()
        {
            while (stream != null)
            {
                int c = stream.Read(data, 0, BUFFERSIZE);
                if (c == 0) { OnModemError(new ModemErrorEventArgs("连接中断")); break; }
                for (int i = 0; i < c; i++)
                {
                    byte b = data[i];
                    if (buffer.Count == 0) buffer.Add(b);
                    else if ((b == 0x0A) && (buffer[buffer.Count - 1] == 0x0D))
                    {
                        buffer.RemoveAt(buffer.Count - 1);
                        if (buffer.Count > 0) linedeal();
                    }
                    else if ((b == 0x20) && (buffer[buffer.Count - 1] == 0x3E))
                    {
                        buffer.Add(b);
                        linedeal();
                    }
                    else buffer.Add(b);
                }
            }
        }

        private void linedeal()
        {
            string line = Encoding.ASCII.GetString(buffer.ToArray());
            lines.Enqueue(line);
            buffer.Clear();
            OnDataReceived(new SerialDataEventArgs(line));
        }

        /// <summary>
        /// 从串口读取一行数据
        /// </summary>
        /// <returns>数据</returns>
        private string readLine()
        {
            int waitime = 0;
            do
                if (lines.Count > 0) return lines.Dequeue();
                else { Thread.Sleep(100); waitime += 100; }
            while (waitime < Timeout);
            throw new TimeoutException("数据读超时");
        }

        private void clearLines()
        {
            while (lines.Count > 0) dealUnsolicitedResultCodes(lines.Dequeue());
        }

        /// <summary>
        /// 向Modem发出一条指令
        /// </summary>
        /// <param name="cmd">指令</param>
        private void sendCmd(string cmd)
        {
            if (stream == null) throw new ModemClosedException("Modem 未打开或已关闭");
            byte[] sendata = Encoding.ASCII.GetBytes(cmd);
            stream.Write(sendata, 0, sendata.Length);
            OnDataSended(new SerialDataEventArgs(cmd));
        }

        private static Regex regexRing = new Regex("^RING");
        private static Regex regexCMTI = new Regex("^\\+CMTI:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)");
        private static Regex regexCREG = new Regex("^\\+CREG:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)");

        /// <summary>
        /// 处理未请求的返回值
        /// </summary>
        private void dealUnsolicitedResultCodes(string line)
        {
            Match match;
            if ((match = regexRing.Match(line)).Success)
            {
                if (Ringing != null) Ringing(this, EventArgs.Empty);
            }
            else if ((match = regexCMTI.Match(line)).Success)
            {
                if (MessageReceived != null)
                {
                    string[] result = match.Result("${ans}").Replace("\"", "").Split(',');
                    MessageReceived(this, new MessageReceivedEventArgs(result[0], int.Parse(result[1])));
                }
            }
            else if ((match = regexCREG.Match(line)).Success)
            {
                if (NetworkRegistrationChanged != null)
                {
                    string[] result = match.Result("${ans}").Replace("\"", "").Split(',');
                    NetworkState state = (NetworkState)int.Parse(result[0]);
                    string locationAreaCode = result.Length > 1 ? result[1] : "";
                    string cellID = result.Length > 2 ? result[2] : "";
                    NetworkRegistrationChanged(this, new NetworkRegistrationChangedEventArgs(state, locationAreaCode, cellID));
                }
            }
        }

        private static Regex regexError1 = new Regex("^ERROR");
        private static Regex regexError2 = new Regex("^\\+CM[ES] ERROR:\\s*(?<errid>\\d+)");
        private static Regex regexOK = new Regex("^OK");

        /// <summary>
        /// 对Modem进行通用操作
        /// </summary>
        /// <param name="instruction">操作指令</param>
        /// <param name="validateRegex">验证表达式</param>
        /// <param name="errorInfo">错误提示</param>
        /// <returns>执行结果</returns>
        private List<Match> generalOperate(string instruction, string validateRegex, string errorInfo)
        {
            lock (modemLock)
            {
                List<Match> matchs = new List<Match>();
                clearLines();
                sendCmd(instruction);
                Match match;
                while (true)
                {
                    string line = readLine();
                    if (regexError1.Match(line).Success) throw new ModemUnsupportedException(errorInfo);
                    if ((match = regexError2.Match(line)).Success) throw new ModemUnsupportedException(int.Parse(match.Result("${errid}")), errorInfo);
                    if (regexOK.Match(line).Success) return matchs;
                    if (validateRegex.Length > 0)
                        if ((match = new Regex(validateRegex).Match(line)).Success) matchs.Add(match);
                        else throw new ModemDataException(line);
                }
            }
        }

        /// <summary>
        /// 定时处理Modem自动发生的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (modemLock) clearLines();
        }

        #endregion

        #region GENERAL COMMANDS

        /// <summary>
        /// 直接对 Modem 发送指令
        /// </summary>
        /// <param name="cmd">指令</param>
        public void SendDirect(string cmd)
        {
            lock (modemLock)
            {
                clearLines();
                sendCmd(cmd);
            }
        }

        /// <summary>
        /// 获取制造商信息
        /// </summary>
        /// <returns>制造商信息</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetManufacturerIdentification()
        {
            return generalOperate("AT+CGMI\r", "^(?<ans>[ \\w]+)", "")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取支持的频带
        /// </summary>
        /// <returns>支持的频带</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetModelIdentification()
        {
            return generalOperate("AT+CGMM\r", "^(?<ans>[ \\w]+)", "")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取Modem固件版本
        /// </summary>
        /// <returns>Modem固件版本</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetRevisionIdentification()
        {
            return generalOperate("AT+CGMR\r", "^(?<ans>[ \\w\\p{P}\\p{S}]+)", "")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取产品序列号
        /// </summary>
        /// <returns>产品序列号</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">Modem EEPROM中没有产品序列号时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetProductSerialNumber()
        {
            return generalOperate("AT+CGSN\r", "^(?<ans>\\d+)", "IMEI not found in EEPROM")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取支持的TE字符集列表
        /// </summary>
        /// <returns>支持的TE字符集列表</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动尚未完成时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string[] GetTECharacterSetList()
        {
            return generalOperate("AT+CSCS=?\r", "^\\+CSCS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "Modem未启动")[0]
                .Result("${ans}").Trim('(', ')').Replace("\"", "").Split(',');
        }

        /// <summary>
        /// 获取当前TE字符集
        /// </summary>
        /// <returns>当前TE字符集</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动尚未完成时将发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetTECharacterSet()
        {
            return generalOperate("AT+CSCS?\r", "^\\+CSCS:\\s*\"(?<ans>\\w+)\"", "Modem未启动")[0].Result("${ans}");
        }

        /// <summary>
        /// 设置当前TE字符集
        /// </summary>
        /// <param name="characterSet">TE字符集</param>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">试图设置Modem不支持的字符集时将发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public void SetTECharacterSet(string characterSet)
        {
            generalOperate(string.Format("AT+CSCS=\"{0}\"\r", characterSet), "", string.Format("Modem不支持字符集\"{0}\"", characterSet));
        }


        /// <summary>
        /// 获取SIM卡国际移动用户标识
        /// </summary>
        /// <returns>SIM卡国际移动用户标识</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动尚未完成或无SIM卡时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetInternationalMobileSubscriberIdentity()
        {
            return generalOperate("AT+CIMI\r", "^(?<ans>\\d+)", "Modem未启动")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取SIM卡标识
        /// </summary>
        /// <returns>SIM卡标识</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动尚未完成时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string GetCardIdentification()
        {
            return generalOperate("AT+CCID\r", "^\\+CCID:\\s*\"(?<ans>\\d+)\"", "Modem未启动")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取Modem功能列表
        /// </summary>
        /// <returns>Modem功能列表</returns>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public string[] GetCapabilitiesList()
        {
            return generalOperate("AT+GCAP\r", "^\\+GCAP:\\s*(?<ans>[ \\w\\p{P}\\p{S}]+)", "")[0]
                .Result("${ans}").Replace(" ", "").Split(',');
        }

        /// <summary>
        /// 关闭Modem ME, 等效于Modem功能等级设置为0
        /// </summary>
        /// <exception cref="ModemClosedException">Modem未打开是发生该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成</exception>
        /// <exception cref="ModemDataException">Modem接受到的数据异常</exception>
        public void PowerOff()
        {
            generalOperate("AT+CPOF\r", "", "");
        }

        /// <summary>
        /// 设置Modem功能级别(0或1)
        /// </summary>
        /// <returns>Modem功能级别</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public int GetPhoneFunctionalityLevel()
        {
            return int.Parse(generalOperate("AT+CFUN?\r", "^\\+CFUN:\\s*(?<ans>\\d+)", "")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置Modem功能级别(0或1)
        /// </summary>
        /// <param name="level">Modem功能级别</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">试图设置不被支持的功能级别引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetPhoneFunctionalityLevel(int level)
        {
            generalOperate(string.Format("AT+CFUN={0}\r", level), "", "不支持的功能级别");
        }

        /// <summary>
        /// 获取Modem活动状态
        /// </summary>
        /// <returns>Modem活动状态</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public ActivityStatus GetPhoneActivityStatus()
        {
            return (ActivityStatus)int.Parse(generalOperate("AT+CPAS\r", "^\\+CPAS:\\s*(?<ans>\\d+)", "")[0].Result("${ans}"));
        }

        /// <summary>
        /// 获取是否报告ME错误
        /// </summary>
        /// <returns>是否报告ME错误</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public bool GetReportMobileEquipmentErrors()
        {
            return generalOperate("AT+CMEE?\r", "^\\+CMEE:\\s*(?<ans>\\d+)", "")[0].Result("${ans}") == "1";
        }

        /// <summary>
        /// 设置是否报告ME错误
        /// </summary>
        /// <param name="isReportMEError">是否报告ME错误</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetReportMobileEquipmentErrors(bool isReportMEError)
        {
            generalOperate(string.Format("AT+CMEE={0}\r", (isReportMEError ? "1" : "0")), "", "");
        }

        /// <summary>
        /// 设置按键控制Pattern
        /// </summary>
        /// <param name="keypadControl">按键控制Pattern</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">试图设置不被支持的功能级别引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        /// <returns></returns>
        public void SetKeypadControl(string keypadControl)
        {
            generalOperate(string.Format("AT+CKPD=\"{0}\"\r", keypadControl), "^\\+CCFC:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)",
                "Modem未启动完成或不支持的键盘控制码");
        }

        /// <summary>
        /// 获取Modem时间(精确到分)
        /// </summary>
        /// <returns>Modem时间</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public DateTime GetModemClock()
        {
            return DateTime.Parse("20" +
                generalOperate("AT+CCLK?\r", "^\\+CCLK:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "")[0].Result("${ans}").Trim('\"'));
        }

        /// <summary>
        /// 设置Modem时钟(精确到分)
        /// </summary>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">日期时间格式错误</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        /// <returns></returns>
        public void SetModemClock(DateTime clock)
        {
            generalOperate(string.Format("AT+CCLK=\"{0:yy/MM/dd,HH:mm:ss}\"\r", clock), "", "日期时间格式错误");
        }

        /// <summary>
        /// 获取Modem闹钟列表(精确到分,尚未响铃)
        /// </summary>
        /// <returns>尚未响铃的Modem闹钟列表</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public DateTime[] GetModemAlarms()
        {
            List<DateTime> alarms = new List<DateTime>();
            List<Match> matchs = generalOperate("AT+CALA?\r", "^\\+CALA:\\s*\"(?<cala>[/:,\\d]+)\",(?<index>\\d+)", "");
            foreach (Match match in matchs) alarms.Add(DateTime.Parse("20" + match.Result("${cala}")));
            return alarms.ToArray();
        }

        /// <summary>
        /// 设置Modem闹钟列表(精确到分)
        /// </summary>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">ModemAlarm位置已满(最多16个闹钟)</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void AddModemAlarm(DateTime alarm)
        {
            generalOperate(string.Format("AT+CALA=\"{0:yy/MM/dd,HH:mm:ss}\"\r", alarm), "", "ModemAlarm位置已满");
        }

        /// <summary>
        /// 获取来电音量(0-15, 6为默认值)
        /// </summary>
        /// <returns>音量</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">不支持的音量数值</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public int GetRingerSoundLevel()
        {
            return int.Parse(generalOperate("AT+CRSL?\r", "^\\+CRSL:\\s*(?<ans>\\d+)", "")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置来电音量(0-15, 6为默认值)
        /// </summary>
        /// <param name="soundLevel">音量</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">不支持的音量数值</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetRingerSoundLevel(int soundLevel)
        {
            generalOperate(string.Format("AT+CRSL={0}\r", soundLevel), "", "不支持的音量数值");
        }

        #endregion

        #region CALL CONTROL COMMANDS
        #endregion

        #region NETWORK SERVICE COMMANDS

        /// <summary>
        /// 获取Modem信号质量
        /// </summary>
        /// <param name="errorRate">当前误码率 取值范围(0-7) 99表示未知或不可检测</param>
        /// <returns>Modem信号强度 取值范围(0-31) 99表示未知或不可检测</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public int GetSignalQuality(out int errorRate)
        {
            Match match = generalOperate("AT+CSQ\r", "^\\+CSQ:\\s*(?<rssi>\\d+),(?<ber>\\d+)", "")[0];
            errorRate = int.Parse(match.Result("${ber}"));
            return int.Parse(match.Result("${rssi}"));
        }

        /// <summary>
        /// 设置网络注册信息模式
        /// </summary>
        /// <param name="mode">注册信息模式</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem 不支持该模式时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetNetworkRegistrationMode(int mode)
        {
            generalOperate(string.Format("AT+CREG={0}\r", mode), "", "不支持的注册信息模式");
        }

        /// <summary>
        /// 获取Modem网络注册信息
        /// </summary>
        /// <param name="mode">模式, 取值范围(0-2)</param>
        /// <param name="locationAreaCode">区域代码</param>
        /// <param name="cellID">蜂窝ID</param>
        /// <returns>Modem网络注册状态</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public NetworkState GetNetworkRegistration(out int mode, out string locationAreaCode, out string cellID)
        {
            Match match = generalOperate("AT+CREG?\r", "^\\+CREG:\\s*(?<mode>\\d+),(?<stat>\\d+)" +
                "(,\"(?<lac>[0-9a-fA-F]+)\",\"(?<ci>[0-9a-fA-F]+)\")*", "")[0];
            mode = int.Parse(match.Result("${mode}"));
            locationAreaCode = match.Result("${lac}");
            cellID = match.Result("${ci}");
            return (NetworkState)int.Parse(match.Result("${stat}"));
        }

        #endregion

        #region SECURITY COMMANDS
        #endregion

        #region PHONEBOOK COMMANDS

        /// <summary>
        /// 获取Modem支持的电话本存储位置列表
        /// </summary>
        /// <returns>电话本存储位置列表</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public string[] GetPhonebookMemoryStorageList()
        {
            return generalOperate("AT+CPBS=?\r", "^\\+CPBS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "Modem未启动")[0]
                .Result("${ans}").Trim('(', ')').Replace("\"", "").Split(',');
        }

        /// <summary>
        /// 获取电话本存储空间信息
        /// </summary>
        /// <param name="usedLocations">已使用的空间</param>
        /// <param name="totalLocations">总可使用的空间</param>
        /// <returns>电话本存储位置</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public string GetPhonebookMemoryStorage(out int usedLocations, out int totalLocations)
        {
            Match match = generalOperate("AT+CPBS?\r", "^\\+CPBS:\\s*\"(?<mem>\\d+)\",(?<used>\\d+),(?<total>\\d+)", "Modem未启动")[0];
            usedLocations = int.Parse(match.Result("${used}"));
            totalLocations = int.Parse(match.Result("${total}"));
            return match.Result("${mem}");
        }

        /// <summary>
        /// 设置电话本存储空间
        /// </summary>
        /// <param name="storage">电话本存储空间</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetPhonebookMemoryStorage(string storage)
        {
            generalOperate(string.Format("AT+CPBS=\"{0}\"\r", storage), "", "Modem未启动");
        }

        /// <summary>
        /// 获取电话本信息
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="endIndex">结束索引</param>
        /// <param name="maxPhoneLenth">电话号码最大长度</param>
        /// <param name="maxTextLenth">关联文本最大长度</param>
        public void GetPhonebookInfo(out int startIndex, out int endIndex, out int maxPhoneLenth, out int maxTextLenth)
        {
            Match match = generalOperate("AT+CPBR=?\r", "^\\+CPBR:\\s*\\((?<si>\\d+)-(?<ei>\\d+)\\),(?<mp>\\d+),(?<mt>\\d+)", "Modem未启动")[0];
            startIndex = int.Parse(match.Result("${si}"));
            endIndex = int.Parse(match.Result("${ei}"));
            maxPhoneLenth = int.Parse(match.Result("${mp}"));
            maxTextLenth = int.Parse(match.Result("${mt}"));
        }

        /// <summary>
        /// 从电话本读取电话号码
        /// </summary>
        /// <param name="index">起始索引</param>
        /// <returns>读取的电话本条目</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public PhonebookEntry ReadPhonebookEntries(int index)
        {
            Match match = generalOperate(string.Format("AT+CPBR={0}\r", index), "^\\+CPBR:\\s*(?<index>\\d+)" +
                ",\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"", "Modem未启动或索引超出范围")[0];
            PhonebookEntry pbe = new PhonebookEntry();
            pbe.Index = int.Parse(match.Result("${index}"));
            pbe.Number = match.Result("${number}");
            pbe.Type = int.Parse(match.Result("${type}"));
            pbe.Text = match.Result("${text}");
            return pbe;
        }

        /// <summary>
        /// 从电话本读取电话号码
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="endIndex">结束索引</param>
        /// <returns>读取的电话本条目</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public PhonebookEntry[] ReadPhonebookEntries(int startIndex, int endIndex)
        {
            List<PhonebookEntry> entries = new List<PhonebookEntry>();
            List<Match> matchs = generalOperate(string.Format("AT+CPBR={0},{1}\r", startIndex, endIndex),
                "^\\+CPBR:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"",
                "Modem未启动或索引超出范围");
            foreach (Match match in matchs)
            {
                PhonebookEntry pbe = new PhonebookEntry();
                pbe.Index = int.Parse(match.Result("${index}"));
                pbe.Number = match.Result("${number}");
                pbe.Type = int.Parse(match.Result("${type}"));
                pbe.Text = match.Result("${text}");
                entries.Add(pbe);
            }
            return entries.ToArray();
        }

        /// <summary>
        /// 从电话本查找电话号码
        /// </summary>
        /// <param name="text">搜索文本</param>
        /// <returns>查找到的电话本条目</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public PhonebookEntry[] FindPhonebookEntries(string text)
        {
            List<PhonebookEntry> entries = new List<PhonebookEntry>();
            List<Match> matchs = generalOperate(string.Format("AT+CPBF=\"{0}\"\r", text),
                "^\\+CPBF:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"",
                "Modem未启动或索引超出范围");
            foreach (Match match in matchs)
            {
                PhonebookEntry pbe = new PhonebookEntry();
                pbe.Index = int.Parse(match.Result("${index}"));
                pbe.Number = match.Result("${number}");
                pbe.Type = int.Parse(match.Result("${type}"));
                pbe.Text = match.Result("${text}");
                entries.Add(pbe);
            }
            return entries.ToArray();
        }

        /// <summary>
        /// 从电话本删除电话本条目
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void DeletePhonebookEntry(int index)
        {
            generalOperate(string.Format("AT+CPBW={0}\r", index), "", "Modem未启动");
        }

        /// <summary>
        /// 向电话本写入电话本条目
        /// </summary>
        /// <param name="index">存储位置</param>
        /// <param name="number">电话号码</param>
        /// <param name="type">号码类型(129:本地号码/145:国际号码)</param>
        /// <param name="text">联系文本</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void WritePhonebookEntry(int index, string number, int type, string text)
        {
            generalOperate(string.Format("AT+CPBW={0},\"{1}\",{2},\"{3}\"\r", index, number, type, text), "", "Modem未启动");
        }

        /// <summary>
        /// 向电话本写入电话本条目
        /// </summary>
        /// <param name="number">电话号码</param>
        /// <param name="type">号码类型(129:本地号码/145:国际号码)</param>
        /// <param name="text">联系文本</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void WritePhonebookEntry(string number, int type, string text)
        {
            generalOperate(string.Format("AT+CPBW=,\"{0}\",{1},\"{2}\"\r", number, type, text), "", "Modem未启动");
        }

        /// <summary>
        /// 由电话号码搜索电话本条目
        /// </summary>
        /// <param name="number">电话号码</param>
        /// <returns>电话本条目</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public PhonebookEntry PhonebookPhoneSearch(string number)
        {
            Match match = generalOperate(string.Format("AT+CPBP=\"{0}\"\r", number),
                "^\\+CPBP:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"",
                "Modem未启动或索引超出范围")[0];
            PhonebookEntry pbe = new PhonebookEntry();
            pbe.Index = int.Parse(match.Result("${index}"));
            pbe.Number = match.Result("${number}");
            pbe.Type = int.Parse(match.Result("${type}"));
            pbe.Text = match.Result("${text}");
            return pbe;
        }

        /// <summary>
        /// 在电话号码本中移动
        /// </summary>
        /// <param name="mode">移动模式</param>
        /// <returns>电话本条目</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或索引超出范围</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public PhonebookEntry MoveActionPhonebook(MoveMode mode)
        {
            Match match = generalOperate(string.Format("AT+CPBN={0}\r", (int)mode),
                "^\\+CPBN:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"",
                "Modem未启动或索引超出范围")[0];
            PhonebookEntry pbe = new PhonebookEntry();
            pbe.Index = int.Parse(match.Result("${index}"));
            pbe.Number = match.Result("${number}");
            pbe.Type = int.Parse(match.Result("${type}"));
            pbe.Text = match.Result("${text}");
            return pbe;
        }
        #endregion

        #region SHORT MESSAGES COMMANDS

        /// <summary>
        /// 获取GSMModem短信服务AT命令版本(0或1)
        /// </summary>
        /// <returns>GSMModem短信服务AT命令版本</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem未启动或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public int GetMessageService()
        {
            return int.Parse(generalOperate("AT+CSMS?\r", "^\\+CSMS:\\s*(?<service>\\d+)" +
                ",(?<mo>\\d+),(?<mt>\\d+),(?<cb>\\d+)", "Modem未启动")[0].Result("${service}"));
        }

        /// <summary>
        /// 设置GSMModem短信服务AT命令版本(0或1)
        /// </summary>
        /// <param name="messageService">GSMModem短信服务AT命令版本</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem不支持该AT命令版本时发生该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        /// <returns></returns>
        public void SetMessageService(int messageService)
        {
            generalOperate(string.Format("AT+CSMS={0}\r", messageService),
                "^\\+CSMS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "不支持的短信协议");
        }

        /// <summary>
        /// 新短信接收确认
        /// 文本模式下, 只能进行肯定确认(自动忽略acknowledge参数)
        /// PDU模式下, 可进行肯定和否定确认
        /// 只有短信AT命令版本1下且+CMT和+CDS设置为显示时才允许确认
        /// </summary>
        /// <param name="acknowledge">肯定确认或否定确认</param>
        /// <param name="pdustr">确认信息</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">没有要确认的短信时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void NewMessageAcknowledgement(bool acknowledge, string pdustr)
        {
            int timeout = Timeout;
            Timeout = 300000;
            try
            {
                if (GetMessageFormat() == MessageFormat.Text)
                    generalOperate("AT+CNMA\r", "(?<ans>[ \\w\\p{P}\\p{S}]+)", "没有要确认的短信");
                else if (pdustr == null || pdustr.Length == 0)
                    generalOperate("AT+CNMA=0\r", "(?<ans>[ \\w\\p{P}\\p{S}]+)", "没有要确认的短信");
                else
                {
                    SendDirect(string.Format("AT+CNMA={0},{1}\r", (acknowledge ? "1" : "2"), pdustr.Length / 2));
                    string line = readLine();
                    if (!line.EndsWith("\u003E\u0020"))
                    {
                        sendCmd("\u001B");
                        throw new ModemDataException("确认短信未发送成功");
                    }
                    generalOperate(string.Format("{0}\u001A", pdustr), "(?<ans>[ \\w\\p{P}\\p{S}]+)", "确认短信未发送成功");
                }
            }
            finally { Timeout = timeout; }
        }

        /// <summary>
        /// 获取短信存储空间列表
        /// </summary>
        /// <param name="rldStorages">读取,列出,删除操作的可选目标空间</param>
        /// <param name="wsStorages">写入,发送的可选目标空间</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void GetMessageStorageList(out string[] rldStorages, out string[] wsStorages)
        {
            Match match = generalOperate("AT+CPMS=?\r", "^\\+CPMS:\\s*\\(\\((?<rld>[\",\\w]+)\\),\\((?<ws>[\",\\w]+)\\)\\)", "Modem未启动")[0];
            rldStorages = match.Result("${rld}").Replace("\"", "").Split(',');
            wsStorages = match.Result("${ws}").Replace("\"", "").Split(',');
        }

        /// <summary>
        /// 获取GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStorage">写入与发送操作目标存储空间</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void GetPreferredMessageStorage(out string rldStorage, out int rldStoreUsed,
            out int rldStoreTotal, out string wsStorage, out int wsStoreUsed, out int wsStoreTotal)
        {
            Match match = generalOperate("AT+CPMS?\r", "^\\+CPMS:\\s*\"(?<rld>\\w+)\",(?<rldu>\\d+)" +
                ",(?<rldt>\\d+),\"(?<ws>\\w+)\",(?<wsu>\\d+),(?<wst>\\d+)", "Modem未启动")[0];
            rldStorage = match.Result("${rld}");
            rldStoreUsed = int.Parse(match.Result("${rldu}"));
            rldStoreTotal = int.Parse(match.Result("${rldt}"));
            wsStorage = match.Result("${ws}");
            wsStoreUsed = int.Parse(match.Result("${wsu}"));
            wsStoreTotal = int.Parse(match.Result("${wst}"));
        }

        /// <summary>
        /// 设置GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">设置了不支持的短信时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetPreferredMessageStorage(string rldStorage, out int rldStoreUsed,
            out int rldStoreTotal, out int wsStoreUsed, out int wsStoreTotal)
        {
            Match match = generalOperate(string.Format("AT+CPMS=\"{0}\"\r", rldStorage),
                "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+)", "Modem未启动")[0];
            rldStoreUsed = int.Parse(match.Result("${rldu}"));
            rldStoreTotal = int.Parse(match.Result("${rldt}"));
            wsStoreUsed = int.Parse(match.Result("${wsu}"));
            wsStoreTotal = int.Parse(match.Result("${wst}"));
        }

        /// <summary>
        /// 设置GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="wsStorage">写入与发送操作目标存储空间, 不需要需要修改时, 值为null时不修改</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">设置了不支持的短信时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetPreferredMessageStorage(string rldStorage, string wsStorage,
            out int rldStoreUsed, out int rldStoreTotal, out int wsStoreUsed, out int wsStoreTotal)
        {
            Match match = generalOperate(string.Format("AT+CPMS=\"{0}\",\"{1}\"\r", rldStorage, wsStorage),
                "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+)", "Modem未启动")[0];
            rldStoreUsed = int.Parse(match.Result("${rldu}"));
            rldStoreTotal = int.Parse(match.Result("${rldt}"));
            wsStoreUsed = int.Parse(match.Result("${wsu}"));
            wsStoreTotal = int.Parse(match.Result("${wst}"));
        }

        /// <summary>
        /// 获取GSMModem短信格式
        /// </summary>
        /// <returns>GSMModem短信格式</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public MessageFormat GetMessageFormat()
        {
            return (MessageFormat)int.Parse(generalOperate("AT+CMGF?\r", "^\\+CMGF:\\s*(?<ans>\\d+)",
                "Modem未启动")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置GSMModem短信格式
        /// </summary>
        /// <param name="messageFormat">GSMModem短信格式</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        /// <returns></returns>
        public void SetMessageFormat(MessageFormat messageFormat)
        {
            generalOperate(string.Format("AT+CMGF={0}\r", (int)messageFormat), "", "Modem未启动");
        }

        /// <summary>
        /// 保存设置信息(短信中心号码和文本模式参数)到EEPROM(SIM卡Phase1)或SIM(SIM卡Phase2)
        /// </summary>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SaveSettings()
        {
            generalOperate("AT+CSAS\r", "", "Modem未启动");
        }

        /// <summary>
        /// 恢复设置信息(短信中心号码和文本模式参数)从EEPROM(SIM卡Phase1)或SIM(SIM卡Phase2)
        /// </summary>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void RestoreSettings()
        {
            generalOperate("AT+CRES\r", "", "Modem未启动");
        }

        /// <summary>
        /// 获取是否显示文本模式参数
        /// </summary>
        /// <returns>是否显示文本模式参数</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public bool GetIsShowTextModeParameters()
        {
            return generalOperate("AT+CSDH?\r", "^\\+CSDH:\\s*(?<ans>\\d+)", "Modem未启动")[0].Result("${ans}") == "1";
        }

        /// <summary>
        /// 设置是否显示文本模式参数
        /// 影响的指令+CMTI, +CMT, +CDS, +CMGR, +CMGL
        /// </summary>
        /// <param name="isShow">是否显示文本模式参数</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetIsShowTextModeParameters(bool isShow)
        {
            generalOperate(string.Format("AT+CSDH={0}\r", (isShow ? "1" : "0")), "", "Modem未启动");
        }

        /// <summary>
        /// 获取新短信提示模式
        /// </summary>
        /// <returns>新短信提示模式</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public NewMessageIndication GetNewMessageIndication()
        {
            Match match = generalOperate("AT+CNMI?\r", "^\\+CNMI:\\s*(?<mode>\\d+)" +
                ",(?<mt>\\d+),(?<bm>\\d+),(?<ds>\\d+),(?<bfr>\\d+)", "Modem未启动")[0];
            NewMessageIndication nmi = new NewMessageIndication();
            nmi.Mode = int.Parse(match.Result("${mode}"));
            nmi.MessageTreat = int.Parse(match.Result("${mt}"));
            nmi.BroadcaseTreat = int.Parse(match.Result("${bm}"));
            nmi.SMSStatusReport = int.Parse(match.Result("${ds}"));
            nmi.BufferTreat = int.Parse(match.Result("${bfr}"));
            return nmi;
        }

        /// <summary>
        /// 设置新短信提示模式
        /// </summary>
        /// <param name="indication">新短信提示模式</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetNewMessageIndication(NewMessageIndication indication)
        {
            generalOperate(string.Format("AT+CNMI={0},{1},{2},{3},{4}\r", indication.Mode, indication.MessageTreat,
                indication.BroadcaseTreat, indication.SMSStatusReport, indication.BufferTreat), "", "Modem未启动");
        }

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="index">短信所在存储空间的索引</param>
        /// <returns>读取的短信</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public Message ReadMessage(int index)
        {
            
            if (GetMessageFormat() == MessageFormat.Pdu)
            {
                List<Match> matchs = generalOperate(string.Format("AT+CMGR={0}\r", index),
                    "(?<ans>[ \\w\\p{P}\\p{S}]+)", "Modem未启动或该位置没有短信");
                string line = matchs[0].Result("${ans}");
                Match match = new Regex("^\\+CMGR:\\s*(?<stat>\\d+),(?<alpha>[ \"\\w\\d]*),(?<lenth>\\d+)").Match(line);
                if (!match.Success) throw new ModemDataException(line);
                MessageState messageState = (MessageState)int.Parse(match.Result("${stat}"));
                int lenth = int.Parse(match.Result("${lenth}"));
                return new Message(index, messageState, lenth, matchs[1].Result("${ans}"));
            }
            else
            {
                List<Match> matchs = generalOperate(string.Format("AT+CMGR={0}\r", index),
                    "(?<ans>[ \\w\\p{P}\\p{S}]+)", "Modem未启动或该位置没有短信");
                string line = matchs[0].Result("${ans}");
                Match match = new Regex("^\\+CMGR:\\s*\"(?<stat>[\\s\\w]+)\"," +
                    "\"(?<oada>\\d+)\",(?<alpha>[ \"\\w\\d]*)(,\"(?<scts>[,/:-\\+\\d]+)\")*").Match(line);
                if (!match.Success) throw new ModemDataException(line);
                MessageState messageState = (MessageState)Enum.Parse(typeof(MessageState), match.Result("${stat}").Replace(" ", "_"), true);
                string terminalAddress = match.Result("${oada}");
                string serviceCenterTimeStamp = match.Result("${scts}");
                return new Message(index, messageState, terminalAddress, serviceCenterTimeStamp, matchs[1].Result("${ans}"));
            }
        }

        /// <summary>
        /// 列出短信
        /// </summary>
        /// <param name="state">筛选短信的状态</param>
        /// <returns>读取的短信</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public Message[] ListMessage(MessageState state)
        {
            List<Message> messages = new List<Message>();
            if (GetMessageFormat() == MessageFormat.Pdu)
            {
                List<Match> matchs = generalOperate(string.Format("AT+CMGL={0}\r", (int)state),
                    "(?<ans>[ \\w\\p{P}\\p{S}]+)", "Modem未启动");
                Regex regex = new Regex("^\\+CMGL:\\s*(?<index>\\d+),(?<stat>\\d+),(?<alpha>[ \"\\w\\d]*),(?<lenth>\\d+)");
                for (int i = 0; i < matchs.Count; i++)
                {
                    string line = matchs[i].Result("${ans}");
                    Match match = regex.Match(line);
                    if (!match.Success) throw new ModemDataException(line);
                    int index = int.Parse(match.Result("${index}"));
                    MessageState messageState = (MessageState)int.Parse(match.Result("${stat}"));
                    int lenth = int.Parse(match.Result("${lenth}"));
                    messages.Add(new Message(index, messageState, lenth, matchs[++i].Result("${ans}")));
                }
            }
            else
            {
                List<Match> matchs = generalOperate(string.Format("AT+CMGL=\"{0}\"\r", state.ToString().Replace('_', ' ')),
                    "(?<ans>[ \\w\\p{P}\\p{S}]+)", "Modem未启动");
                Regex regex = new Regex("^\\+CMGL:\\s*(?<index>\\d+),\"(?<stat>[\\s\\w]+)\",\"(?<oada>\\d+)\","
                    + "(?<alpha>[ \"\\w\\d]*),(\"(?<scts>[,/:-\\+\\d]+)\")*");
                for (int i = 0; i < matchs.Count; i += 2)
                {
                    string line = matchs[i].Result("${ans}");
                    Match match = regex.Match(line);
                    if (!match.Success) throw new ModemDataException(line);
                    int index = int.Parse(match.Result("${index}"));
                    MessageState messageState = (MessageState)Enum.Parse(typeof(MessageState),
                        match.Result("${stat}").Replace(" ", "_"), true);
                    string terminalAddress = match.Result("${oada}");
                    string serviceCenterTimeStamp = match.Result("${scts}");
                    messages.Add(new Message(index, messageState, terminalAddress, serviceCenterTimeStamp, matchs[i + 1].Result("${ans}")));
                }
            }
            return messages.ToArray();
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="message">需要发送的短信</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SendMessage(Message message)
        {
            string tpdu = message.Tpdu;
            int timeout = Timeout;
            Timeout = 300000;
            try
            {
                SendDirect(string.Format("AT+CMGS={0}\r", tpdu.Length / 2));
                string line = readLine();
                if (!line.EndsWith("\u003E\u0020"))
                {
                    sendCmd("\u001B");
                    throw new ModemDataException("短信未发送成功");
                }
                generalOperate(string.Format("00{0}\u001A", tpdu), "(?<ans>[ \\w\\p{P}\\p{S}]+)", "短信未发送成功");
            }
            finally { Timeout = timeout; }
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobileNum">目标号码</param>
        /// <param name="userData">短信内容(PDU串)</param>
        /// <param name="encode">编码</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SendMessage(string mobileNum, string userData, DataCodingScheme encode)
        {
            for (int i = 0; i < userData.Length; i += MAXNOTELETLENTH)
            {
                string str = userData.Length - i > MAXNOTELETLENTH ?
                    userData.Substring(i, MAXNOTELETLENTH) : userData.Substring(i, userData.Length - i);
                SendMessage(new Message(mobileNum, str, encode));
            }
        }

        /// <summary>
        /// 删除短信
        /// </summary>
        /// <param name="index">短信在存储卡中的位置</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void DeleteMessage(int index)
        {
            generalOperate(string.Format("AT+CMGD={0}\r", index), "", "Modem未启动");
        }

        /// <summary>
        /// 删除短信
        /// </summary>
        /// <param name="index">短信在存储卡中的位置</param>
        /// <param name="flag">删除选项</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void DeleteMessage(int index, DeleteFlag flag)
        {
            generalOperate(string.Format("AT+CMGD={0},{1}\r", index, (int)flag), "", "Modem未启动");
        }

        /// <summary>
        /// 获取服务中心号码
        /// </summary>
        /// <returns>服务中心号码</returns>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public string GetServiceCenterAddress()
        {
            return generalOperate("AT+CSCA?\r", "^\\+CSCA:\\s*\"(?<sca>[\\+\\d]+)\",(?<type>\\d+)", "Modem未启动")[0].Result("${sca}");
        }

        /// <summary>
        /// 设置服务中心号码
        /// </summary>
        /// <param name="serviceCenterAddress">服务中心号码</param>
        /// <exception cref="ModemClosedException">Modem未打开时引发该异常</exception>
        /// <exception cref="TimeoutException">读取或发送未在超时时间到期之前完成引发该异常</exception>
        /// <exception cref="ModemUnsupportedException">Modem启动未完成或没有SIM卡时引发该异常</exception>
        /// <exception cref="ModemDataException">Modem接收到的数据异常</exception>
        public void SetServiceCenterAddress(string serviceCenterAddress)
        {
            generalOperate(string.Format("AT+CSCA=\"{0}\"\r", serviceCenterAddress), "", "Modem未启动");
        }

        #endregion

        #region SUPPLEMENTARY SERVICES COMMANDS
        #endregion

        #region DATA COMMANDS
        #endregion

        #region FAX COMMANDS
        #endregion

        #region FAX CLASS 2 COMMANDS
        #endregion

        #region V24-V25 COMMANDS

        /// <summary>
        /// 设置Modem是否回显
        /// </summary>
        /// <param name="echo">True回显, False不回显</param>
        public void SetEcho(bool echo)
        {
            generalOperate(string.Format("ATE{0}\r", echo ? "1" : "0"), "", "Modem未启动");
        }

        /// <summary>
        /// 恢复出厂设置
        /// </summary>
        public void RestoreFactorySetting()
        {
            generalOperate(string.Format("AT&F\r"), "", "Modem未启动");
        }

        #endregion

        #region SPECIFIC AT COMMANDS
        #endregion

        #region DATA COMMANDS
        #endregion

        #region SIM TOOLKIT COMMANDS
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
    public enum MessageFormat
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

    #region 事件参数

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.Datatransmitted事件提供参数
    /// </summary>
    public class SerialDataEventArgs : EventArgs
    {
        /// <summary>
        /// 指示该数据是否为接受的数据
        /// </summary>
        public string SerialString { get; private set; }

        /// <summary>
        /// 初始化类Vultrue.Communication.SerialDataEventArgs的新实例
        /// </summary>
        /// <param name="serialString">传输的数据行</param>
        public SerialDataEventArgs(string serialString) { SerialString = serialString; }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.ModemError事件提供参数
    /// </summary>
    public class ModemErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 初始化类Vultrue.Communication.CommunicationErrorEventArgs的新实例
        /// </summary>
        /// <param name="message">错误信息</param>
        public ModemErrorEventArgs(string message) { Message = message; }
    }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.Ring事件提供参数
    /// </summary>
    public class RingingEventArgs : EventArgs { }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.NoteletReceived事件提供参数
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 短信存储的媒体
        /// </summary>
        public string StoreMedia { get; set; }

        /// <summary>
        /// 短信位置
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 初始化类Vultrue.Communication.NoteletReceivedEventArgs的新实例
        /// </summary>
        /// <param name="storeMedia">短信存储的媒体</param>
        /// <param name="index">短信位置</param>
        public MessageReceivedEventArgs(string storeMedia, int index)
        {
            StoreMedia = storeMedia;
            Index = index;
        }
    }

    /// <summary>
    /// 为 Vultrue.Communication.GSMModem.NetworkRegistrationChanged 事件提供参数
    /// </summary>
    public class NetworkRegistrationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 网络注册状态
        /// </summary>
        public NetworkState State { get; private set; }

        /// <summary>
        /// 区域代码
        /// </summary>
        public string LocationAreaCode { get; private set; }

        /// <summary>
        /// 基站ID
        /// </summary>
        public string CellID { get; private set; }

        /// <summary>
        /// 初始化类 Vultrue.Communication.NetworkRegistrationChangedEventArgs 的新实例
        /// </summary>
        /// <param name="state">网络注册状态</param>
        /// <param name="locationAreaCode">区域代码</param>
        /// <param name="cellID">基站ID</param>
        public NetworkRegistrationChangedEventArgs(NetworkState state, string locationAreaCode, string cellID)
        {
            State = state;
            LocationAreaCode = locationAreaCode;
            CellID = cellID;
        }
    }

    #endregion

    #region 异常

    /// <summary>
    /// Modem流关闭时对Modem进行操作引发该异常
    /// </summary>
    public class ModemClosedException : Exception
    {
        /// <summary>
        /// 构造ModemClosedException异常
        /// </summary>
        /// <param name="message"></param>
        public ModemClosedException(string message) : base(message) { }
    }

    /// <summary>
    /// Modem尚未启动完成时或不支持该功能时, 对Modem进行操作引发该异常
    /// </summary>
    public class ModemUnsupportedException : Exception
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorID { get; private set; }

        /// <summary>
        /// 构造ModemUnsupportedException异常
        /// </summary>
        /// <param name="message"></param>
        public ModemUnsupportedException(string message) : base(message) { }

        /// <summary>
        /// 构造ModemUnsupportedException异常
        /// </summary>
        /// <param name="errorid"></param>
        /// <param name="message"></param>
        public ModemUnsupportedException(int errorid, string message) : base(message)
        {
            ErrorID = errorid;
        }
    }

    /// <summary>
    /// Modem接受的数据异常
    /// </summary>
    public class ModemDataException : Exception
    {
        /// <summary>
        /// 构造ModemDataException异常
        /// </summary>
        /// <param name="message"></param>
        public ModemDataException(string message) : base(message) { }
    }

    #endregion
}
