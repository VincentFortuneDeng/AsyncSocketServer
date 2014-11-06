using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class frmTrailList : Form
    {
        private frmServer serverView = null;
        public frmTrailList()
        {
            InitializeComponent();
        }

        public frmTrailList(frmServer view)
        {
            InitializeComponent();
            serverView = view;
        }

        public void FindFile(string dirPath) //参数dirPath为指定的目录
        {
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            try {
                /*
                foreach(DirectoryInfo d in Dir.GetDirectories()) {
                    //查找子目录
                    FindFile(Dir + d.ToString() + "\\");

                    listTrail.Items.Add(Dir + d.ToString() + "\\"); //listBox1中填加目录名
                }*/

                foreach(FileInfo f in dir.GetFiles("*.*")) {//查找文件

                    //listTrail.Items.Add(f.ToString()); //listBox1中填加文件名
                    ListViewItem lstVitem = lstVTrail.Items.Add(f.ToString());
                    //lstVitem.SubItems.Add(f.ToString());
                    lstVitem.SubItems.Add(f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    lstVitem.SubItems.Add(Math.Round((decimal)f.Length / 1024, 0).ToString() + " KB");

                }
            } catch(Exception e) {
                MessageBox.Show(e.Message);

            }
        }

        private void frmTrailList_Load(object sender, EventArgs e)
        {
            FindFile(Path.Combine(Application.StartupPath, "Trail"));
        }

        private void btnCanle_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(lstVTrail.SelectedItems.Count > 0) {
                this.serverView.TrailName = lstVTrail.SelectedItems[0].Text;
            }
            this.DialogResult = DialogResult.OK;
        }

    }
}
