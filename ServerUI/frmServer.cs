using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
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
using InterfaceFactory;
using TCPServer.Library;
using TCPServer.Interface;



namespace TCPServer
{
    public partial class frmServer : Form
    {
        private IClock m_clock;
        private List<WayPoint> m_listWaypoints;
        private BaseStationSerialiser m_baseStationSerialiser;
        private int m_numConnections;
        private int m_bufferSize;
        private IPEndPoint m_localEndPoint;
        private int m_port;
        //private bool m_isListing;
        private string m_addressFamily;
        private AsyncServer m_Server;        //声明一个Socket实例

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

        private const int DEFAULT_SPEED_WAYPOINT = 30000;

        private const int DEFAULT_SPEED_TRAIL = 500;

        private int m_Speed = DEFAULT_SPEED_TRAIL;

        private const int DEFAULT_STEP = 50;

        private const int START_DELAY = 500;

        //private bool m_Listening;

        //TimerObject m_timerObject ;

        private System.Threading.Timer m_Timer;
        private string m_TrailPath = "Trail";
        private string m_TrailFileName = "";

        TextWriterTraceListener m_TraceListener = new TextWriterTraceListener(System.IO.File.CreateText("BroadcastBaseStationServer.log"));
        //TextWriterTraceListener m_DebugListener = new TextWriterTraceListener(System.IO.File.CreateText("BroadcastBaseStationMessage.log"));


        public string TrailName
        {
            set;
            get;
        }

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

            Implementations.Register(Factory.Singleton);
        }

        private void StartService()
        {
            m_isListing = false;

            m_numConnections = (int)txtClientCapacity.Value;
            m_port = (int)txtServerPort.Value;
            m_addressFamily = cbxIPProtocol.Text;
            m_bufferSize = (int)numCapacityBuffer.Value;

            try {
                m_sessionTable = new Dictionary<string, AsyncUserToken>();
                m_IdToIP = new Dictionary<string, string>();
                this.list_Online.Items.Clear();

                if(m_Server != null) {
                    StopService();
                    // 初始化中心服务器
                }

                // 初始化中心服务器

                if(m_addressFamily.ToLower().Equals("ipv4")) {
                    m_localEndPoint = new IPEndPoint(IPAddress.Any, m_port);
                } else if(m_addressFamily.ToLower().Equals("ipv6")) {
                    m_localEndPoint = new IPEndPoint(IPAddress.IPv6Any, m_port);
                } else {
                    throw new ArgumentException("被指定的地址协议无效");
                }

                //this.server = new DSCServer(m_numConnections, m_receiveSize);//创建DSCServer对象(基础层通讯服务器)
                m_Server = new AsyncServer(m_numConnections, m_bufferSize);

                this.m_Server.OnDataReceived += new EventHandler<AsyncUserToken>(this.svr_OnDataReceived);//注册接收到数据事件
                this.m_Server.OnDisconnected += new EventHandler<AsyncUserToken>(this.svr_OnDisconnected);//注册断开连接事件
                this.m_Server.OnError += new EventHandler<AsyncSocketErrorEventArgs>(svr_OnError);//处理Socket错误
                this.m_Server.OnConnected += new EventHandler<AsyncUserToken>(svr_OnClientConnected);

                m_Server.Init();

                try {
                    m_Server.Start(m_localEndPoint);//启动服务器
                    m_isListing = true;
                    //this.m_isTimeOutTimer.Change(m_interval * 100, m_interval * 100);//启动超时监控定时器
                } catch(AsyncSocketException asyncSocketException) {
                    m_isListing = false;
                    throw asyncSocketException;//启动失败
                }
                ShowClientMessage(string.Format("启动成功,端口号:{0}", m_port.ToString()));
                LogBroadcastServerStatus(string.Format("启动成功,端口号:{0}", m_port.ToString()));

                m_sendIndex = 0;
                statuBar.Text = "服务已启动，等待客户端连接";
                m_isListing = true;
                btnStartService.Text = "停止服务";

                ResetTimer();
            } catch(Exception e) {
                ShowClientMessage("启动失败:" + e.Message);
                LogBroadcastServerStatus("启动失败:" + e.Message);

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

            if(strContent.Length > 1) {
                //All of the data has been read, so displays it to the console 

                strContent = strContent.ToString();

                switch(strContent.ToUpper()) {
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
            string ip = "";
            //string ip = token.Socket.RemoteEndPoint.ToString();
            AsyncUserToken tokenOut;
            int count = 0;
            bool IsOnline;

            lock(((ICollection)m_sessionTable).SyncRoot) {
                IsOnline = this.m_sessionTable.TryGetValue(connectionId, out tokenOut);

                if(IsOnline) {
                    m_sessionTable.Remove(connectionId);
                    lock(((ICollection)m_IdToIP).SyncRoot) {
                        m_IdToIP.TryGetValue(connectionId, out ip);
                        if(!string.IsNullOrEmpty(ip)) {
                            m_IdToIP.Remove(ip);
                        }
                    }
                    count = m_sessionTable.Count;

                }
                //if (this.DtuItemList.TryGetValue(id, out dtuInfo))                        
                //{
                //    DtuItemList.Remove(id);
                //RemoveListViewItem(dtuInfo.Lvi);
                // dtuInfo.Lvi.BackColor = Color.Pink;// 下线为粉红色
                //}
            }


            if(IsOnline) {
                //RemoveListViewItem(dtuInfo.Lvi);
                UserListOperateDelete(ip);

                ShowClientMessage(string.Format("{0}下线", ip));
                UpdateUserNums(count.ToString());

                LogBroadcastServerStatus(string.Format("{0}下线", ip));
                LogBroadcastServerStatus(string.Format("当前客户端连接数:{0}", count.ToString()));
            }

            //SetOnlieNum(svr.NumConnectedSockets.ToString());


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
            if(sender != null)//服务器错误
            {
                ShowClientMessage(string.Format("服务器错误:{0}", tokenError.exception.Message));
                LogBroadcastServerStatus(string.Format("服务器错误:{0}", tokenError.exception.Message));
            } else//客户端错误
            {
                //AsyncUserToken token=(AsyncUserToken)sender;
                ShowClientMessage(string.Format("客户端错误:{0}", tokenError.exception.Message));
                LogBroadcastServerStatus(string.Format("客户端错误:{0}", tokenError.exception.Message));
            }
        }

        void svr_OnClientConnected(object sender, AsyncUserToken token)
        {
            string id = token.ConnectionId;
            string ip = token.Socket.RemoteEndPoint.ToString();
            //DtuInfo dtuInfo;
            AsyncUserToken tokenOut;
            int count = 0;
            bool IsOnline;
            lock(((ICollection)this.m_sessionTable).SyncRoot) {
                //Debug.WriteLine("ConnectEnter!");
                IsOnline = this.m_sessionTable.TryGetValue(id, out tokenOut);
                //Debug.WriteLine("ConnectExit!");
                if(!IsOnline) {
                    this.m_sessionTable.Add(id, token);
                    lock(((ICollection)this.m_IdToIP).SyncRoot) {
                        m_IdToIP.Add(id, ip);
                    }

                    count = m_sessionTable.Count;

                }
            }
            //if (this.DtuItemList.TryGetValue(id, out dtuInfo))// 如果列表中已含有该ID号的DTU
            if(!IsOnline) {
                //UserListOperateAdd(token.Socket.RemoteEndPoint.ToString());


                //ListViewItem lvi = new ListViewItem(new string[] { token.ID, token.PhoneNumber, token.IP, token.LoginTime.ToString(), token.RefreshTime.ToString() });
                //lvi.BackColor = Color.LightGreen;// 上线为亮绿
                UserListOperateAdd(ip);


                //dtuInfo = new DtuInfo();
                //dtuInfo.Lvi = lvi;


                //SetOnlieNum(svr.NumConnectedSockets.ToString());
                ShowClientMessage(string.Format("{0}上线", ip));
                UpdateUserNums(count.ToString());

                LogBroadcastServerStatus(string.Format("当前客户端连接数:{0}", count.ToString()));
                LogBroadcastServerStatus(string.Format("{0}上线", ip));
                if(m_isListing) {
                    if(m_sessionTable.Count == 0) StartTimer();
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
            try {
                try {
                    this.m_Server.Disconnect(connectionId);
                    return true;
                } catch(AsyncSocketException asyncSocketException) {
                    throw asyncSocketException;
                }

            } catch {
                return false;
            }

        }

        //开始停止服务按钮
        private void StartService_Click(object sender, EventArgs e)
        {
            //ThreadStart threadListenDelegate;

            if(!m_isListing) {
                //新建一个委托线程
                //threadListenDelegate = new ThreadStart(Listen);
                //实例化新线程
                //m_threadListen = new Thread(threadListenDelegate);

                //m_threadListen.IsBackground = true;
                //m_threadListen.Start();

                StartService();


            } else {
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
            try {
                m_isListing = false;
                //this.m_hsableInneridToDtu.Clear();
                //this.m_hsableIDToInnerid.Clear();
                //this.m_isTimeOutTimer.Change(Timeout.Infinite, m_interval * 100);
                //this.IsTimeOutTimer.Stop();
                this.m_Server.Shutdown();
                ShowClientMessage("停止成功");
                LogBroadcastServerStatus("停止成功");

                ShowClientMessage("服务器已停止服务" + "\r\n");
                LogBroadcastServerStatus("服务器已停止服务" + "\r\n");
                m_isListing = false;
                btnStartService.Text = "开始服务";
                statuBar.Text = "服务已停止";

                PauseTimer();
            } catch(Exception e) {
                ShowClientMessage("停止失败:" + e.Message);
                LogBroadcastServerStatus("停止失败:" + e.Message);
            } finally {
                lock(((ICollection)list_Online.Items).SyncRoot) {
                    list_Online.Items.Clear();
                }
            }
        }
        //监听函数

        //下面是被定时调用的方法
        private void CheckStatus(Object state)
        {
            if(rdTrail.Checked) {
                SendBroadMessage();
            } else {
                SendBroadcastWayPoints();
            }
        }
        public bool Send(string id, byte[] dataBytes)
        {
            id = id.Trim();
            AsyncUserToken token;

            if(!m_sessionTable.TryGetValue(id, out token)) {
                return false;
            }

            try {
                try {
                    this.m_Server.Send(id, dataBytes);
                    return true;//发送成功                    
                } catch(AsyncSocketException asyncSocketException) {
                    throw asyncSocketException;
                }
            } catch {
                return false;//发送失败
            }
        }

        private void SendBroadMessage(BaseStationMessage message)
        {
            if(m_isListing) {
                if(m_sessionTable.Count > 0) {
                    string strDataLine = String.Concat(message.ToBaseStationString(), "\r\n");

                    byte[] bytes = Encoding.ASCII.GetBytes(strDataLine);
                    if(bytes != null && bytes.Length > 0) {
                        lock(((ICollection)m_sessionTable).SyncRoot) {
                            foreach(AsyncUserToken socketClient in m_sessionTable.Values) {
                                try {
                                    Send(socketClient.ConnectionId, bytes);
                                } catch(Exception ee) {
                                    ShowClientMessage("发送数据出现异常：" + ee.Message);
                                    LogBroadcastServerStatus("发送数据出现异常：" + ee.Message);
                                    return;
                                }
                            }
                        }
                        ShowClientMessage(strDataLine);
                        Debug.WriteLine(strDataLine);

                    }
                }
            }
        }

        private void SendBroadcastWayPoints()
        {
            if(m_isListing) {
                if(m_sessionTable.Count > 0 && m_listWaypoints.Count > 0) {
                    BaseStationMessage waypointMessage;
                    WayPoint point;
                    for(int i = 0;i < m_listWaypoints.Count;i++) {
                        if(i < m_listWaypoints.Count - 1) {
                            //发送前几个点呼号及坐标
                            point = m_listWaypoints[i];
                            waypointMessage = CreateWayPointsCallsign(point.Icao24, point.Callsign);
                            SendBroadMessage(waypointMessage);

                            waypointMessage = CreateWayPointsPosition(point.Icao24, point.Latitude.GetValueOrDefault(), point.Longitude.GetValueOrDefault());
                            SendBroadMessage(waypointMessage);
                        } else {
                            //发送最后一点呼号
                            point = m_listWaypoints[i];
                            waypointMessage = CreateWayPointsCallsign(point.Icao24, point.Callsign);
                            SendBroadMessage(waypointMessage);

                            WayPoint way;

                            //从后向前发送路线上的点
                            for(int j = m_listWaypoints.Count - 1;j >= 0;j--) {
                                way = m_listWaypoints[j];
                                waypointMessage = CreateWayPointsPosition(point.Icao24, way.Latitude.GetValueOrDefault(), way.Longitude.GetValueOrDefault());
                                SendBroadMessage(waypointMessage);
                            }
                            //从前向后发送路线上的点
                            for(int j = 0;j < m_listWaypoints.Count;j++) {
                                way = m_listWaypoints[j];
                                waypointMessage = CreateWayPointsPosition(point.Icao24, way.Latitude.GetValueOrDefault(), way.Longitude.GetValueOrDefault());
                                SendBroadMessage(waypointMessage);
                            }
                        }
                    }
                }
            }
        }
        private void SendBroadMessage()
        {
            if(m_isListing) {
                if(m_listSBS.Count > 0 && m_sessionTable.Count > 0) {
                    string strDataLine = m_listSBS[m_sendIndex++];

                    if(m_sendIndex >= m_listSBS.Count) m_sendIndex = 0;

                    Byte[] sendData = Encoding.ASCII.GetBytes(strDataLine);


                    lock(((ICollection)m_sessionTable).SyncRoot) {
                        foreach(AsyncUserToken socketClient in m_sessionTable.Values) {
                            try {
                                Send(socketClient.ConnectionId, sendData);
                            } catch(Exception ee) {
                                ShowClientMessage("发送数据出现异常：" + ee.Message);
                                LogBroadcastServerStatus("发送数据出现异常：" + ee.Message);
                                return;
                            }
                        }
                    }
                    ShowClientMessage(strDataLine);
                    Debug.WriteLine(strDataLine);
                } else {
                    if(m_listSBS.Count <= 0) {
                        ShowClientMessage("没有数据内容");
                    } else {
                        if(m_isListing) {
                            if(m_sessionTable.Count <= 0) {
                                PauseTimer();
                            }
                        }
                    }
                }
            }
        }

        private void LogBroadcastServerStatus(string message)
        {
            Trace.WriteLine(string.Format("<{0}>:{1}", DateTime.Now.ToString(), message));
        }

        //用来往richtextbox框中显示消息
        private void ShowClientMessage(string message)
        {
            //在线程里以安全方式调用控件
            if(txtShowInfo.InvokeRequired) {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(ShowClientMessage);
                txtShowInfo.Invoke(userInterfaceInvoke, new object[] { message });
            } else {
                //txtShowInfo.AppendText(message);
                this.txtShowInfo.AppendText(string.Format("<{0}>:{1}\r\n", DateTime.Now.ToString(), message));

                SetScroll();
            }
        }

        private void UpdateUserNums(string message)
        {
            if(this.lblNums.InvokeRequired) {
                this.lblNums.Invoke(new UserInterfaceInvoke(UpdateUserNums), message);
            } else {
                this.lblNums.Text = message;
            }
        }

        private void UserListOperateAdd(string message)
        {
            //在线程里以安全方式调用控件
            if(list_Online.InvokeRequired) {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(UserListOperateAdd);
                list_Online.Invoke(userInterfaceInvoke, new object[] { message });
            } else {
                list_Online.Items.Add(message);
            }
        }
        private void UserListOperateDelete(string message)
        {
            //在线程里以安全方式调用控件
            if(list_Online.InvokeRequired) {
                UserInterfaceInvoke userInterfaceInvoke = new UserInterfaceInvoke(UserListOperateDelete);
                list_Online.Invoke(userInterfaceInvoke, new object[] { message });
            } else {
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
            m_clock = Factory.Singleton.Resolve<IClock>();
            m_baseStationSerialiser = new BaseStationSerialiser();
            rdTrail.Checked = true;

            ChangeTimerSpeedDefault();
            //LoadSBSTrail();
            //m_sessionTable = new List<Socket>();
            TimerObject timerObject = new TimerObject();
            TimerCallback timerDelegate = new TimerCallback(CheckStatus);
            //创建一个时间延时无限启动，间隔为m_Speed的定时器
            m_Timer = new System.Threading.Timer(timerDelegate, timerObject, System.Threading.Timeout.Infinite, m_Speed);//System.Threading.Timeout.Infinite

            cbxIPProtocol.SelectedIndex = 0;
            unassignedCountry.CodeBlock = new CodeBlock();
            unassignedCountry.CodeBlock.Country = "Unassigned Country";
            unassignedCountry.CodeBlock.IsMilitary = false;
            unassignedCountry.SignificantBitMask = 0x7FFFFF;
            unassignedCountry.BitMask = 0x0;

            Trace.Listeners.Add(m_TraceListener);
            //Debug.Listeners.Add(m_TraceListener);
            //Debug.Listeners.Add(m_DebugListener);

            //Trace.AutoFlush=true;
            Debug.AutoFlush = true;
            LoadWaypoints();

        }

        private void LoadSBSTrail(string trail)
        {
            if(!string.IsNullOrEmpty(trail)) {
                m_TrailFileName = Path.Combine(Application.StartupPath, m_TrailPath);
                m_TrailFileName = Path.Combine(m_TrailFileName, trail);

                using(StreamReader sr = new StreamReader(m_TrailFileName, Encoding.Default)) {
                    String baseStationLine;
                    m_listSBS.Clear();
                    while((baseStationLine = sr.ReadLine()) != null) {
                        m_listSBS.Add(baseStationLine + System.Environment.NewLine);
                    }
                }
            } else {
                MessageBox.Show("轨迹文件不存在");
            }
        }

        private void LoadSBSTrail()
        {
            using(StreamReader sr = new StreamReader(@".\TrailData.log", Encoding.Default)) {
                String line;
                while((line = sr.ReadLine()) != null) {
                    m_listSBS.Add(line + System.Environment.NewLine);
                }
            }
        }
        private void LoadWaypoints()
        {
            m_listWaypoints = m_baseStationSerialiser.DeSerialiseBaseStation();

            //MessageBox.Show(m_listWaypoints.Count.ToString());

        }


        private void InitWaypoints()
        {
            WayPoint waypoint;
            List<WayPoint> waypoints = new List<WayPoint>();
            /*39.948300 	124.245000
            39.875000 	124.142000
            39.268300 	122.618000
            40.046700 	123.178000

            40.833300 	123.750000
            41.406700 	124.163000
            41.048300 	124.193000
            40.258300 	124.285000*/
            waypoint = CreateWayPoints("001F41", "DF", 39.948300, 124.245000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F42", "P68", 39.875000, 124.142000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F43", "CHI", 39.268300, 122.618000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F44", "NODAL", 40.046700, 123.178000);
            waypoints.Add(waypoint);

            waypoint = CreateWayPoints("001F45", "ISKEM", 40.833300, 123.750000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F46", "BIDIB", 41.406700, 124.163000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F47", "ANSUK", 41.048300, 124.193000);
            waypoints.Add(waypoint);
            waypoint = CreateWayPoints("001F48", "DDG", 40.258300, 124.285000);
            waypoints.Add(waypoint);
            foreach(WayPoint point in waypoints) {
                m_baseStationSerialiser.SerialiseBaseStation(point);
            }
        }

        private WayPoint CreateWayPoints(string icao, string callsign, double latitude, double longitude)
        {
            WayPoint waypoint = new WayPoint();
            waypoint.Altitude = 0;
            waypoint.Callsign = callsign;
            waypoint.GroundSpeed = 0;
            waypoint.Icao24 = icao;
            waypoint.Latitude = latitude;
            waypoint.Longitude = longitude;
            //waypoint.MessageGenerated = m_clock.UtcNow;
            //waypoint.MessageLogged = m_clock.UtcNow;
            waypoint.MessageType = BaseStationMessageType.Transmission;
            waypoint.OnGround = true;
            waypoint.Track = 0;
            waypoint.TransmissionType = BaseStationTransmissionType.SurfacePosition;

            return waypoint;
        }

        private BaseStationMessage CreateWayPointsPosition(string icao, double latitude, double longitude)
        {
            BaseStationMessage waypoint = new BaseStationMessage();
            waypoint.Altitude = 0;
            //waypoint.Callsign = "DF";
            waypoint.GroundSpeed = 0;
            waypoint.Icao24 = icao;
            waypoint.Latitude = latitude;
            waypoint.Longitude = longitude;
            waypoint.MessageGenerated = m_clock.UtcNow;
            waypoint.MessageLogged = m_clock.UtcNow;
            waypoint.MessageType = BaseStationMessageType.Transmission;
            waypoint.TransmissionType = BaseStationTransmissionType.SurfacePosition;
            waypoint.OnGround = true;
            waypoint.Track = 0;

            return waypoint;
        }

        private BaseStationMessage CreateWayPointsCallsign(string icao, string callsign)
        {
            BaseStationMessage waypoint = new BaseStationMessage();

            waypoint.Callsign = callsign;

            waypoint.Icao24 = icao;
            waypoint.MessageGenerated = m_clock.UtcNow;
            waypoint.MessageLogged = m_clock.UtcNow;
            waypoint.MessageType = BaseStationMessageType.Transmission;
            waypoint.TransmissionType = BaseStationTransmissionType.IdentificationAndCategory;

            return waypoint;
        }



        //窗口关闭时中止线程。
        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        CodeBlockBitMask unassignedCountry = new CodeBlockBitMask();


        private BaseStationMessage CreateBaseStationCallsignMessage(DateTime messageReceivedUtc, GPS gpsSMessage)
        {
            BaseStationMessage message = new BaseStationMessage();
            message.MessageLogged = messageReceivedUtc;
            message.MessageGenerated = messageReceivedUtc;
            message.MessageType = BaseStationMessageType.Transmission;
            message.TransmissionType = BaseStationTransmissionType.IdentificationAndCategory;
            message.Icao24 = (gpsSMessage.CarID & unassignedCountry.SignificantBitMask).ToString("X6");
            message.Callsign = "DDG" + gpsSMessage.CarID;

            return message;
        }

        private BaseStationMessage CreateBaseStationSurfacePositionMessage(DateTime messageReceivedUtc, GPS gpsSMessage)
        {
            BaseStationMessage message = new BaseStationMessage();
            message.MessageLogged = messageReceivedUtc;
            message.MessageGenerated = messageReceivedUtc;
            message.MessageType = BaseStationMessageType.Transmission;
            message.TransmissionType = BaseStationTransmissionType.SurfacePosition;
            message.Icao24 = (gpsSMessage.CarID & unassignedCountry.SignificantBitMask).ToString("X6");
            //message.Icao24 = txtICAO24.Text;
            message.Altitude = (int)Math.Round(UnitConverter.ConvertHeight((double)gpsSMessage.Height, HeightUnit.Metres, HeightUnit.Feet), 0);
            message.GroundSpeed = Round.GroundSpeed((float)UnitConverter.ConvertSpeed(gpsSMessage.Speed, SpeedUnit.KilometresPerHour, SpeedUnit.Knots));
            message.Track = Round.Track((float)gpsSMessage.OriginalDirection);
            message.Latitude = Round.Coordinate((double)gpsSMessage.OriginalLat);
            message.Longitude = Round.Coordinate((double)gpsSMessage.OriginalLng);
            message.OnGround = true;

            return message;
        }

        private void SendClick(object sender, EventArgs e)
        {
            GPS gpsMessage = new GPS();
            gpsMessage.CarID = uint.Parse(txtOriginalID.Text);
            gpsMessage.OriginalLat = decimal.Parse(txtWGSLat.Text);
            gpsMessage.OriginalLng = decimal.Parse(txtWGSLng.Text);
            gpsMessage.Speed = 0;
            gpsMessage.Height = 0;
            BaseStationMessage message;

            message = CreateBaseStationCallsignMessage(DateTime.UtcNow, gpsMessage);
            SendBroadMessage(message);
            message = CreateBaseStationSurfacePositionMessage(DateTime.UtcNow, gpsMessage);
            SendBroadMessage(message);

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
            m_Timer.Change(System.Threading.Timeout.Infinite, m_Speed);
        }

        private void StartTimer()
        {
            m_Timer.Change(START_DELAY, m_Speed);
        }

        private void SpeedTimer(string command)
        {
            switch(command) {
                case "-":

                    m_Speed += DEFAULT_STEP;

                    break;
                case "+":

                    m_Speed -= DEFAULT_STEP;

                    if(rdTrail.Checked) {
                        if(m_Speed <= 0) m_Speed = DEFAULT_SPEED_TRAIL;
                    } else {
                        if(m_Speed <= 0) m_Speed = DEFAULT_SPEED_WAYPOINT;
                    }
                    break;

                default:
                    if(rdTrail.Checked) {
                        m_Speed = DEFAULT_SPEED_TRAIL;
                    } else {
                        m_Speed =DEFAULT_SPEED_WAYPOINT ;
                    }
                    break;
            }
            m_Timer.Change(0, m_Speed);
        }

        private void btnSpeedNomal_Click(object sender, EventArgs e)
        {
            SpeedTimer("");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetTimer();
        }

        private void ResetTimer()
        {
            PauseTimer();
            m_sendIndex = 0;
            if(rdTrail.Checked) {
                m_Speed =DEFAULT_SPEED_TRAIL ;
            } else {
                m_Speed = DEFAULT_SPEED_WAYPOINT;
            }
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
            if(this.txtShowInfo.InvokeRequired) {
                this.txtShowInfo.Invoke(new DeleSetTxtInfo(SetClear));
            } else {

                this.txtShowInfo.Clear();

            }
        }

        private void SetScroll()
        {
            if(this.txtShowInfo.InvokeRequired) {
                this.txtShowInfo.Invoke(new DeleSetTxtInfo(SetScroll));
            } else {

                txtShowInfo.SelectionStart = txtShowInfo.Text.Length;

                txtShowInfo.ScrollToCaret();

            }
        }

        private void txtShowInfo_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnChangeID_Click(object sender, EventArgs e)
        {
            txtICAO24.Text = (int.Parse(txtOriginalID.Text) & unassignedCountry.SignificantBitMask).ToString("X6");
        }

        private void lblChange_Click(object sender, EventArgs e)
        {
            txtICAO24.Text = (int.Parse(txtOriginalID.Text) & unassignedCountry.SignificantBitMask).ToString("X6");
        }

        private void lblChangeCoordinate_Click(object sender, EventArgs e)
        {
            ChinaMapShift.Location wgsLoc = new ChinaMapShift.Location();
            ChinaMapShift.Location gcjLoc = new ChinaMapShift.Location();
            try {
                wgsLoc.Lat = double.Parse(txtWGSLat.Text);
                wgsLoc.Lng = double.Parse(txtWGSLng.Text);

                gcjLoc = ChinaMapShift.TransformFromWGSToGCJ(wgsLoc);

                txtGCJLat.Text = gcjLoc.Lat.ToString();
                txtGCJLng.Text = gcjLoc.Lng.ToString();

            } catch {
                MessageBox.Show("坐标输入格式有误");
            }

        }

        private void frmServer_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Convert.ToInt32(textBox1.Text, 16).ToString());
            LoadWaypoints();
        }

        private void btnGenerateWaypoints_Click(object sender, EventArgs e)
        {
            InitWaypoints();
        }

        private void CleanSBS()
        {
            TrailName = string.Empty;
            m_listSBS.Clear();
        }
        private void btnListTrail_Click(object sender, EventArgs e)
        {
            PauseTimer();
            frmTrailList trailListView = new frmTrailList(this);
            if(trailListView.ShowDialog() == DialogResult.OK) {
                //MessageBox.Show("OK");
                if(!string.IsNullOrEmpty(TrailName)) {
                    LoadSBSTrail(TrailName);
                    if(m_isListing) {
                        if(m_sessionTable.Count > 0) {
                            ResetTimer();
                        }

                    } else {
                        CleanSBS();
                    }
                }
            } else {
                //MessageBox.Show("Cancle");
                CleanSBS();
            }
        }

        private void ChangeTimerSpeedDefault()
        {
            if(rdTrail.Checked) {
                m_Speed = DEFAULT_SPEED_TRAIL;
            } else {
                m_Speed = DEFAULT_SPEED_WAYPOINT;
            }
        }

        private void rdTrail_CheckedChanged(object sender, EventArgs e)
        {
            PauseTimer();
            ChangeTimerSpeedDefault();
            if(m_isListing) {
                if(m_sessionTable.Count > 0) {
                    ResetTimer();
                }
            }
        }
    }
}
