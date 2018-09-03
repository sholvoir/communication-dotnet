using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO.Ports;

namespace Vultrue.Communication
{
    [DefaultProperty("PortName")]
    [DefaultEvent("NoteletReceived")]
    public partial class GSMModem : Component
    {
        #region 变量
        private const int MAXNOTELETLENTH = 276;
        private Queue<Task> tasks = new Queue<Task>();
        private List<byte> buffer = new List<byte>();
        private Task test = new SignalRead();
        private Task noteread = new NoteletRead();
        private Task current;
        private int intensity;
        private int waitNum;
        private int testNum;
        private int smsrNum = 45;
        private bool autoTest = true;
        private object tag;
        #endregion

        #region 构造

        public GSMModem()
        {
            InitializeComponent();
        }

        public GSMModem(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 读取或设置是否自动测试
        /// </summary>
        [DefaultValue(true)]
        public bool Autotest
        {
            get { return autoTest; }
            set { autoTest = value; }
        }

        /// <summary>
        /// 读取信号强度
        /// </summary>
        [Browsable(false)]
        public int Intensity
        {
            get { return intensity; }
        }

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
        public string PortName
        {
            get { return serialPort.PortName; }
            set { serialPort.PortName = value; }
        }

        /// <summary>
        /// 读取或设置波特率
        /// </summary>
        public int BaudRate
        {
            get { return serialPort.BaudRate; }
            set { serialPort.BaudRate = value; }
        }

        /// <summary>
        /// 读取或设置奇偶校验位
        /// </summary>
        public Parity Parity
        {
            get { return serialPort.Parity; }
            set { serialPort.Parity = value; }
        }

        /// <summary>
        /// 读取或设置数据位
        /// </summary>
        public int DataBits
        {
            get { return serialPort.DataBits; }
            set { serialPort.DataBits = value; }
        }

        /// <summary>
        /// 读取或设置停止位
        /// </summary>
        public StopBits StopBits
        {
            get { return serialPort.StopBits; }
            set { serialPort.StopBits = value; }
        }

        /// <summary>
        /// 读取或设置握手方式
        /// </summary>
        public Handshake Handshake
        {
            get { return serialPort.Handshake; }
            set { serialPort.Handshake = value; }
        }

        /// <summary>
        /// 读取或设置读取超时毫秒数
        /// </summary>
        public int ReadTimeout
        {
            get { return serialPort.ReadTimeout; }
            set { serialPort.ReadTimeout = value; }
        }

        /// <summary>
        /// 读取或设置写入超时毫秒数
        /// </summary>
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
                StringBuilder strb = new StringBuilder(serialPort.PortName);
                strb.Append(",").Append(serialPort.BaudRate.ToString());
                strb.Append(",").Append(serialPort.Parity.ToString().Substring(0, 1));
                strb.Append(",").Append(serialPort.DataBits.ToString());
                strb.Append(",").Append(((int)serialPort.StopBits).ToString());
                return strb.ToString();
            }
        }

        /// <summary>
        /// 标签
        /// </summary>
        [DefaultValue("")]
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的打开事件的方法。
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的关闭事件的方法。
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的测试失败事件的方法。
        /// </summary>
        public event EventHandler ModemFailed;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的发送/接收数据事件的方法。
        /// </summary>
        public event EventHandler<SerialDataEventArgs> DataTransmitted;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的通信错误事件的方法。
        /// </summary>
        public event EventHandler<CommunicationErrorEventArgs> CommunicationError;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的信号强度改变事件的方法。
        /// </summary>
        public event EventHandler IntensityReaded;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的接收到短信事件的方法。
        /// </summary>
        public event EventHandler<NoteletReceivedEventArgs> NoteletReceived;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的发送出短信事件的方法。
        /// </summary>
        public event EventHandler<NoteletSendedEventArgs> NoteletSended;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的电话本写入事件的方法。
        /// </summary>
        public event EventHandler PhoneBookWrited;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的电话本读取事件的方法。
        /// </summary>
        public event EventHandler<PhoneBookReadedEventArgs> PhoneBookReaded;

        /// <summary>
        /// 表示将处理Vultrue.Communication.GSMModem对象的Modem信息读取事件的方法。
        /// </summary>
        public event EventHandler<ModemInfoReadedEventArgs> ModemInfoReaded;

        /// <summary>
        /// 手动任务完成
        /// </summary>
        public event EventHandler ManualTaskFinished;

        #endregion

        #region 方法

        /// <summary>
        /// 启动
        /// </summary>
        public void Open()
        {
            if (!serialPort.IsOpen) serialPort.Open();
            if (!timerBeat.Enabled) timerBeat.Start();
            if (Opened != null) Opened(this, null);
        }

        /// <summary>
        /// 关闭短信服务
        /// </summary>
        public void Close()
        {
            if (timerBeat.Enabled) timerBeat.Stop();
            if (serialPort.IsOpen)
            {
                System.IO.Ports.Handshake hand = serialPort.Handshake;
                serialPort.Handshake = System.IO.Ports.Handshake.None;
                serialPort.Close();
                serialPort.Handshake = hand;
            }
            if (Closed != null) Closed(this, null);
        }

        /// <summary>
        /// 手工调度
        /// </summary>
        public void HandSchedule()
        {
            timerBeat_Elapsed(null, null);
        }

        /// <summary>
        /// 直接发送字符串指令到串口
        /// </summary>
        /// <param name="cmd">指令</param>
        public void SendDirect(string cmd)
        {
            tasks.Enqueue(new Manual(cmd));
        }

        /// <summary>
        /// 进行一次Modem测试
        /// </summary>
        public void TestModem()
        {
            tasks.Enqueue(test);
        }

        /// <summary>
        /// 读取短信
        /// </summary>
        public void ReadNotelet()
        {
            tasks.Enqueue(noteread);
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="notelet">发送的短信</param>
        public void SendNotelet(Notelet notelet)
        {
            tasks.Enqueue(new NoteletSend(notelet));
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobileNum">目标号码</param>
        /// <param name="pdustr">短信内容(PDU串)</param>
        /// <param name="encode">编码</param>
        public void SendNotelet(string mobileNum, string pdustr, Notelet.DCS encode)
        {
            for (int i = 0; i < pdustr.Length; i += MAXNOTELETLENTH)
            {
                string str = pdustr.Length - i > MAXNOTELETLENTH ?
                    pdustr.Substring(i, MAXNOTELETLENTH) : pdustr.Substring(i, pdustr.Length - i);
                tasks.Enqueue(new NoteletSend(new Notelet(mobileNum, str, encode)));
            }
        }

        /// <summary>
        /// 删除短信
        /// </summary>
        /// <param name="location">短信在存储卡中的位置</param>
        public void DeleteNotelet(int location)
        {
            tasks.Enqueue(new NoteletDelete(location));
        }

        /// <summary>
        /// 从电话本读取电话号码
        /// </summary>
        /// <param name="index">电话号码索引</param>
        public void ReadPhonebook(int index)
        {
            tasks.Enqueue(new PhoneRead(index));
        }

        /// <summary>
        /// 向电话本写入电话号码
        /// </summary>
        /// <param name="index">存储位置</param>
        /// <param name="userName">联系人姓名</param>
        /// <param name="phoneNum">联系人电话</param>
        public void WritePhonebook(int index, string phoneNum, string userName)
        {
            tasks.Enqueue(new PhoneWrite(index, phoneNum, 129, userName));
        }

        /// <summary>
        /// 读取Modem信息
        /// </summary>
        public void ReadModemInfo()
        {
            tasks.Enqueue(new ModemInfo());
        }

        /// <summary>
        /// 清除任务列表
        /// </summary>
        public void TaskClear()
        {
            tasks.Clear();
        }

        #endregion

        #region 核心处理

        /// <summary>
        /// 向Modem发出一条指令
        /// </summary>
        /// <param name="cmd">指令</param>
        private void sendCmd(string cmd)
        {
            if (!serialPort.IsOpen)
            {
                if (CommunicationError != null)
                    CommunicationError(this, new CommunicationErrorEventArgs("Modem is closed"));
                return;
            }
            serialPort.Write(cmd);
            if (DataTransmitted != null) DataTransmitted(this, new SerialDataEventArgs(false, cmd));
        }

        /// <summary>
        /// 串口数据接收处理
        /// </summary>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            waitNum = 0;
            if (!serialPort.IsOpen) return;
            while (serialPort.BytesToRead > 0)
            {
                byte b = (byte)serialPort.ReadByte();
                if (b == 0) break;
                else if (buffer.Count == 0) buffer.Add(b);
                else if ((b == 0x0A) && (buffer[buffer.Count - 1] == 0x0D))
                {
                    buffer.RemoveAt(buffer.Count - 1);
                    if (buffer.Count > 0) lineDeal();
                }
                else if ((b == 0x20) && (buffer[buffer.Count - 1] == 0x3E))
                {
                    buffer.Add(b); lineDeal();
                }
                else buffer.Add(b);
            }
        }

        /// <summary>
        /// 处理接收到的行
        /// </summary>
        /// <param name="line">接收的行</param>
        private void lineDeal()
        {
            string line = Encoding.Default.GetString(buffer.ToArray()); buffer.Clear();
            if (DataTransmitted != null) DataTransmitted(this, new SerialDataEventArgs(true, line));
            if (current == null) { exceptionDataDeal(line); return; }
            switch (current.Receive(line))
            {
                case TaskStatus.Finished: // 任务完成
                    current = null; waitNum = 0;
                    break;
                case TaskStatus.Failed: // 任务失败
                    if (IsOpen) { tasks.Enqueue(current); (current = test).Start(this); }
                    break;
                case TaskStatus.Unexpected: // 无法处理
                    exceptionDataDeal(line);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 非期望数据处理
        /// </summary>
        /// <param name="str"></param>
        private void exceptionDataDeal(string answer)
        {
            Regex[] regex = new Regex[] {
				new Regex("\\+CMTI:\\s*\"(?<storemedia>\\w+)\",(?<index>\\d+)"),
				new Regex("RING\\w*") };
            Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
            if (match[0].Success) tasks.Enqueue(new NoteletRead());
            else if (match[1].Success) tasks.Enqueue(new Ringoff());
        }

        /// <summary>
        /// 任务调度, 每100ms调度一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerBeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (current == null) //当前无任务
            {
                if (tasks.Count > 0) //任务队列中有任务
                {
                    if ((current = tasks.Dequeue()) != null)
                    {
                        testNum = 0; current.Start(this);
                    }
                }
                else if (autoTest) //任务队列中无任务
                {
                    if (testNum < 100) testNum++; //100个周期没有任务进行一次信号强度读取
                    else
                    {
                        testNum = 0; tasks.Enqueue(test);
                        if (smsrNum < 3) smsrNum++; //3次信号强度读取进行一次短信读取
                        else
                        {
                            smsrNum = 0; tasks.Enqueue(noteread);
                        }
                    }
                }
            }
            else //当前有任务
            {
                if (waitNum < 300) waitNum++;
                else if (waitNum < 500)
                {
                    tasks.Enqueue(current); current = test;
                    sendCmd("\u001B"); waitNum = 501; current.Start(this);
                }
                else if (waitNum < 900) waitNum++;
                else
                {
                    current = null; waitNum = 0; Close();
                    if (ModemFailed != null) ModemFailed(this, null);

                }
            }
            timerBeat.Start();
        }

        #endregion

        #region 服务

        /// <summary>
        /// 将Unicode字符串编码为PDU串
        /// </summary>
        /// <param name="pdustr">Unicode字符串</param>
        /// <returns>PDU串</returns>
        public static string Unicode2Pdustr(string unicode)
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
        /// <param name="unicode"></param>
        /// <returns></returns>
        public static string Pdustr2Unicode(string pdustr)
        {
            byte[] bytes = new byte[pdustr.Length / 2];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                bytes[i] = byte.Parse(pdustr.Substring(2 * i + 2, 2), NumberStyles.HexNumber);
                bytes[i + 1] = byte.Parse(pdustr.Substring(2 * i, 2), NumberStyles.HexNumber);
            }
            return Encoding.Unicode.GetString(bytes);
        }

        #endregion
    }
}
