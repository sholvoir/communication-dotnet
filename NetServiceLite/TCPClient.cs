using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;

namespace Vultrue.Communication
{
    public partial class TCPClient : NETClient
    {
        private const int BUFFERSIZE = 8192;
        private const int TIMEOUT = 5000; //单位:毫秒
        private byte[] buffer;
        private Socket socket;
        private Thread threadReceive;

        public TCPClient(Socket socket)
        {
            InitializeComponent();
            initializeComponent(socket);
        }

        public TCPClient(Socket socket, IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent(socket);
        }

        public TCPClient(string host, int port)
        {
            InitializeComponent();
            socket = null;
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try { socket.Connect(ipe); }
                catch { }
                if (socket.Connected) break;
            }
            if (!socket.Connected) throw new CantConnectException();
            initializeComponent(socket);
        }

        private void initializeComponent(Socket socket)
        {
            this.socket = socket;
            socket.Ttl = 5;
            socket.SendTimeout = TIMEOUT;
            socket.ReceiveTimeout = TIMEOUT;
            buffer = new byte[BUFFERSIZE];
            (threadReceive = new Thread(() => { receiveData(); })).Start();
            Disposed += new EventHandler(TCPClient_Disposed);
        }

        private void TCPClient_Disposed(object sender, EventArgs e)
        {
            if (threadReceive.ThreadState == ThreadState.Running) threadReceive.Abort();
            if (socket != null) { socket.Close(); socket = null; }
        }

        public override void Send(string text)
        {
            lock (socket) socket.Send(Encoding.UTF8.GetBytes(text));
        }

        public void Close()
        {
            Dispose();
        }

        private void receiveData()
        {
            for (; ; )
            {
                int count;
                try { count = socket.Receive(buffer); }
                catch { break; }
                if (count == 0) break;
                OnDataReceived(new DataTransEventArgs(Encoding.UTF8.GetString(buffer, 0, count), socket.RemoteEndPoint));
            }
            new Thread(() => { Dispose(); }).Start();
        }
    }

    /// <summary>
    /// 无法连接远程主机
    /// </summary>
    public class CantConnectException : Exception
    {
        public CantConnectException() : base("No default remote host, please connect it!") { }
    }
}
