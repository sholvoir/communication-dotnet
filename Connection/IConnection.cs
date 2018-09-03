using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Vultrue.Communication
{
    /// <summary>
    /// 连接接口
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// 获取远端地址
        /// </summary>
        string Address { get; }

        /// <summary>
        /// 获取远端端口
        /// </summary>
        int Port { get; }

        object Tag { get; set; }

        /// <summary>
        /// 连接关闭前发生
        /// </summary>
        event EventHandler Closing;

        /// <summary>
        /// 数据发送后发生
        /// </summary>
        event EventHandler<DataTransEventArgs> DataSended;

        /// <summary>
        /// 数据接收后发生
        /// </summary>
        event EventHandler<DataTransEventArgs> DataReceived;

        /// <summary>
        /// 向连接发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// 向连接发送数据
        /// </summary>
        /// <param name="message">发送的消息</param>
        void Write(string text);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }

    /// <summary>
    /// 为 Vultrue.Communication.IConnection.DataSended 事件
    /// 和 Vultrue.Communication.IConnection.DataReceived 事件提供参数
    /// </summary>
    public class DataTransEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public int Index { get; private set; }

        public int Count { get; private set; }

        public string Message { get { return Encoding.UTF8.GetString(Data, Index, Count); } }

        public DataTransEventArgs(byte[] data, int index, int count)
        {
            Data = data;
            Index = index;
            Count = count;
        }
    }
}
