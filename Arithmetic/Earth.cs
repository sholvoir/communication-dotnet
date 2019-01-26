using System;
using System.Globalization;

namespace Vultrue.Communication
{
    /// <summary>
    /// 球面及视距相关算法
    /// </summary>
    public static class Earth
    {
        /// <summary>
        /// 地球平均半径(km)
        /// </summary>
        public const double R = 6371.004;

        /// <summary>
        /// 地球上的点
        /// </summary>
        public struct Point
        {
            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="longitude">经度</param>
            /// <param name="latitude">纬度</param>
            /// <param name="altitude">高度(m)</param>
            public Point(double longitude, double latitude, double altitude)
            {
                Longitude = longitude;
                Latitude = latitude;
                Altitude = altitude;
            }

            /// <summary>
            /// 经度
            /// </summary>
            public double Longitude;

            /// <summary>
            /// 纬度
            /// </summary>
            public double Latitude;

            /// <summary>
            /// 高度(m)
            /// </summary>
            public double Altitude;

            /// <summary>
            /// 直角坐标X(m)
            /// </summary>
            public double X
            {
                get
                {
                    return (R + Altitude) * Math.Cos(GetRadian(Latitude)) * Math.Cos(GetRadian(Longitude));
                }
            }

            /// <summary>
            /// 直角坐标Y(m)
            /// </summary>
            public double Y
            {
                get
                {
                    return (R + Altitude) * Math.Cos(GetRadian(Latitude)) * Math.Sin(GetRadian(Longitude));
                }
            }

            /// <summary>
            /// 直角坐标Z(m)
            /// </summary>
            public double Z
            {
                get
                {
                    return (R + Altitude) * Math.Sin(GetRadian(Latitude));
                }
            }
        }

        /// <summary>
        /// 弧度转换为角度
        /// </summary>
        /// <param name="radian">弧度数值</param>
        /// <returns>角度数值</returns>
        public static double GetAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }

        /// <summary>
        /// 角度转换为弧度
        /// </summary>
        /// <param name="angle">角度数值</param>
        /// <returns>弧度数值</returns>
        public static double GetRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        /// <summary>
        /// 地球上两点的直线距离(m)
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        /// <returns>距离(m)</returns>
        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(sqr(a.X - b.X) + sqr(a.Y - b.Y) + sqr(a.Z - b.Z));
        }

        /// <summary>
        /// 最大视距距离(已修正)(km)
        /// </summary>
        /// <param name="h1">高度1(海拔km)</param>
        /// <param name="h2">高低2(海拔km)</param>
        /// <returns>最大视距距离(km)</returns>
        public static double MaxStadia(double h1, double h2)
        {
            return Math.Sqrt(2 * R * h1 + h1 * h1) + Math.Sqrt(2 * R * h2 + h2 * h2);
        }

        /// <summary>
        /// 最大通信半径
        /// </summary>
        /// <param name="p">点P</param>
        /// <param name="signalDistance">点P的最大直线通信距离</param>
        /// <returns>点P的通信范围</returns>
        public static double MaxCommRadius(Point p, double signalDistance)
        {
            double s = signalDistance / 2;
            double h = p.Altitude / 2;
            return Math.Min(2 * Math.Sqrt((R + s + h) * (s + h) * (R - s + h) * (s - h)) / (R + p.Altitude), MaxStadia(p.Altitude, 0));
        }

        /// <summary>
        /// 将10进制经纬度格式转换为度分秒格式
        /// </summary>
        /// <param name="cordinate">十进制经纬度格式</param>
        /// <param name="degree">度</param>
        /// <param name="minute">分</param>
        /// <param name="second">秒</param>
        public static void ConvertLonLatToDegree(double cordinate, out int degree, out int minute, out double second)
        {
            degree = (int)cordinate;
            double dMinute = cordinate - degree;
            dMinute = 60 * dMinute;
            minute = (int)dMinute;
            double dSecond = dMinute - minute;
            second = (60 * dSecond);
        }

        /// <summary>
        /// 将度分秒经纬度格式转换为10进制格式
        /// </summary>
        /// <param name="degree">度</param>
        /// <param name="minute">分</param>
        /// <param name="sencond">秒</param>
        /// <returns>10进制格式的经纬度</returns>
        public static double ConvertDegreeToLonLat(int degree, int minute, int sencond)
        {
            return degree + minute / 60.0 + sencond / 360.0;
        }

        /// <summary>
        ///  将10进制经纬度格式转换为度分秒格式
        /// </summary>
        /// <param name="cordinate">十进制经纬度格式</param>
        /// <param name="precision">需要保留秒的经度</param>
        /// <returns>转化成度分秒格式后的字符串</returns>
        public static string ConvertLonLatToDegree(double cordinate, int precision)
        {
            int degree, minute;
            double second;
            ConvertLonLatToDegree(cordinate, out degree, out minute, out second);
            return string.Format("{0}º{1}'{2}\"", degree, minute, second.ToString("F" + precision.ToString(), CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 平方
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double sqr(double x)
        {
            return x * x;
        }
    }
}