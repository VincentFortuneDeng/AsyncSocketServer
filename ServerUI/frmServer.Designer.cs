namespace TCPServer
{
    partial class frmServer
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtShowInfo = new System.Windows.Forms.RichTextBox();
            this.startService = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statuBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.send = new System.Windows.Forms.Button();
            this.sendmsg = new System.Windows.Forms.TextBox();
            this.list_Online = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timerSender = new System.Windows.Forms.Timer(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSpeedUp = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSpeedNomal = new System.Windows.Forms.Button();
            this.btnSpeedDown = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtServerPort = new System.Windows.Forms.NumericUpDown();
            this.txtClientCapacity = new System.Windows.Forms.NumericUpDown();
            this.numCapacityBuffer = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cbxIPProtocol = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCleanText = new System.Windows.Forms.Button();
            this.lblNums = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtServerPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务端口";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtShowInfo
            // 
            this.txtShowInfo.BackColor = System.Drawing.SystemColors.Info;
            this.txtShowInfo.Location = new System.Drawing.Point(348, 27);
            this.txtShowInfo.Name = "txtShowInfo";
            this.txtShowInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtShowInfo.Size = new System.Drawing.Size(648, 320);
            this.txtShowInfo.TabIndex = 2;
            this.txtShowInfo.Text = "";
            // 
            // startService
            // 
            this.startService.Location = new System.Drawing.Point(17, 331);
            this.startService.Name = "startService";
            this.startService.Size = new System.Drawing.Size(301, 46);
            this.startService.TabIndex = 3;
            this.startService.Text = "开始服务";
            this.startService.UseVisualStyleBackColor = true;
            this.startService.Click += new System.EventHandler(this.StartService_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statuBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 382);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statuBar
            // 
            this.statuBar.Name = "statuBar";
            this.statuBar.Size = new System.Drawing.Size(67, 17);
            this.statuBar.Text = "未启动服务";
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(348, 350);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(87, 27);
            this.send.TabIndex = 5;
            this.send.Text = "发送广播消息";
            this.send.UseVisualStyleBackColor = true;
            this.send.Visible = false;
            this.send.Click += new System.EventHandler(this.SendClick);
            // 
            // sendmsg
            // 
            this.sendmsg.Location = new System.Drawing.Point(441, 353);
            this.sendmsg.Name = "sendmsg";
            this.sendmsg.Size = new System.Drawing.Size(175, 20);
            this.sendmsg.TabIndex = 6;
            this.sendmsg.Visible = false;
            // 
            // list_Online
            // 
            this.list_Online.BackColor = System.Drawing.SystemColors.Info;
            this.list_Online.FormattingEnabled = true;
            this.list_Online.Location = new System.Drawing.Point(17, 27);
            this.list_Online.Name = "list_Online";
            this.list_Online.Size = new System.Drawing.Size(123, 290);
            this.list_Online.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "终端列表";
            // 
            // timerSender
            // 
            this.timerSender.Interval = 1000;
            this.timerSender.Tick += new System.EventHandler(this.timerSender_Tick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(93, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "客户端容量";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Highlight;
            this.groupBox1.Controls.Add(this.btnSpeedUp);
            this.groupBox1.Controls.Add(this.btnReset);
            this.groupBox1.Controls.Add(this.btnSpeedNomal);
            this.groupBox1.Controls.Add(this.btnSpeedDown);
            this.groupBox1.Controls.Add(this.btnPause);
            this.groupBox1.Controls.Add(this.btnStart);
            this.groupBox1.Location = new System.Drawing.Point(147, 160);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(171, 157);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据控制";
            // 
            // btnSpeedUp
            // 
            this.btnSpeedUp.Location = new System.Drawing.Point(8, 28);
            this.btnSpeedUp.Name = "btnSpeedUp";
            this.btnSpeedUp.Size = new System.Drawing.Size(75, 23);
            this.btnSpeedUp.TabIndex = 9;
            this.btnSpeedUp.Text = "加速";
            this.btnSpeedUp.UseVisualStyleBackColor = true;
            this.btnSpeedUp.Click += new System.EventHandler(this.btnSpeed_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(89, 70);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 10;
            this.btnReset.Text = "重置";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSpeedNomal
            // 
            this.btnSpeedNomal.Location = new System.Drawing.Point(8, 70);
            this.btnSpeedNomal.Name = "btnSpeedNomal";
            this.btnSpeedNomal.Size = new System.Drawing.Size(75, 23);
            this.btnSpeedNomal.TabIndex = 9;
            this.btnSpeedNomal.Text = "正常";
            this.btnSpeedNomal.UseVisualStyleBackColor = true;
            this.btnSpeedNomal.Click += new System.EventHandler(this.btnSpeedNomal_Click);
            // 
            // btnSpeedDown
            // 
            this.btnSpeedDown.Location = new System.Drawing.Point(89, 28);
            this.btnSpeedDown.Name = "btnSpeedDown";
            this.btnSpeedDown.Size = new System.Drawing.Size(75, 23);
            this.btnSpeedDown.TabIndex = 9;
            this.btnSpeedDown.Text = "减速";
            this.btnSpeedDown.UseVisualStyleBackColor = true;
            this.btnSpeedDown.Click += new System.EventHandler(this.btnSpeedDown_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(89, 112);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 9;
            this.btnPause.Text = "暂停";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(8, 112);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Highlight;
            this.groupBox2.Controls.Add(this.txtServerPort);
            this.groupBox2.Controls.Add(this.txtClientCapacity);
            this.groupBox2.Controls.Add(this.numCapacityBuffer);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbxIPProtocol);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(147, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(171, 125);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "配置参数";
            // 
            // txtServerPort
            // 
            this.txtServerPort.Location = new System.Drawing.Point(8, 38);
            this.txtServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtServerPort.Name = "txtServerPort";
            this.txtServerPort.Size = new System.Drawing.Size(75, 20);
            this.txtServerPort.TabIndex = 4;
            this.txtServerPort.Value = new decimal(new int[] {
            8888,
            0,
            0,
            0});
            // 
            // txtClientCapacity
            // 
            this.txtClientCapacity.Location = new System.Drawing.Point(89, 38);
            this.txtClientCapacity.Name = "txtClientCapacity";
            this.txtClientCapacity.Size = new System.Drawing.Size(75, 20);
            this.txtClientCapacity.TabIndex = 4;
            this.txtClientCapacity.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // numCapacityBuffer
            // 
            this.numCapacityBuffer.Location = new System.Drawing.Point(89, 88);
            this.numCapacityBuffer.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.numCapacityBuffer.Name = "numCapacityBuffer";
            this.numCapacityBuffer.Size = new System.Drawing.Size(75, 20);
            this.numCapacityBuffer.TabIndex = 4;
            this.numCapacityBuffer.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "地址协议";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbxIPProtocol
            // 
            this.cbxIPProtocol.FormattingEnabled = true;
            this.cbxIPProtocol.Items.AddRange(new object[] {
            "ipv4",
            "ipv6"});
            this.cbxIPProtocol.Location = new System.Drawing.Point(8, 88);
            this.cbxIPProtocol.Name = "cbxIPProtocol";
            this.cbxIPProtocol.Size = new System.Drawing.Size(75, 21);
            this.cbxIPProtocol.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(94, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "接收缓冲";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(350, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "消息日志";
            // 
            // btnCleanText
            // 
            this.btnCleanText.Location = new System.Drawing.Point(629, 350);
            this.btnCleanText.Name = "btnCleanText";
            this.btnCleanText.Size = new System.Drawing.Size(87, 27);
            this.btnCleanText.TabIndex = 5;
            this.btnCleanText.Text = "清除消息";
            this.btnCleanText.UseVisualStyleBackColor = true;
            this.btnCleanText.Click += new System.EventHandler(this.btnCleanText_Click);
            // 
            // lblNums
            // 
            this.lblNums.AutoSize = true;
            this.lblNums.Location = new System.Drawing.Point(85, 6);
            this.lblNums.Name = "lblNums";
            this.lblNums.Size = new System.Drawing.Size(0, 13);
            this.lblNums.TabIndex = 8;
            // 
            // frmServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 404);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblNums);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.list_Online);
            this.Controls.Add(this.sendmsg);
            this.Controls.Add(this.btnCleanText);
            this.Controls.Add(this.send);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.startService);
            this.Controls.Add(this.txtShowInfo);
            this.MaximizeBox = false;
            this.Name = "frmServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADS-B数据模拟服务";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmServer_FormClosing);
            this.Load += new System.EventHandler(this.frmServer_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtServerPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityBuffer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtShowInfo;
        private System.Windows.Forms.Button startService;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statuBar;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.TextBox sendmsg;
        private System.Windows.Forms.ListBox list_Online;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timerSender;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSpeedUp;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnSpeedNomal;
        private System.Windows.Forms.Button btnSpeedDown;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbxIPProtocol;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numCapacityBuffer;
        private System.Windows.Forms.NumericUpDown txtClientCapacity;
        private System.Windows.Forms.NumericUpDown txtServerPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCleanText;
        private System.Windows.Forms.Label lblNums;
    }
}

