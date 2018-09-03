using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace Vultrue.Communication
{
    /// <summary>
    /// GSMModem驱动程序
    /// </summary>
    [DefaultProperty("PortName")]
    [DefaultEvent("NoteletReceived")]
    public partial class GSMModem : Component
    {
        #region 变量
        private const int MAXNOTELETLENTH = 280;
        private bool isOpened = false;
        private object modemLock = new object();
        private Queue<EventArgs> eventQueue;
        #endregion

        #region 构造

        /// <summary>
        /// 构造GSMModem对象
        /// </summary>
        public GSMModem()
        {
            InitializeComponent();
            initializeComponent();
        }

        /// <summary>
        /// 构造GSMModem对象, 并加入容器
        /// </summary>
        /// <param name="container"></param>
        public GSMModem(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            eventQueue = new Queue<EventArgs>();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 读取是否Modem已打开
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        /// <summary>
        /// 读取或设置端口名称
        /// </summary>
        [DefaultValue("COM1")]
        public string PortName
        {
            get { return serialPort.PortName; }
            set { serialPort.PortName = value; }
        }

        /// <summary>
        /// 读取或设置波特率
        /// </summary>
        [DefaultValue(19200)]
        public int BaudRate
        {
            get { return serialPort.BaudRate; }
            set { serialPort.BaudRate = value; }
        }

        /// <summary>
        /// 读取或设置奇偶校验位
        /// </summary>
        [DefaultValue(Parity.None)]
        public Parity Parity
        {
            get { return serialPort.Parity; }
            set { serialPort.Parity = value; }
        }

        /// <summary>
        /// 读取或设置数据位
        /// </summary>
        [DefaultValue(8)]
        public int DataBits
        {
            get { return serialPort.DataBits; }
            set { serialPort.DataBits = value; }
        }

        /// <summary>
        /// 读取或设置停止位
        /// </summary>
        [DefaultValue(StopBits.One)]
        public StopBits StopBits
        {
            get { return serialPort.StopBits; }
            set { serialPort.StopBits = value; }
        }

        /// <summary>
        /// 读取或设置握手方式
        /// </summary>
        [DefaultValue(Handshake.RequestToSend)]
        public Handshake Handshake
        {
            get { return serialPort.Handshake; }
            set { serialPort.Handshake = value; }
        }

        /// <summary>
        /// 读取或设置读取超时毫秒数
        /// </summary>
        [DefaultValue(3000)]
        public int ReadTimeout
        {
            get { return serialPort.ReadTimeout; }
            set { serialPort.ReadTimeout = value; }
        }

        /// <summary>
        /// 读取或设置写入超时毫秒数
        /// </summary>
        [DefaultValue(3000)]
        public int WriteTimeout
        {
            get { return serialPort.WriteTimeout; }
            set { serialPort.WriteTimeout = value; }
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
                    serialPort.PortName,
                    serialPort.BaudRate,
                    serialPort.DataBits,
                    (int)serialPort.Parity,
                    (int)serialPort.StopBits,
                    (int)serialPort.Handshake);
            }
            set
            {
                string[] setting = value.Split(',');
                if (setting.Length < 6) throw new ArgumentException();
                bool isOpen = serialPort.IsOpen;
                if (isOpen) serialPort.Close();
                try
                {
                    serialPort.PortName = setting[0];
                    serialPort.BaudRate = int.Parse(setting[1]);
                    serialPort.DataBits = int.Parse(setting[2]);
                    serialPort.Parity = (System.IO.Ports.Parity)int.Parse(setting[3]);
                    serialPort.StopBits = (System.IO.Ports.StopBits)int.Parse(setting[4]);
                    serialPort.Handshake = (System.IO.Ports.Handshake)int.Parse(setting[5]);
                }
                catch (Exception ex) { throw new ArgumentException(ex.Message, ex); }
                finally { if (isOpen) serialPort.Open(); }
            }
        }

        /// <summary>
        /// 标签
        /// </summary>
        [DefaultValue(null)]
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
        public event EventHandler Closed;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的通信错误事件的方法。
        /// </summary>
        public event EventHandler<ModemErrorEventArgs> ModemError;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的发送/接收数据事件的方法。
        /// </summary>
        public event EventHandler<SerialDataEventArgs> DataTransmitted;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的来电事件的方法。
        /// </summary>
        public event EventHandler Ringing;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的接收到短信事件的方法。
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.GSMModem 对象的网络注册信息已改变的
        /// </summary>
        public event EventHandler<NetworkRegistrationChangedEventArgs> NetworkRegistrationChanged;

        #endregion

        #region 基本方法

        /// <summary>
        /// 打开Modem连接
        /// </summary>
        /// <exception cref="System.InvalidOperationException">指定的端口已打开</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">此实例的一个或多个属性无效。
        ///     例如，System.IO.Ports.SerialPort.Parity、System.IO.Ports.SerialPort.DataBits
        ///     或 System.IO.Ports.SerialPort.Handshake 属性不是有效值；
        ///     System.IO.Ports.SerialPort.BaudRate小于或等于零；
        ///     System.IO.Ports.SerialPort.ReadTimeout 或 System.IO.Ports.SerialPort.WriteTimeout
        ///     属性小于零且不是System.IO.Ports.SerialPort.InfiniteTimeout。</exception>
        /// <exception cref="System.ArgumentException">端口名称不是以“COM”开始的或端口的文件类型不受支持。</exception>
        /// <exception cref="System.IO.IOException">此端口处于无效状态或尝试设置基础端口状态失败。
        ///     例如，从此 System.IO.Ports.SerialPort 对象传递的参数无效。</exception>
        /// <exception cref="System.UnauthorizedAccessException">对端口的访问被拒绝。</exception>
        public void Open()
        {
            serialPort.Open();
            new Thread(() => { isOpened = true; receivedDataDeal(); }).Start();
            if (Opened != null) Opened(this, null);
        }

        /// <summary>
        /// 关闭Modem连接
        /// </summary>
        /// <exception cref="System.InvalidOperationException">指定的端口未打开</exception>
        public void Close()
        {
            isOpened = false;
            System.IO.Ports.Handshake handshake = serialPort.Handshake;
            try
            {
                serialPort.Handshake = System.IO.Ports.Handshake.None;
                serialPort.Close();
            }
            finally
            {
                serialPort.Handshake = handshake;
            }
            if (Closed != null) Closed(this, null);
        }

        #endregion

        #region 核心处理

        /// <summary>
        /// 向Modem发出一条指令
        /// </summary>
        /// <param name="cmd">指令</param>
        private void sendCmd(string cmd)
        {
            if (!serialPort.IsOpen) throw new ModemClosedException("Modem is closed");
            serialPort.Write(cmd);
            eventQueue.Enqueue(new SerialDataEventArgs(false, cmd));
        }

        /// <summary>
        /// 从串口读取一行数据
        /// </summary>
        /// <returns>数据</returns>
        private string readLine()
        {
            if (!serialPort.IsOpen) throw new ModemClosedException("Modem is closed");
            string str = "";
            try
            {
                str = serialPort.ReadLine();
                return str;
            }
            finally
            {
                if (str.Length > 0) eventQueue.Enqueue(new SerialDataEventArgs(true, str));
            }
        }

        /// <summary>
        /// 串口数据接收处理, 单独线程
        /// </summary>
        private void receivedDataDeal()
        {
            while (isOpened)
            {
                try
                {
                    lock (modemLock) { dealUnsolicitedResultCodes(); }
                }
                catch (Exception ex)
                {
                    if (ModemError != null) ModemError(this, new ModemErrorEventArgs(ex.Message));
                }
                while (eventQueue.Count > 0)
                {
                    EventArgs e = eventQueue.Dequeue();
                    if (e is SerialDataEventArgs)
                    {
                        if (DataTransmitted != null) DataTransmitted(this, (SerialDataEventArgs)e);
                    }
                    else if (e is RingingEventArgs)
                    {
                        if (Ringing != null) Ringing(this, (RingingEventArgs)e);
                    }
                    else if (e is MessageReceivedEventArgs)
                    {
                        if (MessageReceived != null) MessageReceived(this, (MessageReceivedEventArgs)e);
                    }
                    else if (e is NetworkRegistrationChangedEventArgs)
                    {
                        if (NetworkRegistrationChanged != null) NetworkRegistrationChanged(this, (NetworkRegistrationChangedEventArgs)e);
                    }
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// 处理未请求的返回值
        /// </summary>
        private void dealUnsolicitedResultCodes()
        {
            if (!serialPort.IsOpen) throw new ModemClosedException("Modem is closed");
            while (serialPort.BytesToRead > 0)
            {
                string line = readLine();
                if (line.Length <= 1) continue;
                Match match = new Regex("^RING").Match(line);
                if (match.Success)
                {
                    eventQueue.Enqueue(new RingingEventArgs());
                    continue;
                }
                match = new Regex("^\\+CMTI:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)").Match(line);
                if (match.Success)
                {
                    string[] result = match.Result("${ans}").Replace("\"", "").Split(',');
                    eventQueue.Enqueue(new MessageReceivedEventArgs(result[0], int.Parse(result[1])));
                    continue;
                }
                match = new Regex("^\\+CREG:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)").Match(line);
                if (match.Success)
                {
                    string[] result = match.Result("${ans}").Replace("\"", "").Split(',');
                    NetworkState state = (NetworkState)int.Parse(result[0]);
                    string locationAreaCode = result.Length > 1 ? result[1] : "";
                    string cellID = result.Length > 2 ? result[2] : "";
                    eventQueue.Enqueue(new NetworkRegistrationChangedEventArgs(state, locationAreaCode, cellID));
                    continue;
                }
            }
        }

        private static Regex regexError1 = new Regex("^ERROR");
        private static Regex regexError2 = new Regex("^\\+CM[ES] ERROR:\\s*(?<errid>\\d+)");
        private static Regex regexOK = new Regex("^OK");

        /// <summary>
        /// 对Modem进行操作
        /// </summary>
        /// <param name="instruction">操作指令</param>
        /// <param name="validateRegex">验证表达式</param>
        /// <param name="errorInfo">错误提示</param>
        /// <returns>执行结果</returns>
        private List<Match> operateModem(string instruction, string validateRegex, string errorInfo)
        {
            lock (modemLock)
            {
                List<Match> matchs = new List<Match>();
                dealUnsolicitedResultCodes();
                sendCmd(instruction);
                while (true)
                {
                    string line;
                    if ((line = readLine()) == "\r") line = readLine();
                    Match match = regexError1.Match(line);
                    if (match.Success) throw new ModemUnsupportedException(errorInfo);
                    match = regexError2.Match(line);
                    if (match.Success) throw new ModemUnsupportedException(int.Parse(match.Result("${errid}")), errorInfo);
                    if (regexOK.Match(line).Success) return matchs;
                    match = new Regex(validateRegex).Match(line);
                    if (!match.Success) throw new ModemDataException(line);
                    matchs.Add(match);
                }
            }
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
                dealUnsolicitedResultCodes();
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
            return operateModem("AT+CGMI\r", "^(?<ans>[ \\w]+)", "")[0].Result("${ans}");
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
            return operateModem("AT+CGMM\r", "^(?<ans>[ \\w]+)", "")[0].Result("${ans}");
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
            return operateModem("AT+CGMR\r", "^(?<ans>[ \\w\\p{P}\\p{S}]+)", "")[0].Result("${ans}");
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
            return operateModem("AT+CGSN\r", "^(?<ans>\\d+)", "IMEI not found in EEPROM")[0].Result("${ans}");
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
            return operateModem("AT+CSCS=?\r", "^\\+CSCS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "Modem未启动")[0]
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
            return operateModem("AT+CSCS?\r", "^\\+CSCS:\\s*\"(?<ans>\\w+)\"", "Modem未启动")[0].Result("${ans}");
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
            operateModem(string.Format("AT+CSCS=\"{0}\"\r", characterSet),
                "", string.Format("Modem不支持字符集\"{0}\"", characterSet));
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
            return operateModem("AT+CIMI\r", "^(?<ans>\\d+)", "Modem未启动")[0].Result("${ans}");
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
            return operateModem("AT+CCID\r", "^\\+CCID:\\s*\"(?<ans>\\d+)\"", "Modem未启动")[0].Result("${ans}");
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
            return operateModem("AT+GCAP\r", "^\\+GCAP:\\s*(?<ans>[ \\w\\p{P}\\p{S}]+)", "")[0]
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
            operateModem("AT+CPOF\r", "", "");
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
            return int.Parse(operateModem("AT+CFUN?\r", "^\\+CFUN:\\s*(?<ans>\\d+)", "")[0].Result("${ans}"));
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
            operateModem(string.Format("AT+CFUN={0}\r", level), "", "不支持的功能级别");
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
            return (ActivityStatus)int.Parse(operateModem("AT+CPAS\r", "^\\+CPAS:\\s*(?<ans>\\d+)", "")[0]
                .Result("${ans}"));
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
            return operateModem("AT+CMEE?\r", "^\\+CMEE:\\s*(?<ans>\\d+)", "")[0].Result("${ans}") == "1";
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
            operateModem(string.Format("AT+CMEE={0}\r", (isReportMEError ? "1" : "0")), "", "");
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
            operateModem(string.Format("AT+CKPD=\"{0}\"\r", keypadControl), "^\\+CCFC:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)",
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
            return DateTime.Parse("20" + operateModem("AT+CCLK?\r", "^\\+CCLK:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "")[0]
                .Result("${ans}").Trim('\"'));
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
            operateModem(string.Format("AT+CCLK=\"{0:yy/MM/dd,HH:mm:ss}\"\r", clock), "", "日期时间格式错误");
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
            List<Match> matchs = operateModem("AT+CALA?\r", "^\\+CALA:\\s*\"(?<cala>[/:,\\d]+)\",(?<index>\\d+)", "");
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
            operateModem(string.Format("AT+CALA=\"{0:yy/MM/dd,HH:mm:ss}\"\r", alarm), "", "ModemAlarm位置已满");
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
            return int.Parse(operateModem("AT+CRSL?\r", "^\\+CRSL:\\s*(?<ans>\\d+)", "")[0].Result("${ans}"));
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
            operateModem(string.Format("AT+CRSL={0}\r", soundLevel), "", "不支持的音量数值");
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
            Match match = operateModem("AT+CSQ\r", "^\\+CSQ:\\s*(?<rssi>\\d+),(?<ber>\\d+)", "")[0];
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
            operateModem(string.Format("AT+CREG={0}\r", mode), "", "不支持的注册信息模式");
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
            Match match = operateModem("AT+CREG?\r", "^\\+CREG:\\s*(?<mode>\\d+),(?<stat>\\d+)" +
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
            return operateModem("AT+CPBS=?\r", "^\\+CPBS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)", "Modem未启动")[0]
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
            Match match = operateModem("AT+CPBS?\r", "^\\+CPBS:\\s*\"(?<mem>\\d+)\",(?<used>\\d+),(?<total>\\d+)",
                "Modem未启动")[0];
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
            operateModem(string.Format("AT+CPBS=\"{0}\"\r", storage), "", "Modem未启动");
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
            Match match = operateModem("AT+CPBR=?\r", "^\\+CPBR:\\s*\\((?<si>\\d+)-(?<ei>\\d+)\\)"
                + ",(?<mp>\\d+),(?<mt>\\d+)", "Modem未启动")[0];
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
            Match match = operateModem(string.Format("AT+CPBR={0}\r", index), "^\\+CPBR:\\s*(?<index>\\d+)" +
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
            List<Match> matchs = operateModem(string.Format("AT+CPBR={0},{1}\r", startIndex, endIndex),
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
            List<Match> matchs = operateModem(string.Format("AT+CPBF=\"{0}\"\r", text),
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
            operateModem(string.Format("AT+CPBW={0}\r", index), "", "Modem未启动");
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
            operateModem(string.Format("AT+CPBW={0},\"{1}\",{2},\"{3}\"\r", index, number, type, text), "", "Modem未启动");
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
            operateModem(string.Format("AT+CPBW=,\"{0}\",{1},\"{2}\"\r", number, type, text), "", "Modem未启动");
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
            Match match = operateModem(string.Format("AT+CPBP=\"{0}\"\r", number),
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
            Match match = operateModem(string.Format("AT+CPBN={0}\r", (int)mode),
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
            return int.Parse(operateModem("AT+CSMS?\r", "^\\+CSMS:\\s*(?<service>\\d+)" +
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
            operateModem(string.Format("AT+CSMS={0}\r", messageService),
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
            string cmd;
            if (GetMessageFormat() == MessageFormat.Text) cmd = "AT+CNMA\r";
            else if (pdustr == null) cmd = "AT+CNMA=0\r";
            else cmd = string.Format("AT+CNMA={0},{1}\r{3}\u001A", (acknowledge ? "1" : "2"), pdustr.Length / 2, pdustr);
            operateModem(cmd, "(?<ans>[ \\w\\p{P}\\p{S}]+)", "没有要确认的短信");
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
            Match match = operateModem("AT+CPMS=?\r", "^\\+CPMS:\\s*\\(\\((?<rld>[\",\\w]+)\\),\\((?<ws>[\",\\w]+)\\)\\)", "Modem未启动")[0];
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
            Match match = operateModem("AT+CPMS?\r", "^\\+CPMS:\\s*\"(?<rld>\\w+)\",(?<rldu>\\d+)" +
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
            Match match = operateModem(string.Format("AT+CPMS=\"{0}\"\r", rldStorage),
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
            Match match = operateModem(string.Format("AT+CPMS=\"{0}\",\"{1}\"\r", rldStorage, wsStorage),
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
            return (MessageFormat)int.Parse(operateModem("AT+CMGF?\r", "^\\+CMGF:\\s*(?<ans>\\d+)",
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
            operateModem(string.Format("AT+CMGF={0}\r", (int)messageFormat), "", "Modem未启动");
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
            operateModem("AT+CSAS\r", "", "Modem未启动");
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
            operateModem("AT+CRES\r", "", "Modem未启动");
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
            return operateModem("AT+CSDH?\r", "^\\+CSDH:\\s*(?<ans>\\d+)", "Modem未启动")[0].Result("${ans}") == "1";
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
            operateModem(string.Format("AT+CSDH={0}\r", (isShow ? "1" : "0")), "", "Modem未启动");
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
            Match match = operateModem("AT+CNMI?\r", "^\\+CNMI:\\s*(?<mode>\\d+)" +
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
            operateModem(string.Format("AT+CNMI={0},{1},{2},{3},{4}\r", indication.Mode, indication.MessageTreat,
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
                List<Match> matchs = operateModem(string.Format("AT+CMGR={0}\r", index),
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
                List<Match> matchs = operateModem(string.Format("AT+CMGR={0}\r", index),
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
                List<Match> matchs = operateModem(string.Format("AT+CMGL={0}\r", (int)state),
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
                List<Match> matchs = operateModem(string.Format("AT+CMGL=\"{0}\"\r", state.ToString().Replace('_', ' ')),
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
                    messages.Add(new Message(index, messageState, terminalAddress,
                        serviceCenterTimeStamp, matchs[i + 1].Result("${ans}")));
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
            int timeout = serialPort.ReadTimeout;
            serialPort.ReadTimeout = 300000;
            try
            {
                operateModem(string.Format("AT+CMGS={0}\r00{1}\u001A", tpdu.Length / 2, tpdu),
                    "(?<ans>[ \\w\\p{P}\\p{S}]+)", "短信未发送成功");
            }
            finally { serialPort.ReadTimeout = timeout; }
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
            operateModem(string.Format("AT+CMGD={0}\r", index), "", "Modem未启动");
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
            operateModem(string.Format("AT+CMGD={0},{1}\r", index, (int)flag), "", "Modem未启动");
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
            return operateModem("AT+CSCA?\r", "^\\+CSCA:\\s*\"(?<sca>[\\+\\d]+)\",(?<type>\\d+)",
                "Modem未启动")[0].Result("${sca}");
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
            operateModem(string.Format("AT+CSCA=\"{0}\"\r", serviceCenterAddress), "", "Modem未启动");
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
            operateModem(string.Format("ATE{0}\r", echo ? "1" : "0"), "", "Modem未启动");
        }

        /// <summary>
        /// 恢复出厂设置
        /// </summary>
        public void RestoreFactorySetting()
        {
            operateModem(string.Format("AT&F\r"), "", "Modem未启动");
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
        /// 传输的数据行
        /// </summary>
        public bool IsReceived { get; private set; }


        /// <summary>
        /// 指示该数据是否为接受的数据
        /// </summary>
        public string SerialString { get; private set; }

        /// <summary>
        /// 初始化类Vultrue.Communication.SerialDataEventArgs的新实例
        /// </summary>
        /// <param name="isReceived">是否为接受的数据</param>
        /// <param name="serialString">传输的数据行</param>
        public SerialDataEventArgs(bool isReceived, string serialString)
        {
            IsReceived = isReceived;
            SerialString = serialString;
        }
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
