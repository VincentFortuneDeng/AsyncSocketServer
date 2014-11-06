namespace TCPServer
{
    partial class frmTrailList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listTrail = new System.Windows.Forms.ListBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.lstVTrail = new System.Windows.Forms.ListView();
            this.ICAO = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DateModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCanle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listTrail
            // 
            this.listTrail.Dock = System.Windows.Forms.DockStyle.Top;
            this.listTrail.FormattingEnabled = true;
            this.listTrail.Location = new System.Drawing.Point(0, 0);
            this.listTrail.Name = "listTrail";
            this.listTrail.Size = new System.Drawing.Size(332, 17);
            this.listTrail.TabIndex = 0;
            this.listTrail.Visible = false;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(88, 214);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lstVTrail
            // 
            this.lstVTrail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ICAO,
            this.DateModified,
            this.Size});
            this.lstVTrail.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstVTrail.Location = new System.Drawing.Point(0, 17);
            this.lstVTrail.MultiSelect = false;
            this.lstVTrail.Name = "lstVTrail";
            this.lstVTrail.Size = new System.Drawing.Size(332, 191);
            this.lstVTrail.TabIndex = 2;
            this.lstVTrail.UseCompatibleStateImageBehavior = false;
            this.lstVTrail.View = System.Windows.Forms.View.Details;
            // 
            // ICAO
            // 
            this.ICAO.Text = "ICAO";
            this.ICAO.Width = 143;
            // 
            // DateModified
            // 
            this.DateModified.Text = "Date Modified";
            this.DateModified.Width = 140;
            // 
            // Size
            // 
            this.Size.Text = "Size";
            // 
            // btnCanle
            // 
            this.btnCanle.Location = new System.Drawing.Point(169, 214);
            this.btnCanle.Name = "btnCanle";
            this.btnCanle.Size = new System.Drawing.Size(75, 23);
            this.btnCanle.TabIndex = 1;
            this.btnCanle.Text = "取消";
            this.btnCanle.UseVisualStyleBackColor = true;
            this.btnCanle.Click += new System.EventHandler(this.btnCanle_Click);
            // 
            // frmTrailList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 261);
            this.Controls.Add(this.lstVTrail);
            this.Controls.Add(this.btnCanle);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.listTrail);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTrailList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "轨迹文件选择";
            this.Load += new System.EventHandler(this.frmTrailList_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listTrail;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ListView lstVTrail;
        private System.Windows.Forms.ColumnHeader ICAO;
        private System.Windows.Forms.ColumnHeader DateModified;
        private System.Windows.Forms.ColumnHeader Size;
        private System.Windows.Forms.Button btnCanle;
    }
}