using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Vultrue.Communication
{
    public partial class Deamon : Component
    {
        public Deamon()
        {
            InitializeComponent();
            initializeComponent();
        }

        public Deamon(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            initializeComponent();
        }

        private void initializeComponent()
        {
            menuItemExit.Click += (object sender, EventArgs e) => { nsp.Stop(); Dispose(); };
            nsp.Start();
        }
    }
}
