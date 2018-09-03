namespace Vultrue.Communication {
	partial class DlgConnectionDebugger {
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
            this.menuItemmenuItemDisconn = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonConnSerial = new System.Windows.Forms.Button();
            this.buttonConnectServer = new System.Windows.Forms.Button();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.buttonListenStop = new System.Windows.Forms.Button();
            this.comboBoxSocketConnected = new System.Windows.Forms.ComboBox();
            this.tcpServerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.buttonListenStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxListenPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.gridConnection = new System.Windows.Forms.DataGridView();
            this.connectionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.container = new System.Windows.Forms.SplitContainer();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.connectionManager = new Vultrue.Communication.ConnectionManager(this.components);
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Connection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tcpServerBindingSource)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridConnection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectionBindingSource)).BeginInit();
            this.container.Panel1.SuspendLayout();
            this.container.Panel2.SuspendLayout();
            this.container.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuItemmenuItemDisconn
            // 
            this.menuItemmenuItemDisconn.Name = "menuItemmenuItemDisconn";
            this.menuItemmenuItemDisconn.Size = new System.Drawing.Size(141, 22);
            this.menuItemmenuItemDisconn.Text = "断开连接(&D)";
            this.menuItemmenuItemDisconn.Click += new System.EventHandler(this.menuItemDisconn_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemmenuItemDisconn});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(142, 26);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonConnSerial);
            this.groupBox1.Controls.Add(this.buttonConnectServer);
            this.groupBox1.Controls.Add(this.textBoxPort);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxServer);
            this.groupBox1.Controls.Add(this.buttonListenStop);
            this.groupBox1.Controls.Add(this.comboBoxSocketConnected);
            this.groupBox1.Controls.Add(this.buttonListenStart);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxListenPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(369, 120);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "连接设置";
            // 
            // buttonConnSerial
            // 
            this.buttonConnSerial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnSerial.Location = new System.Drawing.Point(280, 88);
            this.buttonConnSerial.Name = "buttonConnSerial";
            this.buttonConnSerial.Size = new System.Drawing.Size(80, 23);
            this.buttonConnSerial.TabIndex = 11;
            this.buttonConnSerial.Text = "连接到串口";
            this.buttonConnSerial.UseVisualStyleBackColor = true;
            this.buttonConnSerial.Click += new System.EventHandler(this.buttonConnSerial_Click);
            // 
            // buttonConnectServer
            // 
            this.buttonConnectServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnectServer.Location = new System.Drawing.Point(280, 64);
            this.buttonConnectServer.Name = "buttonConnectServer";
            this.buttonConnectServer.Size = new System.Drawing.Size(80, 23);
            this.buttonConnectServer.TabIndex = 10;
            this.buttonConnectServer.Text = "连接";
            this.buttonConnectServer.UseVisualStyleBackColor = true;
            this.buttonConnectServer.Click += new System.EventHandler(this.buttonConnectServer_Click);
            // 
            // textBoxPort
            // 
            this.textBoxPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPort.Location = new System.Drawing.Point(216, 64);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(56, 21);
            this.textBoxPort.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(176, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "端口:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "服务器:";
            // 
            // textBoxServer
            // 
            this.textBoxServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServer.Location = new System.Drawing.Point(64, 64);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(96, 21);
            this.textBoxServer.TabIndex = 3;
            // 
            // buttonListenStop
            // 
            this.buttonListenStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonListenStop.Location = new System.Drawing.Point(280, 40);
            this.buttonListenStop.Name = "buttonListenStop";
            this.buttonListenStop.Size = new System.Drawing.Size(80, 23);
            this.buttonListenStop.TabIndex = 5;
            this.buttonListenStop.Text = "停止侦听";
            this.buttonListenStop.UseVisualStyleBackColor = true;
            this.buttonListenStop.Click += new System.EventHandler(this.buttonListenStop_Click);
            // 
            // comboBoxSocketConnected
            // 
            this.comboBoxSocketConnected.DataSource = this.tcpServerBindingSource;
            this.comboBoxSocketConnected.DisplayMember = "Port";
            this.comboBoxSocketConnected.FormattingEnabled = true;
            this.comboBoxSocketConnected.Location = new System.Drawing.Point(112, 40);
            this.comboBoxSocketConnected.Name = "comboBoxSocketConnected";
            this.comboBoxSocketConnected.Size = new System.Drawing.Size(160, 20);
            this.comboBoxSocketConnected.TabIndex = 4;
            this.comboBoxSocketConnected.ValueMember = "Port";
            // 
            // tcpServerBindingSource
            // 
            this.tcpServerBindingSource.DataSource = typeof(Vultrue.Communication.TcpLessoner);
            // 
            // buttonListenStart
            // 
            this.buttonListenStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonListenStart.Location = new System.Drawing.Point(280, 16);
            this.buttonListenStart.Name = "buttonListenStart";
            this.buttonListenStart.Size = new System.Drawing.Size(81, 23);
            this.buttonListenStart.TabIndex = 3;
            this.buttonListenStart.Text = "开始侦听";
            this.buttonListenStart.UseVisualStyleBackColor = true;
            this.buttonListenStart.Click += new System.EventHandler(this.buttonListenStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "端口号:";
            // 
            // textBoxListenPort
            // 
            this.textBoxListenPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxListenPort.Location = new System.Drawing.Point(64, 16);
            this.textBoxListenPort.Name = "textBoxListenPort";
            this.textBoxListenPort.Size = new System.Drawing.Size(208, 21);
            this.textBoxListenPort.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "已侦听的端口号:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.gridConnection);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 120);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(369, 357);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "连接信息";
            // 
            // gridConnection
            // 
            this.gridConnection.AllowUserToAddRows = false;
            this.gridConnection.AllowUserToDeleteRows = false;
            this.gridConnection.AllowUserToResizeColumns = false;
            this.gridConnection.AllowUserToResizeRows = false;
            this.gridConnection.AutoGenerateColumns = false;
            this.gridConnection.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridConnection.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.gridConnection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridConnection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Connection});
            this.gridConnection.ContextMenuStrip = this.contextMenuStrip;
            this.gridConnection.DataSource = this.connectionBindingSource;
            this.gridConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridConnection.Location = new System.Drawing.Point(3, 17);
            this.gridConnection.MultiSelect = false;
            this.gridConnection.Name = "gridConnection";
            this.gridConnection.ReadOnly = true;
            this.gridConnection.RowTemplate.Height = 23;
            this.gridConnection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridConnection.Size = new System.Drawing.Size(363, 337);
            this.gridConnection.TabIndex = 2;
            this.gridConnection.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridConnection_RowEnter);
            // 
            // connectionBindingSource
            // 
            this.connectionBindingSource.DataSource = typeof(Vultrue.Communication.IConnection);
            // 
            // container
            // 
            this.container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.container.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.container.Location = new System.Drawing.Point(0, 0);
            this.container.Name = "container";
            // 
            // container.Panel1
            // 
            this.container.Panel1.Controls.Add(this.groupBox2);
            this.container.Panel1.Controls.Add(this.groupBox1);
            // 
            // container.Panel2
            // 
            this.container.Panel2.Controls.Add(this.groupBoxConnection);
            this.container.Size = new System.Drawing.Size(792, 477);
            this.container.SplitterDistance = 369;
            this.container.TabIndex = 2;
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxConnection.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(419, 477);
            this.groupBoxConnection.TabIndex = 0;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "连接监控器";
            // 
            // connectionManager
            // 
            this.connectionManager.ConnectionBuilded += new System.EventHandler<Vultrue.Communication.ConnectionBuildedEventArgs>(this.connectionManager_ConnectionBuilded);
            // 
            // Address
            // 
            this.Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Address.DataPropertyName = "Address";
            this.Address.HeaderText = "地址";
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            // 
            // Connection
            // 
            this.Connection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Connection.DataPropertyName = "Port";
            this.Connection.HeaderText = "端口";
            this.Connection.Name = "Connection";
            this.Connection.ReadOnly = true;
            this.Connection.Width = 54;
            // 
            // DlgConnectionDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 477);
            this.Controls.Add(this.container);
            this.Name = "DlgConnectionDebugger";
            this.Text = "连接调试器";
            this.contextMenuStrip.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tcpServerBindingSource)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridConnection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectionBindingSource)).EndInit();
            this.container.Panel1.ResumeLayout(false);
            this.container.Panel2.ResumeLayout(false);
            this.container.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripMenuItem menuItemmenuItemDisconn;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonListenStop;
		private System.Windows.Forms.ComboBox comboBoxSocketConnected;
		private System.Windows.Forms.Button buttonListenStart;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxListenPort;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.DataGridView gridConnection;
		private System.Windows.Forms.SplitContainer container;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxPort;
		private System.Windows.Forms.TextBox textBoxServer;
		private System.Windows.Forms.Button buttonConnectServer;
		private System.Windows.Forms.Label label4;
        private System.Windows.Forms.BindingSource connectionBindingSource;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonConnSerial;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.BindingSource tcpServerBindingSource;
        private ConnectionManager connectionManager;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewTextBoxColumn Connection;

	}
}