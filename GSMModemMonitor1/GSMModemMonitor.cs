using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication {
    /// <summary>����ͨ�ż�����</summary>
    public partial class GSMModemMonitor : UserControl {
        private enum SendMode { Text, PDU };
        private GSMModem modem;
        private SendMode sendmode = SendMode.Text;
        private EventHandler modemOpened;
        private EventHandler modemClosed;
        private EventHandler<SerialDataEventArgs> modemDataTransmitted;

        /// <summary>
        /// ���촮��ͨ�ż�����
        /// </summary>
        public GSMModemMonitor() {
            InitializeComponent();
            modemOpened = new EventHandler(modem_OpenClosed);
            modemClosed = new EventHandler(modem_OpenClosed);
            modemDataTransmitted = new EventHandler<SerialDataEventArgs>(modem_DataTransmitted);
        }

        /// <summary>
        /// ������ִ�У������ʼ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GSMModemMonitor_Load(object sender, EventArgs e) {
            checkBoxAutoClear_CheckedChanged(checkBoxAutoClear, e);
        }

        /// <summary>
        /// �����ӵ�Modem
        /// </summary>
        public GSMModem Modem {
            get { return modem; }
            set {
                if (modem != null) {
                    modem.Opened -= modemOpened;
                    modem.Closed -= modemClosed;
                    modem.DataTransmitted -= modemDataTransmitted;
                }
                modem = value;
                if (modem != null) {
                    modem.Opened += modemOpened;
                    modem.Closed += modemClosed;
                    modem.DataTransmitted += modemDataTransmitted;
                    modem_OpenClosed(modem, null);
                }
            }
        }

        /// <summary>
        /// Modem��/�ر��¼�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modem_OpenClosed(object sender, EventArgs e) {
            if (InvokeRequired)
                Invoke(new EventHandler(modem_OpenClosed), new object[] { sender, e });
            else {
                bool isopen = modem.IsOpen;
                buttonOpen.Enabled = !isopen;
                buttonClose.Enabled = isopen;
                buttonManual.Enabled = isopen;
                buttonTest.Enabled = isopen;
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
        private void modem_DataTransmitted(object sender, SerialDataEventArgs e) {
            if (InvokeRequired)
                Invoke(new EventHandler<SerialDataEventArgs>(modem_DataTransmitted), new object[] { sender, e });
            else dataShow.Add(e.IsReceived ? "��" : "��", e.SerialString);
        }

        /// <summary>
        /// ���û�鿴Modem����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProperty_Click(object sender, EventArgs e) {
            new DlgGSMModemProperty(modem).ShowDialog(this);
        }

        /// <summary>
        /// �򿪰�ť��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpen_Click(object sender, EventArgs e) {
            modem.Open();
        }

        /// <summary>
        /// �رհ�ť��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e) {
            modem.Close();
        }

        /// <summary>
        /// �ֹ����ȵ�������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonManual_Click(object sender, EventArgs e) {
            modem.HandSchedule();
        }

        /// <summary>
        /// Modem����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTest_Click(object sender, EventArgs e) {
            modem.TestModem();
        }

        /// <summary>
        /// ֱ�ӷ��������ı�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendDirect_Click(object sender, EventArgs e) {
            modem.SendDirect(textBoxCmd.Text + "\r");
            textBoxCmd.Text = "";
        }

        /// <summary>
        /// ���Modem�����б�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTaskClear_Click(object sender, EventArgs e) {
            modem.TaskClear();
        }

        /// <summary>
        /// �Ƿ��Զ�����л�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e) {
            dataShow.AutoClear = checkBoxAutoClear.Checked;
        }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReadSMS_Click(object sender, EventArgs e) {
            modem.ReadNotelet();
        }

        /// <summary>
        /// ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendSMS_Click(object sender, EventArgs e) {
            if (sendmode == SendMode.PDU) modem.SendNotelet(textBoxMobileNum.Text, textBoxNoteletContent.Text, Notelet.DCS.Data);
            else modem.SendNotelet(textBoxMobileNum.Text, GSMModem.Unicode2Pdustr(textBoxNoteletContent.Text), Notelet.DCS.USC2);
        }

        /// <summary>
        /// ���ı���ʽ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSText_Click(object sender, EventArgs e) {
            sendmode = SendMode.Text;
            buttonSendSMS.Text = "�����ı�����";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// ��PDU��ʽ���Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSendSMSPDU_Click(object sender, EventArgs e) {
            sendmode = SendMode.PDU;
            buttonSendSMS.Text = "����PDU����";
            buttonSendSMS_Click(sender, e);
        }

        /// <summary>
        /// ����ѡ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSendOption_Click(object sender, EventArgs e) {
            buttonSendOption.ContextMenuStrip.Show(buttonSendOption, buttonSendOption.Width / 2, buttonSendOption.Height / 2);
        }

        /// <summary>
        /// ���س���ֱ�ӷ��͵�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCmd_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') buttonSendDirect_Click(buttonSendDirect, e);
        }

        /// <summary>
        /// ���س������Ͷ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxNoteletContent_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') buttonSendSMS_Click(buttonSendSMS, e);
        }
    }
}