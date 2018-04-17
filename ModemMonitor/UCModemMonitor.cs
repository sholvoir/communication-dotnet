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
    /// ����ͨ�ż�����
    /// </summary>
    public partial class UCModemMonitor : UserControl
    {
        private CdmaModem modem;
        private delegate void handler();
        private delegate int addShowData(string sendOrReceive, string transData);

        #region ����

        /// <summary>
        /// ���촮��ͨ�ż�����
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

        #region ����

        /// <summary>
        /// �����ӵ�Modem
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

        #region Modem�¼�����

        /// <summary>
        /// Modem��/�ر��¼�����
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
            buttonOpenClose.Text = isopen ? "�ر�" : "��";
        }

        #endregion

        #region ���洦��

        /// <summary>
        /// �Ƿ��Զ�����л�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e)
        {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// �򿪰�ť��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenClose_Click(object sender, EventArgs e)
        {
            if (modem.IsOpen) modem.Close();
            else modem.Open();
        }

        /// <summary>
        /// ֱ�ӷ��������ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e)
        {
            modem.SendDirect(textBoxCmd.Text.Replace("\\r", "\r").Replace("\\n", "\n"), "");
            textBoxCmd.Text = "";
        }

        /// <summary>
        /// ���س���ֱ�ӷ��͵�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        #endregion

        #region ��������

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
        /// ��ȡ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadMessage_Click(object sender, EventArgs e)
        {
            //modem.ReadTextMessage(int.Parse(textBoxMessageIndex.Text));
        }

        /// <summary>
        /// ���Ͷ���
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