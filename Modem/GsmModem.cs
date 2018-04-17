using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;

namespace Vultrue.Communication
{
    /// <summary>
    /// GsmModem
    /// </summary>
    public class GsmModem : Modem
    {
        #region GENERAL COMMANDS

        /// <summary>
        /// 获取支持的TE字符集列表
        /// </summary>
        /// <returns>支持的TE字符集列表</returns>
        public string[] GetTECharacterSetList()
        {
            lock (mt) return ExecTask("AT+CSCS=?\r", "^\\+CSCS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)")[0].Result("${ans}").Trim('(', ')').Replace("\"", "").Split(',');
        }

        /// <summary>
        /// 获取当前TE字符集
        /// </summary>
        /// <returns>当前TE字符集</returns>
        public string GetTECharacterSet()
        {
            lock (mt) return ExecTask("AT+CSCS?\r", "^\\+CSCS:\\s*\"(?<ans>\\w+)\"")[0].Result("${ans}");
        }

        /// <summary>
        /// 设置当前TE字符集
        /// </summary>
        /// <param name="characterSet">TE字符集</param>
        public void SetTECharacterSet(string characterSet)
        {
            lock (mt) ExecTask(string.Format("AT+CSCS=\"{0}\"\r", characterSet), "");
        }

        /// <summary>
        /// 获取Modem功能列表
        /// </summary>
        /// <returns>Modem功能列表</returns>
        public string[] GetCapabilitiesList()
        {
            lock (mt) return ExecTask("AT+GCAP\r", "^\\+GCAP:\\s*(?<ans>[ \\w\\p{P}\\p{S}]+)")[0].Result("${ans}").Replace(" ", "").Split(',');
        }

        /// <summary>
        /// 关闭Modem ME, 等效于Modem功能等级设置为0
        /// </summary>
        public void MTPowerOff()
        {
            lock (mt) ExecTask("AT+CPOF\r", "");
        }

        /// <summary>
        /// 设置Modem功能级别
        /// </summary>
        /// <returns>0:关机; 1:重起</returns>
        public int GetPhoneFunctionalityLevel()
        {
            lock (mt) return int.Parse(ExecTask("AT+CFUN?\r", "^\\+CFUN:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置Modem功能级别(0或1)
        /// </summary>
        /// <param name="level">0:关机; 1:重起</param>
        public void SetPhoneFunctionalityLevel(int level)
        {
            lock (mt) ExecTask(string.Format("AT+CFUN={0}\r", level), "");
        }

        /// <summary>
        /// 获取Modem活动状态
        /// </summary>
        /// <returns>Modem活动状态</returns>
        public ActivityStatus GetPhoneActivityStatus()
        {
            lock (mt) return (ActivityStatus)int.Parse(ExecTask("AT+CPAS\r", "^\\+CPAS:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置按键控制Pattern
        /// </summary>
        /// <param name="keypadControl">按键控制Pattern</param>
        public void SetKeypadControl(string keypadControl)
        {
            lock (mt) ExecTask(string.Format("AT+CKPD=\"{0}\"\r", keypadControl), "^\\+CCFC:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)");
        }

        /// <summary>
        /// 获取Modem时间(精确到分)
        /// </summary>
        /// <returns>Modem时间</returns>
        public DateTime GetModemClock()
        {
            lock (mt) return DateTime.Parse("20" + ExecTask("AT+CCLK?\r", "^\\+CCLK:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)")[0].Result("${ans}").Trim('\"'));
        }

        /// <summary>
        /// 设置Modem时钟(精确到分)
        /// </summary>
        public void SetModemClock(DateTime clock)
        {
            lock (mt) ExecTask(string.Format("AT+CCLK=\"{0:yy/MM/dd,HH:mm:ss}\"\r", clock), "");
        }

        /// <summary>
        /// 获取Modem闹钟列表(精确到分,尚未响铃)
        /// </summary>
        /// <returns>尚未响铃的Modem闹钟列表</returns>
        public DateTime[] GetModemAlarms()
        {
            lock (mt)
            {
                List<DateTime> alarms = new List<DateTime>();
                foreach (Match match in ExecTask("AT+CALA?\r", "^\\+CALA:\\s*\"(?<cala>[/:,\\d]+)\",(?<index>\\d+)"))
                    alarms.Add(DateTime.Parse("20" + match.Result("${cala}")));
                return alarms.ToArray();
            }
        }

        /// <summary>
        /// 设置Modem闹钟列表(精确到分)
        /// </summary>
        public void AddModemAlarm(DateTime alarm)
        {
            lock (mt) ExecTask(string.Format("AT+CALA=\"{0:yy/MM/dd,HH:mm:ss}\"\r", alarm), "");
        }

        /// <summary>
        /// 获取来电音量(0-15, 6为默认值)
        /// </summary>
        /// <returns>音量</returns>
        public int GetRingerSoundLevel()
        {
            lock (mt) return int.Parse(ExecTask("AT+CRSL?\r", "^\\+CRSL:\\s*(?<ans>\\d+)")[0].Result("${ans}"));
        }

        /// <summary>
        /// 设置来电音量(0-15, 6为默认值)
        /// </summary>
        /// <param name="soundLevel">音量</param>
        public void SetRingerSoundLevel(int soundLevel)
        {
            lock (mt) ExecTask(string.Format("AT+CRSL={0}\r", soundLevel), "");
        }

        #endregion

        #region Call Control Command

        /// <summary>
        /// 接听电话
        /// </summary>
        public void AnswerCall()
        {
            ExecTask("ATA\r", "");
        }

        #endregion

        #region PHONEBOOK COMMANDS

        /// <summary>
        /// 获取Modem支持的电话本存储位置列表
        /// </summary>
        /// <returns>电话本存储位置列表</returns>
        public string[] GetPhonebookMemoryStorageList()
        {
            lock (mt) return ExecTask("AT+CPBS=?\r", "^\\+CPBS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)")[0].Result("${ans}").Trim('(', ')').Replace("\"", "").Split(',');
        }

        /// <summary>
        /// 获取电话本信息
        /// </summary>
        /// <returns>Item1:起始索引; Item2:结束索引; Item3:电话号码最大长度; Item4:关联文本最大长度;</returns>
        public Tuple<int, int, int, int> GetPhonebookInfo()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPBR=?\r", "^\\+CPBR:\\s*\\((?<si>\\d+)-(?<ei>\\d+)\\),(?<mp>\\d+),(?<mt>\\d+)")[0];
                return new Tuple<int, int, int, int>(int.Parse(match.Result("${si}")), int.Parse(match.Result("${ei}")),
                    int.Parse(match.Result("${mp}")), int.Parse(match.Result("${mt}")));
            }
        }

        /// <summary>
        /// 从电话本读取电话号码
        /// </summary>
        /// <param name="index">起始索引</param>
        /// <returns>读取的电话本条目</returns>
        public PhonebookEntry ReadPhonebookEntries(int index)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPBR={0}\r", index), "^\\+CPBR:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"")[0];
                return new PhonebookEntry()
                {
                    Index = int.Parse(match.Result("${index}")),
                    Number = match.Result("${number}"),
                    Type = int.Parse(match.Result("${type}")),
                    Text = match.Result("${text}")
                };
            }
        }

        /// <summary>
        /// 从电话本读取电话号码
        /// </summary>
        /// <param name="startIndex">起始索引</param>
        /// <param name="endIndex">结束索引</param>
        /// <returns>读取的电话本条目</returns>
        public PhonebookEntry[] ReadPhonebookEntries(int startIndex, int endIndex)
        {
            lock (mt)
            {
                List<PhonebookEntry> entries = new List<PhonebookEntry>();
                foreach (Match match in ExecTask(string.Format("AT+CPBR={0},{1}\r", startIndex, endIndex),
                    "^\\+CPBR:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\""))
                    entries.Add(new PhonebookEntry()
                    {
                        Index = int.Parse(match.Result("${index}")),
                        Number = match.Result("${number}"),
                        Type = int.Parse(match.Result("${type}")),
                        Text = match.Result("${text}")
                    });
                return entries.ToArray();
            }
        }

        /// <summary>
        /// 从电话本查找电话号码
        /// </summary>
        /// <param name="text">搜索文本</param>
        /// <returns>查找到的电话本条目</returns>
        public PhonebookEntry[] FindPhonebookEntries(string text)
        {
            lock (mt)
            {
                List<PhonebookEntry> entries = new List<PhonebookEntry>();
                foreach (Match match in ExecTask(string.Format("AT+CPBF=\"{0}\"\r", text),
                    "^\\+CPBF:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\""))
                    entries.Add(new PhonebookEntry()
                    {
                        Index = int.Parse(match.Result("${index}")),
                        Number = match.Result("${number}"),
                        Type = int.Parse(match.Result("${type}")),
                        Text = match.Result("${text}")
                    });
                return entries.ToArray();
            }
        }

        /// <summary>
        /// 从电话本删除电话本条目
        /// </summary>
        /// <param name="index"></param>
        public void DeletePhonebookEntry(int index)
        {
            ExecTask(string.Format("AT+CPBW={0}\r", index), "");
        }

        /// <summary>
        /// 向电话本写入电话本条目
        /// </summary>
        /// <param name="index">存储位置</param>
        /// <param name="number">电话号码</param>
        /// <param name="type">号码类型(129:本地号码/145:国际号码)</param>
        /// <param name="text">联系文本</param>
        public void WritePhonebookEntry(int index, string number, int type, string text)
        {
            lock (mt) ExecTask(string.Format("AT+CPBW={0},\"{1}\",{2},\"{3}\"\r", index, number, type, text), "");
        }

        /// <summary>
        /// 向电话本写入电话本条目
        /// </summary>
        /// <param name="number">电话号码</param>
        /// <param name="type">号码类型(129:本地号码/145:国际号码)</param>
        /// <param name="text">联系文本</param>
        public void WritePhonebookEntry(string number, int type, string text)
        {
            lock (mt) ExecTask(string.Format("AT+CPBW=,\"{0}\",{1},\"{2}\"\r", number, type, text), "");
        }

        /// <summary>
        /// 由电话号码搜索电话本条目
        /// </summary>
        /// <param name="number">电话号码</param>
        /// <returns>电话本条目</returns>
        public PhonebookEntry PhonebookPhoneSearch(string number)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPBP=\"{0}\"\r", number),
                    "^\\+CPBP:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"")[0];
                return new PhonebookEntry()
                {
                    Index = int.Parse(match.Result("${index}")),
                    Number = match.Result("${number}"),
                    Type = int.Parse(match.Result("${type}")),
                    Text = match.Result("${text}")
                };
            }
        }

        /// <summary>
        /// 在电话号码本中移动
        /// </summary>
        /// <param name="mode">移动模式</param>
        /// <returns>电话本条目</returns>
        public PhonebookEntry MoveActionPhonebook(MoveMode mode)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPBN={0}\r", (int)mode),
                    "^\\+CPBN:\\s*(?<index>\\d+),\"(?<number>[\\d\\+]+)\",(?<type>\\d+),\"(?<text>\\w+)\"")[0];
                return new PhonebookEntry()
                {
                    Index = int.Parse(match.Result("${index}")),
                    Number = match.Result("${number}"),
                    Type = int.Parse(match.Result("${type}")),
                    Text = match.Result("${text}")
                };
            }
        }

        #endregion

        #region SHORT MESSAGES COMMANDS

        /// <summary>
        /// 新短信接收确认
        /// 文本模式下, 只能进行肯定确认(自动忽略acknowledge参数)
        /// PDU模式下, 可进行肯定和否定确认
        /// 只有短信AT命令版本1下且+CMT和+CDS设置为显示时才允许确认
        /// </summary>
        /// <param name="acknowledge">肯定确认或否定确认</param>
        /// <param name="pdustr">确认信息</param>
        public void NewPduModeMessageAcknowledgement(bool acknowledge, string pdustr)
        {
            //lock (mt) 
            //if (pdustr == null || pdustr.Length == 0)
            //{
            //    ModemTask task = new ModemTask("AT+CNMA=0\r", "(?<ans>[ \\w\\p{P}\\p{S}]+)");
            //    ExecTask(task);
            //}
            //else
            //{
            //    ModemTask task1 = new ModemTask(string.Format("AT+CNMA={0},{1}\r", (acknowledge ? "1" : "2"), pdustr.Length / 2), "\u003E\u0020");
            //    ModemTask task2 = new ModemTask(string.Format("{0}\u001A", pdustr), "(?<ans>[ \\w\\p{P}\\p{S}]+)");
            //    task1.IsNonOKCmd = true;
            //    TaskGroup group = new TaskGroup(new ModemTask[] { task1, task2 });
            //    ExecTask(group);
            //}
        }

        /// <summary>
        /// 获取GSMModem短信服务AT命令版本(0或1)
        /// </summary>
        /// <returns>GSMModem短信服务AT命令版本</returns>
        public int GetMessageService()
        {
            lock (mt) return int.Parse(ExecTask("AT+CSMS?\r", "^\\+CSMS:\\s*(?<service>\\d+),(?<mo>\\d+),(?<mt>\\d+),(?<cb>\\d+)")[0].Result("${service}"));
        }

        /// <summary>
        /// 设置GSMModem短信服务AT命令版本(0或1)
        /// </summary>
        /// <param name="messageService">GSMModem短信服务AT命令版本</param>
        /// <returns></returns>
        public void SetMessageService(int messageService)
        {
            lock (mt) ExecTask(string.Format("AT+CSMS={0}\r", messageService), "^\\+CSMS:\\s*(?<ans>[\\w\\p{P}\\p{S}]+)");
        }

        /// <summary>
        /// 保存设置信息(短信中心号码和文本模式参数)到EEPROM(SIM卡Phase1)或SIM(SIM卡Phase2)
        /// </summary>
        public void SaveSettings()
        {
            lock (mt) ExecTask("AT+CSAS\r", "");
        }

        /// <summary>
        /// 恢复设置信息(短信中心号码和文本模式参数)从EEPROM(SIM卡Phase1)或SIM(SIM卡Phase2)
        /// </summary>
        public void RestoreSettings()
        {
            lock (mt) ExecTask("AT+CRES\r", "");
        }

        /// <summary>
        /// 获取短信存储空间列表
        /// </summary>
        /// <returns>Item1:读取,列出,删除操作的可选目标空间; Item2:写入,发送的可选目标空间;</returns>
        public Tuple<string[], string[]> GetMessageStorageList()
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPMS=?\r", "^\\+CPMS:\\s*\\(\\((?<rld>[\",\\w]+)\\),\\((?<ws>[\",\\w]+)\\)\\)")[0];
                return new Tuple<string[], string[]>(match.Result("${rld}").Replace("\"", "").Split(','), match.Result("${ws}").Replace("\"", "").Split(','));
            }
        }

        /// <summary>
        /// 获取GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStorage">写入与发送操作目标存储空间</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        public void GetPreferredMessageStorage(out string rldStorage, out int rldStoreUsed,
            out int rldStoreTotal, out string wsStorage, out int wsStoreUsed, out int wsStoreTotal)
        {
            lock (mt)
            {
                Match match = ExecTask("AT+CPMS?\r", "^\\+CPMS:\\s*\"(?<rld>\\w+)\",(?<rldu>\\d+),(?<rldt>\\d+),\"(?<ws>\\w+)\",(?<wsu>\\d+),(?<wst>\\d+)")[0];
                rldStorage = match.Result("${rld}");
                rldStoreUsed = int.Parse(match.Result("${rldu}"));
                rldStoreTotal = int.Parse(match.Result("${rldt}"));
                wsStorage = match.Result("${ws}");
                wsStoreUsed = int.Parse(match.Result("${wsu}"));
                wsStoreTotal = int.Parse(match.Result("${wst}"));
            }
        }

        /// <summary>
        /// 设置GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        public void SetPreferredMessageStorage(string rldStorage, out int rldStoreUsed,
            out int rldStoreTotal, out int wsStoreUsed, out int wsStoreTotal)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPMS=\"{0}\"\r", rldStorage),
                    "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+)")[0];
                rldStoreUsed = int.Parse(match.Result("${rldu}"));
                rldStoreTotal = int.Parse(match.Result("${rldt}"));
                wsStoreUsed = int.Parse(match.Result("${wsu}"));
                wsStoreTotal = int.Parse(match.Result("${wst}"));
            }
        }

        /// <summary>
        /// 设置GSMModem默认的短信存储空间
        /// </summary>
        /// <param name="rldStorage">读取,列出,删除操作目标存储空间</param>
        /// <param name="wsStorage">写入与发送操作目标存储空间, 不需要需要修改时, 值为null时不修改</param>
        /// <param name="rldStoreUsed">读取,列出,删除操作存储空间已使用的短信条数</param>
        /// <param name="rldStoreTotal">读取,列出,删除操作存储空间可存储的短信总条数</param>
        /// <param name="wsStoreUsed">写入与发送操作存储空间已使用的短信条数</param>
        /// <param name="wsStoreTotal">写入与发送操作存储空间可存储的短信总条数</param>
        public void SetPreferredMessageStorage(string rldStorage, string wsStorage,
            out int rldStoreUsed, out int rldStoreTotal, out int wsStoreUsed, out int wsStoreTotal)
        {
            lock (mt)
            {
                Match match = ExecTask(string.Format("AT+CPMS=\"{0}\",\"{1}\"\r", rldStorage, wsStorage),
                    "^\\+CPMS:\\s*(?<rldu>\\d+),(?<rldt>\\d+),(?<wsu>\\d+),(?<wst>\\d+)")[0];
                rldStoreUsed = int.Parse(match.Result("${rldu}"));
                rldStoreTotal = int.Parse(match.Result("${rldt}"));
                wsStoreUsed = int.Parse(match.Result("${wsu}"));
                wsStoreTotal = int.Parse(match.Result("${wst}"));
            }
        }

        /// <summary>
        /// 获取是否显示文本模式参数
        /// </summary>
        /// <returns>是否显示文本模式参数</returns>
        public bool GetIsShowTextModeParameters()
        {
            lock (mt) return ExecTask("AT+CSDH?\r", "^\\+CSDH:\\s*(?<ans>\\d+)")[0].Result("${ans}") == "1";
        }

        /// <summary>
        /// 设置是否显示文本模式参数
        /// 影响的指令+CMTI, +CMT, +CDS, +CMGR, +CMGL
        /// </summary>
        /// <param name="isShow">是否显示文本模式参数</param>
        public void SetIsShowTextModeParameters(bool isShow)
        {
            lock (mt) ExecTask(string.Format("AT+CSDH={0}\r", (isShow ? "1" : "0")), "");
        }

        // <summary>
        // 读取短信
        // </summary>
        // <param name="index">短信所在存储空间的索引</param>
        // <returns>读取的短信</returns>
        //public Message ReadPduMessage(int index)
        //{
            //lock (mt) 
            //ModemTask task = new ModemTask(string.Format("AT+CMGR={0}\r", index), "(?<ans>[ \\w\\p{P}\\p{S}]+)");
            //ExecTask(task);
            //string line = task.Matchs[0].Result("${ans}");
            //Match match = new Regex("^\\+CMGR:\\s*(?<stat>\\d+),(?<alpha>[ \"\\w\\d]*),(?<lenth>\\d+)").Match(line);
            //if (!match.Success) throw new ModemDataException(line);
            //MessageState messageState = (MessageState)int.Parse(match.Result("${stat}"));
            //int lenth = int.Parse(match.Result("${lenth}"));
            //return new Message(index, messageState, lenth, task.Matchs[1].Result("${ans}"));
        //}

        // <summary>
        // 列出短信
        // </summary>
        // <param name="state">筛选短信的状态</param>
        // <returns>读取的短信</returns>
        //public Message[] ListPduMessage(MessageState state)
        //{
        //    lock (mt) 
        //    ModemTask task = new ModemTask(string.Format("AT+CMGL={0}\r", (int)state), "(?<ans>[ \\w\\p{P}\\p{S}]+)");
        //    ExecTask(task);
        //    List<Message> messages = new List<Message>();
        //    Regex regex = new Regex("^\\+CMGL:\\s*(?<index>\\d+),(?<stat>\\d+),(?<alpha>[ \"\\w\\d]*),(?<lenth>\\d+)");
        //    for (int i = 0; i < task.Matchs.Count; i++)
        //    {
        //        string line = task.Matchs[i].Result("${ans}");
        //        Match match = regex.Match(line);
        //        if (!match.Success) continue;
        //        int index = int.Parse(match.Result("${index}"));
        //        MessageState messageState = (MessageState)int.Parse(match.Result("${stat}"));
        //        int lenth = int.Parse(match.Result("${lenth}"));
        //        messages.Add(new Message(index, messageState, lenth, task.Matchs[++i].Result("${ans}")));
        //    }
        //    return messages.ToArray();
        //}

        // <summary>
        // 发送短信
        // </summary>
        // <param name="message">需要发送的短信</param>
        //public void SendPduMessage(Message message)
        //{
        //    lock (mt) 
        //    string tpdu = message.Tpdu;
        //    ModemTask task1 = new ModemTask(string.Format("AT+CMGS={0}\r", tpdu.Length / 2), "\u003E\u0020");
        //    ModemTask task2 = new ModemTask(string.Format("{0}\u001A", tpdu), "(?<ans>[ \\w\\p{P}\\p{S}]+)");
        //    task1.IsNonOKCmd = true;
        //    TaskGroup group = new TaskGroup(new ModemTask[] { task1, task2 });
        //    ExecTask(group);
        //}

        // <summary>
        // 发送短信
        // </summary>
        // <param name="mobileNum">目标号码</param>
        // <param name="userData">短信内容(PDU串)</param>
        // <param name="encode">编码</param>
        //public void SendPduMessage(string mobileNum, string userData, DataCodingScheme encode)
        //{
        //    lock (mt) 
        //    for (int i = 0; i < userData.Length; i += MAX_MESSAGE_LENTH)
        //    {
        //        string str = userData.Length - i > MAX_MESSAGE_LENTH ?
        //            userData.Substring(i, MAX_MESSAGE_LENTH) : userData.Substring(i, userData.Length - i);
        //        SendPduMessage(new Message(mobileNum, str, encode));
        //    }
        //}

        // <summary>
        // 发送文本短信
        // </summary>
        // <param name="mobileNum">目标号码</param>
        // <param name="text">短信内容(文本)</param>
        //public void SendPduMessage(string mobileNum, string text)
        //{
        //    SendPduMessage(mobileNum, PduString.GetPdustr(text), DataCodingScheme.USC2);
        //}
        /// <summary>
        /// 获取服务中心号码
        /// </summary>
        /// <returns>服务中心号码</returns>
        public string GetServiceCenterAddress()
        {
            lock (mt) return ExecTask("AT+CSCA?\r", "^\\+CSCA:\\s*\"(?<sca>[\\+\\d]+)\",(?<type>\\d+)")[0].Result("${sca}");
        }

        /// <summary>
        /// 设置服务中心号码
        /// </summary>
        /// <param name="serviceCenterAddress">服务中心号码</param>
        public void SetServiceCenterAddress(string serviceCenterAddress)
        {
            lock (mt) ExecTask(string.Format("AT+CSCA=\"{0}\"\r", serviceCenterAddress), "");
        }

        #endregion
    }
}
