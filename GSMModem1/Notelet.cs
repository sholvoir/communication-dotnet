using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Vultrue.Communication {
	/// <summary>
	/// 短信
	/// </summary>
	public class Notelet {
		public enum DCS { Data = 0x04, USC2 = 0x08 };

		private string smscNum;
		private string mobileNum;
		private int location;
		private DateTime transTime;
		private string pdustr;
		private DCS encode;

		#region 属性

		/// <summary>
		/// 电话号码
		/// </summary>
		public string MobileNum {
			get { return this.mobileNum; }
			set { this.mobileNum = value; }
		}

		/// <summary>
		/// 传输时间
		/// </summary>
		public DateTime TransTime {
			get { return this.transTime; }
			set { this.transTime = value; }
		}

		/// <summary>
		/// 短信内容
		/// </summary>
		public string Pdustr {
			get { return this.pdustr; }
			set { this.pdustr = value; }
		}

		/// <summary>
		/// 短信在Sim卡中的位置
		/// </summary>
		public int Loction {
			get { return this.location; }
			set { this.location = value; }
		}

		/// <summary>
		/// 编码方式
		/// </summary>
		public DCS Encode {
			get { return encode; }
			set { encode = value; }
		}

		#endregion

		#region 构造

		/// <summary>
		/// 默认构造
		/// </summary>
		private Notelet() { }

		/// <summary>
		/// 构造短信
		/// </summary>
		/// <param name="mobileNum">目标号码</param>
		/// <param name="content">短信内容</param>
		/// <param name="encode">编码方式</param>
		public Notelet(string mobileNum, string pdustr, DCS encode) {
			this.mobileNum = mobileNum;
			this.pdustr = pdustr;
			this.encode = encode;
		}

		#endregion

		/// <summary>
		/// 得到发送AT指令
		/// </summary>
		/// <returns>用于发送该短信的AT指令</returns>
		public string GetSMSAT() {
			int i = this.MobileNum.Length;
			if (i % 2 != 0) i++;
			i >>= 1;
			i += 8;
			i += this.pdustr.Length >> 1;
			return "AT+CMGS=" + i.ToString();
		}

		/// <summary>
		/// 得到短信体
		/// </summary>
		/// <returns>短信的PDU内容</returns>
		public string GetSMSBody() {
			StringBuilder cmd = new StringBuilder();
			cmd.Append("00"); //添加短信中心号码 00表示使用SIM存储的中心号码
			cmd.Append("1100"); //添加TPMTI/VFP及消息基准值
			cmd.Append(this.MobileNum.Length.ToString("X2")); //添加目标号码长度
			if (this.MobileNum.Substring(0, 2) == "86") //添加目标号码格式
				cmd.Append("91"); //国际格式
			else cmd.Append("A1"); //本地格式
			cmd.Append(PhoneNum2ConvertedNum(mobileNum)); //添加电话号码
			cmd.Append("00"); //添加协议标识 00为普通GSM点到点方式
			cmd.Append(((int)encode).ToString("X2")); //添加用户信息编码方式, 04为8bit编码, 08为UCS2(16bit)编码
			cmd.Append("A9"); //添加短信有效期信息 A9为3天
			cmd.Append((this.pdustr.Length >> 1).ToString("X2")); //添加用户数据字节数
			cmd.Append(this.pdustr); //添加用户数据
			return cmd.ToString();
		}

		/// <summary>
		/// 从正常号码得到翻转的号码
		/// </summary>
		/// <param name="phoneNum"></param>
		/// <returns></returns>
		public static string PhoneNum2ConvertedNum(string PhoneNum) {
			if (PhoneNum.Length == 0) return "";
			int numLen = PhoneNum.Length >> 1;
			StringBuilder converted = new StringBuilder();
			for (int i = 0; i < numLen; i++) { //添加目标号码
				converted.Append(PhoneNum.Substring(2 * i + 1, 1));
				converted.Append(PhoneNum.Substring(2 * i, 1));
			}
			if (PhoneNum.Length % 2 != 0) {
				converted.Append('F');
				converted.Append(PhoneNum[PhoneNum.Length - 1]);
			}
			return converted.ToString();
		}

		/// <summary>
		/// 从翻转的号码得到正常号码
		/// </summary>
		/// <param name="covertedPhoneNum"></param><returns></returns>
		public static string ConvertedNum2PhoneNum(string CovertedNum) {
			if (CovertedNum.Length == 0) return "";
			char[] charPhoneNum = new char[CovertedNum.Length];
			for (int i = 0; i < CovertedNum.Length / 2; i++) {
				charPhoneNum[2 * i] = CovertedNum[2 * i + 1];
				charPhoneNum[2 * i + 1] = CovertedNum[2 * i];
			}
			string phoneNum = new string(charPhoneNum);
			if (CovertedNum.IndexOf("F") > 0) phoneNum = phoneNum.Substring(0, phoneNum.Length - 1);
			return phoneNum;
		}

		/// <summary>
		/// 从接收到的Pdu串中解析出短信
		/// </summary>
		/// <param name="index">短信在SIM卡中的位置</param>
		/// <param name="pdustr">短信内容</param>
		/// <returns></returns>
		public static Notelet Parse(int index, string pdustr) {
			Notelet notelet = new Notelet();
			notelet.location = index;
			try {
				//解析出短信中心号码
				int smsclen = int.Parse(pdustr.Substring(0, 2), NumberStyles.HexNumber) * 2;
				if (smsclen == 0) notelet.smscNum = "";
				else notelet.smscNum = ConvertedNum2PhoneNum(pdustr.Substring(4, smsclen - 2));
				//解析出回复号码
				int phoneNumIndex = smsclen + 4;
				int phoneNumLen = int.Parse(pdustr.Substring(phoneNumIndex, 2), NumberStyles.HexNumber);
				if (phoneNumLen % 2 != 0) phoneNumLen++;
				string phoneNum = ConvertedNum2PhoneNum(pdustr.Substring(phoneNumIndex + 4, phoneNumLen));
				notelet.mobileNum = phoneNum.Substring(0, 2) == "86" ? phoneNum.Remove(0, 2) : phoneNum;
				//解析出接收时间
				int sendTimeIndex = 2 + smsclen + 2 + 4 + phoneNumLen + 2 + 2;
				StringBuilder timeString = new StringBuilder("20");
				timeString.Append(pdustr[sendTimeIndex + 1]);//年
				timeString.Append(pdustr[sendTimeIndex]);
				timeString.Append("-");
				timeString.Append(pdustr[sendTimeIndex + 3]);//月
				timeString.Append(pdustr[sendTimeIndex + 2]);
				timeString.Append("-");
				timeString.Append(pdustr[sendTimeIndex + 5]);//日
				timeString.Append(pdustr[sendTimeIndex + 4]);
				timeString.Append(" ");
				timeString.Append(pdustr[sendTimeIndex + 7]);//时
				timeString.Append(pdustr[sendTimeIndex + 6]);
				timeString.Append(":");
				timeString.Append(pdustr[sendTimeIndex + 9]);//分
				timeString.Append(pdustr[sendTimeIndex + 8]);
				timeString.Append(":");
				timeString.Append(pdustr[sendTimeIndex + 11]);//秒
				timeString.Append(pdustr[sendTimeIndex + 10]);
				notelet.transTime = DateTime.Parse(timeString.ToString());
				// 解析短信内容
				int pduIndex = sendTimeIndex + 14;
				int pduLen = int.Parse(pdustr.Substring(pduIndex, 2), NumberStyles.HexNumber);
				notelet.pdustr = pdustr.Substring(pduIndex + 2, pduLen * 2);
			}
			catch {
				notelet.smscNum = "";
				notelet.mobileNum = "1860";
				notelet.transTime = DateTime.Now;
				notelet.pdustr = "";
			}
			return notelet;
		}

	}
}