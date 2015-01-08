using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class frmADSBSettings : Form
    {
        public frmADSBSettings()
        {
            InitializeComponent();
        }

        private void btnSerch_Click(object sender, EventArgs e)
        {
            btnSerch.Text = "搜素";
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            MessageBox.Show("设置成功");
        }
    }
}
