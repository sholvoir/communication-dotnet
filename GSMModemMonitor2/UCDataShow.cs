using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Generic
{
    public partial class DataShow : UserControl
    {
        public event DataGridViewCellEventHandler RowEnter;

        /// <summary>
        /// 读取或设置是否自动清除
        /// </summary>
        [DefaultValue(false)]
        public bool AutoClear { get; set; }

        /// <summary>
        /// 读取或设置是否自动滚动
        /// </summary>
        [DefaultValue(true)]
        public bool AutoRoll { get; set; }

        /// <summary>
        /// 提供索引器以获取位于具有指定索引的行
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public DataGridViewRow this[int rowIndex]
        {
            get { return this.dataGrid.Rows[rowIndex]; }
        }

        /// <summary>
        /// 行集合
        /// </summary>
        public DataGridViewRowCollection Rows
        {
            get { return dataGrid.Rows; }
        }

        public DataShow()
        {
            InitializeComponent();
            AutoClear = false;
            AutoRoll = true;
        }

        /// <summary>
        /// 向显示器添加行
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(string name, string value)
        {
            return Add(name, value, null);
        }

        /// <summary>
        /// 向显示器添加行
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int Add(string name, string value, object tag)
        {
            int x = bindingSource.Add(new NameValue(name, value, tag));
            if (AutoClear)
            {
                int removeNum = dataGrid.Rows.Count - dataGrid.Height / dataGrid.RowTemplate.Height;
                for (int i = 0; i < removeNum; i++) bindingSource.RemoveAt(0);
                x -= removeNum;
            }
            else if (AutoRoll && dataGrid.Rows.Count > 0)
                dataGrid.CurrentCell = dataGrid.Rows[dataGrid.RowCount - 1].Cells[0];
            return x;
        }

        /// <summary>
        /// 移除显示的所有元素
        /// </summary>
        public void Clear()
        {
            bindingSource.Clear();
        }

        private void dataGrid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (RowEnter != null) RowEnter(this, e);
        }
    }

    /// <summary>
    /// 名称值类
    /// </summary>
    public class NameValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public object Tag { get; set; }

        public NameValue(string name, string value, object tag)
        {
            Name = name;
            Value = value;
            Tag = tag;
        }
    }
}
