using System;

namespace Vultrue.Communication
{
    /// <summary>
    /// �ֽ�������ʽ�ĵ����ȸ�����
    /// </summary>
    public unsafe static class FloatBytes
    {
        /// <summary>
        /// �õ������ȸ�����
        /// </summary>
        /// <param name="bytes">�ֽ�����</param>
        /// <param name="isBigend">�Ƿ���ֽ��ں�</param>
        /// <returns></returns>
        public static float GetFloat(byte[] bytes, bool isBigend)
        {
            if (bytes.Length != 4) throw new FormatException("�ֽ����鳤��ӦΪ4");
            float temp;
            byte* bf = (byte*)&temp;
            if (isBigend) for (int i = 0; i < 4; i++) *(bf + i) = bytes[3 - i];
            else for (int i = 0; i < 4; i++) *(bf + i) = bytes[i];
            return temp;
        }

        /// <summary>
        /// �õ��ֽ�����
        /// </summary>
        /// <param name="num">�����ȸ�����</param>
        /// <param name="isBigend">�Ƿ���ֽ��ں�</param>
        /// <returns>�ֽ�����</returns>
        public static byte[] GetBytes(float num, bool isBigend)
        {
            float temp = num;
            byte* pb = (byte*)&temp;
            byte[] bytes = new byte[4];
            if (isBigend) for (int i = 0; i < 4; i++) bytes[i] = *(pb + 3 - i);
            else for (int i = 0; i < 4; i++) bytes[i] = *(pb + i);
            return bytes;
        }
    }
}
