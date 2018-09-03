using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Vultrue.Communication
{
    /// <summary>
    /// 利用UDP协议提供通信服务
    /// Send方法, 可能被多个线程调用, 需要互斥访问
    /// 用单独线程接收数据, 每接收到一条数据报, 产生一次DataReceived事件
    /// </summary>
    public partial class UDPClient : NETClient
    {
        #region 变量
        private const int BUFFERSIZE = 8192;
        private byte[] buffer = new byte[BUFFERSIZE];
        private EndPoint source = new IPEndPoint(IPAddress.Any, 0);
        private Socket socket;
        private Thread threadReceive;
        private EndPoint target;
        #endregion

        #region 构造&初始化

        public UDPClient()
        {
            InitializeComponent();
            initializeComponent();
        }

        public UDPClient(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            LocalIPPort = "";
        }

        #endregion

        #region 属性

        [DefaultValue("")]
        public string LocalIPPort { get; set; }

        [DefaultValue("")]
        public string DefaultRemote
        {
            get
            {
                return target == null ? "" : target.ToString();
            }
            set
            {
                if (value == null || value.Length == 0) return;
                string[] point = value.Split(':');
                if (point.Length > 1) target = new IPEndPoint(IPAddress.Parse(point[0]), int.Parse(point[1]));
            }
        }

        #endregion

        #region 方法

        public void Open()
        {
            if (socket != null) return;
            string[] point = LocalIPPort.Split(':');
            if (point.Length < 1) throw new ArgumentException("参数格式错误");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Ttl = 1;
            if (point.Length == 1) socket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(point[0])));
            else socket.Bind(new IPEndPoint(IPAddress.Parse(point[0]), int.Parse(point[1])));
            IsOpened = true;
            (threadReceive = new Thread(() => { receiveData(); })).Start();
        }

        /// <summary>
        /// 关闭UDPClient并释放其资源
        /// </summary>
        public void Close()
        {
            IsOpened = false;
            if (threadReceive != null && threadReceive.ThreadState == ThreadState.Running) threadReceive.Abort();
            if (socket != null) { socket.Close(); socket = null; }
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="text"></param>
        /// <param name="target"></param>
        public void Send(string text, EndPoint target)
        {
            if (socket != null)
            lock (socket) socket.SendTo(Encoding.UTF8.GetBytes(text), target);
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="text"></param>
        public override void Send(string text)
        {
            if (target == null) throw new NoDefaultHostException();
            Send(text, target);
        }

        /// <summary>
        /// 接受数据线程
        /// </summary>
        private void receiveData()
        {
            for (; IsOpened; )
            {
                int count = 0;
                try { count = socket.ReceiveFrom(buffer, ref source); }
                catch { continue; }
                if (count == 0) break;
                OnDataReceived(new DataTransEventArgs(Encoding.UTF8.GetString(buffer, 0, count), source));
            }
        }

        #endregion
    }

    /// <summary>
    /// 没有默认远程主机异常
    /// </summary>
    public class NoDefaultHostException : Exception
    {
        public NoDefaultHostException() : base("No default remote host, please connect it!") { }
    }
}
