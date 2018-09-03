using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Vultrue.Communication
{
    public partial class SocketConnection : Connection
    {
        #region 变量
        private const int BUFFERSIZE = 8192;
        private const int TIMEOUT = 5000; //单位:毫秒
        private SocketAsyncEventArgs sendEventArgs;
        private SocketAsyncEventArgs receiveEventArgs;
        private byte[] receiveBuffer;
        private AutoResetEvent sendHandle;
        private Socket socket;
        private bool isClosed;
        #endregion

        public SocketConnection()
        {
            InitializeComponent();
            initializeComponent();
        }

        public SocketConnection(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            isClosed = false;
            receiveBuffer = new byte[BUFFERSIZE];
            sendHandle = new AutoResetEvent(true);
            sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(sendEventArgs_Completed);
            receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(receiveEventArgs_Completed);
        }


        #region 属性

        /// <summary>
        /// 接受发送Socket
        /// </summary>
        [Browsable(false)]
        public Socket Socket
        {
            get { return socket; }
            set
            {
                if ((socket = value) != null)
                {
                    socket.ReceiveTimeout = TIMEOUT;
                    socket.SendTimeout = TIMEOUT;
                    receiveData();
                }
            }
        }

        /// <summary>
        /// 获取远程主机地址
        /// </summary>
        [Browsable(false)]
        public override string Address
        {
            get
            {
                try { return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString(); }
                catch { return null; }
            }
        }

        [Browsable(false)]
        public override int Port
        {
            get
            {
                try { return ((IPEndPoint)socket.RemoteEndPoint).Port; }
                catch { return 0; }
            }
        }

        #endregion

        #region 发送/接收

        /// <summary>
        /// 向连接发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            sendHandle.WaitOne();
            sendEventArgs.SetBuffer(buffer, offset, count);
            if (!socket.SendAsync(sendEventArgs)) sendEventArgs_Completed(null, sendEventArgs);
        }

        private void sendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) Close();
            OnDataSended(new DataTransEventArgs(e.Buffer, e.Offset, e.Count));
            sendHandle.Set();
        }

        private void receiveData()
        {
            try
            {
                receiveEventArgs.SetBuffer(receiveBuffer, 0, BUFFERSIZE);
                if (!socket.ReceiveAsync(receiveEventArgs)) receiveEventArgs_Completed(null, receiveEventArgs);
            }
            catch { Close(); }
        }

        private void receiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            // 检查是否远程主机关闭连接
            if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success) Close();
            else
            {
                OnDataReceived(new DataTransEventArgs(e.Buffer, 0, e.BytesTransferred));
                receiveData();
            }
        }

        public override void Close()
        {
            if (isClosed) return;
            isClosed = true;
            OnClosing(null);
            try { socket.Shutdown(SocketShutdown.Both); }
            catch (Exception) { }
            sendEventArgs.Dispose();
            receiveEventArgs.Dispose();
            socket.Close();
            Dispose();
        }

        #endregion
    }
}
