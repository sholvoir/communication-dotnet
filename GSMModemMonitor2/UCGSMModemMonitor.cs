using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    internal enum SendMode { Text, PDU };

    /// <summary>
    /// ����ͨ�ż�����
    /// </summary>
    public partial class UCGSMModemMonitor : UserControl
    {
        #region ����
        private GSMModem modem;
        private SendMode sendmode = SendMode.Text;
        private EventHandler modemOpened;
        private EventHandler modemClosed;
        private EventHandler<SerialDataEventArgs> modemDataTransmitted;
        #endregion

        #region ����

        /// <summary>
        /// ���촮��ͨ�ż�����
        /// </summary>
        public UCGSMModemMonitor()
        {
            InitializeComponent();
            modemOpened = new EventHandler(modem_OpenClosed);
            modemClosed = new EventHandler(modem_OpenClosed);
            modemDataTransmitted = new EventHandler<SerialDataEventArgs>(modem_DataTransmitted);
        }

        #endregion

        #region ����

        /// <summary>
        /// �����ӵ�Modem
        /// </summary>
        public GSMModem Modem
        {
            get { return modem; }
            set
            {
                if (modem != null)
                {
                    modem.Opened -= modemOpened;
                    modem.Closed -= modemClosed;
                    modem.DataTransmitted -= modemDataTransmitted;
                }
                if ((modem = value) != null)
                {
                    modem.Opened += modemOpened;
                    modem.Closed += modemClosed;
                    modem.DataTransmitted += modemDataTransmitted;
                    modem_OpenClosed(modem, null);
                }
            }
        }

        #endregion

        #region ��ʼ��

        /// <summary>
        /// ������ִ�У������ʼ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GSMModemMonitor_Load(object sender, EventArgs e)
        {
            checkBoxAutoClear_CheckedChanged(checkBoxAutoClear, e);
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
            if (InvokeRequired)
                Invoke(new EventHandler(modem_OpenClosed), new object[] { sender, e });
            else
            {
                bool isopen = modem.IsOpen;
                buttonOpen.Enabled = !isopen;
                buttonClose.Enabled = isopen;
                buttonSendDirect.Enabled = isopen;
                buttonReadSMS.Enabled = isopen;
                buttonSendSMS.Enabled = isopen;
            }
        }

        /// <summary>
        /// Modem����/�������ݴ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_DataTransmitted(object sender, SerialDataEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<SerialDataEventArgs>(modem_DataTransmitted), new object[] { sender, e });
            else dataShow.Add(e.IsReceived ? "��" : "��", e.SerialString);
        }

        #endregion

        #region �س�������

        /// <summary>
        /// ���س���ֱ�ӷ��͵�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        /// <summary>
        /// ���س������Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxNoteletContent_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') buttonSendSMS_Click(buttonSendSMS, e);
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
        /// ����ѡ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e)
        {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption,
                buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        /// <summary>
        /// ���û�鿴Modem����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProperty_Click(object sender, EventArgs e)
        {
            new DlgGSMModemProperty(modem).ShowDialog(this);
        }

        /// <summary>
        /// �򿪰�ť��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            modem.Open();
        }

        /// <summary>
        /// �رհ�ť��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            modem.Close();
        }

        /// <summary>
        /// ֱ�ӷ��������ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e)
        {
            modem.SendDirect(textBoxCmd.Text.Replace("\\r", "\r").Replace("\\n", "\n"));
            textBoxCmd.Text = "";
        }

        #endregion

        #region ���Ŵ���

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadSMS_Click(object sender, EventArgs e)
        {
            try { modem.ListMessage(MessageState.ALL); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendSMS_Click(object sender, EventArgs e)
        {
            try
            {
                if (sendmode == SendMode.PDU)
                    modem.SendMessage(textBoxMobileNum.Text, textBoxNoteletContent.Text, DataCodingScheme.Data);
                else
                    modem.SendMessage(textBoxMobileNum.Text, PduString.GetPdustr(textBoxNoteletContent.Text),
                        DataCodingScheme.USC2);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// ���ı���ʽ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSText_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.Text;
            buttonSendSMS.Text = "�����ı�����";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// ��PDU��ʽ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSPDU_Click(object sender, EventArgs e)
        {
            sendmode = SendMode.PDU;
            buttonSendSMS.Text = "����PDU����";
            buttonSendSMS_Click(sender, e);
        }

        #endregion
    }
}