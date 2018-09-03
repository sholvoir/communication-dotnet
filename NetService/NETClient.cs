using System;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace Vultrue.Communication
{
    public class NETClient : Component
    {
        /// <summary>
        /// 数据已发送事件
        /// </summary>
        public event EventHandler<DataTransEventArgs> DataSended;

        /// <summary>
        /// 数据已接收事件
        /// </summary>
        public event EventHandler<DataTransEventArgs> DataReceived;

        public virtual void Send(byte[] data) { }

        public virtual void Send(byte[] data, int offset, int size) { }

        public virtual void Send(string text) { }

        protected void OnDataSended(DataTransEventArgs e)
        {
            if (DataSended != null) DataSended(this, e);
        }

        protected void OnDataReceived(DataTransEventArgs e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }
    }

    /// <summary>
    /// 为 Vultrue.Communication.NETClient.DataReceived 事件
    /// 和 Vultrue.Communication.NETClient.DataReceived 事件
    /// 提供参数
    /// </summary>
    public class DataTransEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public EndPoint RemoteEP { get; private set; }

        public string Message { get { return Encoding.UTF8.GetString(Data); } }

        public DataTransEventArgs(byte[] data, EndPoint remoteEP) { Data = data; RemoteEP = remoteEP; }
    }
}
