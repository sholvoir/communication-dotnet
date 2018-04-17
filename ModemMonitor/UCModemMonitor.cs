using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Vultrue.Communication
{
    internal enum SendMode { Text, PDU };

    /// <summary>
    /// 串口通信监视器
    /// </summary>
    public partial class UCModemMonitor : UserControl
    {
        private CdmaModem modem;
        private delegate void handler();
        private delegate int addShowData(string sendOrReceive, string transData);

        #region 构造

        /// <summary>
        /// 构造串口通信监视器
        /// </summary>
        public UCModemMonitor()
        {
            InitializeComponent();
            foreach (MessageMode mode in (MessageMode[])Enum.GetValues(typeof(MessageMode)))
                comboBoxMessageMode.Items.Add(mode);
            comboBoxMessageMode.SelectedIndex = 1;
            foreach (MessageState state in (MessageState[])Enum.GetValues(typeof(MessageState)))
                comboBoxMessageState.Items.Add(state);
            comboBoxMessageState.SelectedIndex = comboBoxMessageState.Items.Count - 1;
            foreach (MessageACK ack in (MessageACK[])Enum.GetValues(typeof(MessageACK)))
                comboBoxMessageACK.Items.Add(ack);
            comboBoxMessageACK.SelectedIndex = 0;
            foreach (MessagePriority prt in (MessagePriority[])Enum.GetValues(typeof(MessagePriority)))
                comboBoxMessagePriority.Items.Add(prt);
            comboBoxMessagePriority.SelectedIndex = 0;
            foreach (MessageFormat fmt in (MessageFormat[])Enum.GetValues(typeof(MessageFormat)))
            {
                comboBoxMessageFormat.Items.Add(fmt);
                comboBoxMessageEncoding.Items.Add(fmt);
            }
            comboBoxMessageFormat.SelectedIndex = 1;
            comboBoxMessageEncoding.SelectedIndex = 1;
            foreach (MessagePreserve prt in (MessagePreserve[])Enum.GetValues(typeof(MessagePreserve)))
                comboBoxMessagePreserve.Items.Add(prt);
            comboBoxMessagePreserve.SelectedIndex = 0;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 被监视的Modem
        /// </summary>
        public CdmaModem Modem
        {
            get { return modem; }
            set
            {
                if (modem != null)
                {
                    modem.Opened -= modem_OpenClosed;
                    modem.Closing -= modem_OpenClosed;
                }
                if ((modem = value) != null)
                {
                    modem.Opened += modem_OpenClosed;
                    modem.Closing += modem_OpenClosed;
                    modem_OpenClosed(modem, null);
                }
            }
        }

        #endregion

        #region Modem事件处理

        /// <summary>
        /// Modem打开/关闭事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_OpenClosed(object sender, EventArgs e)
        {
            Invoke(new handler(modemOpenClosed));
        }

        private void modemOpenClosed()
        {
            bool isopen = modem.IsOpen;
            buttonSendDirect.Enabled = isopen;
            buttonReadMessage.Enabled = isopen;
            buttonSendTextMessage.Enabled = isopen;
            buttonOpenClose.Image = isopen ? Properties.Resources.stop : Properties.Resources.start;
            buttonOpenClose.Text = isopen ? "关闭" : "打开";
        }

        #endregion

        #region 界面处理

        /// <summary>
        /// 是否自动清除切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e)
        {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// 打开按钮单击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenClose_Click(object sender, EventArgs e)
        {
            if (modem.IsOpen) modem.Close();
            else modem.Open();
        }

        /// <summary>
        /// 直接发送命令文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e)
        {
            modem.SendDirect(textBoxCmd.Text.Replace("\\r", "\r").Replace("\\n", "\n"), "");
            textBoxCmd.Text = "";
        }

        /// <summary>
        /// 按回车键直接发送到串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        #endregion

        #region 测试命令

        private void buttonAT_Click(object sender, EventArgs e)
        {
            modem.AT();
        }

        private void buttonSetMessageMode_Click(object sender, EventArgs e)
        {
            modem.SetMessageMode((MessageMode)comboBoxMessageMode.SelectedItem);
        }

        private void buttonListMessage_Click(object sender, EventArgs e)
        {
            //modem.ListMessage((MessageState)comboBoxMessageState.SelectedItem);
        }

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadMessage_Click(object sender, EventArgs e)
        {
            //modem.ReadTextMessage(int.Parse(textBoxMessageIndex.Text));
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendTextMessage_Click(object sender, EventArgs e)
        {
            Encoding encoding;
            switch ((MessageFormat)comboBoxMessageEncoding.SelectedItem)
            {
                case MessageFormat.ASCII:
                    encoding = Encoding.ASCII;
                    break;
                case MessageFormat.UNICODE:
                    encoding = Encoding.BigEndianUnicode;
                    break;
                default:
                    throw new Exception();
            }
            //modem.SendTextMessage(textBoxMobileNum.Text, textBoxMessageContent.Text, encoding);
        }

        private void buttonSendPduText_Click(object sender, EventArgs e)
        {
            //modem.SendPduMessage(textBoxMobileNum.Text, PduString.GetPdustr(textBoxMessageContent.Text), DataCodingScheme.USC2);
        }

        private void buttonSendPduData_Click(object sender, EventArgs e)
        {
            //modem.SendPduMessage(textBoxMobileNum.Text, textBoxMessageContent.Text, DataCodingScheme.Data);
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //modem.ReadPhonebookEntries(int.Parse(textBoxPhonebookEntriesIndex.Text));
        }

        #endregion

        private void buttonSetMessageParameter_Click(object sender, EventArgs e)
        {
        //    modem.SetMessageParameter(new MessageParameter()
        //    {
        //        ACK = (MessageACK)comboBoxMessageACK.SelectedItem,
        //        Priority = (MessagePriority)comboBoxMessagePriority.SelectedItem,
        //        Format = (MessageFormat)comboBoxMessageFormat.SelectedItem,
        //        Preserve = (MessagePreserve)comboBoxMessagePreserve.SelectedItem
        //    });
        }
    }
}