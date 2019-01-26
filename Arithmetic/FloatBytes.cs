using System;

namespace Vultrue.Communication
{
    /// <summary>
    /// 字节数组形式的单精度浮点数
    /// </summary>
    public unsafe static class FloatBytes
    {
        /// <summary>
        /// 得到单精度浮点数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="isBigend">是否高字节在后</param>
        /// <returns></returns>
        public static float GetFloat(byte[] bytes, bool isBigend)
        {
            if (bytes.Length != 4) throw new FormatException("字节数组长度应为4");
            float temp;
            byte* bf = (byte*)&temp;
            if (isBigend) for (int i = 0; i < 4; i++) *(bf + i) = bytes[3 - i];
            else for (int i = 0; i < 4; i++) *(bf + i) = bytes[i];
            return temp;
        }

        /// <summary>
        /// 得到字节数组
        /// </summary>
        /// <param name="num">单精度浮点数</param>
        /// <param name="isBigend">是否高字节在后</param>
        /// <returns>字节数组</returns>
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
