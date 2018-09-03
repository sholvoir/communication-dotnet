using System;

namespace CMPP
{
    /// <summary>
    /// SMS 事件参数。
    /// </summary>
    public class SMSEventArgs : EventArgs
    {

        #region 字段
        private SMS_EVENT _type;
        private object _data;
        private DateTime _time;
        #endregion

        #region 属性
        /// <summary>
        /// 事件类型。
        /// </summary>
        public SMS_EVENT Type
        {
            get
            {
                return _type;
            }
        }
        /// <summary>
        /// 事件数据。
        /// </summary>
        public object Data
        {
            get
            {
                return _data;
            }
        }
        /// <summary>
        /// 引发时间。
        /// </summary>
        public DateTime Time
        {
            get
            {
                return _time;
            }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化 <see cref="SMSEventArgs"/> 类新实例。
        /// </summary>
        public SMSEventArgs(SMS_EVENT type, object data, DateTime time)
        {
            _type = type;
            _data = data;
            _time = time;
        }
        #endregion

    }
}
