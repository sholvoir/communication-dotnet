namespace Vultrue.Communication {
	partial class SerialPortDebugger {
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

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dataShowReceived = new Vultrue.Communication.DataShow();
            this.radioButtonDisplayAscii = new System.Windows.Forms.RadioButton();
            this.radioButtonDisplayHex = new System.Windows.Forms.RadioButton();
            this.checkBoxAutoClearReceived = new System.Windows.Forms.CheckBox();
            this.buttonClearReceived = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataShowSended = new Vultrue.Communication.DataShow();
            this.buttonClearSended = new System.Windows.Forms.Button();
            this.buttonSend = new System.Windows.Forms.Button();
            this.checkBoxAutoClearSended = new System.Windows.Forms.CheckBox();
            this.buttonSendOption = new System.Windows.Forms.Button();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemSendBinary = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSendAscii = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAutoSendBinary = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAutoSendAscii = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemStop = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxSend = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxAutoSendPeriod = new System.Windows.Forms.TextBox();
            this.radioButtonAsAscii = new System.Windows.Forms.RadioButton();
            this.radioButtonAsHex = new System.Windows.Forms.RadioButton();
            this.timer = new System.Timers.Timer();
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.buttonProperty = new System.Windows.Forms.Button();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.contextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.MinimumSize = new System.Drawing.Size(256, 336);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer.Size = new System.Drawing.Size(552, 416);
            this.splitContainer.SplitterDistance = 192;
            this.splitContainer.TabIndex = 21;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataShowReceived);
            this.groupBox3.Controls.Add(this.radioButtonDisplayAscii);
            this.groupBox3.Controls.Add(this.radioButtonDisplayHex);
            this.groupBox3.Controls.Add(this.checkBoxAutoClearReceived);
            this.groupBox3.Controls.Add(this.buttonClearReceived);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(552, 192);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "接收数据";
            // 
            // dataShowReceived
            // 
            this.dataShowReceived.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataShowReceived.Location = new System.Drawing.Point(120, 8);
            this.dataShowReceived.Name = "dataShowReceived";
            this.dataShowReceived.Size = new System.Drawing.Size(432, 183);
            this.dataShowReceived.TabIndex = 12;
            // 
            // radioButtonDisplayAscii
            // 
            this.radioButtonDisplayAscii.AutoSize = true;
            this.radioButtonDisplayAscii.Checked = true;
            this.radioButtonDisplayAscii.Location = new System.Drawing.Point(8, 32);
            this.radioButtonDisplayAscii.Name = "radioButtonDisplayAscii";
            this.radioButtonDisplayAscii.Size = new System.Drawing.Size(89, 16);
            this.radioButtonDisplayAscii.TabIndex = 11;
            this.radioButtonDisplayAscii.TabStop = true;
            this.radioButtonDisplayAscii.Text = "按ACCII显示";
            this.radioButtonDisplayAscii.UseVisualStyleBackColor = true;
            this.radioButtonDisplayAscii.CheckedChanged += new System.EventHandler(this.receivedDataDisplay_CheckedChanged);
            // 
            // radioButtonDisplayHex
            // 
            this.radioButtonDisplayHex.AutoSize = true;
            this.radioButtonDisplayHex.Location = new System.Drawing.Point(8, 16);
            this.radioButtonDisplayHex.Name = "radioButtonDisplayHex";
            this.radioButtonDisplayHex.Size = new System.Drawing.Size(95, 16);
            this.radioButtonDisplayHex.TabIndex = 10;
            this.radioButtonDisplayHex.Text = "按16进制显示";
            this.radioButtonDisplayHex.UseVisualStyleBackColor = true;
            this.radioButtonDisplayHex.CheckedChanged += new System.EventHandler(this.receivedDataDisplay_CheckedChanged);
            // 
            // checkBoxAutoClearReceived
            // 
            this.checkBoxAutoClearReceived.AutoSize = true;
            this.checkBoxAutoClearReceived.Checked = true;
            this.checkBoxAutoClearReceived.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoClearReceived.Location = new System.Drawing.Point(8, 56);
            this.checkBoxAutoClearReceived.Name = "checkBoxAutoClearReceived";
            this.checkBoxAutoClearReceived.Size = new System.Drawing.Size(108, 16);
            this.checkBoxAutoClearReceived.TabIndex = 9;
            this.checkBoxAutoClearReceived.Text = "自动清除接收区";
            this.checkBoxAutoClearReceived.UseVisualStyleBackColor = true;
            // 
            // buttonClearReceived
            // 
            this.buttonClearReceived.Location = new System.Drawing.Point(8, 72);
            this.buttonClearReceived.Name = "buttonClearReceived";
            this.buttonClearReceived.Size = new System.Drawing.Size(88, 23);
            this.buttonClearReceived.TabIndex = 8;
            this.buttonClearReceived.Text = "清除接收区";
            this.buttonClearReceived.UseVisualStyleBackColor = true;
            this.buttonClearReceived.Click += new System.EventHandler(this.buttonClearReceived_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataShowSended);
            this.groupBox2.Controls.Add(this.buttonClearSended);
            this.groupBox2.Controls.Add(this.buttonSend);
            this.groupBox2.Controls.Add(this.checkBoxAutoClearSended);
            this.groupBox2.Controls.Add(this.buttonSendOption);
            this.groupBox2.Controls.Add(this.textBoxSend);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBoxAutoSendPeriod);
            this.groupBox2.Controls.Add(this.radioButtonAsAscii);
            this.groupBox2.Controls.Add(this.radioButtonAsHex);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(552, 220);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "发送数据";
            // 
            // dataShowSended
            // 
            this.dataShowSended.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataShowSended.Location = new System.Drawing.Point(120, 8);
            this.dataShowSended.Name = "dataShowSended";
            this.dataShowSended.Size = new System.Drawing.Size(432, 183);
            this.dataShowSended.TabIndex = 13;
            // 
            // buttonClearSended
            // 
            this.buttonClearSended.Location = new System.Drawing.Point(8, 72);
            this.buttonClearSended.Name = "buttonClearSended";
            this.buttonClearSended.Size = new System.Drawing.Size(88, 23);
            this.buttonClearSended.TabIndex = 12;
            this.buttonClearSended.Text = "清除发送区";
            this.buttonClearSended.UseVisualStyleBackColor = true;
            this.buttonClearSended.Click += new System.EventHandler(this.buttonClearSended_Click);
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Enabled = false;
            this.buttonSend.Location = new System.Drawing.Point(424, 193);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(104, 23);
            this.buttonSend.TabIndex = 11;
            this.buttonSend.Text = "发送ASCII";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // checkBoxAutoClearSended
            // 
            this.checkBoxAutoClearSended.AutoSize = true;
            this.checkBoxAutoClearSended.Checked = true;
            this.checkBoxAutoClearSended.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoClearSended.Location = new System.Drawing.Point(8, 56);
            this.checkBoxAutoClearSended.Name = "checkBoxAutoClearSended";
            this.checkBoxAutoClearSended.Size = new System.Drawing.Size(108, 16);
            this.checkBoxAutoClearSended.TabIndex = 10;
            this.checkBoxAutoClearSended.Text = "自动清除发送区";
            this.checkBoxAutoClearSended.UseVisualStyleBackColor = true;
            // 
            // buttonSendOption
            // 
            this.buttonSendOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendOption.ContextMenuStrip = this.contextMenu;
            this.buttonSendOption.Enabled = false;
            this.buttonSendOption.Image = global::Vultrue.Communication.Properties.Resources.Option;
            this.buttonSendOption.Location = new System.Drawing.Point(528, 193);
            this.buttonSendOption.Name = "buttonSendOption";
            this.buttonSendOption.Size = new System.Drawing.Size(24, 23);
            this.buttonSendOption.TabIndex = 7;
            this.buttonSendOption.UseVisualStyleBackColor = true;
            this.buttonSendOption.Click += new System.EventHandler(this.buttonSendOption_Click);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSendBinary,
            this.menuItemSendAscii,
            this.menuItemAutoSendBinary,
            this.menuItemAutoSendAscii,
            this.menuItemStop});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(163, 114);
            // 
            // menuItemSendBinary
            // 
            this.menuItemSendBinary.Name = "menuItemSendBinary";
            this.menuItemSendBinary.Size = new System.Drawing.Size(162, 22);
            this.menuItemSendBinary.Text = "发送16进制";
            this.menuItemSendBinary.Click += new System.EventHandler(this.menuItemSendBinary_Click);
            // 
            // menuItemSendAscii
            // 
            this.menuItemSendAscii.Name = "menuItemSendAscii";
            this.menuItemSendAscii.Size = new System.Drawing.Size(162, 22);
            this.menuItemSendAscii.Text = "发送ASCII";
            this.menuItemSendAscii.Click += new System.EventHandler(this.menuItemSendAscii_Click);
            // 
            // menuItemAutoSendBinary
            // 
            this.menuItemAutoSendBinary.Name = "menuItemAutoSendBinary";
            this.menuItemAutoSendBinary.Size = new System.Drawing.Size(162, 22);
            this.menuItemAutoSendBinary.Text = "周期发送16进制";
            this.menuItemAutoSendBinary.Click += new System.EventHandler(this.menuItemAutoSendBinary_Click);
            // 
            // menuItemAutoSendAscii
            // 
            this.menuItemAutoSendAscii.Name = "menuItemAutoSendAscii";
            this.menuItemAutoSendAscii.Size = new System.Drawing.Size(162, 22);
            this.menuItemAutoSendAscii.Text = "周期发送ASCII";
            this.menuItemAutoSendAscii.Click += new System.EventHandler(this.menuItemAutoSendAscii_Click);
            // 
            // menuItemStop
            // 
            this.menuItemStop.Name = "menuItemStop";
            this.menuItemStop.Size = new System.Drawing.Size(162, 22);
            this.menuItemStop.Text = "停止周期发送";
            this.menuItemStop.Click += new System.EventHandler(this.menuItemStop_Click);
            // 
            // textBoxSend
            // 
            this.textBoxSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSend.Location = new System.Drawing.Point(8, 194);
            this.textBoxSend.Name = "textBoxSend";
            this.textBoxSend.Size = new System.Drawing.Size(416, 21);
            this.textBoxSend.TabIndex = 6;
            this.textBoxSend.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxSend_KeyPress);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 152);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "发送周期(ms)";
            // 
            // textBoxAutoSendPeriod
            // 
            this.textBoxAutoSendPeriod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxAutoSendPeriod.Location = new System.Drawing.Point(8, 168);
            this.textBoxAutoSendPeriod.Name = "textBoxAutoSendPeriod";
            this.textBoxAutoSendPeriod.Size = new System.Drawing.Size(100, 21);
            this.textBoxAutoSendPeriod.TabIndex = 3;
            // 
            // radioButtonAsAscii
            // 
            this.radioButtonAsAscii.AutoSize = true;
            this.radioButtonAsAscii.Checked = true;
            this.radioButtonAsAscii.Location = new System.Drawing.Point(8, 32);
            this.radioButtonAsAscii.Name = "radioButtonAsAscii";
            this.radioButtonAsAscii.Size = new System.Drawing.Size(89, 16);
            this.radioButtonAsAscii.TabIndex = 1;
            this.radioButtonAsAscii.TabStop = true;
            this.radioButtonAsAscii.Text = "按ASCII显示";
            this.radioButtonAsAscii.UseVisualStyleBackColor = true;
            this.radioButtonAsAscii.CheckedChanged += new System.EventHandler(this.sendedDataDisplay_CheckedChanged);
            // 
            // radioButtonAsHex
            // 
            this.radioButtonAsHex.AutoSize = true;
            this.radioButtonAsHex.Location = new System.Drawing.Point(8, 16);
            this.radioButtonAsHex.Name = "radioButtonAsHex";
            this.radioButtonAsHex.Size = new System.Drawing.Size(95, 16);
            this.radioButtonAsHex.TabIndex = 0;
            this.radioButtonAsHex.Text = "按16进制显示";
            this.radioButtonAsHex.UseVisualStyleBackColor = true;
            this.radioButtonAsHex.CheckedChanged += new System.EventHandler(this.sendedDataDisplay_CheckedChanged);
            // 
            // timer
            // 
            this.timer.SynchronizingObject = this;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timerAutoSend_Elapsed);
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStop.Image = global::Vultrue.Communication.Properties.Resources.Start;
            this.buttonStartStop.Location = new System.Drawing.Point(528, 0);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new System.Drawing.Size(24, 23);
            this.buttonStartStop.TabIndex = 23;
            this.buttonStartStop.UseVisualStyleBackColor = true;
            this.buttonStartStop.Click += new System.EventHandler(this.buttonStartStop_Click);
            // 
            // buttonProperty
            // 
            this.buttonProperty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProperty.Image = global::Vultrue.Communication.Properties.Resources.Property;
            this.buttonProperty.Location = new System.Drawing.Point(504, 0);
            this.buttonProperty.Name = "buttonProperty";
            this.buttonProperty.Size = new System.Drawing.Size(24, 23);
            this.buttonProperty.TabIndex = 22;
            this.buttonProperty.UseVisualStyleBackColor = true;
            this.buttonProperty.Click += new System.EventHandler(this.buttonProperty_Click);
            // 
            // serialPort
            // 
            this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);
            // 
            // SerialPortDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 440);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.buttonStartStop);
            this.Controls.Add(this.buttonProperty);
            this.Name = "SerialPortDebugger";
            this.Text = "串口调试器";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.contextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button buttonStartStop;
        private System.Windows.Forms.Button buttonProperty;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.GroupBox groupBox3;
        private DataShow dataShowReceived;
        private System.Windows.Forms.RadioButton radioButtonDisplayAscii;
        private System.Windows.Forms.RadioButton radioButtonDisplayHex;
        private System.Windows.Forms.CheckBox checkBoxAutoClearReceived;
        private System.Windows.Forms.Button buttonClearReceived;
        private System.Windows.Forms.GroupBox groupBox2;
        private DataShow dataShowSended;
        private System.Windows.Forms.Button buttonClearSended;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.CheckBox checkBoxAutoClearSended;
        private System.Windows.Forms.Button buttonSendOption;
        private System.Windows.Forms.TextBox textBoxSend;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxAutoSendPeriod;
        private System.Windows.Forms.RadioButton radioButtonAsAscii;
        private System.Windows.Forms.RadioButton radioButtonAsHex;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemSendBinary;
        private System.Windows.Forms.ToolStripMenuItem menuItemSendAscii;
        private System.Windows.Forms.ToolStripMenuItem menuItemAutoSendBinary;
        private System.Windows.Forms.ToolStripMenuItem menuItemAutoSendAscii;
        private System.Windows.Forms.ToolStripMenuItem menuItemStop;
        private System.Timers.Timer timer;
        private System.IO.Ports.SerialPort serialPort;





    }
}

