using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Vultrue.Communication {
    public partial class DlgConnectionMonitor : Form {

        public DlgConnectionMonitor(IConnection connection) {
            InitializeComponent();
            connectionMonitor.Disposed += (object sender, EventArgs e) => { Close(); };
            connectionMonitor.Connection = connection;
        }
    }
}
