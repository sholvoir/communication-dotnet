using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Vultrue.Communication
{
    public abstract partial class Connection : Component, IConnection
    {
        #region 构造

        public Connection()
        {
            InitializeComponent();
        }

        public Connection(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        #endregion

        #region 属性

        public abstract string Address { get; }

        public abstract int Port { get; }

        public object Tag { get; set; }

        #endregion

        #region 事件

        public event EventHandler Closing;

        public event EventHandler<DataTransEventArgs> DataSended;

        public event EventHandler<DataTransEventArgs> DataReceived;

        #endregion

        #region 方法

        public abstract void Write(byte[] buffer, int offset, int count);

        public void Write(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            Write(buffer, 0, buffer.Length);
        }

        public abstract void Close();

        #endregion

        #region 事件引发方法

        protected void OnClosing(EventArgs e)
        {
            EventHandler handler = Closing;
            if (handler != null) handler(this, e);
        }

        protected void OnDataSended(DataTransEventArgs e)
        {
            EventHandler<DataTransEventArgs> handler = DataSended;
            if (handler != null) handler(this, e);
        }

        protected void OnDataReceived(DataTransEventArgs e)
        {
            EventHandler<DataTransEventArgs> handler = DataReceived;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}
