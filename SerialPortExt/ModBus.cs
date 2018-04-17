using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.IO.Ports;

namespace Vultrue.Communication
{
    public partial class ModBus : Component
    {
        #region 变量
        private const int TIMEOUT = 1000;
        private SerialPort serialPort;
        private List<byte> received = new List<byte>();
        #endregion

        #region 构造

        public ModBus()
        {
            InitializeComponent();
        }

        public ModBus(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region 属性

        public SerialPort SerialPort
        {
            get { return serialPort; }
            set
            {
                if (serialPort != null) serialPort.DataReceived -= serialPort_DataReceived;
                if ((serialPort = value) == null) return;
                serialPort.DataReceived += serialPort_DataReceived;
                serialPort.ReadTimeout = TIMEOUT;
                serialPort.WriteTimeout = TIMEOUT;
                timer.Interval = 50000D / serialPort.BaudRate;
            }
        }

        public bool IsOpen
        {
            get { return serialPort == null ? false : serialPort.IsOpen; }
        }

        #endregion

        #region 事件

        public event EventHandler Closing;

        public event EventHandler<DataTransEventArgs> DataSended;

        public event EventHandler<DataTransEventArgs> DataReceived;

        protected void OnClosing(EventArgs e)
        {
            if (Closing != null) Closing(this, e);
        }

        protected void OnDataSended(DataTransEventArgs e)
        {
            if (DataSended != null) DataSended(this, e);
        }

        protected void OnDataReceived(DataTransEventArgs e)
        {
            if (DataReceived != null) DataReceived(this, e);
        }

        #endregion

        #region 方法

        public void Open()
        {
            if (serialPort == null) return;
            if (!serialPort.IsOpen) { serialPort.Open(); timer.Enabled = true; }

        }

        public void Close()
        {
            if (serialPort == null) return;
            if (serialPort.IsOpen) { serialPort.Close(); timer.Enabled = false; }
        }

        /// <summary>
        /// 向连接发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Send(byte[] buffer, int offset, int count)
        {
            if (serialPort == null) throw new Exception("No SerialPort Assigned");
            serialPort.Write(buffer, offset, count);
            OnDataSended(new DataTransEventArgs(buffer, offset, count));
        }

        /// <summary>
        /// 向连接发送数据
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort == null) return;
            timer.Stop();
            lock (received)
            {
                while (serialPort.BytesToRead > 0) received.Add((byte)serialPort.ReadByte());
            }
            timer.Interval = 50000D / serialPort.BaudRate;
            timer.Start();
        }

        /// <summary>
        /// 数据帧结束时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (received)
            {
                if (received.Count > 0)
                {
                    byte[] buffer = received.ToArray();
                    received.Clear();
                    OnDataReceived(new DataTransEventArgs(buffer, 0, buffer.Length));
                }
            }
        }

        #endregion

    }

    /// <summary>
    /// 为 Vultrue.Communication.ModBus.DataSended 事件
    /// 和 Vultrue.Communication.ModBus.DataReceived 事件提供参数
    /// </summary>
    public class DataTransEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public int Offset { get; private set; }

        public int Lenth { get; private set; }

        public string Message { get { return Encoding.UTF8.GetString(Data, Offset, Lenth); } }

        public DataTransEventArgs(byte[] data, int offset, int lenth)
        {
            Data = data;
            Offset = offset;
            Lenth = lenth;
        }
    }
}
