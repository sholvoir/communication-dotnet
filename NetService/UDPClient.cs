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
        private const int BUFFERSIZE = 8192;
        private byte[] buffer;
        private Socket socket;
        private Thread threadReceive;
        private EndPoint target;
        private EndPoint source;

        public UDPClient(string localIP, int localport)
        {
            InitializeComponent();
            initializeComponent(localIP, localport);
        }

        public UDPClient(string localIP, int localport, IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent(localIP, localport);
        }

        public void initializeComponent(string localIP, int localport)
        {
            buffer = new byte[BUFFERSIZE];
            source = new IPEndPoint(IPAddress.Any, 0);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Ttl = 5;
            socket.Bind(new IPEndPoint(localIP == null ? IPAddress.Any : IPAddress.Parse(localIP), localport));
            (threadReceive = new Thread(() => { receiveData(); })).Start();
            Disposed += new EventHandler(UDPClient_Disposed);
        }

        private void UDPClient_Disposed(object sender, EventArgs e)
        {
            if (threadReceive.ThreadState == ThreadState.Running) threadReceive.Abort();
            if (socket != null) { socket.Close(); socket = null; }
        }

        /// <summary>
        /// 建立默认远程主机
        /// </summary>
        /// <param name="remote"></param>
        public void Connect(IPEndPoint remote)
        {
            target = remote;
        }

        /// <summary>
        /// 建立默认远程主机
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress addr, int port)
        {
            target = new IPEndPoint(addr, port);
        }

        /// <summary>
        /// 建立默认远程主机
        /// </summary>
        /// <param name="hostIP"></param>
        /// <param name="port"></param>
        public void Connect(string hostIP, int port)
        {
            target = new IPEndPoint(IPAddress.Parse(hostIP), port);
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        public void Send(byte[] data, EndPoint target)
        {
            lock (socket) socket.SendTo(data, target);
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="target"></param>
        public void Send(byte[] data, int offset, int size, EndPoint target)
        {
            lock (socket) socket.SendTo(data, offset, size, SocketFlags.None, target);
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="text"></param>
        /// <param name="target"></param>
        public void Send(string text, EndPoint target)
        {
            Send(Encoding.UTF8.GetBytes(text), target);
        }

        /// <summary>
        /// 发送数据到默认目标端
        /// </summary>
        /// <param name="data"></param>
        public override void Send(byte[] data)
        {
            if (target == null) throw new NoDefaultHostException();
            Send(data, target);
        }

        /// <summary>
        /// 发送数据到目标端
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public override void Send(byte[] buffer, int offset, int size)
        {
            if (target == null) throw new NoDefaultHostException();
            Send(buffer, offset, size, target);
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
        /// 关闭UDPClient并释放其资源
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 接受数据线程
        /// </summary>
        private void receiveData()
        {
            for (; ; )
            {
                int count = 0;
                try { count = socket.ReceiveFrom(buffer, ref source); }
                catch { break; }
                if (count == 0) break;
                byte[] buff = new byte[count];
                for (int i = 0; i < count; i++) buff[i] = buffer[i];
                OnDataReceived(new DataTransEventArgs(buff, source));
            }
            new Thread(() => { Dispose(); }).Start();
        }
    }

    /// <summary>
    /// 没有默认远程主机异常
    /// </summary>
    public class NoDefaultHostException : Exception
    {
        public NoDefaultHostException() : base("No default remote host, please connect it!") { }
    }
}
