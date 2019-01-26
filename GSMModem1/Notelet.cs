using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Vultrue.Communication {
	/// <summary>
	/// ����
	/// </summary>
	public class Notelet {
		public enum DCS { Data = 0x04, USC2 = 0x08 };

		private string smscNum;
		private string mobileNum;
		private int location;
		private DateTime transTime;
		private string pdustr;
		private DCS encode;

		#region ����

		/// <summary>
		/// �绰����
		/// </summary>
		public string MobileNum {
			get { return this.mobileNum; }
			set { this.mobileNum = value; }
		}

		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime TransTime {
			get { return this.transTime; }
			set { this.transTime = value; }
		}

		/// <summary>
		/// ��������
		/// </summary>
		public string Pdustr {
			get { return this.pdustr; }
			set { this.pdustr = value; }
		}

		/// <summary>
		/// ������Sim���е�λ��
		/// </summary>
		public int Loction {
			get { return this.location; }
			set { this.location = value; }
		}

		/// <summary>
		/// ���뷽ʽ
		/// </summary>
		public DCS Encode {
			get { return encode; }
			set { encode = value; }
		}

		#endregion

		#region ����

		/// <summary>
		/// Ĭ�Ϲ���
		/// </summary>
		private Notelet() { }

		/// <summary>
		/// �������
		/// </summary>
		/// <param name="mobileNum">Ŀ�����</param>
		/// <param name="content">��������</param>
		/// <param name="encode">���뷽ʽ</param>
		public Notelet(string mobileNum, string pdustr, DCS encode) {
			this.mobileNum = mobileNum;
			this.pdustr = pdustr;
			this.encode = encode;
		}

		#endregion

		/// <summary>
		/// �õ�����ATָ��
		/// </summary>
		/// <returns>���ڷ��͸ö��ŵ�ATָ��</returns>
		public string GetSMSAT() {
			int i = this.MobileNum.Length;
			if (i % 2 != 0) i++;
			i >>= 1;
			i += 8;
			i += this.pdustr.Length >> 1;
			return "AT+CMGS=" + i.ToString();
		}

		/// <summary>
		/// �õ�������
		/// </summary>
		/// <returns>���ŵ�PDU����</returns>
		public string GetSMSBody() {
			StringBuilder cmd = new StringBuilder();
			cmd.Append("00"); //��Ӷ������ĺ��� 00��ʾʹ��SIM�洢�����ĺ���
			cmd.Append("1100"); //���TPMTI/VFP����Ϣ��׼ֵ
			cmd.Append(this.MobileNum.Length.ToString("X2")); //���Ŀ����볤��
			if (this.MobileNum.Substring(0, 2) == "86") //���Ŀ������ʽ
				cmd.Append("91"); //���ʸ�ʽ
			else cmd.Append("A1"); //���ظ�ʽ
			cmd.Append(PhoneNum2ConvertedNum(mobileNum)); //��ӵ绰����
			cmd.Append("00"); //���Э���ʶ 00Ϊ��ͨGSM�㵽�㷽ʽ
			cmd.Append(((int)encode).ToString("X2")); //����û���Ϣ���뷽ʽ, 04Ϊ8bit����, 08ΪUCS2(16bit)����
			cmd.Append("A9"); //��Ӷ�����Ч����Ϣ A9Ϊ3��
			cmd.Append((this.pdustr.Length >> 1).ToString("X2")); //����û������ֽ���
			cmd.Append(this.pdustr); //����û�����
			return cmd.ToString();
		}

		/// <summary>
		/// ����������õ���ת�ĺ���
		/// </summary>
		/// <param name="phoneNum"></param>
		/// <returns></returns>
		public static string PhoneNum2ConvertedNum(string PhoneNum) {
			if (PhoneNum.Length == 0) return "";
			int numLen = PhoneNum.Length >> 1;
			StringBuilder converted = new StringBuilder();
			for (int i = 0; i < numLen; i++) { //���Ŀ�����
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
		/// �ӷ�ת�ĺ���õ���������
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
		/// �ӽ��յ���Pdu���н���������
		/// </summary>
		/// <param name="index">������SIM���е�λ��</param>
		/// <param name="pdustr">��������</param>
		/// <returns></returns>
		public static Notelet Parse(int index, string pdustr) {
			Notelet notelet = new Notelet();
			notelet.location = index;
			try {
				//�������������ĺ���
				int smsclen = int.Parse(pdustr.Substring(0, 2), NumberStyles.HexNumber) * 2;
				if (smsclen == 0) notelet.smscNum = "";
				else notelet.smscNum = ConvertedNum2PhoneNum(pdustr.Substring(4, smsclen - 2));
				//�������ظ�����
				int phoneNumIndex = smsclen + 4;
				int phoneNumLen = int.Parse(pdustr.Substring(phoneNumIndex, 2), NumberStyles.HexNumber);
				if (phoneNumLen % 2 != 0) phoneNumLen++;
				string phoneNum = ConvertedNum2PhoneNum(pdustr.Substring(phoneNumIndex + 4, phoneNumLen));
				notelet.mobileNum = phoneNum.Substring(0, 2) == "86" ? phoneNum.Remove(0, 2) : phoneNum;
				//����������ʱ��
				int sendTimeIndex = 2 + smsclen + 2 + 4 + phoneNumLen + 2 + 2;
				StringBuilder timeString = new StringBuilder("20");
				timeString.Append(pdustr[sendTimeIndex + 1]);//��
				timeString.Append(pdustr[sendTimeIndex]);
				timeString.Append("-");
				timeString.Append(pdustr[sendTimeIndex + 3]);//��
				timeString.Append(pdustr[sendTimeIndex + 2]);
				timeString.Append("-");
				timeString.Append(pdustr[sendTimeIndex + 5]);//��
				timeString.Append(pdustr[sendTimeIndex + 4]);
				timeString.Append(" ");
				timeString.Append(pdustr[sendTimeIndex + 7]);//ʱ
				timeString.Append(pdustr[sendTimeIndex + 6]);
				timeString.Append(":");
				timeString.Append(pdustr[sendTimeIndex + 9]);//��
				timeString.Append(pdustr[sendTimeIndex + 8]);
				timeString.Append(":");
				timeString.Append(pdustr[sendTimeIndex + 11]);//��
				timeString.Append(pdustr[sendTimeIndex + 10]);
				notelet.transTime = DateTime.Parse(timeString.ToString());
				// ������������
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