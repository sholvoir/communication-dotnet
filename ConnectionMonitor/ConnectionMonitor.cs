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
        #region ����
        private SendMode sendmode = SendMode.text;
        private IConnection connection;
        private event EventHandler connectionClosing;
        private event EventHandler<DataTransEventArgs> connectionDataSended;
        private event EventHandler<DataTransEventArgs> connectionDataReceived;
        #endregion

        #region ���캯��

        /// <summary>
        /// ���� Vultrue.Communication.ConnectionMonitor �ؼ�
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

        #region ����

        /// <summary>
        /// ���Ӷ���
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

        #region �����¼�����

        /// <summary>
        /// ���������¼�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_Closing(object sender, EventArgs e)
        {
            if (InvokeRequired) Invoke(connectionClosing, new object[] { sender, e });
            else { connection = null; Dispose(); }
        }

        /// <summary>
        /// ���ӷ������ݴ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_DataSended(object sender, DataTransEventArgs e)
        {
            if (InvokeRequired) Invoke(new EventHandler<DataTransEventArgs>(connection_DataSended), new object[] { sender, e });
            else dataShow.Add("��", (checkBoxBS.Checked ? ByteString.GetByteString(e.Data) : e.Message.Replace("\r", "\\r")));
        }

        /// <summary>
        /// ���ӽ������ݴ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connection_DataReceived(object sender, DataTransEventArgs e)
        {
            if (InvokeRequired) Invoke(new EventHandler<DataTransEventArgs>(connection_DataReceived), new object[] { sender, e });
            else dataShow.Add("��", (checkBoxBS.Checked ? ByteString.GetByteString(e.Data) : e.Message.Replace("\r", "\\r")));
        }

        #endregion

        #region �����¼�����

        /// <summary>
        /// ��������Ŀ�귢������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (connection == null) { MessageBox.Show("û���κ�����", "��ʾ"); return; }
            if (sendmode == SendMode.bytes)
            {
                byte[] bytes = ByteString.GetBytes(textBoxSend.Text.Replace(" ", ""));
                connection.Write(bytes, 0, bytes.Length);
            }
            else connection.Write(textBoxSend.Text);
        }

        /// <summary>
        /// �л����ַ�����ģʽ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendStringMenuItem_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.text;
            buttonSend.Text = "���ַ�����";
            buttonSend_Click(sender, e);
        }

        /// <summary>
        /// �л����ֽ�ģʽ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendBytesMenuItem_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.bytes;
            buttonSend.Text = "���ֽڷ���";
            buttonSend_Click(sender, e);
        }

        /// <summary>
        /// �л��Ƿ��Զ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_Click(object sender, EventArgs e)
        {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// ���س�����
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
        /// ��ʾ����ѡ��
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
