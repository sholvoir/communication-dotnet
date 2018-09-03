using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Vultrue.Communication.CmppMsg;

namespace Vultrue.Communication
{
    /// <summary>
    /// CMPP协议的客户端，具有的登陆、发送、接受功能
    /// 会开3 个线程处理:
    /// 1、处理需要发送 MO(下行)的消息
    /// 2、处理从移动服务器发送过来CMPP的消息
    /// 3、处理连接断等信息，检查需要重发的消息，检查收到的报告、短信，并调用 OnReport 事件 OnSMS事件
    /// </summary>
    public class CmppClient
    {
        #region 常量

        /// <summary>
        /// 长连接的测试间隔(?)
        /// </summary>
        public const int ACTIVE_TEST_C_TICKS = 30;

        /// <summary>
        /// 消息失败时间(秒)
        /// </summary>
        public const int ACTIVE_TEST_T_TICKS = 60;

        /// <summary>
        /// 活动测试次数
        /// </summary>
        public const int ACTIVE_TEST_N_COUNT = 3;

        /// <summary>
        /// 一次取得的最大消息数量
        /// </summary>
        public const int MSG_MAX=100;

        /// <summary>
        /// 服务器服务端口
        /// </summary>
        public static int SERVER_PORT = 7890;

        #endregion

        #region 字段
        private Socket tcp = null;
        private IPHostEntry ip = null;
        private IPEndPoint cmpp_ep = null;
        private int RecvTimeOut = 1000;       //2000ms的接受超时
        private int SendTimeout = 2000;       //2000ms的发送超时
        private string CMPP_Server = "";   //移动的服务器IP或者DNS名
        private string systemID = "";   //企业编号
        private string userName = "";   //sp的号码 /企业编号
        private string PassWord = "";   //口令 
        private bool isStop = false;   //本服务是否终止运行
        private bool isLogin = false;   //是否已经登录   
        private Thread Send_Thread;   //发送线程,专门处理对移动的数据包
        private Thread Recv_Thread;   //专门处理接收包
        private Thread Deamo_Thread;   //监控线程
        private string ErrorInfo = "";   //存放最后一次发生的错误信息 或者参考信息     
        private DateTime _current_time = DateTime.Now;     //上一次 ping的时间 
        private uint lastSequence;   //流水号，每一次重新启动都需要重新设定 lastSequence
        private SortedList _outSeqQueue = new SortedList();   //消息队列存储 QueueItem,存储发送队列中的状态
        private SortedList _waitingSeqQueue = new SortedList(); //消息队列存储 QueueItem
        private int sub_resp = 0;       //最后返回的包 Sequence
        private DateTime _lastOkTime;      //最后正确发送消息时间
        private bool _bNre = false;      //空引用错误，套接字错误
        #endregion

        #region 事件

        public event EventHandler<ReportEventArgs> onReportHandler;
        public event EventHandler<SMSEventArgs> onSMSHandler;     //短信到来处理
        public event EventHandler<TestEventArgs> onTestHandler;
        public event EventHandler<TestRespEventArgs> onTestRespHandler;
        public event EventHandler<ConnectRespEventArgs> onConnectRespHandler;
        public event EventHandler<CancelRespEventArgs> onCancelRespHandler;
        public event EventHandler<TerminateEventArgs> onTerminateHandler;//收到终止信号
        public event EventHandler<TerminateRespEventArgs> onTerminateRespHandler;//回应事件发生
        public event EventHandler<SubmitRespEventArgs> onSubmitRespHandler;
        public event EventHandler<QueryRespEventArgs> onQueryRespHandler;
        public event EventHandler onLogonSuccEventHandler;//当成功登录系统
        public event EventHandler onSocketClosedHandler;//当套接字被检测到关闭
        public event EventHandler<WaitingQueueItemEventArgs> onWaitingItemDeltedHandler; //当等待队列消息超时
        public event EventHandler<ClientQueueStateArgs> onClientSvcStopedHandler;  //当CMPP服务停止时候的事件
        #endregion

        //private 函数区域//////////////////////////////////////////////////////////////////////

        //private ManualResetEvent _connectionDone=new ManualResetEvent(false); //是否连接到套接字服务器,也就是CMPP服务器
        //private ManualResetEvent _lastsendDone=new ManualResetEvent(false);  //上一次发送是否完毕
        //private ManualResetEvent _lastrecvDone=new ManualResetEvent(false);  //上一次接收是否完毕

        private void ping()    //发送一次ping包 ，不经过_outSeqQueue 直接存储在 out queue中
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_TEST test = new CMPP_MSG_TEST(seq);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST, 0, 0);
            q.setmsgObj(test);
            this.addToOutQueue(q);
        }

        private string getValIdTime(DateTime d)        //返回短信存活时间
        {
            DateTime n = d.AddHours(2); //2小时
            return (n.Year.ToString().Substring(2) + n.Month.ToString().PadLeft(2, '0') + n.Day.ToString().PadLeft(2, '0') + n.Hour.ToString().PadLeft(2, '0') + n.Minute.ToString().PadLeft(2, '0') + n.Second.ToString().PadLeft(2, '0') + "032+");
        }

        private bool isPingTime()  //是否到了ping一次的时间
        {
            System.TimeSpan l = (DateTime.Now - this._current_time);

            if (l.TotalSeconds >= (CmppClient.ACTIVE_TEST_C_TICKS))
            {
                lock (this)
                {
                    this._current_time = DateTime.Now;
                    return (true);
                }
            }
            else
            {
                return (false);
            }
        }

        private void checkReSend()    //是否需要再一次ping //查询 _waitingSeqQueue 是否存在 上一次 没有相应的消息
        {   //调查waiting queue 中的所有消息，如果入列时间超过60
            for (int i = 0; i < this._waitingSeqQueue.Count; i++)
            {
                Thread.Sleep(20);
                QueueItem q = (QueueItem)this._waitingSeqQueue.GetByIndex(i);
                if (q != null)
                {
                    DateTime this_time = DateTime.Now; //去当前时间
                    TimeSpan t = this_time - q.inQueueTime;
                    if (t.TotalSeconds > CmppClient.ACTIVE_TEST_T_TICKS) //达到超时时间
                    {//需要重新发送消息
                        if (q.FailedCount >= CmppClient.ACTIVE_TEST_N_COUNT)
                        {
                            //报告消息发送失败
                            if (this.onWaitingItemDeltedHandler != null)
                            {
                                WaitingQueueItemEventArgs e = new WaitingQueueItemEventArgs(q);
                                this.onWaitingItemDeltedHandler(this, e);
                            }
                            this.delFromWaitingQueue(q); //从等待队列中删除
                            //q.MsgState =(int)MSG_STATE.SENDED_WAITTING;
                        }
                        else
                        {//可以尝试继续发送
                            q.inQueueTime = this_time;
                            q.FailedCount++;
                            q.MsgState = (int)MsgState.SendWaiting;
                            this.sendQueueItem(q);
                        }
                    }
                }
            }

        }

        private void startThreads()
        {
            Deamo_Thread = new Thread(new ThreadStart(this.DeamonThread));
            Deamo_Thread.Start();
        }

        private QueueItem newQueueItem(int msgtype, int msgstate, object msg)  //生成一个消息队列成员对象实例
        {
            uint seq = this.getNextSequence();   //
            QueueItem q = new QueueItem(seq, (uint)msgtype, 0, msgstate);
            q.setmsgObj(msg);       //设定消息为 object
            return (q);
        }

        private QueueItem getOutQueueItem(uint seq)  //获取MT 队列中的消息项目
        {
            lock (this)
            {
                return ((QueueItem)this._outSeqQueue[seq]);
            }
        }

        private QueueItem getWaitingQueueItem(uint seq)  //获取等待队列中的消息
        {
            return ((QueueItem)this._waitingSeqQueue[seq]);
        }

        private void addToOutQueue(QueueItem q)
        {
            lock (this)
            {
                this._outSeqQueue.Add(q.Sequence, q);
            }
        }

        private void addToWaitingQueue(QueueItem q)
        {
            lock (this)
            {
                if (!this._waitingSeqQueue.ContainsKey(q.Sequence))
                {
                    this._waitingSeqQueue.Add(q.Sequence, q);
                }
            }
        }

        private QueueItem getTopOutQueue()     //需要在取之前进行判断
        {
            for (int i = 0; i < this._outSeqQueue.Count; i++)
            {
                QueueItem q = (QueueItem)this._outSeqQueue.GetByIndex(i);
                if (q != null)
                {
                    if (q.MsgState == (int)MsgState.New)  //新消息，立即返回
                    {
                        lock (this)
                        {
                            q.MsgState = (int)MsgState.Sending; //发送状态
                        }
                        return (q);
                    }
                    else
                    {
                        q = null;
                    }
                }
            }
            return (null);
        }

        private ArrayList getTop16Queue() //返回16条最顶的消息
        {
            int arrlength = 0;
            ArrayList reArr = new ArrayList();
            QueueItem q = getTopOutQueue();
            while (q != null || arrlength <= 16)
            {
                if (q != null)
                {
                    reArr.Add(q);
                    arrlength++;
                }
                else
                {
                    break;
                }
                q = getTopOutQueue();
            }

            if (arrlength > 0)
            {
                return (reArr);
            }
            else
            {
                return (null);
            }
        }

        private void delFromOutQueue(QueueItem q)
        {
            lock (this)
            {
                this._outSeqQueue.Remove(q.Sequence);
            }
        }

        private void delFromOutQueue(uint seq)
        {
            lock (this)
            {
                this._outSeqQueue.Remove(seq);
            }
        }

        private void delFromWaitingQueue(QueueItem q)
        {
            lock (this)
            {
                this._waitingSeqQueue.Remove(q.Sequence);
            }
        }

        private void delFromWaitingQueue(uint seq)
        {
            this._waitingSeqQueue.Remove(seq);
        }

        private void SendLogin(string SystemID, string spNum, string Password)
        {//发送登录验证包   
            systemID = SystemID;
            userName = spNum;
            PassWord = Password;
            uint seq = this.getNextSequence(); //取得一个流水号
            CMPP_MSG_CONNECT cn = new CMPP_MSG_CONNECT(seq);
            cn.Password = Password.Trim();
            cn.SourceAdd = SystemID.Trim();
            tcp.Send(cn.ToBytes());
        }


        private byte[] prepairPKs(QueueItem outitem)//将QueueItem发送出去
        {
            uint seq = outitem.Sequence;
            uint msgtype = outitem.MsgType;
            switch (msgtype)
            {
                case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST:
                    CMPP_MSG_TEST test = (CMPP_MSG_TEST)outitem.getMsgObj(); //发送队列中取出
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);    //等待服务器的active_TEST_resp
                    }
                    outitem.MsgState = (int)MsgState.SendWaiting;
                    return (test.toBytes());


                case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP:
                    CMPP_MSG_TEST_RESP test_reply = (CMPP_MSG_TEST_RESP)outitem.getMsgObj(); //发送队列中取出//取出需要发送的具体消息
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                    }
                    outitem.MsgState = (int)MsgState.SendFinished;  //完成
                    return (test_reply.toBytes());



                case (uint)CMPP_COMMAND_ID.CMPP_CANCEL:
                    CMPP_MSG_CANCEL cancel = (CMPP_MSG_CANCEL)outitem.getMsgObj();    //还原成消息类
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);    //等待回应
                    }
                    outitem.MsgState = (int)MsgState.SendWaiting;
                    return (cancel.toBytes());

                case (uint)CMPP_COMMAND_ID.CMPP_DELIVER_RESP:
                    CMPP_MSG_DELIVER_RESP deliver_resp = (CMPP_MSG_DELIVER_RESP)outitem.getMsgObj(); //发送队列中取出;
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                    }
                    outitem.MsgState = (int)MsgState.SendFinished;  //完成
                    return (deliver_resp.toBytes());


                case (uint)CMPP_COMMAND_ID.CMPP_QUERY:
                    CMPP_MSG_QUERY query = (CMPP_MSG_QUERY)outitem.getMsgObj(); //发送队列中取出;
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);
                    }
                    outitem.MsgState = (int)MsgState.SendWaiting; //等待回应
                    return (query.toBytes());

                case (uint)CMPP_COMMAND_ID.CMPP_SUBMIT:
                    CMPP_MSG_SUBMIT submit = (CMPP_MSG_SUBMIT)outitem.getMsgObj(); //发送队列中取出;
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);
                    }
                    outitem.MsgState = (int)MsgState.SendFinished;
                    return (submit.toBytes());

                case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE:
                    CMPP_MSG_TERMINATE terminate = (CMPP_MSG_TERMINATE)outitem.getMsgObj(); //发送队列中取出;
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);
                    }
                    outitem.MsgState = (int)MsgState.SendWaiting;
                    return (terminate.toBytes());

                case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE_RESP:
                    CMPP_MSG_TERMINATE_RESP terminate_resp = (CMPP_MSG_TERMINATE_RESP)outitem.getMsgObj(); //发送队列中取出;
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                    }
                    outitem.MsgState = (int)MsgState.SendFinished;
                    return (terminate_resp.toBytes());

                default:
                    test = (CMPP_MSG_TEST)outitem.getMsgObj(); //发送队列中取出
                    lock (this)
                    {
                        outitem.MsgState = (int)MsgState.Sending;
                        this.delFromOutQueue(seq);
                        this.addToWaitingQueue(outitem);    //等待服务器的active_TEST_resp
                    }
                    outitem.MsgState = (int)MsgState.SendWaiting;
                    return (test.toBytes());
            }
        }

        private void sendQueueItem(QueueItem outitem)//将QueueItem发送出去
        {
            uint seq = outitem.Sequence;
            uint msgtype = outitem.MsgType;
            try
            {
                switch (msgtype)
                {
                    case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST:
                        CMPP_MSG_TEST test = (CMPP_MSG_TEST)outitem.getMsgObj(); //发送队列中取出
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                            this.addToWaitingQueue(outitem);    //等待服务器的active_TEST_resp
                        }
                        tcp.Send(test.toBytes());
                        outitem.MsgState = (int)MsgState.SendWaiting;
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP:
                        CMPP_MSG_TEST_RESP test_reply = (CMPP_MSG_TEST_RESP)outitem.getMsgObj(); //发送队列中取出//取出需要发送的具体消息
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                        }
                        tcp.Send(test_reply.toBytes());
                        outitem.MsgState = (int)MsgState.SendFinished;  //完成
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_CANCEL:
                        CMPP_MSG_CANCEL cancel = (CMPP_MSG_CANCEL)outitem.getMsgObj();    //还原成消息类
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                            this.addToWaitingQueue(outitem);    //等待回应
                        }
                        tcp.Send(cancel.toBytes());
                        outitem.MsgState = (int)MsgState.SendWaiting;
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_DELIVER_RESP:
                        CMPP_MSG_DELIVER_RESP deliver_resp = (CMPP_MSG_DELIVER_RESP)outitem.getMsgObj(); //发送队列中取出;
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                        }
                        tcp.Send(deliver_resp.toBytes());
                        outitem.MsgState = (int)MsgState.SendFinished;  //完成
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_QUERY:
                        CMPP_MSG_QUERY query = (CMPP_MSG_QUERY)outitem.getMsgObj(); //发送队列中取出;
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                            this.addToWaitingQueue(outitem);
                        }
                        tcp.Send(query.toBytes());
                        outitem.MsgState = (int)MsgState.SendWaiting; //等待回应
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_SUBMIT:
                        CMPP_MSG_SUBMIT submit = (CMPP_MSG_SUBMIT)outitem.getMsgObj(); //发送队列中取出;
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                            this.addToWaitingQueue(outitem);
                        }
                        tcp.Send(submit.toBytes());
                        outitem.MsgState = (int)MsgState.SendFinished;
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE:
                        CMPP_MSG_TERMINATE terminate = (CMPP_MSG_TERMINATE)outitem.getMsgObj(); //发送队列中取出;
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                            this.addToWaitingQueue(outitem);
                        }
                        if (this.tcpIsCanUse())
                        {
                            tcp.Send(terminate.toBytes());
                            outitem.MsgState = (int)MsgState.SendWaiting;
                        }
                        this.isStop = true;     //通知其他线程可以退出了
                        break;

                    case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE_RESP:
                        CMPP_MSG_TERMINATE_RESP terminate_resp = (CMPP_MSG_TERMINATE_RESP)outitem.getMsgObj(); //发送队列中取出;
                        lock (this)
                        {
                            outitem.MsgState = (int)MsgState.Sending;
                            this.delFromOutQueue(seq);
                        }
                        tcp.Send(terminate_resp.toBytes());
                        outitem.MsgState = (int)MsgState.SendFinished;
                        break;
                }
                LogLastOkTime(DateTime.Now);  //记录当前最后一次消息soket正确时间
            }
            catch (SocketException se)
            {
                //发生套接字错误
                this.ErrorInfo = this.ErrorInfo + "\r\n" + se.ToString();
            }
            catch (NullReferenceException nre)
            {
                this._bNre = true;  //出现空引用错误
                this.ErrorInfo = this.ErrorInfo + "\r\n" + nre.ToString();
            }
        }

        private bool tcpIsCanUse()  //测试当前tcp是否可用
        {
            bool reval = true;
            DateTime t = DateTime.Now;
            TimeSpan ts = t - this._lastOkTime;
            if (ts.TotalSeconds > CmppClient.ACTIVE_TEST_T_TICKS) //60秒
            {
                reval = false;  //不可用
            }
            if (this._bNre)
            {
                reval = false;
            }
            return (reval);
        }

        private void _reStartRecvNSend()
        {
            Send_Thread = new Thread(new ThreadStart(this.SendSPMsgThread));
            Send_Thread.Start();
            Recv_Thread = new Thread(new ThreadStart(this.RecvISMGMsgThread));
            Recv_Thread.Start();
        }

        private void LogLastOkTime(DateTime lastoktime)
        {
            lock (this)
            {
                this._lastOkTime = lastoktime;  //设定最后成功消息交互时间
            }
        }

        private void defaultReportHandler() //却省的报告事件处理函数
        {

        }

        private void defaultSMSHandler()
        {

        }

        private void defaultTeminateHandler()
        {

        }

        private void defaultTestEventHandler()
        {

        }
        private void defaultTestRespEventHandler()
        {

        }
        private void defaultTerminateEventHandler()
        {
        }
        private void defaultTerminateRespEventHandler()
        {
        }
        private void defaultCancelRespEventHandler()
        {
        }
        private void defaultQueryRespEventHandler()
        {
        }

        private void defaultConnectRespEventHandler()
        {
            QueueItem q = new QueueItem(this.getNextSequence(), (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST, 0, (int)MsgState.New);
            CMPP_MSG_TEST test = new CMPP_MSG_TEST(q.Sequence); //立即发送包过去
            q.setmsgObj(test);
            this.addToOutQueue(q);
        }
        private void defaultSubmitRespEventHandler()
        {
        }

        private void defaultClientStopEventHandler()
        { }

        private void rePortError(string info)
        {

        }

        private bool _init(string CMPPServer, int CMPPPort)
        {
            bool reVal = false;
            CMPP_Server = CMPPServer;
            SERVER_PORT = CMPPPort;
            try
            {
                tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ip = Dns.GetHostEntry(CMPP_Server);
                cmpp_ep = new IPEndPoint(ip.AddressList[0], SERVER_PORT);
                tcp.Connect(cmpp_ep); //连接
                reVal = true;
            }
            catch (SocketException se)
            {
                ErrorInfo = "Socker Error:" + se.ToString();
            }
            return (reVal);
        }
        private uint getNextSequence()
        {
            lock (typeof(CmppClient))
            {
                try
                {
                    lastSequence++;
                }
                catch (OverflowException ofe)
                {
                    this.ErrorInfo = this.ErrorInfo + "\r\n" + ofe.ToString();
                    lastSequence = uint.MinValue;
                }
                return (lastSequence);
            }
        }

        private void RecvISMGMsgThread()   //处理ISMG消息的线程
        {
            while (!this.isStop)
            {
                try
                {
                    byte[] rbuf = new byte[10240]; //结果缓冲区
                    byte[] recv_temp = new Byte[1024]; //recv临时缓冲区
                    int index = 0;
                    int msglength = tcp.Receive(rbuf);  //阻塞接收//分析收到的数据

                    CMPP_MSG_Header header;  //=new CMPP_MSG_Header(rbuf,index); //取得一个消息                   
                    while (index < msglength) //逐个消息分析
                    {
                        header = new CMPP_MSG_Header(rbuf, index); //取得一个消息      
                        byte[] the_pk = new byte[header.MSGLength];   //生成此消息的大小
                        for (int i = 0; i < header.MSGLength; i++)
                        {
                            the_pk[i] = rbuf[index++];
                        }
                        uint seq; //取得回复消息的下一个流水序列号
                        switch (header.Command_ID)
                        {
                            case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST: //服务器给客户的测试信号
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + "收到：CMPP_ACTIVE_TEST";
                                CMPP_MSG_TEST test = new CMPP_MSG_TEST(the_pk);
                                seq = test.Sequence;       //取得发送过来的流水号
                                CMPP_MSG_TEST_RESP test_reply = new CMPP_MSG_TEST_RESP(seq);
                                tcp.Send(test_reply.toBytes());    //马上送出回应包,不需要进入队列 
                                if (this.onTestHandler != null)
                                {
                                    TestEventArgs e = new TestEventArgs(test);
                                    onTestHandler(this, e);
                                }
                                else
                                {
                                    defaultTestEventHandler();
                                }
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + "发送：CMPP_ACTIVE_TEST_RESP ";
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP: //服务器的回应消息,应当丢弃不管
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_ACTIVE_TEST_RESP ");
                                CMPP_MSG_TEST_RESP test_reply2 = new CMPP_MSG_TEST_RESP(the_pk); //构造消息
                                seq = test_reply2.Sequence;    //寻找 曾经发送过去的消息        
                                this.delFromWaitingQueue(seq);      //删除等待队列中的消息 //清空等待回应队列
                                if (this.onTestRespHandler != null)
                                {
                                    TestRespEventArgs e = new TestRespEventArgs(test_reply2);
                                    onTestRespHandler(this, e);
                                }
                                else
                                {
                                    defaultTestRespEventHandler();
                                }
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_CANCEL_RESP:
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_CANCEL_RESP ");
                                CMPP_MSG_CANCEL_RESP cancel_reply = new CMPP_MSG_CANCEL_RESP(the_pk);//构造消息
                                seq = cancel_reply.Sequence;
                                this.delFromWaitingQueue(seq);
                                if (this.onCancelRespHandler != null)
                                {
                                    CancelRespEventArgs e = new CancelRespEventArgs(cancel_reply);
                                    onCancelRespHandler(this, e);
                                }
                                else
                                {
                                    defaultCancelRespEventHandler();
                                }
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_CONNECT_RESP:   //检查下消息的正确性，清除等待队列 设定连接成功标志
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_CONNECT_RESP ");
                                CMPP_MSG_CONNECT_RESP cn_reply = new CMPP_MSG_CONNECT_RESP(the_pk);
                                seq = cn_reply.Sequence;     //取得消息的seq
                                if (this.onConnectRespHandler != null)
                                {
                                    ConnectRespEventArgs e = new ConnectRespEventArgs(cn_reply);
                                    onConnectRespHandler(this, e);
                                }
                                else
                                {
                                    defaultConnectRespEventHandler();
                                }
                                if (cn_reply.isOk)
                                {
                                    this.isLogin = true;
                                }
                                else
                                {
                                    this.isLogin = false;
                                }
                                this.delFromWaitingQueue(seq);    //删除队列中的等待连接信息包
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_DELIVER:    //检查消息正确定，立即返回 正确 或者 失败,正确则处理是否状态包，不是状态包则存到MO缓存，表示收到信息,时状态包则判断缓存消息进行消息送达处理
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_DELIVER ");
                                BIConvert.DumpBytes(the_pk, "c:\\CMPP_DELIVER.txt");//保留映像
                                CMPP_MSG_DELIVER deliver = new CMPP_MSG_DELIVER(the_pk);
                                seq = (uint)deliver.ISMGSequence;       //发过来的流水号,需要立即发送一个deliver_resp       //一条 ISMG--〉SP 的消息
                                CMPP_MSG_DELIVER_RESP deliver_resp = new CMPP_MSG_DELIVER_RESP(seq);
                                deliver_resp.MsgID = deliver.MsgID;
                                deliver_resp.Result = 0;
                                byte[] t = deliver_resp.toBytes();
                                tcp.Send(t);
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("发送：CMPP__DELIVER_RESP ");
                                if (deliver.isReport)
                                {      //删除等待队列的消息//报告消息已经正确发送到        
                                    //UInt64 ReportMsgID=deliver.ReportMsgID ; //取得消息ID ,更新 MsgID
                                    string StateReport = deliver.StateReport; //取得关于此消息的状态
                                    //_debugBs(the_pk);
                                    ReportEventArgs arg = new ReportEventArgs(the_pk, CMPP_MSG_Header.HeaderLength + 8 + 21 + 10 + 1 + 1 + 1 + 21 + 1 + 1);    //构造报告事件参数
                                    //ReportEventArgs arg=new ReportEventArgs(ReportMsgID.ToString(),
                                    if (this.onReportHandler != null) //ReportEventArgs传递的字节数组是 报告信息包的数据,在此不考虑多个报告的情况
                                    {
                                        onReportHandler(this, arg);
                                    }
                                    else
                                    {
                                        this.defaultReportHandler();
                                    }
                                }
                                else
                                {//SMSEventArgs 传递的整个deliver包
                                    SMSEventArgs smsarg = new SMSEventArgs(the_pk, CMPP_MSG_Header.HeaderLength);
                                    if (this.onSMSHandler != null)
                                    {
                                        onSMSHandler(this, smsarg);   //触发事件,应当很快结束处理，不要靠考虑存储之类的耗费资源事宜
                                    }
                                    else
                                    {
                                        defaultSMSHandler();
                                    }
                                }
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_QUERY_RESP:
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_QUERY_RESP ");
                                //收到消息，处理后存入数据库
                                CMPP_MSG_QUERY_RESP query_resp = new CMPP_MSG_QUERY_RESP(the_pk);
                                this.delFromWaitingQueue(query_resp.Sequence);   //将等待的队列中的元素删除
                                if (this.onQueryRespHandler != null)
                                {
                                    QueryRespEventArgs e = new QueryRespEventArgs(query_resp);
                                }
                                else
                                {
                                    defaultQueryRespEventHandler();
                                }
                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_SUBMIT_RESP:    //收到服务器送达的慧英消息
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + ("收到：CMPP_SUBMIT_RESP ");
                                CMPP_MSG_SUBMIT_RESP submit_resp = new CMPP_MSG_SUBMIT_RESP(the_pk);
                                BIConvert.DumpBytes(the_pk, "c:\\CMPP_SUBMIT_RESP.txt");//保留映像
                                //BIConvert.DumpBytes(initValue,"c:\\CMPP_SUBMIT_RESP.txt");//保留映像
                                sub_resp++; //该变量仅供测试使用
                                delFromWaitingQueue(submit_resp.Sequence);  //删除需要等待的消息
                                if (this.onSubmitRespHandler != null)
                                {
                                    SubmitRespEventArgs e = new SubmitRespEventArgs(submit_resp);
                                    //submit_resp.
                                    onSubmitRespHandler(this, e);
                                }
                                else
                                {
                                    defaultSubmitRespEventHandler();
                                }

                                break;

                            case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE:
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + "收到：CMPP_TERMINATE";
                                CMPP_MSG_TERMINATE terminate = new CMPP_MSG_TERMINATE(the_pk);
                                seq = terminate.Sequence;
                                CMPP_MSG_TERMINATE_RESP terminate_resp = new CMPP_MSG_TERMINATE_RESP(seq);
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + "收到：CMPP_TERMINATE_RESP";
                                tcp.Send(terminate_resp.toBytes());
                                if (this.onTerminateHandler != null)
                                {
                                    TerminateEventArgs e = new TerminateEventArgs(terminate);
                                    onTerminateHandler(this, e);
                                    this.StopMe(); //准备自我停止?
                                }
                                else
                                {
                                    defaultTerminateEventHandler();
                                }
                                this._StopMe();  //发出终止设定        
                                return;   //退出线程        

                            case (uint)CMPP_COMMAND_ID.CMPP_TERMINATE_RESP:
                                this.ErrorInfo = this.ErrorInfo + "\r\n" + "收到：CMPP_TERMINATE_RESP";
                                CMPP_MSG_TERMINATE_RESP ter_resp = new CMPP_MSG_TERMINATE_RESP(the_pk);
                                seq = ter_resp.Sequence;  //取得流水信号
                                this.delFromOutQueue(seq);   //删除输出表重点项目
                                if (this.onTerminateRespHandler != null)
                                {
                                    TerminateRespEventArgs e = new TerminateRespEventArgs(ter_resp);
                                    onTerminateRespHandler(this, e);
                                }
                                else
                                {
                                    defaultTerminateRespEventHandler();
                                }
                                this._StopMe();
                                break;
                        }
                    }
                    LogLastOkTime(DateTime.Now);  //记录当前最后一次消息soket正确时间
                }
                catch (SocketException)
                {
                    //超时   
                }
                Thread.Sleep(50);
            }
        }
        //debug
        //  private void _debugBs(byte[] the_pk) //存储byte字节
        //  {
        //   
        //  }
        //debug

        private void DeamonThread()    //监视本系统连接是否正常
        {//此线程是监视线程
            int t_count = 0;   //循环时间计数
            _reStartRecvNSend();   //启动接收和发送
            while (!this.isStop)
            {
                t_count++;    //0.1秒   
                if (tcpIsCanUse())
                {
                    if (this.isPingTime())
                    {
                        this.ping();  //发送一个ping包
                    }
                    if (t_count > 50)  // 500*100=50000=50秒
                    {
                        t_count = 0;
                        checkReSend(); //检查需要重新发送的消息
                        //触发一个事件，让系统自动检查消息队列，存储消息队列中的消息状态
                    }
                }
                else
                {
                    EventArgs e = new EventArgs();
                    if (this.onSocketClosedHandler != null)
                    {
                        onSocketClosedHandler(this, e);
                    }
                    else
                    {
                    }
                    this.isStop = true;  //通知其他线程退出
                }
                Thread.Sleep(1000);
            }
        }

        private void SendSPMsgThread()
        {
            while (!this.isStop)
            {
                Thread.Sleep(10);
                if (this.isLogin)
                {
                    ArrayList lists = this.getTop16Queue();  //取出16条最顶的消息    
                    if (lists != null && lists.Count > 0)
                    {
                        int count = lists.Count;
                        ArrayList pks = new ArrayList(count); //定义容量
                        for (int i = 0; i < lists.Count; i++)
                        {
                            QueueItem outitem = (QueueItem)lists[i]; //取出每一个消息对象
                            if (outitem != null)
                            {
                                try
                                {
                                    sendQueueItem(outitem);    //发送每一个消息
                                }
                                catch (SocketException)
                                {
                                    //发送失败
                                    outitem.FailedCount++;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void _StopMe()
        {
            lock (this)
            {
                this.isStop = true;
            }
        }

        private void _forcedSubThread(Thread t)   //强制停止线程
        {
            try
            {
                t.Abort();
                t.Join();
            }
            catch (Exception)
            { }
        }

        public bool Init(string CMPPServer, int CMPPPort)
        {
            return (this._init(CMPPServer, CMPPPort));
        }

        public bool Init(string CMPPServer, int CMPPPort, int recvtimeout, int sendtimeout)
        {
            this.RecvTimeOut = recvtimeout;
            this.SendTimeout = sendtimeout;
            return (this._init(CMPPServer, CMPPPort));
        }

        public bool Init(string CMPPServer, int CMPPPort, int recvtimeout)
        {
            this.RecvTimeOut = recvtimeout;
            this.SendTimeout = recvtimeout;
            return (this._init(CMPPServer, CMPPPort));
        }

        public bool Login(string SystemID, string UserName, string Password)
        {
            try
            {
                SendLogin(SystemID, UserName, Password);
                this.LogLastOkTime(DateTime.Now);    //最后一次正确的发送
            }
            catch (SocketException se)
            {
                //发送出错
                this.ErrorInfo = this.ErrorInfo + "\r\n" + se.ToString();
                return (false);
            }
            DateTime t1 = DateTime.Now;
            while (!this.isLogin)
            {
                byte[] rbuf = new Byte[400];
                int l;
                try
                {
                    l = tcp.Receive(rbuf);
                    if (l > 16)
                    {
                        if (BIConvert.Bytes2UInt(rbuf, 4) == (uint)CMPP_COMMAND_ID.CMPP_CONNECT_RESP)
                        {
                            CMPP_MSG_CONNECT_RESP resp = new CMPP_MSG_CONNECT_RESP(rbuf);
                            if (resp.isOk)
                            {
                                EventArgs e = new EventArgs();
                                if (onLogonSuccEventHandler != null)
                                {
                                    onLogonSuccEventHandler(this, e);
                                }
                                else
                                {
                                    this.defaultConnectRespEventHandler();
                                }
                                this.isLogin = true;
                            }
                            else
                            {
                            }
                            break;
                        }
                    }
                    this._lastOkTime = DateTime.Now;  //更新当前最后成功收发套接字的时间
                }
                catch (SocketException)
                {
                }
                System.TimeSpan t = DateTime.Now - t1;
                if (t.TotalSeconds > 10)
                {
                    break;
                }
            }
            if (this.isLogin)
            { //登录ok,就立即发送active_test包
                this.ErrorInfo = this.ErrorInfo + "\r\n" + " Logon succ! ";
                startThreads();  // 启动 主监视程序de线程
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public uint SubmitSMS(string to_user, string fee_code, string svc_code, string fee_user, string spnum, string content, int fee_usertype)
        {
            CMPP_MSG_SUBMIT sndmsg;
            uint seq = this.getNextSequence();   //取得下一个sequence
            sndmsg = new CMPP_MSG_SUBMIT(seq);
            sndmsg.FeeCode = fee_code;
            sndmsg.FeeTerminalId = to_user;
            sndmsg.FeeType = FeeType.FEE_TERMINAL_PERITEM; //按条收取
            sndmsg.FeeUserType = fee_usertype;
            sndmsg.Msg_Level = 0;
            sndmsg.MSGFormat = (uint)Msg_Format.UCS2;
            sndmsg.SMS_Content = content;
            sndmsg.SrcID = spnum;         //长号码
            sndmsg.SPID = this.systemID;
            sndmsg.Svc_Code = svc_code;
            sndmsg.UDHI = 0;
            sndmsg.ValIdTime = getValIdTime(DateTime.Now);        //存活时间
            sndmsg.addTerminalID(to_user);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_SUBMIT, 0, 0);
            q.setmsgObj(sndmsg);
            this.addToOutQueue(q);
            return (seq);
        }

        public uint SendMsg(string to_user, string fee_user, string fee, string svccode, string content, string spnum)
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_SUBMIT sndmsg = new CMPP_MSG_SUBMIT(seq);
            sndmsg.FeeCode = fee;
            sndmsg.FeeType = FeeType.FEE_TERMINAL_PERITEM;
            sndmsg.FeeTerminalId = fee_user;
            sndmsg.FeeUserType = (int)FeeUserType.FEE_NULL;    //计费 按照计费号码计费
            sndmsg.SPID = this.systemID;         //企业代码
            sndmsg.UDHI = 0;             //
            sndmsg.MSGFormat = (uint)Msg_Format.GB2312;
            sndmsg.SMS_Content = content;
            sndmsg.SrcID = spnum;
            sndmsg.Svc_Code = svccode;
            sndmsg.addTerminalID(to_user);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_SUBMIT, 0, 0);
            q.setmsgObj(sndmsg);
            this.addToOutQueue(q);
            return (seq);
        }

        public uint SendSMC(string fee_user, string feecode, string svccode)  //向计费用户发送一条包月计费信息
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_SUBMIT sndmsg = new CMPP_MSG_SUBMIT(seq);
            sndmsg.SMS_Delivery_Type = 2;         //产生包月SMC
            sndmsg.FeeCode = feecode;
            sndmsg.FeeType = FeeType.FEE_TERMINAL_MONTH;   //包月计费
            sndmsg.FeeTerminalId = fee_user;
            sndmsg.FeeUserType = (int)FeeUserType.FEE_TERMINAL_ID;    //计费 按照计费号码计费
            sndmsg.SPID = this.systemID;         //企业代码
            sndmsg.UDHI = 0;             //
            sndmsg.MSGFormat = (uint)Msg_Format.UCS2;
            sndmsg.SMS_Content = "SMC";
            sndmsg.SrcID = this.userName;         //sp的特符号码
            sndmsg.Svc_Code = svccode;
            sndmsg.addTerminalID(fee_user);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_SUBMIT, 0, 0);
            q.setmsgObj(sndmsg);
            this.addToOutQueue(q);
            return (seq);
        }

        public uint SendSMT(string to_user, string feecode, string svccode, string spnum, string content)
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_SUBMIT sndmsg = new CMPP_MSG_SUBMIT(seq);
            sndmsg.SMS_Delivery_Type = 1;         //产生包月SMC
            sndmsg.FeeCode = feecode;          //包月计费代码
            sndmsg.FeeType = FeeType.FEE_TERMINAL_MONTH;   //包月计费
            sndmsg.FeeTerminalId = to_user;
            sndmsg.FeeUserType = (int)FeeUserType.FEE_TERMINAL_ID;    //计费 按照计费号码计费
            sndmsg.SPID = this.systemID;         //企业代码
            sndmsg.UDHI = 0;             //
            sndmsg.MSGFormat = (uint)Msg_Format.UCS2;
            sndmsg.SMS_Content = content;
            sndmsg.SrcID = spnum;         //sp的特符号码
            sndmsg.Svc_Code = svccode;
            sndmsg.addTerminalID(to_user);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_SUBMIT, 0, 0);
            q.setmsgObj(sndmsg);
            this.addToOutQueue(q);
            return (seq);
        }

        public uint SendQuery(string svccode, string whichday) //查询某个业务的总计数
        {
            string wd = whichday.Trim();
            int query_type = 0;
            if (svccode == null || svccode.CompareTo("") == 0)
            {//查询全部页数量
            }
            else
            {//查询某项业务
                query_type = 1;
            }
            if (wd == null || wd.CompareTo("") == 0)
            {
                DateTime d = DateTime.Now;
                wd = d.Year.ToString() + d.Month.ToString().PadLeft(2, '0') + d.Day.ToString().PadLeft(2, '0');
            }
            uint seq = this.getNextSequence();
            CMPP_MSG_QUERY query = new CMPP_MSG_QUERY(seq);
            query.Query_Type = query_type;
            query.Query_Code = svccode;
            query.Time = wd;     //设定那一天
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_QUERY, 0, 0);
            q.setmsgObj(query);
            this.addToOutQueue(q);
            return (seq);   //返回消息的内部编号
        }

        public uint StopCMPPConnection()   //停止CMPP协议的socket连接
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_TERMINATE t = new CMPP_MSG_TERMINATE(seq);
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_TERMINATE, 0, 0);
            q.setmsgObj(t);
            this.addToOutQueue(q);
            return (seq); //返回终止消息，便于等待
        }


        public uint CancelMsg(string msgid)
        {
            uint seq = this.getNextSequence();
            CMPP_MSG_CANCEL cancel = new CMPP_MSG_CANCEL(seq);
            cancel.MsgID = msgid;
            QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_CANCEL, 0, 0);
            q.setmsgObj(cancel);
            this.addToOutQueue(q);
            return (seq);   //返回消息的内部编号
        }

        public void StopMe()
        {
            if (!this.isStop)
            {
                if (this.tcpIsCanUse())//发送一条对服务器的通告
                {
                    uint seq = this.getNextSequence();
                    CMPP_MSG_TERMINATE t = new CMPP_MSG_TERMINATE(seq);
                    QueueItem q = new QueueItem(seq, (uint)CMPP_COMMAND_ID.CMPP_TERMINATE, 0, 0);
                    q.setmsgObj(t);
                    this.addToOutQueue(q);
                }
                Thread.Sleep(500);   //等待1000ms,告知服务器
                this._StopMe();
                tcp.Close();

                if (this.onClientSvcStopedHandler != null)
                {
                    ClientQueueStateArgs arg = new ClientQueueStateArgs(this._outSeqQueue, this._waitingSeqQueue);
                    onClientSvcStopedHandler(this, arg);
                }
                else
                {
                    this.defaultClientStopEventHandler();
                }
                Thread.Sleep(500);   //再次主动等待线程结束         
                //此处报告 2个队列中的信息    
            }
            //准备强行结束
            _forcedSubThread(this.Send_Thread);
            Thread.Sleep(500);   //等待1000ms,告知服务器
            _forcedSubThread(this.Recv_Thread);
            Thread.Sleep(500);   //等待1000ms,告知服务器
            _forcedSubThread(this.Deamo_Thread);
            Thread.Sleep(500);   //等待1000ms,告知服务器
        }

        public string getLogInfo()
        {
            string t = this.ErrorInfo;
            this.ErrorInfo = "";
            return (t);
        }

        public int getQueueItemState(uint seq)  //根据seq寻找发送内部队列的消息对象的状态
        {
            int status = 0;       //状态未知
            if (this._outSeqQueue.ContainsKey(seq)) //存在于outSeqQueue中
            {
                if (this._waitingSeqQueue.Contains(seq))
                {
                    //正在发送等待返回，状态未定
                }
                else
                {
                    //还没有发送
                }
            }
            else
            {
                if (this._waitingSeqQueue.ContainsKey(seq))
                {
                    //正等待回应
                }
                else
                {
                    //已经发送结束了
                }
            }
            return (status);
        }

        public void TestSubmit(string[] nums, int topI, int topJ)  //发送测试包
        {
            int count = 0;
            int total = 0;
            ArrayList pks = new ArrayList();
            //准备100个包
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    uint seq = this.getNextSequence(); //准备流水号
                    CMPP_MSG_SUBMIT sndmsg = new CMPP_MSG_SUBMIT(seq);
                    sndmsg.FeeCode = "000001";
                    sndmsg.FeeTerminalId = nums[i];
                    sndmsg.FeeType = FeeType.FEE_TERMINAL_PERITEM; //按条收取
                    sndmsg.FeeUserType = 0;  //终端用户计费
                    sndmsg.Msg_Level = 0;
                    sndmsg.MSGFormat = (uint)Msg_Format.UCS2;
                    sndmsg.SMS_Content = "test";
                    sndmsg.SrcID = "09880";         //长号码
                    sndmsg.SPID = this.systemID;
                    sndmsg.Svc_Code = "cmcctest";
                    sndmsg.UDHI = 0;
                    sndmsg.ValIdTime = getValIdTime(DateTime.Now);        //存活时间
                    sndmsg.addTerminalID(nums[i]);
                    pks.Add(sndmsg.toBytes());   //存入数组
                }
            }

            DateTime t1 = DateTime.Now;
            for (int i = 0; i < topI; i++)
            {
                for (int j = 0; j < topJ; j++)
                {
                    try
                    {
                        tcp.Send((byte[])pks[i * 10 + j]);
                        count++;
                        total++;
                    }
                    catch (SocketException se)
                    {
                        this.ErrorInfo = this.ErrorInfo + "\r\n" + "发送错误： " + se.ToString();
                    }
                    if (count >= 16)
                    {
                        count = 0;  //复位
                        Thread.Sleep(50);  //暂停20ms
                    }
                }
            }
            DateTime t2 = DateTime.Now;
            TimeSpan t = t2 - t1;
            this.ErrorInfo = this.ErrorInfo + "\r\n" + "发送: " + total + " 条消息, 总计花费时间:" + t.TotalMilliseconds + "毫秒";
        }  //测试函数////////////////////////////////////////////////供测试移动网络测试

    }

    internal enum MsgState     //CMPP消息在队列中的状态枚举值
    {
        New = 0,      //加入到队列等待发送出去
        Sending = 1,     //正被某个线程锁定
        SendWaiting = 2,   //发送出去，现在等待resp消息返回
        SendFinished = 3   //得到回应，一般等待被清理出队列
    }

    public class QueueItem   //代表一个存储在缓存队列中的消息，序列号由CMPPClient产生
    {
        uint _sequence;    //消息索引 就是流水号
        uint _msgType;    //消息的类别就是COMMAND_ID,根据此值决定 Object _msgObj的原类型
        int _failedCount = 0;   //失败计数，如果失败次数超过3此需要进行清理
        int _msgState;    //当前消息状态,具体为 MSG_STATE的枚举类型
        object _msgObj;    //存放消息对象,具体类型参考 _msgType
        DateTime _inQueueTime;  //消息进入队列的时间,上次消息的发送时间

        public int FailedCount
        {
            set
            {
                this._failedCount = value;
            }
            get
            {
                return (this._failedCount);
            }

        }

        public object getMsgObj()
        {
            return (this._msgObj);
        }
        public void setmsgObj(object inmsg)
        {
            this._msgObj = inmsg;
        }
        public DateTime inQueueTime
        {
            set
            {
                this._inQueueTime = value;
            }
            get
            {
                return (this._inQueueTime);
            }
        }
        public uint MsgType
        {
            get
            {
                return (this._msgType);
            }
        }
        public int MsgState
        {
            get
            {
                return (this._msgState);
            }
            set
            {
                this._msgState = value;
            }
        }
        public uint Sequence
        {
            get
            {
                return (this._sequence);
            }
            set
            {
                this._sequence = value;
            }
        }
        public QueueItem(uint sequence, uint msgtype, int faildedcount, int msgstate)
        {
            this._failedCount = faildedcount;
            this._msgState = msgstate;
            this._msgType = msgtype;
            this._sequence = sequence;
        }
        public QueueItem(uint sequence, uint msgtype, int faildedcount, int msgstate, object msgobj)
        {
            this._failedCount = faildedcount;
            this._msgState = msgstate;
            this._msgType = msgtype;
            this._sequence = sequence;
            this.setmsgObj(msgobj);
        }
    }

    public class BIConvert  //字节 整形 转换类 网络格式转换为内存格式
    {
        public static byte[] Int2Bytes(uint i)  //转换整形数据网络次序的字节数组
        {
            byte[] t = BitConverter.GetBytes(i);
            byte b = t[0];
            t[0] = t[3];
            t[3] = b;
            b = t[1];
            t[1] = t[2];
            t[2] = b;
            return (t);
        }

        public static uint Bytes2UInt(byte[] bs, int startIndex) //返回字节数组代表的整数数字，4个数组
        {
            byte[] t = new byte[4];
            for (int i = 0; i < 4 && i < bs.Length - startIndex; i++)
            {
                t[i] = bs[startIndex + i];
            }
            byte b = t[0];
            t[0] = t[3];
            t[3] = b;
            b = t[1];
            t[1] = t[2];
            t[2] = b;
            return (BitConverter.ToUInt32(t, 0));
        }

        public static uint Bytes2UInt(byte[] bs)  //没有指定起始索引
        {
            return (Bytes2UInt(bs, 0));
        }

        public static void DumpBytes(byte[] bs, string txt)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(txt);
            for (int i = 0; i < bs.Length; i++)
            {
                byte b = bs[i];
                sw.WriteLine(b.ToString("X") + " ");
            }
            sw.WriteLine("-----" + DateTime.Now.ToLocalTime());
            sw.Close();
        }

        public static void DebugString(string bs, string txt)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(txt);
            sw.WriteLine(bs);
            sw.WriteLine("-----" + DateTime.Now.ToLocalTime());
            sw.Close();
        }
    }

    public class ReportEventArgs : EventArgs   //报告事件携带的数据
    {
        string _smsState;      //发送短信的应答结果，含义与SMPP协议要求中stat字段定义相同 。SP根据该字段确定CMPP_SUBMIT消息的处理状态。
        string _submitTime;      //提交短信的时间，也可根据此时间决定是否重发
        string _doneTime;      //送达目的地的时间
        string _destNum;      //送达的号码
        string _msgID;       //关于那一条消息的报告
        uint _sequence;       //CMPP网关产生的流水号

        UInt64 _msg_id;   //被报告的提交短信的msgID,ISMG在submit_resp返回给SP的

        public string State
        {
            get
            {
                return (this._smsState);
            }
            set
            {
                this._smsState = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return (this._sequence);
            }
            set
            {
                this._sequence = value;
            }
        }

        public string toUserNum
        {
            get
            {
                return (this._destNum);
            }
            set
            {
                this._destNum = value;
            }
        }

        public string MsgID
        {
            get
            {
                return (this._msgID);
            }
            set
            {
                this._msgID = value;
            }
        }

        public string SubmitTime
        {
            get
            {
                return (this._submitTime);
            }
            set
            {
                this._submitTime = value;
            }
        }

        public string DoneTime
        {
            get
            {
                return (this._doneTime);
            }
            set
            {
                this._doneTime = value;
            }
        }

        public ReportEventArgs(byte[] bs)  //从一个字节数组中获得报告
        {
            byte[] temp = new byte[8 + 7 + 10 + 10 + 21 + 4];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bs[i];
            }
            init(temp);
        }

        public ReportEventArgs(byte[] bs, int startIndex) //起始
        {
            byte[] temp = new byte[8 + 7 + 10 + 10 + 21 + 4];//定义长度
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bs[startIndex + i];
            }
            init(temp);
        }

        public ReportEventArgs(string msgid, string destnum, string smsstate, string submittime, string donetime, uint seq)
        {
            this._msgID = msgid;
            this._destNum = destnum;
            this._smsState = smsstate;
            this._submitTime = submittime;
            this._doneTime = donetime;
            this._sequence = seq;
        }

        public DateTime getSubmitTime()
        {
            return (getTime(this._submitTime));
        }

        public DateTime getDoneTime()
        {
            return (getTime(this._doneTime));
        }

        private DateTime getTime(string time_string)
        {
            int index = 0;
            int yy = Convert.ToInt32("20" + time_string.Substring(index, 2));
            index += 2;
            int mm = Convert.ToInt32(time_string.Substring(index, 2));
            index += 2;
            int dd = Convert.ToInt32(time_string.Substring(index, 2));
            index += 2;
            int hh = Convert.ToInt32(time_string.Substring(index, 2));
            index += 2;
            int mms = Convert.ToInt32(time_string.Substring(index, 2));
            DateTime t = new DateTime(yy, mm, dd, hh, mms, 0);
            return (t);
        }
        private void init(byte[] bs)
        {
            BIConvert.DumpBytes(bs, "c:\\ReportEventArgs.txt");//保留映像
            int index = 0;
            this._msg_id = BitConverter.ToUInt64(bs, index);   //BitConverter.ToUInt64(bs,index);
            this._msgID = (this._msg_id.ToString());
            // BIConvert.DebugString(this._msgID ,"c:\\MSGID.txt"); 
            index += 8;
            this._smsState = Encoding.ASCII.GetString(bs, index, 7);
            index += 7;
            this._submitTime = Encoding.ASCII.GetString(bs, index, 10);
            index += 10;
            this._doneTime = Encoding.ASCII.GetString(bs, index, 10);
            index += 10;
            this._destNum = Encoding.ASCII.GetString(bs, index, 21);
            index += 21;
            this._sequence = BIConvert.Bytes2UInt(bs, index);
        }
    }

    public class SMSEventArgs : EventArgs
    {
        UInt64 _msgid;    //8字节的消息标示
        string _destID;   //接受信息的目标ID
        string _svcCode;    //业务代码
        int _tpPID;    //参考GSM协议
        int _tpUDHI;   //
        int _msgFrm;   //消息的编码格式
        string _srcTerminateID;  //源终端ID,如果使报告
        int _msgLength;   //消息的字节数，并非实际字符串长度
        string _Content;   //消息正文内容


        public SMSEventArgs(byte[] bs)
        {
            int msglen = BitConverter.ToInt32(bs, 8 + 21 + 10 + 1 + 1 + 1 + 21 + 1);  //取得消息长度字节长度
            int tempLen = msglen + 8 + 21 + 10 + 1 + 1 + 1 + 21 + 1 + 1 + msglen + 8;
            byte[] temp = new byte[tempLen];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bs[i];
            }
            init(temp);
        }

        public SMSEventArgs(byte[] bs, int baseIndex)
        {
            int msglen = (int)bs[CMPP_MSG_Header.HeaderLength + 8 + 21 + 10 + 1 + 1 + 1 + 21 + 1];  //取得消息长度字节长度
            int tempLen = 8 + 21 + 10 + 1 + 1 + 1 + 21 + 1 + 1 + msglen + 8;
            byte[] temp = new byte[tempLen];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = bs[i + baseIndex];
            }
            init(temp);
        }

        private void init(byte[] bs)
        {
            BIConvert.DumpBytes(bs, "c:\\SMSEventArgs.txt");//保留映像

            int index = 0;
            this._msgid = BitConverter.ToUInt64(bs, 0);
            index += 8;
            this._destID = Encoding.ASCII.GetString(bs, index, _getRealBytesLength(bs, index));
            index += 21;
            this._svcCode = Encoding.ASCII.GetString(bs, index, _getRealBytesLength(bs, index));
            index += 10;
            this._tpPID = (int)bs[index++];
            this._tpUDHI = (int)bs[index++];
            this._msgFrm = (int)bs[index++];
            this._srcTerminateID = Encoding.ASCII.GetString(bs, index, _getRealBytesLength(bs, index));
            index += 21;
            index++;        //是否是状态报告字节
            this._msgLength = (int)bs[index++];  //取得字节长度

            switch (this._msgFrm)
            {
                case (int)Msg_Format.ASCII:
                    this._Content = Encoding.ASCII.GetString(bs, index, this._msgLength);
                    break;

                case (int)Msg_Format.GB2312:
                    this._Content = Encoding.Default.GetString(bs, index, this._msgLength);
                    break;

                case (int)Msg_Format.UCS2:
                    this._Content = Encoding.BigEndianUnicode.GetString(bs, index, this._msgLength);
                    break;

                case (int)Msg_Format.BINARY:
                    break;

                case (int)Msg_Format.WRITECARD:
                    break;

                default:
                    break;

            }

        }

        public string toSPNum
        {
            get
            {
                return (this._destID);
            }
        }

        public string SrcNum
        {
            get
            {
                return (this._srcTerminateID);
            }
        }

        public string Content
        {
            get
            {
                return (this._Content);
            }
        }
        public string SvcCode
        {
            get
            {
                return (this._svcCode);
            }
        }

        public UInt64 MsgID
        {
            get
            {
                return (this._msgid);
            }
            //set
            //{
            // this._msgid =value;
            //}
        }

        public string MsgIDString
        {
            get
            {
                return (this._msgid).ToString();
            }
        }

        private int _getRealBytesLength(byte[] bts, int index)
        {
            int i = index;
            for (; i < bts.Length; i++)
            {
                if (bts[i] == 0)
                {
                    break;
                }
            }
            return i - index;
        }
    }

    public class TerminateEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_TERMINATE _msg;

        public TerminateEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public TerminateEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_TERMINATE)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

    }

    public class TerminateRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_TERMINATE_RESP _msg;

        public TerminateRespEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public TerminateRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_TERMINATE_RESP)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }
    public class TestEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_TEST _msg;

        public TestEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public TestEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_TEST)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }

    public class TestRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_TEST_RESP _msg;

        public TestRespEventArgs(uint seq)
        {
            this._seq = seq;
        }
        public TestRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_TEST_RESP)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }

    public class CancelRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_CANCEL_RESP _msg;

        public CancelRespEventArgs(uint seq)
        {
            this._seq = seq;
        }
        public CancelRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_CANCEL_RESP)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }

    public class QueryRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_QUERY_RESP _msg;

        public QueryRespEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public QueryRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_QUERY_RESP)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }
        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }

    }

    public class ConnectRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_CONNECT_RESP _msg;

        public ConnectRespEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public ConnectRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_CONNECT_RESP)msg;
            this._seq = this._msg.Sequence;
        }

        public object getMSG()
        {
            return (this._msg);
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }

    public class SubmitRespEventArgs : EventArgs
    {
        private uint _seq;
        private CMPP_MSG_SUBMIT_RESP _msg;

        public SubmitRespEventArgs(uint seq)
        {
            this._seq = seq;
        }
        public SubmitRespEventArgs(object msg)
        {
            this._msg = (CMPP_MSG_SUBMIT_RESP)msg;
            this._seq = this._msg.Sequence;
        }
        public object getMSG()
        {
            return (this._msg);
        }
        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }
    }

    public class WaitingQueueItemEventArgs : EventArgs
    {
        private uint _seq;
        private object _q;

        public WaitingQueueItemEventArgs(uint seq)
        {
            this._seq = seq;
        }

        public WaitingQueueItemEventArgs(object q)
        {
            this._q = q;
        }

        public uint Sequence
        {
            set
            {
                this._seq = value;
            }
            get
            {
                return (this._seq);
            }
        }

        public object getQueueItem()
        {
            return (this._q);
        }
    }

    public class ClientQueueStateArgs : EventArgs  //当CMPP client的服务停止时候，队列的状态参数
    {
        private SortedList _waiting;
        private SortedList _out;

        public ClientQueueStateArgs(SortedList outQueue, SortedList inQueue)
        {
            this._waiting = inQueue;
            this._out = outQueue;
        }

        public SortedList WaitingQueue
        {
            get
            {
                return (this._waiting);
            }
            set
            {
                this._waiting = value;
            }
        }

        public SortedList OutQueue
        {
            get
            {
                return (this._out);
            }
            set
            {
                this._out = value;
            }
        }
    }

    public class StateObject
    {
        public Socket workSocket = null;              // Client socket.
        public const int BufferSize = 1024;            // Size of receive buffer.
        public byte[] buffer = new byte[BufferSize];  // Receive buffer.
        public byte[] result_buf = null;     //接收最终的结果缓冲区  
        public int _msglength = 0;      //接收到多少个字节数据
    }

    public enum SMSDBQueue_Status : int
    {
        New = 0,   //新消息，等待发送
        Sended = 1,  //正在发送，等待回应
        Resp = 2,   //送达SMSC，得到msgid
        Delived = 3,  //得到报告，状态ok
        Expired = 4,  //过期
        Deleted = 5,  //已经删除此消息 UNDELIV
        Accepted = 6,  // ACCEPTD 状态 未送达 ACCEPTED为中间状态，网关若从短信中心收到后应丢弃，不做任何操作
        Undelived = 7,  //未送达
        Unknown = 8,  //未知
        Rejected = 9   //被弹回
    }

    public class SMSDBQueue
    {
        private uint _sequence = 0;      //对应的cmppclient的序列号
        private int _smsdbID = 0;       //对应的数据库的发送自动增长ID
        private int _cmpp_msgType;      //cmpp协议的定义的消息类型
        private object _cmpp_msg_object;    //cmpp消息对象,备检索
        private int _currentStatus;      //当前消息体状态
        private DateTime _inQueueTime = DateTime.Now;  //消息产生设定值
        private UInt64 _msgid;       //消息返回的SMSC给出的id遍号

        public uint Sequence
        {
            get
            {
                return (this._sequence);
            }
            set
            {
                this._sequence = value;
            }
        }

        public int SMSDBID
        {
            get
            {
                return (this._smsdbID);
            }
            set
            {
                this._smsdbID = value;
            }
        }

        public int CurrentStatus
        {
            get
            {
                return (this._currentStatus);
            }
            set
            {
                this._currentStatus = value;
            }
        }

        public int CMPPMsgType
        {
            get
            {
                return (this._cmpp_msgType);
            }
            set
            {
                this._cmpp_msgType = value;
            }
        }

        public DateTime InQueueTime
        {
            get
            {
                return (this._inQueueTime);
            }
            set
            {
                this._inQueueTime = value;
            }
        }

        public object MsgObject
        {
            get
            {
                return (this._cmpp_msg_object);
            }
            set
            {
                this._cmpp_msg_object = value;
            }
        }

        public UInt64 UIMsgID
        {
            get
            {
                return (this._msgid);
            }
            set
            {
                this._msgid = value;
            }
        }

        public string MsgID
        {
            get
            {
                return (this._msgid.ToString());
            }
            set
            {
                this._msgid = Convert.ToUInt64(value);
            }
        }
    }

    public interface ISMSStore   //定义一个存储接口
    {
        SMSRec[] getSMSsFromStore();     //从外部存储中获取消息 
        void updateStoreObjec(int storeObjectID, string state); //更新id代表的对象状态,用于监测状态报告
        void updateSMSCMsgID(int storeObjectID, UInt64 msgid);
        bool addSMS2Store(SMSRec sms);       //向store中存储短消息
        string getFeeCode(string svccode);      //根据svccode返回消息的收费,根据移动参数进行设定  
    }

    public class SMSRec     //SMS载数据库中的表达
    {
        private int _RecID;
        private string _feeUser;  //计费用户
        private int _smsType = 0;   //短信类型 0 普通文字短信 1 闪烁短信 2 查询命令短信 3 包月短信
        private string _svccode;  //服务代码
        private string _msg;   //消息
        private string _state;   //消息状态
        private string _feecode;
        private string _spnum;
        private string _toUser;

        public SMSRec(int recid, string feeuser, string feecode, string svccode, string msg, string spnum)
        {
            this._RecID = recid;
            this._feeUser = feeuser;
            this._toUser = feeuser;
            this._svccode = svccode;
            this._msg = msg;
            this._spnum = spnum;
        }

        public SMSRec(int recid, string feeuser, string touser, string feecode, string svccode, string msg, string spnum)
        {
            this._RecID = recid;
            this._feeUser = feeuser;
            this._toUser = touser;
            this._svccode = svccode;
            this._msg = msg;
            this._spnum = spnum;
        }

        public string SvcCode
        {
            get
            {
                return (this._svccode);
            }
            set
            {
                this._svccode = value;
            }
        }

        public string FeeCode
        {
            get
            {
                return (this._feecode);
            }
            set
            {
                this._feecode = value;
            }
        }

        public string FeeUser
        {
            get
            {
                return (this._feeUser);
            }
            set
            {
                this._feeUser = value;
            }
        }

        private string ToUser
        {
            get
            {
                return (this._toUser);
            }
            set
            {
                this._toUser = value;
            }
        }

        public string SPNum
        {
            get
            {
                return (this._spnum);
            }
            set
            {
                this._spnum = value;
            }
        }

        public string Message
        {
            get
            {
                return (this._msg);
            }
            set
            {
                this._msg = value;
            }
        }


    }
    //****************************** 接口类结束 *********************************

    //*************************定义 处理数据库接口的SMS系统类，该类对外提供CMPP处理功能***********
    //**

    /// <summary>
    /// 功能，实现队列监测，处理失败消息、成功消息，处理定时存储更新等抽象功能的组织，将CMPPClient包装提供
    /// </summary>
    public class SMSSystem
    {
        private ISMSStore dbcls = null;
        private CmppClient client = null;
        private string pwd;
        private string systemid;
        private string spnum;

        public void setISMSStoreInterface(ISMSStore ismsstore)
        {
            dbcls = ismsstore;
        }

        public SMSSystem(string systemid, string spnum, string password, string cmppserverip, int cmppport)
        {
            client = new CmppClient();
            client.Init(cmppserverip, cmppport);
            this.spnum = spnum;
            this.systemid = systemid;
            this.pwd = password;
        }

    }
}
