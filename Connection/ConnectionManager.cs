using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO.Ports;

namespace Vultrue.Communication
{
    public partial class ConnectionManager : Component
    {
        private List<TcpLessoner> listeners;
        private List<IConnection> connections;

        #region 构造

        public ConnectionManager()
        {
            InitializeComponent();
            initializeComponent();
        }

        public ConnectionManager(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            listeners = new List<TcpLessoner>();
            connections = new List<IConnection>();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 管理的所有连接
        /// </summary>
        public List<IConnection> Connections
        {
            get
            {
                List<IConnection> conns = new List<IConnection>();
                conns.AddRange(connections);
                foreach (TcpLessoner ts in listeners)
                    conns.AddRange(ts.Connections);
                return conns;
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 连接建立后事件
        /// </summary>
        public event EventHandler<ConnectionBuildedEventArgs> ConnectionBuilded;

        #endregion

        #region 方法

        /// <summary>
        /// 新建串口连接
        /// </summary>
        /// <param name="serialPort">用来进行连接的串口</param>
        /// <returns></returns>
        public IConnection NewSerialConnect(SerialPort serialPort)
        {
            SerialConnection serialConn = new SerialConnection(components);
            serialConn.SerialPort = serialPort;
            if (ConnectionBuilded != null) ConnectionBuilded(this, new ConnectionBuildedEventArgs(serialConn));
            return serialConn;
        }

        /// <summary>
        /// 连接到网络服务器
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IConnection NewTcpClient(string server, int port)
        {
            //建立Socket
            Socket socket = null;
            IPHostEntry hostEntry = Dns.GetHostEntry(server);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try { socket.Connect(ipe); } catch { }
                if (socket.Connected) break;
            }
            if (!socket.Connected) return null;
            //建立Socket连接
            SocketConnection socketConn = new SocketConnection();
            socketConn.Socket = socket;
            connections.Add(socketConn);
            socketConn.Closing += (object sender, EventArgs e) => { connections.Remove((IConnection)sender); };
            if (ConnectionBuilded != null) ConnectionBuilded(this, new ConnectionBuildedEventArgs(socketConn));
            return socketConn;
        }

        /// <summary>
        /// 开始侦听端口
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="maxNumAccepts">允许的最大连接数</param>
        /// <returns></returns>
        public TcpLessoner NewTcpLessoner(int port, int maxNumberConnections)
        {
            TcpLessoner server = new TcpLessoner(components);
            server.MaxNumberConnections = maxNumberConnections;
            server.ConnectionBuilded += (object sender, ConnectionBuildedEventArgs e) => { if (ConnectionBuilded != null) ConnectionBuilded(this, e); };
            server.Start(port);
            return server;
        }

        /// <summary>
        /// 对所有连接进行广播
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Broadcast(byte[] buffer, int offset, int count)
        {
            foreach (IConnection conn in connections)
                conn.Write(buffer, offset, count);
            foreach (TcpLessoner listener in listeners)
                listener.Broadcast(buffer, offset, count);
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

    /// <summary>
    /// 连接建立事件参数
    /// </summary>
    public class ConnectionBuildedEventArgs : EventArgs
    {
        private IConnection connection;
        public IConnection Connection { get { return connection; } }
        public ConnectionBuildedEventArgs(IConnection connection) { this.connection = connection; }
    }
}
