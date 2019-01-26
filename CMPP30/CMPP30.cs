using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace CMPP
{
    /// <summary>
    /// CMPP30 短信网关通讯组件（供 SP 使用）。
    /// </summary>
    public class CMPP30
    {

        #region 字段

        #region SP  登录信息
        /// <summary>
        /// SP 企业代码。
        /// </summary>
        private readonly string m_strSPID;
        /// <summary>
        /// SP 密码。
        /// </summary>
        private readonly string m_strPassword;
        /// <summary>
        /// 短信网关地址。
        /// </summary>
        private readonly string m_strAddress;
        /// <summary>
        /// 短信网关端口号。
        /// </summary>
        private readonly int m_iPort;
        #endregion

        #region 网络通讯相关
        private TcpClient m_TcpClient;
        private NetworkStream m_NetworkStream;
        /// <summary>
        /// 最近一次网络传输时间（用于发送 CMPP_ACTIVE_TEST 网络检测包）。
        /// </summary>
        private DateTime m_dtLastTransferTime;
        /// <summary>
        /// TcpClient 接收和发送超时（以秒为单位）。
        /// </summary>
        private int m_iTcpClientTimeout = 5;
        /// <summary>
        /// TcpClient 发送间隔，以毫秒为单位。
        /// </summary>
        private int m_iSendSpan = 10;
        /// <summary>
        /// ACTIVETEST 的时间间隔（C，以秒为单位；标准为 3 分钟）。
        /// </summary>
        /// <remarks>
        /// 当信道上没有数据传输时，通信双方应每隔时间 C 发送链路检测包以维持此连接。
        /// </remarks>
        private int m_iActiveTestSpan = 150;
        /// <summary>
        /// 响应超时时间（T,以秒为单位）。
        /// </summary>
        /// <remarks>
        /// 网关与 SP 之间、网关之间的消息发送后等待 T 秒后未收到响应，应立即重发，再连续发送 N-1 次后仍未得到响应则停发。
        /// </remarks>
        private int m_iTimeOut = 60;
        /// <summary>
        /// 最大发送次数（N）。
        /// </summary>
        /// <remarks>
        /// 网关与 SP 之间、网关之间的消息发送后等待 T 秒后未收到响应，应立即重发，再连续发送 N-1 次后仍未得到响应则停发。
        /// </remarks>
        private int m_iSendCount = 3;
        #endregion

        #region 消息队列相关
        /// <summary>
        /// 滑动窗口（窗口大小固定，CMPP3.0 协议规定：现阶段为 16）。
        /// </summary>
        /// <remarks>
        /// 消息采用并发方式发送，加以滑动窗口流量控制，窗口大小参数W可配置，现阶段建议为 16，即接收方在应答前一次收到的消息最多不超过 16 条。
        /// </remarks>
        private DATA_PACKAGE[] SlidingWindow = new DATA_PACKAGE[16];
        /// <summary>
        /// 消息队列，用于保存所有待发送数据。
        /// </summary>
        private Queue<DATA_PACKAGE> m_MessageQueue = new Queue<DATA_PACKAGE>();
        /// <summary>
        /// 消息流水号（Messae Header）,顺序累加,步长为1,循环使用（每对请求和应答消息的流水号必须相同）。
        /// </summary>
        private static UInt32 m_iSeqID = 0;
        #endregion

        #region 工作线程相关
        private Thread m_SendThread;
        private Thread m_ReceiveThread;
        private object syncRoot = new object();
        private bool cmpp30Stoped = false;
        /// <summary>
        /// 是否执行发送工作的开关（ManualResetEvent 持久开关）。
        /// </summary>
        private ManualResetEvent m_eventSend = new ManualResetEvent(false);
        /// <summary>
        /// 是否执行接收工作的开关（ManualResetEvent 持久开关）。
        /// </summary>
        private ManualResetEvent m_eventReceive = new ManualResetEvent(false);
        /// <summary>
        /// 退出发送线程的开关（一次性开关）。
        /// </summary>
        private AutoResetEvent m_eventSendExit = new AutoResetEvent(false);
        /// <summary>
        /// 退出接收线程的开关（一次性开关）。
        /// </summary>
        private AutoResetEvent m_eventReceiveExit = new AutoResetEvent(false);
        /// <summary>
        /// 启动连接的开关（一次性开关）。
        /// </summary>
        private AutoResetEvent m_eventConnect = new AutoResetEvent(false);
        /// <summary>
        /// 断开连接后再重新连接的开关（一次性开关）。
        /// </summary>
        private AutoResetEvent m_eventDisconnect = new AutoResetEvent(false);
        #endregion

        private SynchronizationContext context = SynchronizationContext.Current;
        #endregion

        #region 常量
        /// <summary>
        /// CMPP 版本。
        /// </summary>
        public const byte CMPP_VERSION_30 = 0x30;
        /// <summary>
        /// 网络故障。
        /// </summary>
        public const uint CMD_ERROR = 0xFFFFFFFF;

        #region SMS数据格式定义
        /// <summary>
        /// ASCII 串。
        /// </summary>
        public const byte CODING_ASCII = 0;
        /// <summary>
        /// 二进制信息。
        /// </summary>
        public const byte CODING_BINARY = 4;
        /// <summary>
        /// UCS2编码。
        /// </summary>
        public const byte CODING_UCS2 = 8;
        /// <summary>
        /// 含GB汉字。
        /// </summary>
        public const byte CODING_GBK = 15;
        #endregion

        #region COMMAND_ID 定义
        /// <summary>
        /// 请求连接。
        /// </summary>
        public const uint CMD_CONNECT = 0x00000001;
        /// <summary>
        /// 请求连接应答。
        /// </summary>
        public const uint CMD_CONNECT_RESP = 0x80000001;
        /// <summary>
        /// 终止连接。
        /// </summary>
        public const uint CMD_TERMINATE = 0x00000002;
        /// <summary>
        /// 终止连接应答。
        /// </summary>
        public const uint CMD_TERMINATE_RESP = 0x80000002;
        /// <summary>
        /// 提交短信。
        /// </summary>
        public const uint CMD_SUBMIT = 0x00000004;
        /// <summary>
        /// 提交短信应答。
        /// </summary>
        public const uint CMD_SUBMIT_RESP = 0x80000004;
        /// <summary>
        /// 短信下发。
        /// </summary>
        public const uint CMD_DELIVER = 0x00000005;
        /// <summary>
        /// 下发短信应答。
        /// </summary>
        public const uint CMD_DELIVER_RESP = 0x80000005;
        /// <summary>
        /// 删除短信。
        /// </summary>
        public const uint CMD_CANCEL = 0x00000007;
        /// <summary>
        /// 删除短信应答。
        /// </summary>
        public const uint CMD_CANCEL_RESP = 0x80000007;
        /// <summary>
        /// 激活测试。
        /// </summary>
        public const uint CMD_ACTIVE_TEST = 0x00000008;
        /// <summary>
        /// 激活测试应答。
        /// </summary>
        public const uint CMD_ACTIVE_TEST_RESP = 0x80000008;
        #endregion

        #endregion

        #region 属性
        /// <summary>
        /// SMS 事件。
        /// </summary>
        public event EventHandler<SMSEventArgs> SMS;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化 <see cref="CMPP30"/> 类新实例。
        /// </summary>
        public CMPP30(string spid, string password, string address, int port)
        {
            m_strSPID = spid;
            m_strPassword = password;
            m_strAddress = address;
            m_iPort = port;

            // 初始化滑动窗口。
            for (int i = 0; i < SlidingWindow.Length; i++)
            {
                SlidingWindow[i] = new DATA_PACKAGE();
            }
        }
        #endregion

        #region 私有方法

        #region 帮助函数
        /// <summary>
        /// 连接到 ISMG。
        /// </summary>
        private bool Connect()
        {
            try
            {
                m_TcpClient = new TcpClient();
                m_TcpClient.ReceiveTimeout = m_TcpClient.SendTimeout = m_iTcpClientTimeout * 1000;
                m_TcpClient.Connect(m_strAddress, m_iPort);
                m_NetworkStream = m_TcpClient.GetStream();

                DateTime dt = DateTime.Now;
                CMPP_CONNECT conn = new CMPP_CONNECT();
                conn.Head = new CMPP_HEAD();
                conn.Head.CommandID = CMPP30.CMD_CONNECT;
                conn.Head.SequenceID = CreateSeqID();
                conn.SourceAddress = m_strSPID;
                conn.TimeStamp = System.Convert.ToUInt32(string.Format("{0:MMddhhmmss}", dt));
                conn.AuthenticatorSource = CreateDigest(dt);
                conn.Version = CMPP_VERSION_30;

                byte[] bytes = conn.GetBytes();
                m_NetworkStream.Write(bytes, 0, (int)conn.Head.TotalLength);

                // 等待 RESPONSE 5 秒。
                int i;
                for (i = 0; i < 5000 && !m_NetworkStream.DataAvailable; i += 10)
                {
                    Thread.Sleep(10);
                }

                if (i >= 5000)
                {
                    FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "等待 CONNECT_RESP 超时", DateTime.Now));
                    return false;
                }

                CMPP_HEAD head = ReadHead();

                if (head.CommandID != CMD_CONNECT_RESP)
                {
                    FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "未正确接收 CONNECT_RESP", DateTime.Now));
                    return false;
                }

                // 读取 RESP。
                CMPP_CONNECT_RESP resp = new CMPP_CONNECT_RESP();
                resp.Head = head;
                try
                {
                    if (m_NetworkStream.DataAvailable)
                    {
                        byte[] buffer = new byte[resp.Head.TotalLength - Marshal.SizeOf(resp.Head)];
                        m_NetworkStream.Read(buffer, 0, buffer.Length);
                        resp.Status = Convert.ToUInt32(buffer, 0);
                        resp.AuthenticatorISMG = new Byte[16];
                        Array.Copy(buffer, 4, resp.AuthenticatorISMG, 0, 16);
                        resp.Version = buffer[buffer.Length - 1];
                    }
                }
                catch
                {
                    resp.Head.CommandID = CMD_ERROR;
                }

                if (resp.Status != 0)
                {
                    switch (resp.Status)
                    {
                        case 1:
                            FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "消息结构错", DateTime.Now));
                            break;
                        case 2:
                            FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "非法源地址", DateTime.Now));
                            break;
                        case 3:
                            FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "认证错", DateTime.Now));
                            break;
                        case 4:
                            FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, "版本太高", DateTime.Now));
                            break;
                        default:
                            FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, string.Format("其他错误（错误码：{0}）", resp.Status), DateTime.Now));
                            break;
                    }
                    return false;
                }

                // 连接完毕。
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT, resp, DateTime.Now));
                return true;
            }
            catch (Exception ex)
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_CONNECT_ERROR, ex.Message, DateTime.Now));
                return false;
            }
        }
        /// <summary>
        /// 断开与 ISMG 的连接。
        /// </summary>
        private void Disconnect()
        {
            try
            {
                CMPP_HEAD Head = new CMPP_HEAD();

                Head.CommandID = CMPP30.CMD_TERMINATE;
                Head.SequenceID = CreateSeqID();
                Head.TotalLength = (uint)Marshal.SizeOf(Head);

                byte[] buffer = Head.GetBytes();
                m_NetworkStream.Write(
                    buffer,
                    0,
                    (int)Head.TotalLength);

                // 等待RESPONSE 5 秒。
                int i;
                for (i = 0; i < 5000 && !m_NetworkStream.DataAvailable; i += 10)
                {
                    Thread.Sleep(10);
                }

                CloseTcpClient();

                if (i >= 5000)
                {
                    FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_DISCONNECT_ERROR, "等待 TERMINATE_RESP 超时", DateTime.Now));
                    return;
                }

                if (ReadHead().CommandID != CMD_TERMINATE_RESP)
                {
                    FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_DISCONNECT_ERROR, "未正确接收 TERMINATE_RESP", DateTime.Now));
                    return;
                }
                // 断开连接完毕。
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_DISCONNECT, null, DateTime.Now));
            }
            catch (Exception ex)
            {
                CloseTcpClient();
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SP_DISCONNECT_ERROR, ex.Message, DateTime.Now));
            }
        }
        /// <summary>
        /// 断开 TCP 连接。
        /// </summary>
        private void CloseTcpClient()
        {
            if (m_NetworkStream != null)
            {
                m_NetworkStream.Close();
            }
            if (m_TcpClient != null)
            {
                m_TcpClient.Close();
            }
            m_TcpClient = null;
            m_NetworkStream = null;
        }
        /// <summary>
        /// 计算 CMPP_CONNECT 包的 AuthenticatorSource 字段。
        /// </summary>
        /// <remarks>
        /// MD5(Source_Addr + 9字节的0 + shared secret + timestamp);
        /// </remarks>
        private byte[] CreateDigest(DateTime dt)
        {
            byte[] btContent = new byte[25 + m_strPassword.Length];
            Array.Clear(btContent, 0, btContent.Length);

            // Source_Addr，SP的企业代码（6位）。
            int iPos = 0;
            foreach (char ch in m_strSPID)
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }

            // 9字节的0。
            iPos += 9;

            // password，由 China Mobile 提供（长度不固定）。
            foreach (char ch in m_strPassword)
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }

            // 时间戳（10位）。
            foreach (char ch in string.Format("{0:MMddhhmmss}", dt))
            {
                btContent[iPos] = (byte)ch;
                iPos++;
            }
            return new MD5CryptoServiceProvider().ComputeHash(btContent);
        }
        /// <summary>
        /// 引发 SMS 事件。
        /// </summary>
        private void FireOnSMSStateChangedEvent(SMSEventArgs e)
        {
            if (context == null)
            {
                try
                {
                    OnSMS(e);
                }
                catch
                { }
            }
            else
            {
                context.Post(delegate(object obj) { ((Action<SMSEventArgs>)obj)(e); }, new Action<SMSEventArgs>(OnSMS));
            }
        }
        #endregion

        #region 接收函数
        /// <summary>
        /// 读取“消息头”。
        /// </summary>
        private CMPP_HEAD ReadHead()
        {
            CMPP_HEAD head = new CMPP_HEAD();
            head.CommandID = 0;
            try
            {
                if (m_NetworkStream.DataAvailable)
                {
                    byte[] buffer = new byte[12];
                    m_NetworkStream.Read(buffer, 0, buffer.Length);
                    head.TotalLength = Convert.ToUInt32(buffer, 0);
                    head.CommandID = Convert.ToUInt32(buffer, 4);
                    head.SequenceID = Convert.ToUInt32(buffer, 8);
                }
            }
            catch
            {
                head.CommandID = CMD_ERROR;
            }
            return head;
        }

        #region 没有被研究的代码
        private CMPP_ACTIVE_TEST_RESP ReadActiveTestResponse(CMPP_HEAD Head)
        {
            CMPP_ACTIVE_TEST_RESP resp = new CMPP_ACTIVE_TEST_RESP();
            resp.Head = Head;
            string strError = string.Empty;
            bool bOK = true;
            try
            {
                if (m_NetworkStream.DataAvailable)
                {
                    Byte[] buffer = new Byte[resp.Head.TotalLength - Marshal.SizeOf(resp.Head)];
                    m_NetworkStream.Read(buffer, 0, buffer.Length);
                    resp.Reserved = buffer[0];

                    lock (SlidingWindow)
                    {
                        for (int i = 0; i < SlidingWindow.Length; i++)
                        {
                            if ((SlidingWindow[i].Status == 1) &&//已发送，等待回应
                             (SlidingWindow[i].SequenceID == resp.Head.SequenceID) &&//序列号相同
                             (SlidingWindow[i].Command == CMD_ACTIVE_TEST))//是ACTIVE_TEST
                            {
                                SlidingWindow[i].Status = 0;//清空窗口
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Head.CommandID = CMD_ERROR;
                strError = ex.Message;
                bOK = false;
            }

            if (bOK)
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST_RESPONSE, resp, DateTime.Now));
            else
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST_RESPONSE_ERROR, strError, DateTime.Now));

            return resp;
        }
        private CMPP_SUBMIT_RESP ReadSubmitResp(CMPP_HEAD Head)
        {
            CMPP_SUBMIT_RESP resp = new CMPP_SUBMIT_RESP();
            resp.Head = Head;
            string strError = string.Empty;
            bool bOK = true;
            try
            {
                if (m_NetworkStream.DataAvailable)
                {
                    Byte[] buffer = new Byte[resp.Head.TotalLength - Marshal.SizeOf(resp.Head)];
                    m_NetworkStream.Read(buffer, 0, buffer.Length);
                    resp.MsgID = (UInt64)BitConverter.ToUInt64(buffer, 0);
                    resp.Result = Convert.ToUInt32(buffer, 8);

                    lock (SlidingWindow)
                    {
                        for (int i = 0; i < SlidingWindow.Length; i++)
                        {
                            if ((SlidingWindow[i].Status == 1) &&//已发送，等待回应
                             (SlidingWindow[i].SequenceID == resp.Head.SequenceID) &&//序列号相同
                             (SlidingWindow[i].Command == CMD_SUBMIT))//是Submit
                            {
                                SlidingWindow[i].Status = 0;// 清空窗口

                                // 异步操作支持。
                                if (SlidingWindow[i]._ar != null)
                                {
                                    SlidingWindow[i]._ar._returnValue = resp;
                                    SlidingWindow[i]._ar._isCompleted = true;
                                    SlidingWindow[i]._ar._asyncWaitHandle.Set();
                                    // 回调。
                                    if (SlidingWindow[i]._ar._cb != null)
                                    {
                                        if (SlidingWindow[i]._ar._context == null)
                                        {
                                            try
                                            {
                                                SlidingWindow[i]._ar._cb(SlidingWindow[i]._ar);
                                            }
                                            catch
                                            { }
                                        }
                                        else
                                        {
                                            SlidingWindow[i]._ar._context.Post(delegate(object obj) { ((AsyncCallback)obj)(SlidingWindow[i]._ar); }, SlidingWindow[i]._ar._cb);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Head.CommandID = CMD_ERROR;
                strError = ex.Message;
                bOK = false;
            }
            if (bOK)
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SUBMIT_RESPONSE, resp, DateTime.Now));
            }
            else
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SUBMIT_RESPONSE_ERROR, strError, DateTime.Now));
            }
            return resp;

        }
        private CMPP_DELIVER ReadDeliver(CMPP_HEAD Head)
        {
            CMPP_DELIVER deliver = new CMPP_DELIVER();
            deliver.Head = Head;
            string strError = string.Empty;
            try
            {
                if (m_NetworkStream.DataAvailable)
                {
                    Byte[] buffer = new Byte[deliver.Head.TotalLength - Marshal.SizeOf(deliver.Head)];
                    m_NetworkStream.Read(buffer, 0, buffer.Length);
                    deliver.Init(buffer);

                    // 为接收到的 CMPP_DELIVER 发送 RESP 包。

                    CMPP_DELIVER_RESP resp = new CMPP_DELIVER_RESP();
                    resp.Head = new CMPP_HEAD();
                    resp.Head.CommandID = CMPP30.CMD_DELIVER_RESP;
                    resp.Head.SequenceID = deliver.Head.SequenceID;
                    resp.MsgID = deliver.MsgID;
                    resp.Result = 0;

                    DATA_PACKAGE dp = new DATA_PACKAGE();
                    dp.SequenceID = resp.Head.SequenceID;
                    dp.Command = resp.Head.CommandID;
                    dp.SendCount = 0;
                    dp.Data = resp;
                    dp.Status = 1;

                    lock (m_MessageQueue)
                    {
                        m_MessageQueue.Enqueue(dp);
                    }
                }
            }
            catch (Exception ex)
            {
                deliver.Head.CommandID = CMD_ERROR;
                strError = ex.Message;
            }
            if ((deliver.Head.CommandID == CMD_DELIVER) && (deliver.RegisteredDelivery == 0))////是短消息
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.DELIVER, deliver, DateTime.Now));
            }
            else if ((deliver.Head.CommandID == CMD_DELIVER) && (deliver.RegisteredDelivery == 1))////是状态报告
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.REPORT, deliver, DateTime.Now));
            }
            else//错误
            {
                FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.DELIVER_ERROR, strError, DateTime.Now));
            }
            return deliver;
        }
        #endregion

        #endregion

        #region 工作线程
        /// <summary>
        /// 发送线程。
        /// </summary>
        private void Send()
        {
            while (true)
            {

                #region 是否结束
                if (m_eventSendExit.WaitOne(0, false))
                {
                    Disconnect();
                    break;
                }
                #endregion

                #region 开始连接
                if (m_eventConnect.WaitOne(0, false))//连接
                {
                    if (Connect())//连接上，开始发送和接收
                    {
                        // 发送和接收工作开关启动。
                        m_eventSend.Set();
                        m_eventReceive.Set();
                    }
                    else
                    {
                        CloseTcpClient();
                        Thread.Sleep(5000);
                        // 连接不成功继续连接。
                        m_eventConnect.Set();
                    }
                }
                #endregion

                #region 断开重连
                if (m_eventDisconnect.WaitOne(0, false))//拆除连接
                {
                    // 断开后发送和接收工作停滞（开关关闭）。
                    m_eventSend.Reset();
                    m_eventReceive.Reset();
                    Disconnect();
                    Thread.Sleep(5000);
                    // 在断开后重新连接。
                    m_eventConnect.Set();
                }
                #endregion

                #region 是否发送
                if ((m_eventSend.WaitOne(0, false)) && (m_NetworkStream != null))
                {
                    // 是否有网络错误，如果有则重新连接。
                    bool bOK = true;

                    if ((DateTime.Now - m_dtLastTransferTime).TotalSeconds > m_iActiveTestSpan)
                    {
                        // 当信道上没有数据传输时，通信双方应每隔时间 C 发送链路检测包以维持此连接。
                        CMPP_HEAD Head = new CMPP_HEAD();
                        Head.TotalLength = 12;
                        Head.CommandID = CMPP30.CMD_ACTIVE_TEST;
                        Head.SequenceID = CreateSeqID();

                        DATA_PACKAGE dp = new DATA_PACKAGE();
                        dp.SequenceID = Head.SequenceID;
                        dp.Command = Head.CommandID;
                        dp.SendCount = 0;
                        dp.Data = Head;
                        dp.Status = 1;

                        lock (m_MessageQueue)
                        {
                            m_MessageQueue.Enqueue(dp);
                        }
                    }

                    lock (SlidingWindow)
                    {
                        // 首先用消息队列中的数据填充滑动窗口。
                        for (int i = 0; i < SlidingWindow.Length; i++)
                        {
                            if (SlidingWindow[i].Status == 0)
                            {
                                lock (m_MessageQueue)
                                {
                                    if (m_MessageQueue.Count > 0)
                                    {
                                        SlidingWindow[i] = m_MessageQueue.Dequeue();
                                    }
                                }
                            }
                        }

                        // 是否执行了发送工作。
                        bool hasWorked = false;

                        for (int i = 0; i < SlidingWindow.Length; i++)
                        {
                            DATA_PACKAGE dp = SlidingWindow[i];

                            if (dp.Status == 1)
                            {
                                // 第一次发送。
                                if (dp.SendCount == 0)
                                {
                                    try
                                    {
                                        Thread.Sleep(m_iSendSpan);
                                        bOK = Send(dp.Command, dp.Data);
                                    }
                                    catch
                                    {
                                        bOK = false;
                                    }

                                    if (bOK)
                                    {
                                        if (dp.Command > 0x80000000)
                                        {
                                            // 清空窗口；发送的是 Response 类的消息，不需等待 Response。
                                            SlidingWindow[i].Status = 0;
                                        }
                                        else // 发送的是需要等待 Response 的消息。
                                        {
                                            SlidingWindow[i].SendTime = DateTime.Now;
                                            SlidingWindow[i].SendCount++;
                                        }
                                    }
                                    else
                                    {
                                        break; // 网络出错，不再发送其他数据包（等待重新连接）。
                                    }
                                    hasWorked = true;
                                }
                                else // 已发送 m_iSendCount 次,丢弃数据包。
                                {
                                    //第 N 次发送。
                                    if (dp.SendCount > m_iSendCount - 1)
                                    {
                                        // 清空窗口。
                                        SlidingWindow[i].Status = 0;

                                        // 异步操作支持。
                                        if (SlidingWindow[i]._ar != null)
                                        {
                                            SlidingWindow[i]._ar._isCompleted = true;
                                            SlidingWindow[i]._ar._asyncWaitHandle.Set();
                                            SlidingWindow[i]._ar._exception = new ApplicationException("发送超时");

                                            // 回调。
                                            if (SlidingWindow[i]._ar._context == null)
                                            {
                                                try
                                                {
                                                    SlidingWindow[i]._ar._cb(SlidingWindow[i]._ar);
                                                }
                                                catch
                                                { }
                                            }
                                            else
                                            {
                                                SlidingWindow[i]._ar._context.Post(delegate(object obj) { ((AsyncCallback)obj)(SlidingWindow[i]._ar); }, SlidingWindow[i]._ar._cb);
                                            }
                                        }

                                        if (dp.Command == CMPP30.CMD_ACTIVE_TEST)
                                        {
                                            // ActiveTest 出错，网络连接有问题，不再发送其他数据包（等待重新连接）。
                                            bOK = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // 超时后未收到回应包。
                                        if ((DateTime.Now - dp.SendTime).TotalSeconds >= m_iTimeOut)
                                        {
                                            // 再次发送。
                                            try
                                            {
                                                Thread.Sleep(m_iSendSpan);
                                                bOK = Send(dp.Command, dp.Data);
                                            }
                                            catch
                                            {
                                                bOK = false;
                                            }

                                            if (bOK)
                                            {
                                                SlidingWindow[i].SendTime = DateTime.Now;
                                                SlidingWindow[i].SendCount++;
                                            }
                                            else
                                            {
                                                break;// 网络出错，不再发送其他数据包（等待重新连接）。
                                            }
                                        }
                                        hasWorked = true;
                                    }
                                }
                            }
                        }

                        if (!hasWorked)
                        {
                            // 没有执行任何工作，休息10毫秒，这样可以防止CPU占用率过高。
                            Thread.Sleep(10);
                        }
                    }


                    if (!bOK)
                    {
                        CloseTcpClient();// 关闭连接
                        Thread.Sleep(5000);// 等待5秒。
                        // 重新启动连接。
                        m_eventSend.Reset();
                        m_eventConnect.Set();
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
                #endregion

            }
        }
        /// <summary>
        /// 接收线程。
        /// </summary>
        private void Receive()
        {
            while (true)
            {

                #region 是否结束
                if (m_eventReceiveExit.WaitOne(0, false))
                {
                    break;
                }
                #endregion

                #region 是否接收
                if ((m_eventReceive.WaitOne(0, false) && (m_NetworkStream != null)))
                {
                    try
                    {
                        CMPP_HEAD head = ReadHead();

                        if (head.CommandID != 0)
                        {
                            Process(head);
                        }
                        else
                        {
                            // 没有接收的数据，休息10毫秒，这样可以避免CPU占用率过高。
                            Thread.Sleep(10);
                        }
                    }
                    catch
                    { }
                }
                else
                {
                    // 没有执行任何工作，休息10毫秒，这样可以避免CPU占用率过高。
                    Thread.Sleep(10);
                }
                #endregion

            }
        }
        #endregion

        #endregion

        #region 保护方法
        /// <summary>
        /// 创建消息流水号。
        /// </summary>
        /// <remarks>
        /// 消息流水号，顺序累加，步长为 1，循环使用（每对请求和应答消息的流水号必须相同）。
        /// </remarks>
        protected uint CreateSeqID()
        {
            return m_iSeqID++;
        }
        /// <summary>
        /// 发送数据包。
        /// </summary>
        protected virtual bool Send(uint command, ICMPP_MESSAGE data)
        {
            try
            {
                switch (command)
                {
                    case CMD_ACTIVE_TEST:
                    case CMD_ACTIVE_TEST_RESP:
                    case CMD_DELIVER_RESP:
                    case CMD_SUBMIT:
                        byte[] bytes = data.GetBytes();
                        m_NetworkStream.Write(bytes, 0, bytes.Length);
                        m_dtLastTransferTime = DateTime.Now;
                        break;
                }

                // 发送成功，引发相应事件。
                switch (command)
                {
                    case CMD_ACTIVE_TEST:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST, data, DateTime.Now));
                        break;
                    case CMD_ACTIVE_TEST_RESP:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST_RESPONSE, data, DateTime.Now));
                        break;
                    case CMD_DELIVER_RESP:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.DELIVER_RESPONSE, data, DateTime.Now));
                        break;
                    case CMD_SUBMIT:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SUBMIT, data, DateTime.Now));
                        break;
                }
            }
            catch (Exception ex)
            {
                // 发送失败，引发相应事件。
                switch (command)
                {
                    case CMD_ACTIVE_TEST:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST_ERROR, ex.Message, DateTime.Now));
                        break;
                    case CMD_ACTIVE_TEST_RESP:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.ACTIVE_TEST_RESPONSE_ERROR, ex.Message, DateTime.Now));
                        break;
                    case CMD_DELIVER_RESP:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.DELIVER_RESPONSE_ERROR, ex.Message, DateTime.Now));
                        break;
                    case CMD_SUBMIT:
                        FireOnSMSStateChangedEvent(new SMSEventArgs(SMS_EVENT.SUBMIT_ERROR, ex.Message, DateTime.Now));
                        break;
                }
                return false;
            }
            // 这里，即使是位未知的 COMMAND_ID，也返回 true。
            return true;
        }
        /// <summary>
        /// CMPP 消息接收处理。
        /// </summary>
        protected virtual void Process(CMPP_HEAD head)
        {
            switch (head.CommandID)
            {
                case CMPP30.CMD_SUBMIT_RESP:
                    ReadSubmitResp(head);
                    break;
                case CMPP30.CMD_ACTIVE_TEST:
                    // 为接收到的 ACTIVE_TEST 发送 RESP 包。
                    CMPP_ACTIVE_TEST_RESP resp = new CMPP_ACTIVE_TEST_RESP();
                    resp.Head = new CMPP_HEAD();
                    resp.Head.CommandID = CMPP30.CMD_ACTIVE_TEST_RESP;
                    resp.Head.SequenceID = head.SequenceID;
                    resp.Reserved = 0;

                    DATA_PACKAGE dp = new DATA_PACKAGE();
                    dp.SequenceID = resp.Head.SequenceID;
                    dp.Command = resp.Head.CommandID;
                    dp.SendCount = 0;
                    dp.Data = resp;
                    dp.Status = 1;

                    lock (m_MessageQueue)
                    {
                        m_MessageQueue.Enqueue(dp);
                    }
                    break;
                case CMPP30.CMD_ACTIVE_TEST_RESP:
                    ReadActiveTestResponse(head);
                    break;
                case CMPP30.CMD_DELIVER:
                    ReadDeliver(head);
                    break;
                case CMPP30.CMD_ERROR:
                    // 这里不能阻止 m_eventSend，因为断开操作是在发送线程里做的，如果阻止将可能导致
                    m_eventReceive.Reset();
                    m_eventDisconnect.Set();
                    break;
            }
        }
        /// <summary>
        /// 引发 SMS 事件。
        /// </summary>
        protected virtual void OnSMS(SMSEventArgs e)
        {
            if (SMS != null)
            {
                SMS(this, e);
            }
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 启动 CMPP30 服务。
        /// </summary>
        public void Start()
        {
            lock (syncRoot)
            {
                if (cmpp30Stoped)
                {
                    throw new ApplicationException("CMPP30 已经被停止，无法再次启动");
                }
                if (m_SendThread != null && m_ReceiveThread != null)
                {
                    throw new ApplicationException("CMPP30 已经被启动");
                }

                if (m_SendThread == null)
                {
                    m_dtLastTransferTime = DateTime.Now;
                    m_SendThread = new Thread(Send);
                    m_SendThread.IsBackground = true;
                    m_SendThread.Name = m_strSPID + "_Send";
                    m_SendThread.Start();
                }
                if (m_ReceiveThread == null)
                {
                    m_ReceiveThread = new Thread(Receive);
                    m_ReceiveThread.IsBackground = true;
                    m_ReceiveThread.Name = m_strSPID + "_Receive";
                    m_ReceiveThread.Start();
                }
                m_eventConnect.Set();

                if (context == null)
                {
                    context = SynchronizationContext.Current;
                }
            }
        }
        /// <summary>
        /// 发送短信。
        /// </summary>
        /// /// <param name="text">
        /// 信息内容。
        /// </param>
        /// <param name="encoding">
        /// 信息编码。
        /// </param>
        /// <param name="sourceID">
        /// SP的服务代码，将显示在最终用户手机上的短信主叫号码。
        /// </param>
        /// <param name="destinations">
        /// 接收短信的电话号码列表。
        /// </param>
        /// <param name="serviceID">
        /// 业务标识（如：woodpack）。
        /// </param>
        /// <param name="needReport">
        /// 是否要求返回状态报告。
        /// </param>
        /// <param name="feeType">
        /// 资费类别。
        /// </param>
        /// <param name="feeUserType">
        /// 计费用户。
        /// </param>
        /// <param name="feeUser">
        /// 被计费的号码（feeUserType 值为 FeeUser 时有效）。
        /// </param>
        /// <param name="realUser">
        /// 被计费号码的真实身份（“真实号码”或“伪码”）。
        /// </param>
        /// <param name="informationFee">
        /// 信息费（以“分”为单位，如：10 分代表 1角）。
        /// </param>
        /// <param name="linkID">
        /// 点播业务的 LinkID。
        /// </param>
        public CMPP_SUBMIT_RESP Send(
            string text,
            CEncoding encoding,
            string sourceID,
            string[] destinations,
            string serviceID,
            bool needReport,
            FeeType feeType,
            FeeUserType feeUserType,
            string feeUser,
            bool realUser,
            int informationFee,
            string linkID)
        {
            IAsyncResult ar = BeginSend(
                text,
                encoding,
                sourceID,
                destinations,
                serviceID,
                needReport,
                feeType,
                feeUserType,
                feeUser,
                realUser,
                informationFee,
                linkID,
                null, null);
            ar.AsyncWaitHandle.WaitOne();
            return EndSend(ar);
        }
        /// <summary>
        /// 发送短信。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CMPP_SUBMIT_RESP Send(CMPP_SUBMIT submit)
        {
            IAsyncResult ar = BeginSend(submit, null, null);
            ar.AsyncWaitHandle.WaitOne();
            return EndSend(ar);
        }
        /// <summary>
        /// 开始异步发送短信。
        /// </summary>
        /// <param name="text">
        /// 信息内容。
        /// </param>
        /// <param name="encoding">
        /// 信息编码。
        /// </param>
        /// <param name="sourceID">
        /// SP的服务代码，将显示在最终用户手机上的短信主叫号码。
        /// </param>
        /// <param name="destinations">
        /// 接收短信的电话号码列表。
        /// </param>
        /// <param name="serviceID">
        /// 业务标识（如：woodpack）。
        /// </param>
        /// <param name="needReport">
        /// 是否要求返回状态报告。
        /// </param>
        /// <param name="feeType">
        /// 资费类别。
        /// </param>
        /// <param name="feeUserType">
        /// 计费用户。
        /// </param>
        /// <param name="feeUser">
        /// 被计费的号码（feeUserType 值为 FeeUser 时有效）。
        /// </param>
        /// <param name="realUser">
        /// 被计费号码的真实身份（“真实号码”或“伪码”）。
        /// </param>
        /// <param name="informationFee">
        /// 信息费（以“分”为单位，如：10 分代表 1角）。
        /// </param>
        /// <param name="linkID">
        /// 点播业务的 LinkID。
        /// </param>
        /// <param name="callback">
        /// 异步回调函数。
        /// </param>
        /// <param name="asyncState">
        /// 传递给异步回调函数的参数。
        /// </param>
        public IAsyncResult BeginSend(
            string text,
            CEncoding encoding,
            string sourceID,
            string[] destinations,
            string serviceID,
            bool needReport,
            FeeType feeType,
            FeeUserType feeUserType,
            string feeUser,
            bool realUser,
            int informationFee,
            string linkID,
            AsyncCallback callback,
            object asyncState)
        {
            CMPP_SUBMIT submit = new CMPP_SUBMIT();

            // 信息内容。
            submit.MsgContent = text;
            // 信息编码。
            submit.MsgFmt = (byte)encoding;
            // SP的服务代码，将显示在最终用户手机上的短信主叫号码。
            submit.SrcID = sourceID;
            // 接收短信的电话号码列表。
            submit.DestTerminalID = destinations;
            // 业务标识（如：woodpack）。
            submit.ServiceID = serviceID;
            // 是否要求返回状态报告。
            submit.RegisteredDelivery = (byte)(needReport ? 1 : 0);
            // 资费类别。
            submit.FeeType = string.Format("D2", (int)feeType);
            // 计费用户。
            submit.FeeUserType = (byte)feeUserType;
            // 被计费的号码（feeUserType 值为 FeeUser 时有效）。
            submit.FeeTerminalID = feeUser;
            // 被计费号码的真实身份（“真实号码”或“伪码”）。
            submit.FeeTerminalType = (byte)(realUser ? 0 : 1);
            // 信息费（以“分”为单位，如：10 分代表 1角）。
            submit.FeeCode = informationFee.ToString();
            // 点播业务的 linkId。
            submit.LinkID = linkID;

            return BeginSend(submit, callback, asyncState);
        }
        /// <summary>
        /// 开始异步发送短信。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IAsyncResult BeginSend(
            CMPP_SUBMIT submit,
            AsyncCallback cb,
            object asyncState)
        {
            if (!m_eventSend.WaitOne(0, false))
            {
                throw new ApplicationException("网络通讯错误，未能连接到服务器");
            }

            submit.Head = new CMPP_HEAD();
            submit.Head.CommandID = CMPP30.CMD_SUBMIT;
            submit.Head.SequenceID = CreateSeqID();
            submit.MsgID = 0;
            submit.PkTotal = 1;
            submit.PkNumber = 1;
            submit.MsgLevel = 0;
            submit.TPPID = 0;
            submit.TPUdhi = 0;
            submit.MsgSrc = m_strSPID;
            submit.ValidTime = "";
            submit.AtTime = "";

            if (submit.DestTerminalID != null)
            {
                submit.DestUsrTl = (byte)submit.DestTerminalID.Length;
            }
            submit.DestTerminalType = 0;//真实号码
            submit.MsgLength = Convert.Length(submit.MsgContent, submit.MsgFmt);

            DATA_PACKAGE dp = new DATA_PACKAGE();
            dp.SequenceID = submit.Head.SequenceID;
            dp.Command = submit.Head.CommandID;
            dp.SendCount = 0;
            dp.Data = submit;
            dp.Status = 1;
            dp._ar = new DATA_PACKAGE_AsyncResult(asyncState, this, cb);

            lock (m_MessageQueue)
            {
                // 入队（待发送队列）。
                m_MessageQueue.Enqueue(dp);
            }
            return dp._ar;
        }
        /// <summary>
        /// 结束异步发送短信。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CMPP_SUBMIT_RESP EndSend(IAsyncResult ar)
        {
            DATA_PACKAGE_AsyncResult dpar = ar as DATA_PACKAGE_AsyncResult;

            if (dpar == null || dpar._cmpp30 != this)
            {
                throw new ApplicationException("非法 IAsyncResult 对象");
            }

            if (dpar._ended)
            {
                throw new ApplicationException("对 IAsyncResult 两次调用了 EndEndSend 方法");
            }

            dpar._ended = true;

            if (dpar._exception != null)
            {
                throw dpar._exception;
            }
            return dpar._returnValue;
        }
        /// <summary>
        /// 停止 CMPP30 服务。
        /// </summary>
        public void Stop()
        {
            lock (syncRoot)
            {
                if (cmpp30Stoped)
                {
                    throw new ApplicationException("CMPP30 已经被停止");
                }
                if (m_SendThread == null && m_ReceiveThread == null)
                {
                    throw new ApplicationException("CMPP30 尚未被启动");
                }

                cmpp30Stoped = true;
                m_eventSend.Reset();
                m_eventReceive.Reset();
                m_eventReceiveExit.Set();
                m_eventSendExit.Set();
            }
        }
        #endregion

        #region DATA_PACKAGE 结构
        /// <summary>
        /// 待发送数据包（供等待队列与滑动窗口使用）。
        /// </summary>
        private struct DATA_PACKAGE
        {
            /// <summary>
            /// 命令或响应类型（Message Header）。
            /// </summary>
            public uint Command;
            /// <summary>
            /// 流水号。
            /// </summary>
            public uint SequenceID;
            /// <summary>
            /// 数据。
            /// </summary>
            public ICMPP_MESSAGE Data;
            /// <summary>
            /// 数据包发送时间。
            /// </summary>
            public DateTime SendTime;
            /// <summary>
            /// 发送次数。
            /// </summary>
            public int SendCount;
            /// <summary>
            /// 数据包状态（0：空，1：待发送，2：已发送）。
            /// </summary>
            public int Status;
            public DATA_PACKAGE_AsyncResult _ar;
        }
        #endregion

        #region DATA_PACKAGE_AsyncResult 类
        private class DATA_PACKAGE_AsyncResult : IAsyncResult
        {

            #region 字段
            public object _asyncState;
            public ManualResetEvent _asyncWaitHandle = new ManualResetEvent(false);
            public bool _isCompleted;

            public CMPP30 _cmpp30;
            public CMPP_SUBMIT_RESP _returnValue;

            public bool _ended;
            public Exception _exception;
            public AsyncCallback _cb;
            public SynchronizationContext _context = SynchronizationContext.Current;
            #endregion

            #region 构造函数
            public DATA_PACKAGE_AsyncResult(object asyncState, CMPP30 cmpp30, AsyncCallback cb)
            {
                _asyncState = asyncState;
                _cmpp30 = cmpp30;
                _cb = cb;
            }
            #endregion

            #region IAsyncResult 成员
            object IAsyncResult.AsyncState
            {
                get
                {
                    return _asyncState;
                }
            }
            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }
            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    return _asyncWaitHandle;
                }
            }
            bool IAsyncResult.IsCompleted
            {
                get
                {
                    return _isCompleted;
                }
            }
            #endregion

        }
        #endregion

    }
}