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
    public partial class TcpLessoner : Component
    {
        #region 变量
        private Socket listener;
        private SocketAsyncEventArgs acceptEventArgs;
        private Semaphore semaphore;
        #endregion

        #region 构造

        public TcpLessoner()
        {
            InitializeComponent();
            initializeComponent();
        }

        public TcpLessoner(IContainer container)
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
            MaxNumberConnections = 100;
            Connections = new List<IConnection>();
            acceptEventArgs = new SocketAsyncEventArgs();
            acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArgs_Completed);
        }

        #endregion

        #region 属性
        /// <summary>
        /// 最大允许连接数
        /// </summary>
        [DefaultValue(100)]
        public int MaxNumberConnections { get; set; }

        /// <summary>
        /// 建立的连接集合
        /// </summary>
        [Browsable(false)]
        public List<IConnection> Connections { get; private set; }

        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }

        public object Tag { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 连接建立事件
        /// </summary>
        public event EventHandler<ConnectionBuildedEventArgs> ConnectionBuilded;

        #endregion

        #region 方法

        /// <summary>
        /// 开始侦听端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            Port = port;
            Start();
        }

        /// <summary>
        /// 开始侦听端口
        /// </summary>
        public void Start()
        {
            semaphore = new Semaphore(MaxNumberConnections, MaxNumberConnections);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            listener.Listen(100);
            startAccept();
        }

        /// <summary>
        /// 停止侦听端口
        /// </summary>
        public void Stop()
        {
            listener.Close();
        }

        /// <summary>
        /// 异步接受连接
        /// </summary>
        private void startAccept()
        {
            semaphore.WaitOne();
            acceptEventArgs.AcceptSocket = null;
            if (!listener.AcceptAsync(acceptEventArgs)) acceptEventArgs_Completed(this, acceptEventArgs);
        }

        /// <summary>
        /// 建立连接后处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acceptEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            SocketConnection socketConn = new SocketConnection();
            socketConn.Socket = e.AcceptSocket;
            socketConn.Closing += new EventHandler(socketConn_Closing);
            Connections.Add(socketConn);
            if (ConnectionBuilded != null) ConnectionBuilded(this, new ConnectionBuildedEventArgs(socketConn));
            startAccept();
        }

        /// <summary>
        /// 连接关闭处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void socketConn_Closing(object sender, EventArgs e)
        {
            Connections.Remove((IConnection)sender);
            try { semaphore.Release(); }
            catch (Exception) { }
        }

        /// <summary>
        /// 对所有连接进行广播
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Broadcast(byte[] buffer, int offset, int count)
        {
            foreach (IConnection connection in Connections)
            {
                try { connection.Write(buffer, offset, count); }
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

        #endregion
    }
}