namespace Vultrue.Communication {
    partial class NetModem {
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
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
            // 
            // serialPort
            // 
            this.serialPort.BaudRate = 19200;
            this.serialPort.Handshake = System.IO.Ports.Handshake.RequestToSend;
            this.serialPort.ReadTimeout = 3000;
            this.serialPort.WriteTimeout = 3000;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            ((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort;
        private System.Timers.Timer timer;
    }
}
