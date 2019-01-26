using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Vultrue.Communication.CMPPMessages;
using System.Net;

namespace Vultrue.Communication
{
    public class CMPPClient
    {
        #region 常量
        private const int IDLE_TIME = 180;
        private const int ACTIVE_INTERVAL = 60;
        private const int ACTIVE_NUMBER = 3;
        private const int WINDOW_SIZE = 16;
        #endregion

        #region 字段
        private string server;
        private int port;
        private string sourceAddr;
        private string password;
        private uint sequenceId;
        private TcpClient client;
        private NetworkStream nstream;
        private Thread receiveThread;
        private Thread sendThread;
        private object receiveLock = new object();
        private object sendLock = new object();
        private Queue<CMPP_MESSAGE> sendMessages = new Queue<CMPP_MESSAGE>();
        private List<CMPP_MESSAGE> messagesWindow = new List<CMPP_MESSAGE>();
        private byte[] receivedBuffer = new byte[1024];
        private int idletime = 0;
        #endregion

        public CMPPClient(string server, int port, string sourceAddr, string password, uint sequenceId)
        {
            this.server = server;
            this.port = port;
            this.sourceAddr = sourceAddr;
            this.password = password;
            this.sequenceId = sequenceId;
        }

        #region 属性

        public bool IsConnected { get; private set; }

        public uint SequenceId { get { return sequenceId++; } }

        #endregion

        #region 事件

        public event EventHandler Connected;
        public event EventHandler Closing;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected void OnConnected()
        {
            if (Connected != null) Connected(this, EventArgs.Empty);
        }

        public void OnClosing()
        {
            if (Closing != null) Closing(this, EventArgs.Empty);
        }

        #endregion

        public void Connect()
        {
            client = new TcpClient(server, port);
            nstream = client.GetStream();

            (sendThread = new Thread(sendThreadM)).Start();
            (receiveThread = new Thread(receiveThreadM)).Start();

            CMPP_CONNECT message = new CMPP_CONNECT(sourceAddr, password, DateTime.Now, 1, SequenceId);
            lock (messagesWindow) messagesWindow.Add(message);
            sendDirect(message);
        }

        private void close()
        {
            IsConnected = false;
            if (sendThread != null)
            {
                sendThread.Abort();
                sendThread = null;
            }
            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
            nstream.Close();
            client.Close();
        }

        private void sendMessage(CMPP_MESSAGE message)
        {
            lock (sendMessages) sendMessages.Enqueue(message);
        }

        private void sendDirect(CMPP_MESSAGE message)
        {
            byte[] msg = message.ToBytes();
            lock (nstream)
                nstream.Write(msg, 0, msg.Length);
        }

        private void sendThreadM()
        {
            while (true)
            {
                if (messagesWindow.Count >= WINDOW_SIZE || sendMessages.Count == 0)
                {
                    Thread.Sleep(1000);
                    //if (++idletime > IDLE_TIME)
                        //sendMessage(new CMPP_ACTIVE_TEST(SequenceId));
                    continue;
                }
                //idletime = 0;
                CMPP_MESSAGE message;
                lock (sendMessages) message = sendMessages.Dequeue();
                lock (messagesWindow) messagesWindow.Add(message);
                sendDirect(message);
                Thread.Sleep(10);
            }
        }

        private CMPP_MESSAGE findMessage(uint sequenceid)
        {
            return messagesWindow.FirstOrDefault(x => x.Header.SequenceId == sequenceid);
        }

        #region Deal ISMG Message

        private void receiveThreadM()
        {
            while (true)
            {
                int i = 0;
                for (; i < MessageHeader.HEAD_LENTH; i++)
                {
                    int b = nstream.ReadByte();
                    if (b == -1) goto EndThread;
                    receivedBuffer[i] = (byte)i;
                }
                MessageHeader header = new MessageHeader(receivedBuffer);
                for (; i < header.TotalLength; i++)
                {
                    int b = nstream.ReadByte();
                    if (b == -1) goto EndThread;
                    receivedBuffer[i] = (byte)i;
                }
                switch (header.CommandId)
                {
                    case CMPP_COMMAND_ID.CMPP_CONNECT_RESP:
                        DealISMGMessage(new CMPP_CONNECT_RESP(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_TERMINATE:
                        DealISMGMessage(new CMPP_TERMINATE(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_TERMINATE_RESP:
                        DealISMGMessage(new CMPP_TERMINATE_RESP(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_SUBMIT_RESP:
                        DealISMGMessage(new CMPP_SUBMIT_RESP(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_QUERY_RESP:
                        DealISMGMessage(new CMPP_QUERY_RESP(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_DELIVER:
                        DealISMGMessage(new CMPP_DELIVER(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_CANCEL_RESP:
                        DealISMGMessage(new CMPP_CANCEL_RESP(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_ACTIVE_TEST:
                        DealISMGMessage(new CMPP_ACTIVE_TEST(header, receivedBuffer));
                        break;
                    case CMPP_COMMAND_ID.CMPP_ACTIVE_TEST_RESP:
                        DealISMGMessage(new CMPP_ACTIVE_TEST_RESP(header, receivedBuffer));
                        break;
                    default: break;
                }
            }
        EndThread:
            IsConnected = false;
        }

        private void DealISMGMessage(CMPP_CONNECT_RESP message)
        {
            if (message.Status == 0) IsConnected = true;
            CMPP_MESSAGE smessage = findMessage(message.Header.SequenceId);
            if (smessage != null)
                lock (messagesWindow)
                    messagesWindow.Remove(smessage);
            OnConnected();
        }

        private void DealISMGMessage(CMPP_TERMINATE message)
        {
            sendDirect(new CMPP_TERMINATE_RESP(message.Header.SequenceId));
            close();
        }

        private void DealISMGMessage(CMPP_TERMINATE_RESP message)
        {
            close();
        }

        private void DealISMGMessage(CMPP_SUBMIT_RESP message)
        {
            
        }

        private void DealISMGMessage(CMPP_QUERY_RESP message)
        {
            
        }

        private void DealISMGMessage(CMPP_DELIVER message)
        {
            
        }

        private void DealISMGMessage(CMPP_CANCEL_RESP message)
        {
            
        }

        private void DealISMGMessage(CMPP_ACTIVE_TEST message)
        {
            sendDirect(new CMPP_ACTIVE_TEST_RESP(message.Header.SequenceId, 0));
        }

        private void DealISMGMessage(CMPP_ACTIVE_TEST_RESP message)
        {
            CMPP_MESSAGE smessage = findMessage(message.Header.SequenceId);
            if (smessage != null)
                lock (messagesWindow)
                    messagesWindow.Remove(smessage);
        }

        #endregion

        public void Query(DateTime time, byte queryType, string queryCode, string reserve)
        {
            if (!IsConnected) throw new Exception("No Connection");
            sendMessage(new CMPP_QUERY(time, queryType, queryCode, reserve, SequenceId));
        }

        public void Submit(ulong msgId, string feeTerminalId, string[] destTerminalId, string msgContent)
        {
            if (!IsConnected) throw new Exception("No Connection");
            sendMessage(new CMPP_SUBMIT(SequenceId)
            {
                MsgId = msgId,
                PkTotal = 1,
                PkNumber = 1,
                RegisteredDelivery = 1,
                MsgLevel = 1,
                ServiceId = "abcdefghij",
                FeeUserType = 3,
                FeeTerminalId = feeTerminalId,
                FeeTerminalType = 0,
                TPpId = 0,
                TPudhi = 0,
                MsgFmt = 4,
                MsgSrc = sourceAddr,
                FeeType = "02",
                FeeCode = "000010",
                ValIdTime = DateTime.Now.AddDays(3).ToString("MMddHHmmss") + "032+",
                AtTime = DateTime.Now.ToString("MMddHHmmss") + "032+",
                SrcId = "10016",
                DestTerminalId = destTerminalId,
                DestTerminalType = 0,
                MsgContent = msgContent,
                LinkId = ""
            });
        }

        public void Terminate()
        {
            if (!IsConnected) throw new Exception("No Connection");
            sendMessage(new CMPP_TERMINATE(SequenceId));
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {

    }
}