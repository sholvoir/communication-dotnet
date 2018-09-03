using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Vultrue.Communication
{
    public partial class SerialConnection : Connection
    {
        #region 变量
        private const int TIMEOUT = 1000;
        private SerialPort serialPort;
        private List<byte> received = new List<byte>();
        #endregion

        #region 构造

        public SerialConnection()
        {
            InitializeComponent();
        }

        public SerialConnection(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 串口
        /// </summary>
        [Browsable(false)]
        public SerialPort SerialPort
        {
            get { return serialPort; }
            set
            {
                if (value == null) return;
                (serialPort = value).DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                serialPort.ReadTimeout = TIMEOUT;
                serialPort.WriteTimeout = TIMEOUT;
                if (!serialPort.IsOpen) serialPort.Open();
            }
        }

        public override string Address
        {
            get { return "COM"; }
        }

        public override int Port
        {
            get { return int.Parse(serialPort.PortName.Remove(0, 3)); }
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
            try { serialPort.Write(buffer, offset, count); }
            catch (Exception) { Close(); return; }
            OnDataSended(new DataTransEventArgs(buffer, offset, count));
        }

        /// <summary>
        /// 串口数据接收处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            timerModbus.Stop();
            try
            {
                lock (received)
                {
                    while (serialPort.BytesToRead > 0) received.Add((byte)serialPort.ReadByte());
                }
            }
            catch (Exception) { Close(); }
            timerModbus.Interval = 50000D / serialPort.BaudRate;
            timerModbus.Start();
        }

        /// <summary>
        /// 数据帧结束时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerModbus_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            OnClosing(null);
            timerModbus.Close();
            serialPort.Close();
            Dispose();
        }

        #endregion
    }
}
