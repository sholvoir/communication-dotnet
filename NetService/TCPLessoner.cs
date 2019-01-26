using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Vultrue.Communication
{
    public partial class TCPLessoner : Component
    {
        #region 变量
        private bool isRunning;
        private Socket socket;
        private Semaphore semaphore;
        private EndPoint local;
        private Thread threadAccept;
        private IContainer connections;
        #endregion

        #region 构造

        public TCPLessoner()
        {
            InitializeComponent();
            initializeComponent();
        }

        public TCPLessoner(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void initializeComponent()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Ttl = 5;
            local = new IPEndPoint(IPAddress.Any, 1234);
            isRunning = false;
            connections = new Container();
            Disposed += new EventHandler(TCPLessoner_Disposed);
        }

        private void TCPLessoner_Disposed(object sender, EventArgs e)
        {
            Stop();
            socket.Close();
            connections.Dispose();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port
        {
            get { return ((IPEndPoint)local).Port; }
        }

        /// <summary>
        /// Tag
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 连接建立事件
        /// </summary>
        public event EventHandler<TCPClientBuildedEventArgs> ConnectionBuilded;

        #endregion

        #region 方法

        /// <summary>
        /// 开始侦听端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port, int maxConnectionCount)
        {
            ((IPEndPoint)local).Port = port;
            semaphore = new Semaphore(maxConnectionCount, maxConnectionCount);
            socket.Bind(local);
            socket.Listen(100);
            isRunning = true;
            (threadAccept = new Thread(() => { startAccept(); })).Start();
        }

        /// <summary>
        /// 停止侦听端口
        /// </summary>
        public void Stop()
        {
            if (threadAccept != null)
            {
                threadAccept.Abort();
                threadAccept = null;
            }
            if (semaphore != null)
            {
                semaphore.Close();
                semaphore = null;
            }
            isRunning = false;
        }

        /// <summary>
        /// 异步接受连接
        /// </summary>
        private void startAccept()
        {
            while (isRunning)
            {
                semaphore.WaitOne();
                TCPClient tcpClient = new TCPClient(socket.Accept());
                tcpClient.Disposed += (object sender, EventArgs e) => { semaphore.Release(); };
                connections.Add(tcpClient);
                if (ConnectionBuilded != null) ConnectionBuilded(this, new TCPClientBuildedEventArgs(tcpClient));
            }
        }

        /// <summary>
        /// 对所有连接进行广播
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Broadcast(byte[] buffer, int offset, int count)
        {
            foreach (Component connection in connections.Components)
            {
                try { ((TCPClient)connection).Send(buffer, offset, count); }
                catch { continue; }
            }
        }

        /// <summary>
        /// 对所有连接进行广播
        /// </summary>
        /// <param name="text"></param>
        public void Broadcast(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            Broadcast(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 添加一个管理的TCPClient
        /// </summary>
        /// <param name="tcpClient"></param>
        public void AddTCPClinet(TCPClient tcpClient)
        {
            connections.Add(tcpClient);
        }

        #endregion
    }

    /// <summary>
    /// 连接建立事件参数
    /// </summary>
    public class TCPClientBuildedEventArgs : EventArgs
    {
        public TCPClient TCPClient { get; private set; }
        public TCPClientBuildedEventArgs(TCPClient tcpClient) { TCPClient = tcpClient; }
    }
}