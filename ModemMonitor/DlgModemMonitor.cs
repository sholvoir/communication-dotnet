using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication
{
    public partial class DlgModemMonitor : Form
    {
        private CdmaModem modem;

        public DlgModemMonitor(CdmaModem modem)
        {
            InitializeComponent();
            this.modem = modem;
        }

        private void DlgModemMonitor_Shown(object sender, EventArgs e)
        {
            gsmModemMonitor.Modem = modem;
        }
    }
}
