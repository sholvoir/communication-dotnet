namespace Vultrue.Communication {
	partial class ConnectionMonitor {
		/// <summary> 
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.textBoxSend = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.buttonSendOption = new System.Windows.Forms.Button();
            this.contextMenuStripSend = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sendStringMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendBytesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxAutoClear = new System.Windows.Forms.CheckBox();
            this.checkBoxBS = new System.Windows.Forms.CheckBox();
            this.dataShow = new Vultrue.Generic.DataShow();
            this.contextMenuStripSend.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxSend
            // 
            this.textBoxSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSend.Location = new System.Drawing.Point(0, 0);
            this.textBoxSend.Name = "textBoxSend";
            this.textBoxSend.Size = new System.Drawing.Size(344, 21);
            this.textBoxSend.TabIndex = 10;
            this.textBoxSend.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxSend_KeyPress);
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Image = global::Vultrue.Communication.Properties.Resources.notesend;
            this.buttonSend.Location = new System.Drawing.Point(344, 0);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(104, 23);
            this.buttonSend.TabIndex = 11;
            this.buttonSend.Text = "按字符发送";
            this.buttonSend.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // buttonSendOption
            // 
            this.buttonSendOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendOption.ContextMenuStrip = this.contextMenuStripSend;
            this.buttonSendOption.Image = global::Vultrue.Communication.Properties.Resources.option;
            this.buttonSendOption.Location = new System.Drawing.Point(448, 0);
            this.buttonSendOption.Name = "buttonSendOption";
            this.buttonSendOption.Size = new System.Drawing.Size(16, 23);
            this.buttonSendOption.TabIndex = 12;
            this.buttonSendOption.UseVisualStyleBackColor = true;
            this.buttonSendOption.Click += new System.EventHandler(this.buttonSendOption_Click);
            // 
            // contextMenuStripSend
            // 
            this.contextMenuStripSend.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendStringMenuItem,
            this.sendBytesMenuItem});
            this.contextMenuStripSend.Name = "contextMenuStripSend";
            this.contextMenuStripSend.Size = new System.Drawing.Size(153, 48);
            // 
            // sendStringMenuItem
            // 
            this.sendStringMenuItem.Name = "sendStringMenuItem";
            this.sendStringMenuItem.Size = new System.Drawing.Size(152, 22);
            this.sendStringMenuItem.Text = "按字符发送(&S)";
            this.sendStringMenuItem.Click += new System.EventHandler(this.sendStringMenuItem_Click);
            // 
            // sendBytesMenuItem
            // 
            this.sendBytesMenuItem.Name = "sendBytesMenuItem";
            this.sendBytesMenuItem.Size = new System.Drawing.Size(152, 22);
            this.sendBytesMenuItem.Text = "按字节发送(&B)";
            this.sendBytesMenuItem.Click += new System.EventHandler(this.sendBytesMenuItem_Click);
            // 
            // checkBoxAutoClear
            // 
            this.checkBoxAutoClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoClear.AutoSize = true;
            this.checkBoxAutoClear.Checked = true;
            this.checkBoxAutoClear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoClear.Location = new System.Drawing.Point(568, 4);
            this.checkBoxAutoClear.Name = "checkBoxAutoClear";
            this.checkBoxAutoClear.Size = new System.Drawing.Size(72, 16);
            this.checkBoxAutoClear.TabIndex = 13;
            this.checkBoxAutoClear.Text = "自动清除";
            this.checkBoxAutoClear.UseVisualStyleBackColor = true;
            this.checkBoxAutoClear.CheckedChanged += new System.EventHandler(this.checkBoxAutoClear_Click);
            // 
            // checkBoxBS
            // 
            this.checkBoxBS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxBS.AutoSize = true;
            this.checkBoxBS.Location = new System.Drawing.Point(472, 4);
            this.checkBoxBS.Name = "checkBoxBS";
            this.checkBoxBS.Size = new System.Drawing.Size(96, 16);
            this.checkBoxBS.TabIndex = 14;
            this.checkBoxBS.Text = "作为字节显示";
            this.checkBoxBS.UseVisualStyleBackColor = true;
            // 
            // dataShow
            // 
            this.dataShow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataShow.ColumnHeadersVisible = false;
            this.dataShow.Location = new System.Drawing.Point(0, 24);
            this.dataShow.Name = "dataShow";
            this.dataShow.NameColumnAutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataShow.NameColumnHeaderText = "Name";
            this.dataShow.RowHeadersVisible = false;
            this.dataShow.Size = new System.Drawing.Size(640, 456);
            this.dataShow.TabIndex = 15;
            this.dataShow.ValueColumnAutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataShow.ValueColumnHeaderText = "Value";
            // 
            // ConnectionMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxBS);
            this.Controls.Add(this.dataShow);
            this.Controls.Add(this.checkBoxAutoClear);
            this.Controls.Add(this.textBoxSend);
            this.Controls.Add(this.buttonSendOption);
            this.Controls.Add(this.buttonSend);
            this.Name = "ConnectionMonitor";
            this.Size = new System.Drawing.Size(640, 480);
            this.Load += new System.EventHandler(this.checkBoxAutoClear_Click);
            this.contextMenuStripSend.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.TextBox textBoxSend;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Button buttonSendOption;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSend;
        private System.Windows.Forms.ToolStripMenuItem sendStringMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendBytesMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoClear;
        private System.Windows.Forms.CheckBox checkBoxBS;
        private Vultrue.Generic.DataShow dataShow;
	}
}
