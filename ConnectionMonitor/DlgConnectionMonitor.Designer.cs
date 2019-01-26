namespace Vultrue.Communication {
    partial class DlgConnectionMonitor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.connectionMonitor = new Vultrue.Communication.ConnectionMonitor();
            this.SuspendLayout();
            // 
            // connectionMonitor
            // 
            this.connectionMonitor.Connection = null;
            this.connectionMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectionMonitor.Location = new System.Drawing.Point(0, 0);
            this.connectionMonitor.Name = "connectionMonitor";
            this.connectionMonitor.Size = new System.Drawing.Size(784, 564);
            this.connectionMonitor.TabIndex = 0;
            // 
            // DlgConnectionMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 564);
            this.Controls.Add(this.connectionMonitor);
            this.Name = "ConnectionDebugger";
            this.Text = "连接调试器";
            this.ResumeLayout(false);

        }

        #endregion

        private ConnectionMonitor connectionMonitor;
    }
}