﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Vultrue.Communication
{
    /// <summary>
    /// 字节字符串类, 用字符串来表示字节组
    /// 每个字节用两个字符来表示
    /// 如 0x2B 表示为"2B"
    /// 静态类, 提供一组方法来处理字节字符串
    /// </summary>
    public static class ByteString
    {
        private static readonly byte[] crctablehi = new byte[]
        {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };

        private static readonly byte[] crctablelo = new byte[]
        {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 
            0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 
            0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 
            0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 0x11, 0xD1, 0xD0, 0x10, 
            0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 
            0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 
            0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 
            0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 
            0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 
            0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 
            0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 
            0xB4, 0x74, 0x75, 0xB5, 0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 
            0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 
            0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 
            0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C, 
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80, 0x40
            
        };

        /// <summary>
        /// 对字节字符串进行高位在前与低位在前转换
        /// </summary>
        /// <param name="bytestr">字节字符串</param>
        /// <returns></returns>
        public static string Reverse(string bytestr)
        {
            if (bytestr.Length % 2 != 0)
                throw new FormatException("不是字节字符串");
            StringBuilder result = new StringBuilder();
            for (int i = bytestr.Length - 2; i >= 0; i -= 2)
                result.Append(bytestr.Substring(i, 2));
            return result.ToString();
        }

        /// <summary>
        /// 得到字节字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>字节字符串</returns>
        public static string GetByteString(byte[] bytes)
        {
            StringBuilder strb = new StringBuilder();
            foreach (byte b in bytes)
                strb.Append(b.ToString("X2"));
            return strb.ToString();
        }

        /// <summary>
        /// 得到字节字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="lenth">长度</param>
        /// <returns>字节字符串</returns>
        public static string GetByteString(byte[] bytes, int offset, int lenth)
        {
            int len = offset + lenth;
            if (bytes.Length < len) throw new Exception("超出索引");
            StringBuilder strb = new StringBuilder();
            for (int i = offset; i < len; i++)
                strb.Append(bytes[i].ToString("X2"));
            return strb.ToString();
        }

        /// <summary>
        /// 得到字节数组
        /// </summary>
        /// <param name="bytestr">字节字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] GetBytes(string bytestr)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < bytestr.Length; i += 2)
                bytes.Add(byte.Parse(bytestr.Substring(i, 2), NumberStyles.HexNumber));
            return bytes.ToArray();
        }

        /// <summary>
        /// 得到显示字符串, 每个字节的表示用空格来分开, 便于显示
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>显示字符串</returns>
        public static string GetDisplayString(byte[] bytes)
        {
            StringBuilder strb = new StringBuilder();
            foreach (byte b in bytes) strb.Append(b.ToString("X2")).Append(" ");
            return strb.ToString();
        }

        /// <summary>
        /// 得到显示字符串, 每个字节的表示用空格来分开, 便于显示
        /// </summary>
        /// <param name="bytestr">字节字符串</param>
        /// <returns>显示字符串</returns>
        public static string GetDisplayString(string bytestr)
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < bytestr.Length; i += 2)
            {
                strb.Append(bytestr.Substring(i, 2) + " ");
            }
            return strb.ToString();
        }

        /// <summary>
        /// 得到校验和
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>校验和</returns>
        public static int CheckSum(byte[] bytes)
        {
            int cs = 0;
            unchecked
            {
                for (int i = 0; i <bytes.Length; i++) cs += bytes[i];
            }
            return cs;
        }

        /// <summary>
        /// 得到校验和
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="lenth">长度</param>
        /// <returns>校验和</returns>
        public static int CheckSum(byte[] bytes, int offset, int lenth)
        {

            int len = offset + lenth;
            if (len > bytes.Length) len = bytes.Length;
            int cs = 0;
            unchecked
            {
                for(int i = offset; i < len; i++)  cs += bytes[i];
            }
            return cs;
        }

        /// <summary>
        /// 得到校验和
        /// </summary>
        /// <param name="bytestr">字节字符串</param>
        /// <returns>校验和</returns>
        public static int CheckSum(string bytestr)
        {
            byte b;
            int cs = 0;
            unchecked
            {
                for (int i = 0; i < bytestr.Length; i += 2)
                    cs += byte.TryParse(bytestr.Substring(i, 2), NumberStyles.HexNumber, null, out b) ? b : 0;
            }
            return cs;
        }

        /// <summary>
        /// 得到MD5加密字节字符串
        /// </summary>
        /// <param name="str">待加密字符串</param>
        /// <returns>字节字符串密文</returns>
        public static string MDString(string str)
        {
            return GetByteString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(str)));
        }

        /// <summary>
        /// 计算CRC16校验码
        /// </summary>
        /// <param name="data">数据存放区</param>
        /// <param name="offset"></param>
        /// <param name="lenth"></param>
        /// <param name="crchigh">校验码高字节</param>
        /// <param name="crclow">校验码高字节</param>
        /// <returns>CRC16校验码</returns>
        public static ushort CRC16(byte[] data, int offset, int lenth, out byte crchigh, out byte crclow)
        {
            int len = offset + lenth;
            if (len > data.Length) len = data.Length;
            crchigh = 0xff;  // high crc byte initialized
            crclow = 0xff;  // low crc byte initialized 
            for (int i = offset; i < len; i++)
            {
                int index = crchigh ^ data[i]; // calculate the crc lookup index
                crchigh = (byte)(crclow ^ crctablehi[index]);
                crclow = crctablelo[index];
            }
            return (ushort)((crchigh << 8) | crclow);
        }

        /// <summary>
        /// 半字节高低位交换
        /// </summary>
        /// <param name="bytestr">字节字符串</param>
        /// <returns></returns>
        public static string SemiByteExchange(string bytestr)
        {
            if (bytestr.Length % 2 != 0) throw new FormatException("不是字节字符串");
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < bytestr.Length; i += 2)
                strb.Append(bytestr[i + 1]).Append(bytestr[i]);
            return strb.ToString();
        }
    }
}
