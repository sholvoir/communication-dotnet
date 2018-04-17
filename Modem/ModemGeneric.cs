using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Vultrue.Communication
{
    partial class Modem
    {
        private static Regex regexRing = new Regex("^RING");
        private static Regex regexConnect = new Regex("^CONNECT");
        private static Regex regexNoCarrier = new Regex("^NO CARRIER");
        private static Regex regexNoDialTone = new Regex("^NO DIALTONE");
        private static Regex regexNoAnswer = new Regex("^NO ANSWER");
        private static Regex regexBusy = new Regex("^BUSY");
        private static Regex regexCmti = new Regex("\\+CMTI:\\s*\"(?<mem>\\w+)\",(?<index>\\d+)");
        private static Regex regexCreg = new Regex("^\\+CREG:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)");
        /// <summary>
        /// 
        /// </summary>
        protected static Regex RegexPrompt = new Regex("> ");

        #region V24-V25 COMMANDS

        /// <summary>
        /// 设置Modem是否回显
        /// </summary>
        /// <param name="echo">0不回显, 1回显</param>
        public void SetEcho(int echo)
        {
            lock (mt) ExecTask(string.Format("ATE{0}\r", echo), "");
        }

        /// <summary>
        /// Set DCE response format
        /// </summary>
        /// <param name="rf">0:numeric result codes; 1:verbose response text</param>
        public void SetResponseFormat(int rf)
        {
            lock (mt) ExecTask(string.Format("ATV{0}\r", rf), "");
        }

        /// <summary>
        /// Result code suppression
        /// </summary>
        /// <param name="rcs">0:DCE transmits result codes; 1:Result codes are suppressed</param>
        public void SetResultCodeSuppression(int rcs)
        {
            lock (mt) ExecTask(string.Format("ATQ{0}\r", rcs), "");
        }

        /// <summary>
        /// 恢复出厂设置
        /// </summary>
        public void RestoreFactorySetting()
        {
            lock (mt) ExecTask("AT&F\r", "");
        }

        #endregion

        #region GENERAL COMMANDS

        /// <summary>
        /// 退出数据模式进入命令模式
        /// </summary>
        public void ExitDataMode()
        {
            SerialPort.Write("\u001B");
        }

        /// <summary>
        /// 直接发送指令
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="validateRegex"></param>
        public void SendDirect(string instruction, string validateRegex)
        {
            lock (mt) ExecTask(instruction, validateRegex);
        }

        /// <summary>
        /// 执行AT空指令
        /// </summary>
        public void AT()
        {
            lock (mt) ExecTask("AT\r", "");
        }
        
        /// <summary>
        /// 获取制造商信息
        /// </summary>
        /// <returns>制造商信息</returns>
        public string GetManufacturerIdentification()
        {
            lock (mt) return ExecTask("AT+CGMI\r", "^(?<ans>[ \\w]+)")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取支持的频带
        /// </summary>
        /// <returns>支持的频带</returns>
        public string GetModelIdentification()
        {
            lock (mt) return ExecTask("AT+CGMM\r", "^(?<ans>[ \\w]+)")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取Modem固件版本
        /// </summary>
        /// <returns>Modem固件版本</returns>
        public string GetRevisionIdentification()
        {
            lock (mt) return ExecTask("AT+CGMR\r", "^(?<ans>[ \\w\\p{P}\\p{S}]+)")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取产品序列号
        /// </summary>
        /// <returns>产品序列号</returns>
        public string GetProductSerialNumber()
        {
            lock (mt) return ExecTask("AT+CGSN\r", "^(?<ans>\\d+)")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取SIM卡国际移动用户标识
        /// </summary>
        /// <returns>SIM卡国际移动用户标识</returns>
        public string GetInternationalMobileSubscriberIdentity()
        {
            lock (mt) return ExecTask("AT+CIMI\r", "^(?<ans>\\d+)")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取SIM卡标识
        /// </summary>
        /// <returns>SIM卡标识</returns>
        public string GetCardIdentification()
        {
            lock (mt) return ExecTask("AT+CCID\r", "^\\+CCID:\\s*\"(?<ans>\\d+)\"")[0].Result("${ans}");
        }

        /// <summary>
        /// 获取ME错误报告模式
        /// </summary>
        /// <returns>ME错误报告模式</returns>
        public ReportMobileEquipmentErrorsMode GetReportMobileEquipmentErrors()
        {
            lock (mt) return (ReportMobileEquipmentErrorsMode)int.Parse(
                ExecTask("AT+CMEE?\r", "^\\+CMEE:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置ME错误报告模式
        /// </summary>
        /// <param name="reportMEErrorMode">ME错误报告模式</param>
        public void SetReportMobileEquipmentErrors(ReportMobileEquipmentErrorsMode reportMEErrorMode)
        {
            lock (mt) ExecTask(string.Format("AT+CMEE={0}\r", (int)reportMEErrorMode), "");
        }

        #endregion

        #region NETWORK SERVICE COMMANDS

        /// <summary>
        /// 获取Modem信号质量
        /// </summary>
        /// <returns>Item1:Modem信号强度 取值范围(0-31) 99表示未知或不可检测, Item2:当前误码率 取值范围(0-7) 99表示未知或不可检测</returns>
        public Tuple<int, int> GetSignalQuality()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CSQ\r", "^\\+CSQ:\\s*(?<rssi>\\d+),(?<ber>\\d+)")[0];
                return new Tuple<int, int>(int.Parse(match.Result("${rssi}")), int.Parse(match.Result("${ber}")));
            }
        }

        /// <summary>
        /// 设置网络注册信息模式
        /// </summary>
        /// <param name="mode">注册信息模式</param>
        public void SetNetworkRegistrationMode(int mode)
        {
            lock (mt) ExecTask(string.Format("AT+CREG={0}\r", mode), "");
        }

        /// <summary>
        /// 获取Modem网络注册信息
        /// </summary>
        /// <returns>Item1:模式, 取值范围(0-2); Item2:Modem网络注册状态; Item3:区域代码; Item4:蜂窝ID</returns>
        public Tuple<int, NetworkState, string, string> GetNetworkRegistration()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CREG?\r", "^\\+CREG:\\s*(?<mode>\\d+),(?<stat>\\d+)(,\"(?<lac>[0-9a-fA-F]+)\",\"(?<ci>[0-9a-fA-F]+)\")*")[0];
                return new Tuple<int, NetworkState, string, string>(int.Parse(match.Result("${mode}")),
                    (NetworkState)int.Parse(match.Result("${stat}")), match.Result("${lac}"), match.Result("${ci}"));
            }
        }

        #endregion

        #region PHONEBOOK COMMANDS

        /// <summary>
        /// 获取电话本存储空间信息
        /// </summary>
        /// <returns>Item1:电话本存储位置; Item2:已使用的空间; Item3:总可使用的空间</returns>
        public Tuple<string, int, int> GetPhonebookMemoryStorage()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPBS?\r", "^\\+CPBS:\\s*\"(?<mem>\\d+)\",(?<used>\\d+),(?<total>\\d+)")[0];
                return new Tuple<string, int, int>(match.Result("${mem}"), int.Parse(match.Result("${used}")), int.Parse(match.Result("${total}")));
            }
        }

        /// <summary>
        /// 设置电话本存储空间
        /// </summary>
        /// <param name="storage">电话本存储空间</param>
        public void SetPhonebookMemoryStorage(string storage)
        {
            lock (mt) ExecTask(string.Format("AT+CPBS=\"{0}\"\r", storage), "");
        }

        #endregion

        #region SHORT MESSAGES COMMANDS

        /// <summary>
        /// 获取Modem短信模式
        /// </summary>
        /// <returns>GSMModem短信模式</returns>
        public MessageMode GetMessageMode()
        {
            lock (mt) return (MessageMode)int.Parse(ExecTask("AT+CMGF?\r", "^\\+CMGF:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置Modem短信模式
        /// </summary>
        /// <param name="messageMode">GSMModem短信模式</param>
        public void SetMessageMode(MessageMode messageMode)
        {
            lock (mt) ExecTask(string.Format("AT+CMGF={0}\r", (int)messageMode), "");
        }

        /// <summary>
        /// 获取新短信提示模式
        /// </summary>
        /// <returns>新短信提示模式</returns>
        public NewMessageIndication GetNewMessageIndication()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CNMI?\r", "^\\+CNMI:\\s*(?<mode>\\d+),(?<mt>\\d+),(?<bm>\\d+),(?<ds>\\d+),(?<bfr>\\d+)")[0];
                return new NewMessageIndication()
                {
                    Mode = int.Parse(match.Result("${mode}")),
                    MessageTreat = int.Parse(match.Result("${mt}")),
                    BroadcaseTreat = int.Parse(match.Result("${bm}")),
                    SMSStatusReport = int.Parse(match.Result("${ds}")),
                    BufferTreat = int.Parse(match.Result("${bfr}"))
                };
            }
        }

        /// <summary>
        /// 设置新短信提示模式
        /// </summary>
        /// <param name="indication">新短信提示模式</param>
        public void SetNewMessageIndication(NewMessageIndication indication)
        {
            lock (mt) ExecTask(string.Format("AT+CNMI={0},{1},{2},{3},{4}\r",
                indication.Mode,
                indication.MessageTreat,
                indication.BroadcaseTreat,
                indication.SMSStatusReport,
                indication.BufferTreat), "");
        }

        /// <summary>
        /// 删除短信
        /// </summary>
        /// <param name="index">短信在存储卡中的位置</param>
        public void DeleteMessage(int index)
        {
            lock (mt) ExecTask(string.Format("AT+CMGD={0}\r", index), "");
        }

        /// <summary>
        /// 删除短信
        /// </summary>
        /// <param name="index">短信在存储卡中的位置</param>
        /// <param name="flag">删除选项</param>
        public void DeleteMessage(int index, DeleteFlag flag)
        {
            lock (mt) ExecTask(string.Format("AT+CMGD={0},{1}\r", index, (int)flag), "");
        }

        #endregion

        #region Unslicited

        /// <summary>
        /// 表示将处理 Vultrue.Communication.Modem 对象的来电事件的方法。
        /// </summary>
        public event EventHandler Ringing;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler NoCarrier;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler NoDialTone;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler NoAnswer;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Busying;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.Modem 对象的接收到短信事件的方法。
        /// </summary>
        public event EventHandler<MessageArrivaledEventArgs> MessageArrivaled;

        /// <summary>
        /// 表示将处理 Vultrue.Communication.Modem 对象的网络注册信息已改变的方法。
        /// </summary>
        public event EventHandler<NetworkRegistrationChangedEventArgs> NetworkRegistrationChanged;

        /// <summary>
        /// 
        /// </summary>
        protected void OnConnected()
        {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnNoCarrier()
        {
            if (NoCarrier != null)
                NoCarrier(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnNoDialTone()
        {
            if (NoDialTone != null)
                NoDialTone(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnNoAnswer()
        {
            if (NoAnswer != null)
                NoAnswer(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnBusying()
        {
            if (Busying != null)
                Busying(this, EventArgs.Empty);
        }

        /// <summary>
        /// 主动上报数据处理
        /// </summary>
        /// <param name="line"></param>
        /// <returns>匹配正确返回True，不正确返回False</returns>
        protected virtual bool UnsolicitedDeal(string line)
        {
            Match match;
            if ((match = regexRing.Match(line)).Success)
            {
                if (Ringing != null)
                    new Thread(() => { Ringing(this, EventArgs.Empty); }).Start();
                return true;
            }
            if ((match = regexCmti.Match(line)).Success)
            {
                if (MessageArrivaled != null)
                {
                    new Thread(() =>
                    {
                        MessageArrivaled(this, new MessageArrivaledEventArgs(match.Result("${mem}"), int.Parse(match.Result("${index}"))));
                    }).Start();
                }
                return true;
            }
            if ((match = regexCreg.Match(line)).Success)
            {
                if (NetworkRegistrationChanged != null)
                {
                    string[] result = match.Result("${ans}").Replace("\"", "").Split(',');
                    NetworkState state = (NetworkState)int.Parse(result[0]);
                    string locationAreaCode = result.Length > 1 ? result[1] : "";
                    string cellID = result.Length > 2 ? result[2] : "";
                    new Thread(() => { NetworkRegistrationChanged(this, new NetworkRegistrationChangedEventArgs(state, locationAreaCode, cellID)); }).Start();
                }
                return true;
            }
            return false;
        }

        #endregion
    }

    #region 事件参数

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.Ring事件提供参数
    /// </summary>
    public class RingingEventArgs : EventArgs { }

    /// <summary>
    /// 为Vultrue.Communication.GSMModem.NoteletArrivaled事件提供参数
    /// </summary>
    public class MessageArrivaledEventArgs : EventArgs
    {
        /// <summary>
        /// 短信存储的媒体
        /// </summary>
        public string StoreMedia { get; set; }

        /// <summary>
        /// 短信位置
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 初始化类Vultrue.Communication.NoteletArrivaledEventArgs的新实例
        /// </summary>
        /// <param name="storeMedia">短信存储的媒体</param>
        /// <param name="index">短信位置</param>
        public MessageArrivaledEventArgs(string storeMedia, int index)
        {
            StoreMedia = storeMedia;
            Index = index;
        }
    }

    /// <summary>
    /// 为 Vultrue.Communication.GSMModem.NetworkRegistrationChanged 事件提供参数
    /// </summary>
    public class NetworkRegistrationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 网络注册状态
        /// </summary>
        public NetworkState State { get; private set; }

        /// <summary>
        /// 区域代码
        /// </summary>
        public string LocationAreaCode { get; private set; }

        /// <summary>
        /// 基站ID
        /// </summary>
        public string CellID { get; private set; }

        /// <summary>
        /// 初始化类 Vultrue.Communication.NetworkRegistrationChangedEventArgs 的新实例
        /// </summary>
        /// <param name="state">网络注册状态</param>
        /// <param name="locationAreaCode">区域代码</param>
        /// <param name="cellID">基站ID</param>
        public NetworkRegistrationChangedEventArgs(NetworkState state, string locationAreaCode, string cellID)
        {
            State = state;
            LocationAreaCode = locationAreaCode;
            CellID = cellID;
        }
    }

    #endregion
}
