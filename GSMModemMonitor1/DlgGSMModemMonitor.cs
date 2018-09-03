using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication {
    public partial class DlgGSMModemMonitor : Form {
        public DlgGSMModemMonitor(GSMModem modem) {
            InitializeComponent();
            gsmModemMonitor.Modem = modem;
        }
    }
}
