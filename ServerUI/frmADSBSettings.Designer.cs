namespace TCPServer
{
    partial class frmADSBSettings
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
            this.btnSerch = new System.Windows.Forms.Button();
            this.lstBox = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblMask = new System.Windows.Forms.Label();
            this.lblGateway = new System.Windows.Forms.Label();
            this.btnSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSerch
            // 
            this.btnSerch.Location = new System.Drawing.Point(271, 249);
            this.btnSerch.Name = "btnSerch";
            this.btnSerch.Size = new System.Drawing.Size(75, 23);
            this.btnSerch.TabIndex = 0;
            this.btnSerch.Text = "搜索...";
            this.btnSerch.UseVisualStyleBackColor = true;
            this.btnSerch.Click += new System.EventHandler(this.btnSerch_Click);
            // 
            // lstBox
            // 
            this.lstBox.FormattingEnabled = true;
            this.lstBox.Items.AddRange(new object[] {
            "192.168.144.145:34-17-EB-D1-A0-1D"});
            this.lstBox.Location = new System.Drawing.Point(211, 18);
            this.lstBox.Name = "lstBox";
            this.lstBox.Size = new System.Drawing.Size(195, 225);
            this.lstBox.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(74, 54);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(74, 115);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 2;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(74, 176);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 2;
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(21, 58);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(41, 13);
            this.lblIP.TabIndex = 3;
            this.lblIP.Text = "IP地址";
            // 
            // lblMask
            // 
            this.lblMask.AutoSize = true;
            this.lblMask.Location = new System.Drawing.Point(14, 119);
            this.lblMask.Name = "lblMask";
            this.lblMask.Size = new System.Drawing.Size(55, 13);
            this.lblMask.TabIndex = 3;
            this.lblMask.Text = "子网掩码";
            // 
            // lblGateway
            // 
            this.lblGateway.AutoSize = true;
            this.lblGateway.Location = new System.Drawing.Point(21, 180);
            this.lblGateway.Name = "lblGateway";
            this.lblGateway.Size = new System.Drawing.Size(31, 13);
            this.lblGateway.TabIndex = 3;
            this.lblGateway.Text = "网关";
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(71, 249);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(75, 23);
            this.btnSet.TabIndex = 0;
            this.btnSet.Text = "设置";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // frmADSBSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 284);
            this.Controls.Add(this.lblGateway);
            this.Controls.Add(this.lblMask);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lstBox);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnSerch);
            this.Name = "frmADSBSettings";
            this.Text = "ADS-B 网络设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSerch;
        private System.Windows.Forms.ListBox lstBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblMask;
        private System.Windows.Forms.Label lblGateway;
        private System.Windows.Forms.Button btnSet;
    }
}