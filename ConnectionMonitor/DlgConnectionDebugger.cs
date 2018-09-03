using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Vultrue.Communication;
using System.Net.Sockets;
using System.IO.Ports;

namespace Vultrue.Communication
{
    public partial class DlgConnectionDebugger : Form
    {
        /// <summary>
        /// 构造
        /// </summary>
        public DlgConnectionDebugger()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 侦听端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListenStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(textBoxListenPort.Text);
                tcpServerBindingSource.Add(connectionManager.NewTcpLessoner(port, 100));
            }
            catch (Exception err) { MessageBox.Show(err.Message, "发生错误"); }
        }

        /// <summary>
        /// 停止侦听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListenStop_Click(object sender, EventArgs e)
        {
            TcpLessoner listener = (TcpLessoner)comboBoxSocketConnected.SelectedItem;
            tcpServerBindingSource.Remove(listener);
            listener.Stop();
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnectServer_Click(object sender, EventArgs e)
        {
            try { connectionManager.NewTcpClient(textBoxServer.Text, int.Parse(textBoxPort.Text)); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "错误"); }
        }

        /// <summary>
        /// 连接到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConnSerial_Click(object sender, EventArgs e)
        {
            SerialPort serialPort = new SerialPort();
            if (new DlgSerialPortProperty(serialPort).ShowDialog() == DialogResult.OK)
                connectionManager.NewSerialConnect(serialPort);
        }

        /// <summary>
        /// 表示处理连接管理器已连接事件的方法。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectionManager_ConnectionBuilded(object sender, ConnectionBuildedEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<ConnectionBuildedEventArgs>(connectionManager_ConnectionBuilded), new object[] { sender, e });
            else
            {
                ConnectionMonitor monitor = new ConnectionMonitor();
                e.Connection.Closing += new EventHandler(connection_Closing);
                monitor.Dock = DockStyle.Fill;
                monitor.Connection = e.Connection;
                e.Connection.Tag = monitor;
                connectionBindingSource.Add(e.Connection);
            }
        }

        /// <summary>
        /// 表示处理连接关闭事件的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_Closing(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(connection_Closing), new object[] { sender, e });
            else
            {
                IConnection connection = (IConnection)sender;
                ConnectionMonitor monitor = (ConnectionMonitor)connection.Tag;
                monitor.Dispose();
                connectionBindingSource.Remove(connection);
                if (connectionBindingSource.Count == 0) groupBoxConnection.Text = "连接监控器";
            }
        }

        /// <summary>
        /// gridConnection行进入处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridConnection_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            IConnection connection = (IConnection)gridConnection.Rows[e.RowIndex].DataBoundItem;
            groupBoxConnection.Controls.Clear();
            try
            {
                groupBoxConnection.Text = string.Format("连接监控器 {0}:{1}", connection.Address, connection.Port);
                groupBoxConnection.Controls.Add((ConnectionMonitor)connection.Tag);
            }
            catch { }
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemDisconn_Click(object sender, EventArgs e)
        {
            if (gridConnection.SelectedRows.Count > 0)
                ((IConnection)gridConnection.SelectedRows[0].DataBoundItem).Close();
        }
    }
}