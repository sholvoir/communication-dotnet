using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Vultrue.Communication {
	partial class GSMModem {
		/// <summary>
		/// 任务执行的结果
		/// </summary>
		private enum TaskStatus { Unfinished, Finished, Unexpected, Failed }

		/// <summary>
		/// 任务虚基类
		/// </summary>
		private abstract class Task {
            protected const int REPEATNUMBER = 3;
			protected GSMModem modem;
			protected int state;
			protected int repeat = 0;

			protected abstract void step0();

			/// <summary>
			/// 调度这个任务, 使之开始执行
			/// </summary>
			public void Start(GSMModem modem) {
                this.modem = modem;
                state = 1;
                step0();
            }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public abstract TaskStatus Receive(string answer);

            /// <summary>
            /// 任务失败的执行
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            protected TaskStatus taskFail(string message) {
                if (modem.CommunicationError != null)
                    modem.CommunicationError(modem, new CommunicationErrorEventArgs(message));
                return TaskStatus.Failed;
            }
		}

		/// <summary>
		/// 手工任务, 直接发送数据到Modem
		/// </summary>
		private class Manual : Task {
			private string cmd;

			/// <summary>
			/// 构造
			/// </summary>
			/// <param name="cmd">指令</param>
			public Manual(string cmd) { this.cmd = cmd; }

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() {
                modem.sendCmd(cmd);
            }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
                if (modem.ManualTaskFinished != null) modem.ManualTaskFinished(modem, null);
                return TaskStatus.Finished;
            }
		}

		/// <summary>
		/// Modem信号强度读取
		/// </summary>
		private class SignalRead : Task {
			private int si; //信号强度

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() { modem.sendCmd("AT\r"); }

			private void step1() { modem.sendCmd("AT+CSQ\r"); }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //成功, 进入下一步
							state = 2; step1();
							return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //失败, 重复执行但不超过3次
                            if (repeat++ >= REPEATNUMBER) { modem.Close(); return taskFail("Modem测试失败"); }
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"\+CSQ:\s*(?<rssi>\d+),(?<ber>\d+)"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //成功, 进入下一步
							si = int.Parse(match[0].Result("${rssi}"));
							state = 3; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //失败, 重复执行但不超过3次
                            if (repeat++ >= REPEATNUMBER) { modem.Close(); return taskFail("Modem测试失败"); }
							step1(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex("OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //成功完成任务
                            if (si >= 0 && si <= 31) {
                                modem.intensity = si;
                                if (modem.IntensityReaded != null) modem.IntensityReaded(modem, null);
                            }
                            return TaskStatus.Finished;
                        }
                        else return TaskStatus.Unexpected;
					default: Start(modem); return TaskStatus.Unfinished;
				}
			}
		}

		/// <summary>
		/// 短信发送任务
		/// </summary>
		private class NoteletSend : Task {
			private Notelet notelet;

			/// <summary>
			/// 构造函数
			/// </summary>
			/// <param name="sms">待发送的短信</param>
            public NoteletSend(Notelet notelet) {
                this.notelet = notelet;
            }

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
            protected override void step0() {
                modem.sendCmd(notelet.GetSMSAT() + "\r");
            }

            private void step1() {
                modem.sendCmd(notelet.GetSMSBody() + "\u001A");
            }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex(new string(new char[] { '\x3e', '\x20' })), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //成功, 进入下一步
							state = 2; step1();
							return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //失败, 重复执行但不超过3次 
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"\+CMGS:\s*\d+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //发送成功, 进入下一步
							state = 3; return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //发送失败, 重第0步开始
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							state = 1; step0();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex("OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //任务结束
                            if (modem.NoteletSended != null) modem.NoteletSended(modem, new NoteletSendedEventArgs(notelet));
                            return TaskStatus.Finished;
                        }
                        else return TaskStatus.Unexpected;
					default: Start(modem); return TaskStatus.Unfinished;
				}
			}
		}

		/// <summary>
		/// 短信读取任务
		/// </summary>
		private class NoteletRead : Task {
			private int index; //短信所在位置
			private List<Notelet> notelets = new List<Notelet>();

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
            protected override void step0() {
                notelets.Clear();
                modem.sendCmd("AT+CMGL=4\r");
            }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] {
                            new Regex(@"\+CMGL:\s*(?<index>\d+),(?<stat>\d+),(?<alpha>\d*),(?<length>\d+)"),
                            new Regex(@"OK"),
                            new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] {
                            regex[0].Match(answer),
                            regex[1].Match(answer),
                            regex[2].Match(answer) };
						if (match[0].Success) { //有短信, 得到位置信息, 进入下一步读取短信内容
							index = int.Parse(match[0].Result("${index}"));
							state = 2; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //命令执行成功
                            if (notelets.Count > 0 && modem.NoteletReceived != null)
                                modem.NoteletReceived(modem, new NoteletReceivedEventArgs(notelets.ToArray()));
                            return TaskStatus.Finished;
                        }
                        else if (match[2].Success) { //命令执行错误, 重复执行但不超过最大重复次数
                            if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
                            step0(); return TaskStatus.Unfinished;
                        }
                        else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"[0-9a-fA-F]+") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //接收到短信内容, 进入第一步接收下一条
							Notelet notelet = Notelet.Parse(index, answer);
							notelets.Add(notelet); state = 1;
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					default: Start(modem); return TaskStatus.Unfinished;
				}
			}
		}

		/// <summary>
		/// 短信删除任务
		/// </summary>
		private class NoteletDelete : Task {
			private int location;

			/// <summary>
			/// 构造短信删除任务
			/// </summary>
			/// <param name="loacion">待删除的短消息位置</param>
			public NoteletDelete(int loacion) { this.location = loacion; }

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() {
				modem.sendCmd("AT+CMGD=" + location.ToString() + "\r");
			}

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
				if ((match[0].Success) || (match[1].Success)) return TaskStatus.Finished;
				else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// 电话簿读取任务
		/// </summary>
		private class PhoneRead : Task {
			private int index;
			private string phoneNum;
			private string userName;

			/// <summary>
			/// 构造一个号码读去任务
			/// </summary>
			/// <param name="index">存放位置</param>
			public PhoneRead(int index) { this.index = index; }

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() { modem.sendCmd("AT+CPBR=" + index.ToString() + "\r"); }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] {
					new Regex("\\+CPBR:\\s*(?<index>\\d+),\"(?<number>\\d+)\",(?<type>\\d+),\"(?<name>)\""),
					new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
				if (match[0].Success) { //读出成功
					index = int.Parse(match[0].Result("${index}"));
					phoneNum = match[0].Result("${number}");
					userName = match[0].Result("${name}");
                    if (modem.PhoneBookReaded != null) modem.PhoneBookReaded(modem, new PhoneBookReadedEventArgs(index, userName, phoneNum));
					return TaskStatus.Finished;
				}
				else if (match[1].Success) { //读取失败, 重复执行但不超过最大重复次数
					if (repeat++ >= REPEATNUMBER) return taskFail("电话本读取错误");
					step0(); return TaskStatus.Unfinished;
				}
				else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// 电话簿写入任务
		/// </summary>
		private class PhoneWrite : Task {
			private int index;
			private string phoneNumber;
			private int type;
			private string name;

			/// <summary>
			/// 构造电话本写入任务
			/// </summary>
			/// <param name="index">写入位置</param>
			/// <param name="phoneNumber">电话号码</param>
			/// <param name="type">类型</param>
			/// <param name="name">名称</param>
			public PhoneWrite(int index, string phoneNumber, int type, string name) {
				this.index = index; this.phoneNumber = phoneNumber; this.type = type; this.name = name;
			}

			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() {
				string cmd = "AT+CPBW=" + index.ToString() + ",\"" + phoneNumber + "\"";
				if (name.Length > 0) cmd += "," + type.ToString() + "\"" + name + "\"";
				modem.sendCmd(cmd + "\r");
			}

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                if (match[0].Success) { //写入成功
                    if (modem.PhoneBookWrited != null) modem.PhoneBookWrited(modem, null);
                    return TaskStatus.Unfinished;
                }
                else if (match[1].Success) { //写入失败, 重复执行但不超过最大重复执行次数
                    if (repeat++ >= REPEATNUMBER) return taskFail("电话本写入错误");
                    step0(); return TaskStatus.Unfinished;
                }
                else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// 来电挂断任务
		/// </summary>
		private class Ringoff : Task {
			/// <summary>
			/// 任务执行的第一个步骤
			/// </summary>
			protected override void step0() { modem.sendCmd("ATH\r"); }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
				if (match[0].Success) return TaskStatus.Finished;
				else if (match[1].Success) {
					if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
					step0(); return TaskStatus.Unfinished;
				}
				else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// Modem信息读取
		/// </summary>
		private class ModemInfo : Task {
            private string factoryInfo;
            private string modemInfo;
            private string modemVersion;
            private string smsCenter;

			/// <summary>
            /// 任务执行的第一个步骤
            /// </summary>
			protected override void step0() { modem.sendCmd("AT+CGMI\r"); }

			private void step1() { modem.sendCmd("AT+CGMM\r"); }

			private void step2() { modem.sendCmd("AT+CGMR\r"); }

			private void step3() { modem.sendCmd("AT+CSCA?\r"); }

			/// <summary>
			/// 对返回行进行处理
			/// </summary>
			/// <param name="answer">接收到的行</param>
			/// <returns>任务执行的结果</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex(@"[a-zA-Z]+"), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //成功, 准备接收OK
                            factoryInfo = answer;
                            state = 2; return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //失败, 重复执行但不超过最大重复执行次数
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //第一条执行完毕, 执行第二条
							state = 3; step1();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex(@"[a-zA-Z0-9]+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //成功, 准备接收OK
                            modemInfo = answer;
                            state = 4; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //失败, 重复执行但不超过最大重复执行次数
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
                            step1(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 4:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //第二条执行完毕, 执行第三条
							state = 5; step2();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 5:
						regex = new Regex[] { new Regex(@"[a-zA-Z0-9]+\W[0-9]+\W[0-9]+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //成功, 准备接收OK
                            modemVersion = answer;
							state = 6; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //失败, 重复执行但不超过最大重复执行次数
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step2(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 6:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //第三条执行完毕, 执行第四条
							state = 7; step3();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 7:
						regex = new Regex[] { new Regex(@"\+CSCA:\s*\W+(?<Num>\d+)\W+(?<Index>\d+)"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //成功, 准备接收OK
                            smsCenter = match[0].Result("${Num}");
							state = 8; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //失败, 重复执行但不超过最大重复执行次数
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step3(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 8:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //第四条执行完毕, 任务完成
                            if (modem.ModemInfoReaded != null)
                                modem.ModemInfoReaded(modem, new ModemInfoReadedEventArgs(factoryInfo, modemInfo, modemVersion, smsCenter));
							return TaskStatus.Finished;
						}
						else return TaskStatus.Unexpected;
					default: Start(modem); return TaskStatus.Unfinished;
				}
			}
		}
	}
}