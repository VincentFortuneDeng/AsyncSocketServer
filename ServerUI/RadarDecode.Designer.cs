namespace TCPServer
{
    partial class RadarDecode
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
            this.RadarLog = new System.Windows.Forms.TextBox();
            this.btnUDP = new System.Windows.Forms.Button();
            this.btnFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RadarLog
            // 
            this.RadarLog.Location = new System.Drawing.Point(12, 3);
            this.RadarLog.Multiline = true;
            this.RadarLog.Name = "RadarLog";
            this.RadarLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.RadarLog.Size = new System.Drawing.Size(966, 311);
            this.RadarLog.TabIndex = 1;
            // 
            // btnUDP
            // 
            this.btnUDP.Location = new System.Drawing.Point(107, 321);
            this.btnUDP.Name = "btnUDP";
            this.btnUDP.Size = new System.Drawing.Size(75, 23);
            this.btnUDP.TabIndex = 2;
            this.btnUDP.Text = "Udp";
            this.btnUDP.UseVisualStyleBackColor = true;
            this.btnUDP.Click += new System.EventHandler(this.btnUDP_Click);
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(210, 321);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(75, 23);
            this.btnFile.TabIndex = 2;
            this.btnFile.Text = "File";
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
            // 
            // RadarDecode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 369);
            this.Controls.Add(this.btnFile);
            this.Controls.Add(this.btnUDP);
            this.Controls.Add(this.RadarLog);
            this.Name = "RadarDecode";
            this.Text = "RadarDecode";
            this.Load += new System.EventHandler(this.RadarDecode_Load);
            this.DoubleClick += new System.EventHandler(this.RadarDecode_DoubleClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox RadarLog;
        private System.Windows.Forms.Button btnUDP;
        private System.Windows.Forms.Button btnFile;
    }
}