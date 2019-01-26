namespace Vultrue.Communication {
    partial class DlgGSMModemMonitor {
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
            this.gsmModemMonitor = new Vultrue.Communication.GSMModemMonitor();
            this.SuspendLayout();
            // 
            // gsmModemMonitor
            // 
            this.gsmModemMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gsmModemMonitor.Location = new System.Drawing.Point(0, 0);
            this.gsmModemMonitor.Modem = null;
            this.gsmModemMonitor.Name = "gsmModemMonitor";
            this.gsmModemMonitor.Size = new System.Drawing.Size(784, 564);
            this.gsmModemMonitor.TabIndex = 0;
            // 
            // DlgGSMModemDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 564);
            this.Controls.Add(this.gsmModemMonitor);
            this.Name = "DlgGSMModemDebugger";
            this.Text = "DlgGSMModemDebugger";
            this.ResumeLayout(false);

        }

        #endregion

        private GSMModemMonitor gsmModemMonitor;
    }
}