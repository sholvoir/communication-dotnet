using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace Vultrue.Communication
{
    internal enum SendMode { bytes, text };

    public partial class ConnectionMonitor : UserControl
    {
        #region 变量
        private SendMode sendmode = SendMode.text;
        private IConnection connection;
        private event EventHandler connectionClosing;
        private event EventHandler<DataTransEventArgs> connectionDataSended;
        private event EventHandler<DataTransEventArgs> connectionDataReceived;
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造 Vultrue.Communication.ConnectionMonitor 控件
        /// </summary>
        public ConnectionMonitor()
        {
            InitializeComponent();
            connectionClosing = new EventHandler(connection_Closing);
            connectionDataSended = new EventHandler<DataTransEventArgs>(connection_DataSended);
            connectionDataReceived = new EventHandler<DataTransEventArgs>(connection_DataReceived);
            Disposed += (object sender, EventArgs e) => { Connection = null; };
        }

        #endregion

        #region 属性

        /// <summary>
        /// 连接对象
        /// </summary>
        public IConnection Connection
        {
            get { return connection; }
            set
            {
                if (connection != null)
                {
                    connection.Closing -= connectionClosing;
                    connection.DataSended -= connectionDataSended;
                    connection.DataReceived -= connectionDataReceived;
                }
                if ((connection = value) != null)
                {
                    connection.Closing += connectionClosing;
                    connection.DataSended += connectionDataSended;
                    connection.DataReceived += connectionDataReceived;
                }
            }
        }

        #endregion

        #region 连接事件处理

        /// <summary>
        /// 连接销毁事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_Closing(object sender, EventArgs e)
        {
            if (InvokeRequired) Invoke(connectionClosing, new object[] { sender, e });
            else { connection = null; Dispose(); }
        }

        /// <summary>
        /// 连接发送数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_DataSended(object sender, DataTransEventArgs e)
        {
            if (InvokeRequired) Invoke(new EventHandler<DataTransEventArgs>(connection_DataSended), new object[] { sender, e });
            else dataShow.Add("发", (checkBoxBS.Checked ? ByteString.GetByteString(e.Data) : e.Message.Replace("\r", "\\r")));
        }

        /// <summary>
        /// 连接接收数据处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_DataReceived(object sender, DataTransEventArgs e)
        {
            if (InvokeRequired) Invoke(new EventHandler<DataTransEventArgs>(connection_DataReceived), new object[] { sender, e });
            else dataShow.Add("收", (checkBoxBS.Checked ? ByteString.GetByteString(e.Data) : e.Message.Replace("\r", "\\r")));
        }

        #endregion

        #region 界面事件处理

        /// <summary>
        /// 从连接向目标发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (connection == null) { MessageBox.Show("没有任何连接", "提示"); return; }
            if (sendmode == SendMode.bytes)
            {
                byte[] bytes = ByteString.GetBytes(textBoxSend.Text.Replace(" ", ""));
                connection.Write(bytes, 0, bytes.Length);
            }
            else connection.Write(textBoxSend.Text);
        }

        /// <summary>
        /// 切换到字符发送模式并发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendStringMenuItem_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.text;
            buttonSend.Text = "按字符发送";
            buttonSend_Click(sender, e);
        }

        /// <summary>
        /// 切换到字节模式并发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendBytesMenuItem_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.bytes;
            buttonSend.Text = "按字节发送";
            buttonSend_Click(sender, e);
        }

        /// <summary>
        /// 切换是否自动清除特性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_Click(object sender, EventArgs e)
        {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// 按回车发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r') return;
            textBoxSend.Text += "\r";
            buttonSend_Click(buttonSend, e);
            textBoxSend.Text = "";
        }

        /// <summary>
        /// 显示发送选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e)
        {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption, buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        #endregion
    }
}
