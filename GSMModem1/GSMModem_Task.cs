using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Vultrue.Communication {
	partial class GSMModem {
		/// <summary>
		/// ����ִ�еĽ��
		/// </summary>
		private enum TaskStatus { Unfinished, Finished, Unexpected, Failed }

		/// <summary>
		/// ���������
		/// </summary>
		private abstract class Task {
            protected const int REPEATNUMBER = 3;
			protected GSMModem modem;
			protected int state;
			protected int repeat = 0;

			protected abstract void step0();

			/// <summary>
			/// �����������, ʹ֮��ʼִ��
			/// </summary>
			public void Start(GSMModem modem) {
                this.modem = modem;
                state = 1;
                step0();
            }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public abstract TaskStatus Receive(string answer);

            /// <summary>
            /// ����ʧ�ܵ�ִ��
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
		/// �ֹ�����, ֱ�ӷ������ݵ�Modem
		/// </summary>
		private class Manual : Task {
			private string cmd;

			/// <summary>
			/// ����
			/// </summary>
			/// <param name="cmd">ָ��</param>
			public Manual(string cmd) { this.cmd = cmd; }

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() {
                modem.sendCmd(cmd);
            }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
                if (modem.ManualTaskFinished != null) modem.ManualTaskFinished(modem, null);
                return TaskStatus.Finished;
            }
		}

		/// <summary>
		/// Modem�ź�ǿ�ȶ�ȡ
		/// </summary>
		private class SignalRead : Task {
			private int si; //�ź�ǿ��

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() { modem.sendCmd("AT\r"); }

			private void step1() { modem.sendCmd("AT+CSQ\r"); }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //�ɹ�, ������һ��
							state = 2; step1();
							return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //ʧ��, �ظ�ִ�е�������3��
                            if (repeat++ >= REPEATNUMBER) { modem.Close(); return taskFail("Modem����ʧ��"); }
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"\+CSQ:\s*(?<rssi>\d+),(?<ber>\d+)"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //�ɹ�, ������һ��
							si = int.Parse(match[0].Result("${rssi}"));
							state = 3; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //ʧ��, �ظ�ִ�е�������3��
                            if (repeat++ >= REPEATNUMBER) { modem.Close(); return taskFail("Modem����ʧ��"); }
							step1(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex("OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //�ɹ��������
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
		/// ���ŷ�������
		/// </summary>
		private class NoteletSend : Task {
			private Notelet notelet;

			/// <summary>
			/// ���캯��
			/// </summary>
			/// <param name="sms">�����͵Ķ���</param>
            public NoteletSend(Notelet notelet) {
                this.notelet = notelet;
            }

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
            protected override void step0() {
                modem.sendCmd(notelet.GetSMSAT() + "\r");
            }

            private void step1() {
                modem.sendCmd(notelet.GetSMSBody() + "\u001A");
            }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex(new string(new char[] { '\x3e', '\x20' })), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //�ɹ�, ������һ��
							state = 2; step1();
							return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //ʧ��, �ظ�ִ�е�������3�� 
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"\+CMGS:\s*\d+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //���ͳɹ�, ������һ��
							state = 3; return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //����ʧ��, �ص�0����ʼ
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							state = 1; step0();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex("OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //�������
                            if (modem.NoteletSended != null) modem.NoteletSended(modem, new NoteletSendedEventArgs(notelet));
                            return TaskStatus.Finished;
                        }
                        else return TaskStatus.Unexpected;
					default: Start(modem); return TaskStatus.Unfinished;
				}
			}
		}

		/// <summary>
		/// ���Ŷ�ȡ����
		/// </summary>
		private class NoteletRead : Task {
			private int index; //��������λ��
			private List<Notelet> notelets = new List<Notelet>();

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
            protected override void step0() {
                notelets.Clear();
                modem.sendCmd("AT+CMGL=4\r");
            }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
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
						if (match[0].Success) { //�ж���, �õ�λ����Ϣ, ������һ����ȡ��������
							index = int.Parse(match[0].Result("${index}"));
							state = 2; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //����ִ�гɹ�
                            if (notelets.Count > 0 && modem.NoteletReceived != null)
                                modem.NoteletReceived(modem, new NoteletReceivedEventArgs(notelets.ToArray()));
                            return TaskStatus.Finished;
                        }
                        else if (match[2].Success) { //����ִ�д���, �ظ�ִ�е�����������ظ�����
                            if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
                            step0(); return TaskStatus.Unfinished;
                        }
                        else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"[0-9a-fA-F]+") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //���յ���������, �����һ��������һ��
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
		/// ����ɾ������
		/// </summary>
		private class NoteletDelete : Task {
			private int location;

			/// <summary>
			/// �������ɾ������
			/// </summary>
			/// <param name="loacion">��ɾ���Ķ���Ϣλ��</param>
			public NoteletDelete(int loacion) { this.location = loacion; }

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() {
				modem.sendCmd("AT+CMGD=" + location.ToString() + "\r");
			}

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
				if ((match[0].Success) || (match[1].Success)) return TaskStatus.Finished;
				else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// �绰����ȡ����
		/// </summary>
		private class PhoneRead : Task {
			private int index;
			private string phoneNum;
			private string userName;

			/// <summary>
			/// ����һ�������ȥ����
			/// </summary>
			/// <param name="index">���λ��</param>
			public PhoneRead(int index) { this.index = index; }

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() { modem.sendCmd("AT+CPBR=" + index.ToString() + "\r"); }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] {
					new Regex("\\+CPBR:\\s*(?<index>\\d+),\"(?<number>\\d+)\",(?<type>\\d+),\"(?<name>)\""),
					new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
				if (match[0].Success) { //�����ɹ�
					index = int.Parse(match[0].Result("${index}"));
					phoneNum = match[0].Result("${number}");
					userName = match[0].Result("${name}");
                    if (modem.PhoneBookReaded != null) modem.PhoneBookReaded(modem, new PhoneBookReadedEventArgs(index, userName, phoneNum));
					return TaskStatus.Finished;
				}
				else if (match[1].Success) { //��ȡʧ��, �ظ�ִ�е�����������ظ�����
					if (repeat++ >= REPEATNUMBER) return taskFail("�绰����ȡ����");
					step0(); return TaskStatus.Unfinished;
				}
				else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// �绰��д������
		/// </summary>
		private class PhoneWrite : Task {
			private int index;
			private string phoneNumber;
			private int type;
			private string name;

			/// <summary>
			/// ����绰��д������
			/// </summary>
			/// <param name="index">д��λ��</param>
			/// <param name="phoneNumber">�绰����</param>
			/// <param name="type">����</param>
			/// <param name="name">����</param>
			public PhoneWrite(int index, string phoneNumber, int type, string name) {
				this.index = index; this.phoneNumber = phoneNumber; this.type = type; this.name = name;
			}

			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() {
				string cmd = "AT+CPBW=" + index.ToString() + ",\"" + phoneNumber + "\"";
				if (name.Length > 0) cmd += "," + type.ToString() + "\"" + name + "\"";
				modem.sendCmd(cmd + "\r");
			}

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				Regex[] regex = new Regex[] { new Regex("OK"), new Regex(@"\w*ERROR\w*") };
				Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                if (match[0].Success) { //д��ɹ�
                    if (modem.PhoneBookWrited != null) modem.PhoneBookWrited(modem, null);
                    return TaskStatus.Unfinished;
                }
                else if (match[1].Success) { //д��ʧ��, �ظ�ִ�е�����������ظ�ִ�д���
                    if (repeat++ >= REPEATNUMBER) return taskFail("�绰��д�����");
                    step0(); return TaskStatus.Unfinished;
                }
                else return TaskStatus.Unexpected;
			}
		}

		/// <summary>
		/// ����Ҷ�����
		/// </summary>
		private class Ringoff : Task {
			/// <summary>
			/// ����ִ�еĵ�һ������
			/// </summary>
			protected override void step0() { modem.sendCmd("ATH\r"); }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
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
		/// Modem��Ϣ��ȡ
		/// </summary>
		private class ModemInfo : Task {
            private string factoryInfo;
            private string modemInfo;
            private string modemVersion;
            private string smsCenter;

			/// <summary>
            /// ����ִ�еĵ�һ������
            /// </summary>
			protected override void step0() { modem.sendCmd("AT+CGMI\r"); }

			private void step1() { modem.sendCmd("AT+CGMM\r"); }

			private void step2() { modem.sendCmd("AT+CGMR\r"); }

			private void step3() { modem.sendCmd("AT+CSCA?\r"); }

			/// <summary>
			/// �Է����н��д���
			/// </summary>
			/// <param name="answer">���յ�����</param>
			/// <returns>����ִ�еĽ��</returns>
			public override TaskStatus Receive(string answer) {
				switch (state) {
					case 1:
						Regex[] regex = new Regex[] { new Regex(@"[a-zA-Z]+"), new Regex(@"\w*ERROR\w*") };
						Match[] match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
						if (match[0].Success) { //�ɹ�, ׼������OK
                            factoryInfo = answer;
                            state = 2; return TaskStatus.Unfinished;
						}
						else if (match[1].Success) { //ʧ��, �ظ�ִ�е�����������ظ�ִ�д���
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step0(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 2:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //��һ��ִ�����, ִ�еڶ���
							state = 3; step1();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 3:
						regex = new Regex[] { new Regex(@"[a-zA-Z0-9]+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //�ɹ�, ׼������OK
                            modemInfo = answer;
                            state = 4; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //ʧ��, �ظ�ִ�е�����������ظ�ִ�д���
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
                            step1(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 4:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //�ڶ���ִ�����, ִ�е�����
							state = 5; step2();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 5:
						regex = new Regex[] { new Regex(@"[a-zA-Z0-9]+\W[0-9]+\W[0-9]+"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //�ɹ�, ׼������OK
                            modemVersion = answer;
							state = 6; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //ʧ��, �ظ�ִ�е�����������ظ�ִ�д���
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step2(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 6:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
                        if (match[0].Success) { //������ִ�����, ִ�е�����
							state = 7; step3();
							return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 7:
						regex = new Regex[] { new Regex(@"\+CSCA:\s*\W+(?<Num>\d+)\W+(?<Index>\d+)"), new Regex(@"\w*ERROR\w*") };
						match = new Match[] { regex[0].Match(answer), regex[1].Match(answer) };
                        if (match[0].Success) { //�ɹ�, ׼������OK
                            smsCenter = match[0].Result("${Num}");
							state = 8; return TaskStatus.Unfinished;
						}
                        else if (match[1].Success) { //ʧ��, �ظ�ִ�е�����������ظ�ִ�д���
							if (repeat++ >= REPEATNUMBER) return TaskStatus.Failed;
							step3(); return TaskStatus.Unfinished;
						}
						else return TaskStatus.Unexpected;
					case 8:
						regex = new Regex[] { new Regex(@"OK") };
						match = new Match[] { regex[0].Match(answer) };
						if (match[0].Success) { //������ִ�����, �������
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