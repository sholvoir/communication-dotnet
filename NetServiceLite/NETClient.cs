using System;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace Vultrue.Communication
{
    public class NETClient : Component
    {
        [Browsable(false)]
        public bool IsOpened { get; protected set; }

        /// <summary>
        /// 数据已发送事件
        /// </summary>
        public event EventHandler<DataTransEventArgs> DataSended;

        /// <summary>
        /// 数据已接收事件
        /// </summary>
        public event EventHandler<DataTransEventArgs> DataReceived;

        public virtual void Send(string text) { }

        protected void OnDataSended(DataTransEventArgs e)
        {
            if (DataSended != null && IsOpened) DataSended(this, e);
        }

        protected void OnDataReceived(DataTransEventArgs e)
        {
            if (DataReceived != null && IsOpened) DataReceived(this, e);
        }
    }

    /// <summary>
    /// 为 Vultrue.Communication.NETClient.DataReceived 事件
    /// 和 Vultrue.Communication.NETClient.DataReceived 事件
    /// 提供参数
    /// </summary>
    public class DataTransEventArgs : EventArgs
    {
        public DataTransEventArgs(string message, EndPoint remoteEP)
        {
            Message = message;
            IP = ((IPEndPoint)remoteEP).Address.ToString();
            Port = ((IPEndPoint)remoteEP).Port;
        }

        public string IP { get; private set; }
        public int Port { get; private set; }
        public string Message { get; private set; }
    }
}
