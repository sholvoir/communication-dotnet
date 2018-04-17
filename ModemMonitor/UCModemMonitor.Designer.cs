namespace Vultrue.Communication {
	partial class UCModemMonitor {
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
            this.textBoxCmd = new System.Windows.Forms.TextBox();
            this.checkBoxAutoClear = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxMobileNum = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMessageContent = new System.Windows.Forms.TextBox();
            this.buttonSendTextMessage = new System.Windows.Forms.Button();
            this.buttonReadMessage = new System.Windows.Forms.Button();
            this.buttonSendDirect = new System.Windows.Forms.Button();
            this.buttonOpenClose = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.textBoxPhonebookEntriesIndex = new System.Windows.Forms.TextBox();
            this.buttonAT = new System.Windows.Forms.Button();
            this.buttonListMessage = new System.Windows.Forms.Button();
            this.textBoxMessageIndex = new System.Windows.Forms.TextBox();
            this.comboBoxMessageState = new System.Windows.Forms.ComboBox();
            this.buttonSendPduText = new System.Windows.Forms.Button();
            this.buttonSendPduData = new System.Windows.Forms.Button();
            this.buttonSetMessageMode = new System.Windows.Forms.Button();
            this.comboBoxMessageMode = new System.Windows.Forms.ComboBox();
            this.comboBoxMessageACK = new System.Windows.Forms.ComboBox();
            this.comboBoxMessagePriority = new System.Windows.Forms.ComboBox();
            this.comboBoxMessageFormat = new System.Windows.Forms.ComboBox();
            this.comboBoxMessagePreserve = new System.Windows.Forms.ComboBox();
            this.buttonSetMessageParameter = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dataShow = new Vultrue.Communication.DataShow();
            this.comboBoxMessageEncoding = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxCmd
            // 
            this.textBoxCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxCmd.Location = new System.Drawing.Point(0, 384);
            this.textBoxCmd.Name = "textBoxCmd";
            this.textBoxCmd.Size = new System.Drawing.Size(200, 21);
            this.textBoxCmd.TabIndex = 22;
            this.textBoxCmd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxCmd_KeyPress);
            // 
            // checkBoxAutoClear
            // 
            this.checkBoxAutoClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoClear.AutoSize = true;
            this.checkBoxAutoClear.Checked = true;
            this.checkBoxAutoClear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoClear.Location = new System.Drawing.Point(728, 4);
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
            this.label1.Location = new System.Drawing.Point(192, 416);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 28;
            this.label1.Text = "目标号码";
            // 
            // textBoxMobileNum
            // 
            this.textBoxMobileNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxMobileNum.Location = new System.Drawing.Point(248, 408);
            this.textBoxMobileNum.Name = "textBoxMobileNum";
            this.textBoxMobileNum.Size = new System.Drawing.Size(144, 21);
            this.textBoxMobileNum.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 440);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 30;
            this.label2.Text = "短信内容";
            // 
            // textBoxMessageContent
            // 
            this.textBoxMessageContent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxMessageContent.Location = new System.Drawing.Point(248, 432);
            this.textBoxMessageContent.Name = "textBoxMessageContent";
            this.textBoxMessageContent.Size = new System.Drawing.Size(144, 21);
            this.textBoxMessageContent.TabIndex = 31;
            // 
            // buttonSendTextMessage
            // 
            this.buttonSendTextMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSendTextMessage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendTextMessage.Location = new System.Drawing.Point(392, 408);
            this.buttonSendTextMessage.Name = "buttonSendTextMessage";
            this.buttonSendTextMessage.Size = new System.Drawing.Size(152, 23);
            this.buttonSendTextMessage.TabIndex = 32;
            this.buttonSendTextMessage.Text = "用文本模式发送文本短信";
            this.buttonSendTextMessage.UseVisualStyleBackColor = true;
            this.buttonSendTextMessage.Click += new System.EventHandler(this.buttonSendTextMessage_Click);
            // 
            // buttonReadMessage
            // 
            this.buttonReadMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReadMessage.Location = new System.Drawing.Point(0, 456);
            this.buttonReadMessage.Name = "buttonReadMessage";
            this.buttonReadMessage.Size = new System.Drawing.Size(96, 23);
            this.buttonReadMessage.TabIndex = 27;
            this.buttonReadMessage.Text = "读短信";
            this.buttonReadMessage.UseVisualStyleBackColor = true;
            this.buttonReadMessage.Click += new System.EventHandler(this.buttonReadMessage_Click);
            // 
            // buttonSendDirect
            // 
            this.buttonSendDirect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSendDirect.Location = new System.Drawing.Point(200, 384);
            this.buttonSendDirect.Name = "buttonSendDirect";
            this.buttonSendDirect.Size = new System.Drawing.Size(96, 23);
            this.buttonSendDirect.TabIndex = 23;
            this.buttonSendDirect.Text = "发送到串口";
            this.buttonSendDirect.UseVisualStyleBackColor = true;
            this.buttonSendDirect.Click += new System.EventHandler(this.buttonSendDirect_Click);
            // 
            // buttonOpenClose
            // 
            this.buttonOpenClose.Image = global::Vultrue.Communication.Properties.Resources.start;
            this.buttonOpenClose.Location = new System.Drawing.Point(0, 0);
            this.buttonOpenClose.Name = "buttonOpenClose";
            this.buttonOpenClose.Size = new System.Drawing.Size(80, 23);
            this.buttonOpenClose.TabIndex = 18;
            this.buttonOpenClose.Text = "打开";
            this.buttonOpenClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonOpenClose.UseVisualStyleBackColor = true;
            this.buttonOpenClose.Click += new System.EventHandler(this.buttonOpenClose_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTest.Location = new System.Drawing.Point(360, 384);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(96, 23);
            this.buttonTest.TabIndex = 34;
            this.buttonTest.Text = "读取电话本";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // textBoxPhonebookEntriesIndex
            // 
            this.textBoxPhonebookEntriesIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPhonebookEntriesIndex.Location = new System.Drawing.Point(456, 384);
            this.textBoxPhonebookEntriesIndex.Name = "textBoxPhonebookEntriesIndex";
            this.textBoxPhonebookEntriesIndex.Size = new System.Drawing.Size(88, 21);
            this.textBoxPhonebookEntriesIndex.TabIndex = 35;
            // 
            // buttonAT
            // 
            this.buttonAT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAT.Location = new System.Drawing.Point(304, 384);
            this.buttonAT.Name = "buttonAT";
            this.buttonAT.Size = new System.Drawing.Size(48, 23);
            this.buttonAT.TabIndex = 36;
            this.buttonAT.Text = "AT";
            this.buttonAT.UseVisualStyleBackColor = true;
            this.buttonAT.Click += new System.EventHandler(this.buttonAT_Click);
            // 
            // buttonListMessage
            // 
            this.buttonListMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonListMessage.Location = new System.Drawing.Point(0, 432);
            this.buttonListMessage.Name = "buttonListMessage";
            this.buttonListMessage.Size = new System.Drawing.Size(96, 23);
            this.buttonListMessage.TabIndex = 37;
            this.buttonListMessage.Text = "列出短信";
            this.buttonListMessage.UseVisualStyleBackColor = true;
            this.buttonListMessage.Click += new System.EventHandler(this.buttonListMessage_Click);
            // 
            // textBoxMessageIndex
            // 
            this.textBoxMessageIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxMessageIndex.Location = new System.Drawing.Point(96, 456);
            this.textBoxMessageIndex.Name = "textBoxMessageIndex";
            this.textBoxMessageIndex.Size = new System.Drawing.Size(88, 21);
            this.textBoxMessageIndex.TabIndex = 38;
            // 
            // comboBoxMessageState
            // 
            this.comboBoxMessageState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessageState.FormattingEnabled = true;
            this.comboBoxMessageState.Location = new System.Drawing.Point(96, 432);
            this.comboBoxMessageState.Name = "comboBoxMessageState";
            this.comboBoxMessageState.Size = new System.Drawing.Size(88, 20);
            this.comboBoxMessageState.TabIndex = 40;
            // 
            // buttonSendPduText
            // 
            this.buttonSendPduText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSendPduText.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendPduText.Location = new System.Drawing.Point(392, 432);
            this.buttonSendPduText.Name = "buttonSendPduText";
            this.buttonSendPduText.Size = new System.Drawing.Size(152, 23);
            this.buttonSendPduText.TabIndex = 41;
            this.buttonSendPduText.Text = "用PDU模式发送文本短信";
            this.buttonSendPduText.UseVisualStyleBackColor = true;
            this.buttonSendPduText.Click += new System.EventHandler(this.buttonSendPduText_Click);
            // 
            // buttonSendPduData
            // 
            this.buttonSendPduData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSendPduData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSendPduData.Location = new System.Drawing.Point(392, 456);
            this.buttonSendPduData.Name = "buttonSendPduData";
            this.buttonSendPduData.Size = new System.Drawing.Size(152, 23);
            this.buttonSendPduData.TabIndex = 42;
            this.buttonSendPduData.Text = "用PDU模式发送数据短信";
            this.buttonSendPduData.UseVisualStyleBackColor = true;
            this.buttonSendPduData.Click += new System.EventHandler(this.buttonSendPduData_Click);
            // 
            // buttonSetMessageMode
            // 
            this.buttonSetMessageMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSetMessageMode.Location = new System.Drawing.Point(0, 408);
            this.buttonSetMessageMode.Name = "buttonSetMessageMode";
            this.buttonSetMessageMode.Size = new System.Drawing.Size(96, 23);
            this.buttonSetMessageMode.TabIndex = 43;
            this.buttonSetMessageMode.Text = "设置短信模式";
            this.buttonSetMessageMode.UseVisualStyleBackColor = true;
            this.buttonSetMessageMode.Click += new System.EventHandler(this.buttonSetMessageMode_Click);
            // 
            // comboBoxMessageMode
            // 
            this.comboBoxMessageMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessageMode.FormattingEnabled = true;
            this.comboBoxMessageMode.Location = new System.Drawing.Point(96, 408);
            this.comboBoxMessageMode.Name = "comboBoxMessageMode";
            this.comboBoxMessageMode.Size = new System.Drawing.Size(88, 20);
            this.comboBoxMessageMode.TabIndex = 44;
            // 
            // comboBoxMessageACK
            // 
            this.comboBoxMessageACK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessageACK.FormattingEnabled = true;
            this.comboBoxMessageACK.Location = new System.Drawing.Point(608, 384);
            this.comboBoxMessageACK.Name = "comboBoxMessageACK";
            this.comboBoxMessageACK.Size = new System.Drawing.Size(104, 20);
            this.comboBoxMessageACK.TabIndex = 45;
            // 
            // comboBoxMessagePriority
            // 
            this.comboBoxMessagePriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessagePriority.FormattingEnabled = true;
            this.comboBoxMessagePriority.Location = new System.Drawing.Point(608, 408);
            this.comboBoxMessagePriority.Name = "comboBoxMessagePriority";
            this.comboBoxMessagePriority.Size = new System.Drawing.Size(104, 20);
            this.comboBoxMessagePriority.TabIndex = 46;
            // 
            // comboBoxMessageFormat
            // 
            this.comboBoxMessageFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessageFormat.FormattingEnabled = true;
            this.comboBoxMessageFormat.Location = new System.Drawing.Point(608, 432);
            this.comboBoxMessageFormat.Name = "comboBoxMessageFormat";
            this.comboBoxMessageFormat.Size = new System.Drawing.Size(104, 20);
            this.comboBoxMessageFormat.TabIndex = 47;
            // 
            // comboBoxMessagePreserve
            // 
            this.comboBoxMessagePreserve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessagePreserve.FormattingEnabled = true;
            this.comboBoxMessagePreserve.Location = new System.Drawing.Point(608, 456);
            this.comboBoxMessagePreserve.Name = "comboBoxMessagePreserve";
            this.comboBoxMessagePreserve.Size = new System.Drawing.Size(104, 20);
            this.comboBoxMessagePreserve.TabIndex = 48;
            // 
            // buttonSetMessageParameter
            // 
            this.buttonSetMessageParameter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSetMessageParameter.Location = new System.Drawing.Point(712, 456);
            this.buttonSetMessageParameter.Name = "buttonSetMessageParameter";
            this.buttonSetMessageParameter.Size = new System.Drawing.Size(88, 23);
            this.buttonSetMessageParameter.TabIndex = 49;
            this.buttonSetMessageParameter.Text = "设置短信参数";
            this.buttonSetMessageParameter.UseVisualStyleBackColor = true;
            this.buttonSetMessageParameter.Click += new System.EventHandler(this.buttonSetMessageParameter_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(552, 392);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 50;
            this.label3.Text = "要求确认";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(552, 416);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 51;
            this.label4.Text = "优先级";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(552, 440);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 52;
            this.label5.Text = "编码格式";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(552, 464);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 53;
            this.label6.Text = "保密级别";
            // 
            // dataShow
            // 
            this.dataShow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataShow.AutoClear = true;
            this.dataShow.Location = new System.Drawing.Point(0, 24);
            this.dataShow.Name = "dataShow";
            this.dataShow.Size = new System.Drawing.Size(800, 360);
            this.dataShow.TabIndex = 16;
            // 
            // comboBoxMessageEncoding
            // 
            this.comboBoxMessageEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMessageEncoding.FormattingEnabled = true;
            this.comboBoxMessageEncoding.Location = new System.Drawing.Point(248, 456);
            this.comboBoxMessageEncoding.Name = "comboBoxMessageEncoding";
            this.comboBoxMessageEncoding.Size = new System.Drawing.Size(144, 20);
            this.comboBoxMessageEncoding.TabIndex = 54;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 464);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 55;
            this.label7.Text = "短信编码";
            // 
            // UCModemMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.comboBoxMessageEncoding);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonSetMessageParameter);
            this.Controls.Add(this.comboBoxMessagePreserve);
            this.Controls.Add(this.comboBoxMessageFormat);
            this.Controls.Add(this.comboBoxMessagePriority);
            this.Controls.Add(this.comboBoxMessageACK);
            this.Controls.Add(this.comboBoxMessageMode);
            this.Controls.Add(this.buttonSetMessageMode);
            this.Controls.Add(this.buttonSendPduData);
            this.Controls.Add(this.buttonSendPduText);
            this.Controls.Add(this.comboBoxMessageState);
            this.Controls.Add(this.buttonAT);
            this.Controls.Add(this.dataShow);
            this.Controls.Add(this.textBoxMessageIndex);
            this.Controls.Add(this.textBoxMobileNum);
            this.Controls.Add(this.buttonListMessage);
            this.Controls.Add(this.textBoxPhonebookEntriesIndex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonSendTextMessage);
            this.Controls.Add(this.buttonOpenClose);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.textBoxCmd);
            this.Controls.Add(this.buttonReadMessage);
            this.Controls.Add(this.checkBoxAutoClear);
            this.Controls.Add(this.textBoxMessageContent);
            this.Controls.Add(this.buttonSendDirect);
            this.Name = "UCModemMonitor";
            this.Size = new System.Drawing.Size(800, 480);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private DataShow dataShow;
        private System.Windows.Forms.Button buttonOpenClose;
        private System.Windows.Forms.TextBox textBoxCmd;
        private System.Windows.Forms.Button buttonSendDirect;
        private System.Windows.Forms.CheckBox checkBoxAutoClear;
        private System.Windows.Forms.Button buttonReadMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMobileNum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMessageContent;
        private System.Windows.Forms.Button buttonSendTextMessage;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.TextBox textBoxPhonebookEntriesIndex;
        private System.Windows.Forms.Button buttonAT;
        private System.Windows.Forms.Button buttonListMessage;
        private System.Windows.Forms.TextBox textBoxMessageIndex;
        private System.Windows.Forms.ComboBox comboBoxMessageState;
        private System.Windows.Forms.Button buttonSendPduText;
        private System.Windows.Forms.Button buttonSendPduData;
        private System.Windows.Forms.Button buttonSetMessageMode;
        private System.Windows.Forms.ComboBox comboBoxMessageMode;
        private System.Windows.Forms.ComboBox comboBoxMessageACK;
        private System.Windows.Forms.ComboBox comboBoxMessagePriority;
        private System.Windows.Forms.ComboBox comboBoxMessageFormat;
        private System.Windows.Forms.ComboBox comboBoxMessagePreserve;
        private System.Windows.Forms.Button buttonSetMessageParameter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxMessageEncoding;
        private System.Windows.Forms.Label label7;
	}
}
