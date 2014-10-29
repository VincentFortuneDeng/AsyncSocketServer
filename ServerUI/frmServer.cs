using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using AsyncSockets;
using AsyncSockets.AsyncSocketServer;
using System.Diagnostics;

namespace TCPServer
{
    public partial class frmServer : Form
    {
        private int m_numConnections;
        private int m_bufferSize;
        private IPEndPoint m_localEndPoint;
        private int m_port;
        //private bool m_isListing;
        private string m_addressFamily;
        private AsyncServer server;        //声明一个Socket实例

        private bool m_isListing = false;  //开始停止服务按钮状态
        //private Thread m_threadListen;       //声明一个线程实例

        //public Socket socketServer;
        //public Socket socketClient;
        private delegate void UserInterfaceInvoke(string str);
        //private IPEndPoint m_localEP;
        //private int m_localPort;
        //private EndPoint m_remote;
        private Dictionary<string, AsyncUserToken> m_sessionTable;
        private Dictionary<string, string> m_IdToIP;
        //private object m_lockSessionTable=new object();

        private List<string> m_listSBS = new List<string>();

        private int m_sendIndex = 0;

        private int speed = 500;

        //private bool m_Listening;

        //TimerObject m_timerObject ;

        System.Threading.Timer m_timer;


        //用来设置服务端监听的端口号
        //private int ServerPort
        //{
        //    get;
        //    set;
        //}
        //创建委托对象TimerCallback，该委托将被定时调用

        //TimerCallback m_timerDelegate;
        //System.Threading.Timer timer;
        public frmServer()
        {
            InitializeComponent();

        }

        private void StartService()
        {
            m_isListing = false;

            m_numConnections = (int)txtClientCapacity.Value;
            m_port = (int)txtServerPort.Value;
            m_addressFamily = cbxIPProtocol.Text;
            m_bufferSize = (int)numCapacityBuffer.Value;

            try
            {
                m_sessionTable = new Dictionary<string, AsyncUserToken>();
                m_IdToIP = new Dictionary<string, string>();
                this.list_Online.Items.Clear();


                if (server != null)
                {
                    StopService();
                    // 初始化中心服务器
                }

                // 初始化中心服务器

                if (m_addressFamily.ToLower().Equals("ipv4"))
                {
                    m_localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
                }
                else if (m_addressFamily.ToLower().Equals("ipv6"))
                {
                    m_localEndPoint = new IPEndPoint(IPAddress.IPv6Any, m_port);
                }
                else
                {
                    throw new ArgumentException("被指定的地址协议无效");
                }

                //this.server = new DSCServer(m_numConnections, m_receiveSize);//创建DSCServer对象(基础层通讯服务器)
                server = new AsyncServer(m_numConnections, m_bufferSize);

                this.server.OnDataReceived += new EventHandler<AsyncUserToken>(this.svr_OnDataReceived);//注册接收到数据事件
                this.server.OnDisconnected += new EventHandler<AsyncUserToken>(this.svr_OnDisconnected);//注册断开连接事件
                this.server.OnError += new EventHandler<AsyncSocketErrorEventArgs>(svr_OnError);//处理Socket错误
                this.server.OnConnected += new EventHandler<AsyncUserToken>(svr_OnClientConnected);

                server.Init();

                try
                {
                    server.Start(m_localEndPoint);//启动服务器
                    m_isListing = true;
                    //this.m_isTimeOutTimer.Change(m_interval * 100, m_interval * 100);//启动超时监控定时器
                }

                catch (AsyncSocketException asyncSocketException)
                {
                    m_isListing = false;
                    throw asyncSocketException;//启动失败
                }
                ShowClientMessage(string.Format("启动成功,端口号:{0}", m_port.ToString()));

                m_sendIndex = 0;
                statuBar.Text = "服务已启动，等待客户端连接";
                m_isListing = true;
                startService.Text = "停止服务";

                RestTimer();
            }

            catch (Exception e)
            {
                ShowClientMessage("启动失败:" + e.Message);

            }
            //m_mode = mode;//数据模式
            //m_interval = interval;//刷新时间间隔
            //m_isTimeOutTimer = new System.Threading.Timer(new TimerCallback(IsTimeOutTimer_Elapsed), null, Timeout.Infinite, m_interval * 100);//刷新定时器
            //this.IsTimeOutTimer = new System.Timers.Timer(20000.0);      
            //this.IsTimeOutTimer.SynchronizingObject = null;//参数对象
            //this.IsTimeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.IsTimeOutTimer_Elapsed);//注册刷新事件

            //m_numOnLine = 0;//连接计数器
            //m_dtuArgsBufferManager = new DtuArgsBufferManager(bufferSize * numConnections * opsToPreAlloc,
            //bufferSize);

            //m_dtuArgsBufferManager.InitBuffer();
            //m_hsableInneridToDtu = new Dictionary<string, DtuArgs>();//ID Dtu 映射表
            //m_hsableIDToInnerid = new Dictionary<string, string>();//id 内部id 映射表
            //m_reuseDtuPool = new ReuseDtuArgsPool(numConnections);
            //DtuArgs dtuArgs;
            //for (int i = 0; i < numConnections; i++)
            //{
            //    dtuArgs = new DtuArgs();
            //    m_dtuArgsBufferManager.SetBuffer(dtuArgs);
            //    m_reuseDtuPool.Push(dtuArgs);

            //}
            // 支持两种地址族类型: ipv4 和 ipv6 

        }

        private void svr_OnDataReceived(object sender, AsyncUserToken token)
        {
            //bool isRegistered;//是否存在Dtu
            //DtuArgs dtuArgs;
            //object obj2;
            //Debug.WriteLine("svr_OnDataIn 1");
            string connectionId = token.ConnectionId;
            //复制接收到的数据
            //byte[] receiveBytes = new byte[token.BytesReceived];
            //Array.Copy(token.ReceiveBuffer, token.Offset, receiveBytes, 0, token.BytesReceived);

            string strContent = System.Text.Encoding.ASCII.GetString(token.ReceiveBuffer, token.Offset, token.BytesReceived);

            if (strContent.Length > 1)
            {
                //All of the data has been read, so displays it to the console 

                strContent = strContent.ToString();

                switch (strContent.ToUpper())
                {
                    case "CLOSE":

                        //将要发送给连接上来的客户端的提示字符串

                        //当客户端终止连接时
                        Disconnect(token.ConnectionId);

                        break;

                    case "RESET":

                        break;

                    case "SPEED+":

                        break;

                    case "SPEED-":

                        break;

                    case "SPEEDRESET":

                        break;

                    case "STOP":

                        break;

                    case "START":

                        break;

                    default:

                        break;
                }
            }

        }

        private void svr_OnDisconnected(object sender, AsyncUserToken token)
        {
            //bool isRegistered;// 是否注册过Dtu
            string connectionId = token.ConnectionId;
            string ip = m_IdToIP[connectionId];
            //string ip = token.Socket.RemoteEndPoint.ToString();
            AsyncUserToken tokenOut;
            bool IsOnline;

            lock (((ICollection)m_sessionTable).SyncRoot)
            {
                IsOnline = this.m_sessionTable.TryGetValue(connectionId, out tokenOut);

                if (IsOnline)
                {
                    m_sessionTable.Remove(connectionId);
                    m_IdToIP.Remove(ip);
                    UpdateUserNums(m_sessionTable.Count.ToString());
                }
                //if (this.DtuItemList.TryGetValue(id, out dtuInfo))                        
                //{
                //    DtuItemList.Remove(id);
                //RemoveListViewItem(dtuInfo.Lvi);
                // dtuInfo.Lvi.BackColor = Color.Pink;// 下线为粉红色
                //}
            }

            if (IsOnline)
            {
                //RemoveListViewItem(dtuInfo.Lvi);
                UserListOperateDelete(ip);
                lock (((ICollection)this.m_sessionTable).SyncRoot)
                {
                    //Debug.WriteLine("ConnectEnter!");
                    UpdateUserNums(m_sessionTable.Count.ToString());
                    //Debug.WriteLine("ConnectExit!");
                }
            }



            //SetOnlieNum(svr.NumConnectedSockets.ToString());
            ShowClientMessage(string.Format("{0}下线", ip));

            //DtuArgs dtuArgs;
            //lock (((ICollection)m_hsableInneridToDtu).SyncRoot)
            //{
            //    isRegistered = this.m_hsableInneridToDtu.TryGetValue(connectionId, out dtuArgs);// 发送过注册包
            //}
            //if (isRegistered)// 发送过注册包
            //{
            //    if (dtuArgs.IsOnline)
            //    {
            //        Interlocked.Decrement(ref this.m_numOnLine);

            //        EventHandler<DtuArgs> handler = OnClientDisconnected;
            //        // 如果订户事件将为空(null)
            //        if (handler != null)
            //        {
            //            Debug.WriteLine("svr_OnDisconnected 4");
            //            handler(this, dtuArgs);//抛出掉线事件
            //        }
            //    }
            //    lock (((ICollection)m_hsableIDToInnerid).SyncRoot)
            //    {
            //        this.m_hsableIDToInnerid.Remove(dtuArgs.ID);
            //    }
            //    FreeResueDtuArgs(dtuArgs);
            //    lock (((ICollection)m_hsableInneridToDtu).SyncRoot)
            //    {
            //        this.m_hsableInneridToDtu.Remove(connectionId);
            //    }
            //}
            //else
            //{
            //    Debug.WriteLine("Bug");
            //}

        }

        private void svr_OnError(object sender, AsyncSocketErrorEventArgs tokenError)
        {
            if (sender != null)//服务器错误
            {
                ShowClientMessage(string.Format("服务器错误:{0}", tokenError.exception.Message));
            }
            else//客户端错误
            {
                //AsyncUserToken token=(AsyncUserToken)sender;
                ShowClientMessage(string.Format("客户端错误:{0}", tokenError.exception.Message));
            }
        }

        void svr_OnClientConnected(object sender, AsyncUserToken token)
        {
            string id = token.ConnectionId;
            string ip = token.Socket.RemoteEndPoint.ToString();
            //DtuInfo dtuInfo;
            AsyncUserToken tokenOut;
            bool IsOnline;
            lock (((ICollection)this.m_sessionTable).SyncRoot)
            {
                //Debug.WriteLine("ConnectEnter!");
                IsOnline = this.m_sessionTable.TryGetValue(id, out tokenOut);
                //Debug.WriteLine("ConnectExit!");
            }
            //if (this.DtuItemList.TryGetValue(id, out dtuInfo))// 如果列表中已含有该ID号的DTU
            if (IsOnline)
            {
                //UserListOperateAdd(token.Socket.RemoteEndPoint.ToString());
            }

            else
            {
                if (m_isListing)
                {
                    //ListViewItem lvi = new ListViewItem(new string[] { token.ID, token.PhoneNumber, token.IP, token.LoginTime.ToString(), token.RefreshTime.ToString() });
                    //lvi.BackColor = Color.LightGreen;// 上线为亮绿
                    UserListOperateAdd(ip);


                    //dtuInfo = new DtuInfo();
                    //dtuInfo.Lvi = lvi;

                    lock (((ICollection)m_sessionTable).SyncRoot)
                    {
                        this.m_sessionTable.Add(id, token);
                        m_IdToIP.Add(id, ip);

                        UpdateUserNums(m_sessionTable.Count.ToString());

                    }
                    //SetOnlieNum(svr.NumConnectedSockets.ToString());
                    ShowClientMessage(string.Format("{0}上线", ip));
                    StartTimer();
                }
            }
        }

        public bool Disconnect(string connectionId)
        {
            //id = id.Trim();
            //string connectionId;
            //if (!TryIdToInnerid(id, out connectionId))
            //{
            //    return false;
            //}
            try
            {
                try
                {
                    this.server.Disconnect(connectionId);
                    return true;
                }
                catch (AsyncSocketException asyncSocketException)
                {
                    throw asyncSocketException;
                }

            }
            catch
            {
                return false;
            }

        }

        //开始停止服务按钮
        private void StartService_Click(object sender, EventArgs e)
        {
            //ThreadStart threadListenDelegate;

            if (!m_isListing)
            {
                //新建一个委托线程
                //threadListenDelegate = new ThreadStart(Listen);
                //实例化新线程
                //m_threadListen = new Thread(threadListenDelegate);

                //m_threadListen.IsBackground = true;
                //m_threadListen.Start();

                StartService();


            }

            else
            {
                //lock (((ICollection)m_sessionTable).SyncRoot)
                //{
                //    //try
                //    //{
                //    foreach (Socket socketClient in m_sessionTable)
                //    {
                //        RemoveClient(socketClient);
                //    }
                //    //}

                //    //catch
                //    //{

                //    //}
                //}
                ////停止服务（绑定的套接字没有关闭,因此客户端还是可以连接上来）
                ////m_threadListen.Interrupt();
                ////m_threadListen.Abort();
                //m_sockServer.Close();

                //m_threadListen.
                StopService();



                //PauseTimer();
            }

        }

        private void StopService()
        {
            try
            {
                m_isListing = false;
                //this.m_hsableInneridToDtu.Clear();
                //this.m_hsableIDToInnerid.Clear();
                //this.m_isTimeOutTimer.Change(Timeout.Infinite, m_interval * 100);
                //this.IsTimeOutTimer.Stop();
                this.server.Shutdown();
                ShowClientMessage("停止成功");

                ShowClientMessage("服务器已停止服务" + "\r\n");
                m_isListing = false;
                startService.Text = "开始服务";
                statuBar.Text = "服务已停止";

                PauseTimer();
            }
            catch (Exception e)
            {
                ShowClientMessage("停止失败:" + e.Message);
            }
            finally
            {
                lock (((ICollection)list_Online.Items).SyncRoot)
                {
                    list_Online.Items.Clear();
                }
            }
        }
        //监听函数

        //下面是被定时调用的方法
        private void CheckStatus(Object state)
        {
            this.SendBroadMessage();
        }
        public bool Send(string id, byte[] dataBytes)
        {
            id = id.Trim();
            AsyncUserToken token;

            if (!m_sessionTable.TryGetValue(id, out token))
            {
                return false;
            }

            try
            {
                try
                {
                    this.server.Send(id, dataBytes);
                    return true;//发送成功                    
                }

                catch (AsyncSocketException asyncSocketException)
                {
                    throw asyncSocketException;
                }
            }
            catch
            {
                return false;//发送失败
            }
        }
        private void SendBroadMessage()
        {
            lock (((ICollection)m_sessionTable).SyncRoot)
            {
                if (m_listSBS.Count > 0 && m_sessionTable.Count > 0)
                {
                    string strDataLine = m_listSBS[m_sendIndex++];

                    if (m_sendIndex >= m_listSBS.Count) m_sendIndex = 0;

                    Byte[] sendData = Encoding.ASCII.GetBytes(strDataLine);

                    /*
                    IDictionaryEnumerator myEnumerator = _sessionTable.GetEnumerator();
                    while (myEnumerator.MoveNext())
                    {
                        EndPoint tempend = (EndPoint)_sessionTable.Values;
                        Client.SendTo(sendData, tempend);
                    }
                     * */
                    //try
                    //{

                    //if (m_sessionTable.Count > 0)
                    //{
                    foreach (AsyncUserToken socketClient in m_sessionTable.Values)
                    {


                        try
                        {
                            Send(socketClient.ConnectionId, sendData);
                            //ShowClientMessage(string.Format("向{0}发送数据:{1}", socketClient.ConnectionId, strDataLine));
                            //this.richTextBox_log.AppendText(string.Format("<{0}>:向{1}发送数据:{2}\r\n", DateTime.Now.ToString(), id, data));
                            //SetSedText(string.Format("向{0}发送数据:{1}", id, data));
                        }
                        catch (Exception ee)
                        {
                            ShowClientMessage("发送数据出现异常：" + ee.Message);
                            return;
                        }
                    }

                    ShowClientMessage(strDataLine);
                    //}

                    //}
                    //catch
                    //{

                    //}


                }

                else
                {
                    if (m_listSBS.Count <= 0)
                    {
                        ShowClientMessage("没有数据内容");
                    }

                    else if (m_sessionTable.Count <= 0)
                    {
                        PauseTimer();
                    }
                    //PauseTimer();

                }
            }

        }



        //用来往richtextbox框中显示消息
        private void ShowClientMessage(string message)
        {
            //在线程里以安全方式调用控件
            if (txtShowInfo.InvokeRequired)
            {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(ShowClientMessage);
                txtShowInfo.Invoke(userInterfaceInvoke, new object[] { message });
            }

            else
            {
                //txtShowInfo.AppendText(message);
                this.txtShowInfo.AppendText(string.Format("<{0}>:{1}\r\n", DateTime.Now.ToString(), message));

                SetScroll();
            }
        }

        private void UpdateUserNums(string message)
        {
            if (this.lblNums.InvokeRequired)
            {
                this.lblNums.Invoke(new UserInterfaceInvoke(UpdateUserNums), message);
            }

            else
            {
                this.lblNums.Text = message;
            }
        }

        private void UserListOperateAdd(string message)
        {
            //在线程里以安全方式调用控件
            if (list_Online.InvokeRequired)
            {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(UserListOperateAdd);
                list_Online.Invoke(userInterfaceInvoke, new object[] { message });
            }

            else
            {
                list_Online.Items.Add(message);
            }
        }
        private void UserListOperateDelete(string message)
        {
            //在线程里以安全方式调用控件
            if (list_Online.InvokeRequired)
            {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(UserListOperateDelete);
                list_Online.Invoke(userInterfaceInvoke, new object[] { message });
            }

            else
            {
                list_Online.Items.Remove(message);
            }
        }

        //Timer信息对象 7
        private class TimerObject
        {
            public int Counter = 0;
        }

        private class StateObject
        {
            public Socket workSocket = null;
            public const int BUFFER_SIZE = 1024;
            public byte[] buffer = new byte[BUFFER_SIZE];
            public StringBuilder sb = new StringBuilder();
        }

        //当有客户端连接时的处理

        //以下实现发送广播消息

        private void frmServer_Load(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(@".\780587.log", Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                m_listSBS.Add(line + System.Environment.NewLine);
            }
            //m_sessionTable = new List<Socket>();
            TimerObject timerObject = new TimerObject();
            TimerCallback timerDelegate = new TimerCallback(CheckStatus);
            //创建一个时间延时2s启动，间隔为1s的定时器
            m_timer = new System.Threading.Timer(timerDelegate, timerObject, System.Threading.Timeout.Infinite, 500);//System.Threading.Timeout.Infinite

            cbxIPProtocol.SelectedIndex = 0;

        }

        //窗口关闭时中止线程。
        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SendClick(object sender, EventArgs e)
        {
            SendBroadMessage();
        }

        private void timerSender_Tick(object sender, EventArgs e)
        {
            this.SendBroadMessage();
        }

        private void btnSpeed_Click(object sender, EventArgs e)
        {

            SpeedTimer("+");
        }

        private void btnSpeedDown_Click(object sender, EventArgs e)
        {
            SpeedTimer("-");
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            PauseTimer();
        }
        private void PauseTimer()
        {
            m_timer.Change(System.Threading.Timeout.Infinite, speed);
        }

        private void StartTimer()
        {
            m_timer.Change(0, speed);
        }

        private void SpeedTimer(string command)
        {
            switch (command)
            {
                case "-":
                    if (speed <= 500)
                    {
                        speed += 50;
                    }

                    else
                    {
                        speed += 500;
                    }
                    break;
                case "+":
                    if (speed <= 500)
                    {
                        speed -= 50;
                    }

                    else
                    {
                        speed -= 500;
                    }

                    if (speed <= 0) speed = 500;
                    break;

                default:
                    speed = 500;
                    break;
            }
            m_timer.Change(0, speed);
        }

        private void btnSpeedNomal_Click(object sender, EventArgs e)
        {
            SpeedTimer("");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            RestTimer();
        }

        private void RestTimer()
        {
            PauseTimer();
            m_sendIndex = 0;
            speed = 500;
            StartTimer();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartTimer();
        }

        private void btnCleanText_Click(object sender, EventArgs e)
        {
            SetClear();
        }

        private delegate void DeleSetTxtInfo();

        private void SetClear()
        {
            if (this.txtShowInfo.InvokeRequired)
            {
                this.txtShowInfo.Invoke(new DeleSetTxtInfo(SetClear));
            }

            else
            {

                this.txtShowInfo.Clear();

            }
        }

        private void SetScroll()
        {
            if (this.txtShowInfo.InvokeRequired)
            {
                this.txtShowInfo.Invoke(new DeleSetTxtInfo(SetScroll));
            }

            else
            {

                txtShowInfo.SelectionStart = txtShowInfo.Text.Length;

                txtShowInfo.ScrollToCaret();

            }
        }
    }


}
