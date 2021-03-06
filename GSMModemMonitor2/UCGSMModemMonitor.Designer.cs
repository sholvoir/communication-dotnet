﻿namespace Vultrue.Communication {
	partial class UCGSMModemMonitor {
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
            Modem = null;
			base.Dispose(disposing);
		}

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.textBoxCmd = new System.Windows.Forms.TextBox();
            this.checkBoxAutoClear = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxMobileNum = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxNoteletContent = new System.Windows.Forms.TextBox();
            this.contextMenuStripSend = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemSendSMSText = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSendSMSPDU = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSendSMS = new System.Windows.Forms.Button();
            this.buttonSendOption = new System.Windows.Forms.Button();
            this.buttonReadSMS = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonSendDirect = new System.Windows.Forms.Button();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.buttonProperty = new System.Windows.Forms.Button();
            this.dataShow = new Vultrue.Generic.DataShow();
            this.contextMenuStripSend.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxCmd
            // 
            this.textBoxCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCmd.Location = new System.Drawing.Point(176, 0);
            this.textBoxCmd.Name = "textBoxCmd";
            this.textBoxCmd.Size = new System.Drawing.Size(448, 21);
            this.textBoxCmd.TabIndex = 22;
            this.textBoxCmd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxCmd_KeyPress);
            // 
            // checkBoxAutoClear
            // 
            this.checkBoxAutoClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoClear.AutoSize = true;
            this.checkBoxAutoClear.Checked = true;
            this.checkBoxAutoClear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoClear.Location = new System.Drawing.Point(728, 3);
            this.checkBoxAutoClear.Name = "checkBoxAutoClear";
            this.checkBoxAutoClear.Size = new System.Drawing.Size(72, 16);
            this.checkBoxAutoClear.TabIndex = 24;
            this.checkBoxAutoClear.Text = "自动清除";
            this.checkBoxAutoClear.UseVisualStyleBackColor = true;
            this.checkBoxAutoClear.CheckedChanged += new System.EventHandler(this.checkBoxAutoClear_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(82, 462);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 28;
            this.label1.Text = "目标号码";
            // 
            // textBoxMobileNum
            // 
            this.textBoxMobileNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxMobileNum.Location = new System.Drawing.Point(138, 457);
            this.textBoxMobileNum.Name = "textBoxMobileNum";
            this.textBoxMobileNum.Size = new System.Drawing.Size(104, 21);
            this.textBoxMobileNum.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(248, 464);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 30;
            this.label2.Text = "短信内容";
            // 
            // textBoxNoteletContent
            // 
            this.textBoxNoteletContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNoteletContent.Location = new System.Drawing.Point(304, 457);
            this.textBoxNoteletContent.Name = "textBoxNoteletContent";
            this.textBoxNoteletContent.Size = new System.Drawing.Size(368, 21);
            this.textBoxNoteletContent.TabIndex = 31;
            this.textBoxNoteletContent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxNoteletContent_KeyPress);
            // 
            // contextMenuStripSend
            // 
            this.contextMenuStripSend.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSendSMSText,
            this.menuItemSendSMSPDU});
            this.contextMenuStripSend.Name = "contextMenuStrip";
            this.contextMenuStripSend.Size = new System.Drawing.Size(150, 48);
            // 
            // menuItemSendSMSText
            // 
            this.menuItemSendSMSText.Name = "menuItemSendSMSText";
            this.menuItemSendSMSText.Size = new System.Drawing.Size(149, 22);
            this.menuItemSendSMSText.Text = "发送文本短信";
            this.menuItemSendSMSText.Click += new System.EventHandler(this.menuItemSendSMSText_Click);
            // 
            // menuItemSendSMSPDU
            // 
            this.menuItemSendSMSPDU.Name = "menuItemSendSMSPDU";
            this.menuItemSendSMSPDU.Size = new System.Drawing.Size(149, 22);
            this.menuItemSendSMSPDU.Text = "发送PDU短信";
            this.menuItemSendSMSPDU.Click += new System.EventHandler(this.menuItemSendSMSPDU_Click);
            // 
            // buttonSendSMS
            // 
            this.buttonSendSMS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendSMS.Image = global::Vultrue.Communication.Properties.Resources.notesend;
            this.buttonSendSMS.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendSMS.Location = new System.Drawing.Point(672, 456);
            this.buttonSendSMS.Name = "buttonSendSMS";
            this.buttonSendSMS.Size = new System.Drawing.Size(112, 23);
            this.buttonSendSMS.TabIndex = 32;
            this.buttonSendSMS.Text = "发送文本短信";
            this.buttonSendSMS.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSendSMS.UseVisualStyleBackColor = true;
            this.buttonSendSMS.Click += new System.EventHandler(this.buttonSendSMS_Click);
            // 
            // buttonSendOption
            // 
            this.buttonSendOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendOption.ContextMenuStrip = this.contextMenuStripSend;
            this.buttonSendOption.Image = global::Vultrue.Communication.Properties.Resources.option;
            this.buttonSendOption.Location = new System.Drawing.Point(784, 456);
            this.buttonSendOption.Name = "buttonSendOption";
            this.buttonSendOption.Size = new System.Drawing.Size(16, 23);
            this.buttonSendOption.TabIndex = 33;
            this.buttonSendOption.UseVisualStyleBackColor = true;
            this.buttonSendOption.Click += new System.EventHandler(this.buttonSendOption_Click);
            // 
            // buttonReadSMS
            // 
            this.buttonReadSMS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReadSMS.Image = global::Vultrue.Communication.Properties.Resources.noteread;
            this.buttonReadSMS.Location = new System.Drawing.Point(2, 456);
            this.buttonReadSMS.Name = "buttonReadSMS";
            this.buttonReadSMS.Size = new System.Drawing.Size(72, 23);
            this.buttonReadSMS.TabIndex = 27;
            this.buttonReadSMS.Text = "读短信";
            this.buttonReadSMS.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonReadSMS.UseVisualStyleBackColor = true;
            this.buttonReadSMS.Click += new System.EventHandler(this.buttonReadSMS_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Image = global::Vultrue.Communication.Properties.Resources.stop;
            this.buttonClose.Location = new System.Drawing.Point(112, 0);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(56, 23);
            this.buttonClose.TabIndex = 19;
            this.buttonClose.Text = "关闭";
            this.buttonClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonSendDirect
            // 
            this.buttonSendDirect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendDirect.Image = global::Vultrue.Communication.Properties.Resources.sendserial;
            this.buttonSendDirect.Location = new System.Drawing.Point(624, 0);
            this.buttonSendDirect.Name = "buttonSendDirect";
            this.buttonSendDirect.Size = new System.Drawing.Size(96, 23);
            this.buttonSendDirect.TabIndex = 23;
            this.buttonSendDirect.Text = "发送到串口";
            this.buttonSendDirect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSendDirect.UseVisualStyleBackColor = true;
            this.buttonSendDirect.Click += new System.EventHandler(this.buttonSendDirect_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.Image = global::Vultrue.Communication.Properties.Resources.start;
            this.buttonOpen.Location = new System.Drawing.Point(56, 0);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(56, 23);
            this.buttonOpen.TabIndex = 18;
            this.buttonOpen.Text = "打开";
            this.buttonOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonProperty
            // 
            this.buttonProperty.Image = global::Vultrue.Communication.Properties.Resources.property;
            this.buttonProperty.Location = new System.Drawing.Point(0, 0);
            this.buttonProperty.Name = "buttonProperty";
            this.buttonProperty.Size = new System.Drawing.Size(56, 23);
            this.buttonProperty.TabIndex = 17;
            this.buttonProperty.Text = "属性";
            this.buttonProperty.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonProperty.UseVisualStyleBackColor = true;
            this.buttonProperty.Click += new System.EventHandler(this.buttonProperty_Click);
            // 
            // dataShow
            // 
            this.dataShow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataShow.AutoClear = true;
            this.dataShow.Location = new System.Drawing.Point(0, 24);
            this.dataShow.Name = "dataShow";
            this.dataShow.Size = new System.Drawing.Size(800, 432);
            this.dataShow.TabIndex = 16;
            // 
            // GSMModemMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSendSMS);
            this.Controls.Add(this.buttonSendOption);
            this.Controls.Add(this.dataShow);
            this.Controls.Add(this.textBoxMobileNum);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonReadSMS);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.buttonProperty);
            this.Controls.Add(this.buttonSendDirect);
            this.Controls.Add(this.checkBoxAutoClear);
            this.Controls.Add(this.textBoxCmd);
            this.Controls.Add(this.textBoxNoteletContent);
            this.Name = "GSMModemMonitor";
            this.Size = new System.Drawing.Size(800, 480);
            this.Load += new System.EventHandler(this.GSMModemMonitor_Load);
            this.contextMenuStripSend.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private Vultrue.Generic.DataShow dataShow;
        private System.Windows.Forms.Button buttonProperty;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBoxCmd;
        private System.Windows.Forms.Button buttonSendDirect;
        private System.Windows.Forms.CheckBox checkBoxAutoClear;
        private System.Windows.Forms.Button buttonReadSMS;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMobileNum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxNoteletContent;
        private System.Windows.Forms.Button buttonSendSMS;
        private System.Windows.Forms.Button buttonSendOption;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSend;
        private System.Windows.Forms.ToolStripMenuItem menuItemSendSMSText;
        private System.Windows.Forms.ToolStripMenuItem menuItemSendSMSPDU;
	}
}
