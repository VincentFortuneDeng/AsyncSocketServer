using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SenserModels;
using SenserModels.Entity;

namespace SerialPortController
{
    public abstract class SenserController
    {
        protected Dictionary<string, SerialListener> m_listenerDictionary;

        protected ISenser currentSenser;

        /// <summary>
        /// 输出线程
        /// </summary>
        protected Thread reportThread;

        /// <summary>
        /// 报告同步对象
        /// </summary>
        protected object reportSignal;

        /// <summary>
        /// 距离传感器列表
        /// </summary>
        protected System.Collections.Generic.Dictionary<byte, ISenser> senserDictionary;

        public System.Collections.Generic.Dictionary<byte, ISenser> SenserDictionary
        {
            get { return senserDictionary; }
        }

        /// <summary>
        /// 获得距离线程
        /// </summary>
        protected Thread pollThread;

        protected abstract void LoadSetting();

        /// <summary>
        /// 获取数据同步对象
        /// </summary>
        protected object pollSignal;

        /// <summary>
        /// 轮询间隔时间
        /// </summary>
        protected int pollInterval;

        public int PollInterval
        {
            get
            {
                return pollInterval;
            }

            set
            {
                pollInterval = value;
            }
        }

        /// <summary>
        /// 添加通讯控制器
        /// </summary>
        /// <param name="setting">设置</param>
        protected abstract SerialListener AddComController(string setting);

        /// <summary>
        /// 创建传感器列表
        /// </summary>
        protected abstract void AddSenser(SerialListener dadComController, string addressList);

        /// <summary>
        /// 输出线程
        /// </summary>
        protected abstract void ReportEventHandler();

        /// <summary>
        /// 获取数据线程
        /// </summary>
        protected abstract void PollHandler();

        protected abstract ISenser NextSenser();

        /// <summary>
        /// 轮询传感器索引
        /// </summary>
        protected int indexSenser;

        /// <summary>
        /// 报告事件
        /// </summary>
        public event System.EventHandler<EventArgs> ReportEvent;

        /// <summary>
        /// 拆解数据
        /// </summary>
        /// <param name="data">要拆分的数据 返回包含时间的数据</param>
        protected abstract void SplitRecordData(DeviceType deviceType, byte[] data, int singleRecordLength);

        protected virtual void RaiseComOnEvent(ComOnEventArgs e)
        {
            if (this.ComOnEvent != null)
            {
                this.ComOnEvent(this, e);
            }
        }

        /// <summary>
        /// 串口异常
        /// </summary>
        public event System.EventHandler<ComOnEventArgs> ComOnEvent;

        protected virtual void RaiseWorkStateEvent(WorkStateEventArgs e)
        {
            if (this.WorkStateEvent != null)
            {
                this.WorkStateEvent(this, e);
            }
        }

        protected virtual void RaiseReportEvent(EventArgs e)
        {
            if (this.ReportEvent != null)
            {
                this.ReportEvent(this, e);
            }
        }

        public event System.EventHandler<WorkStateEventArgs> WorkStateEvent;

        protected virtual void ComController_ComOnEvent(object sender, ComOnEventArgs e)
        {
            if (!e.ComOn)
            {
                SetSenserFault();
            }
            RaiseComOnEvent(e);
        }

        protected virtual void SetSenserFault()
        {
            foreach (ISenser senser in this.senserDictionary.Values)
            {
                senser.WorkState = DeviceWorkState.Fault;
            }
        }

        protected virtual void Senser_WorkStateEvent(object sender, WorkStateEventArgs e)
        {
            RaiseWorkStateEvent(e);
        }

        protected ReportWorkMode workMode;

        /// <summary>
        /// 响应事件
        /// </summary>
        /*public event System.EventHandler<ResponseEventArgs> Response;*/

        /// <summary>
        /// </summary>
        protected string baseSettings;

        public string BaseSettings
        {
            get
            {
                return baseSettings;
            }

            set
            {
                baseSettings = value;
                Properties.Settings.Default.BaseSetting = baseSettings;
                Properties.Settings.Default.Save();
            }
        }

        protected void RegisterComEvent()
        {

            foreach (SerialListener comController in this.m_listenerDictionary.Values)
            {

                comController.ReportEvent += new EventHandler<EventArgs>(ComController_ReportEvent);
            }

        }

        protected void UnRegisterComEvent()
        {

            foreach (SerialListener comController in this.m_listenerDictionary.Values)
            {
                comController.ReportEvent -= new EventHandler<EventArgs>(ComController_ReportEvent);
            }


        }

        protected abstract void ComController_ReportEvent(object sender, EventArgs e);
        /*
        protected void StartPoll()
        {
            this.pollThread = new Thread(new ThreadStart(this.PollHandler));
            this.pollThread.IsBackground = true;
            this.pollThread.Start();

        }*/
        /*
        protected void StopPoll()
        {
            this.pollThread.Abort();
            this.pollThread = null;
        }*/

        public virtual bool ChangeWorkMode(ReportWorkMode workMode)
        {
            if (this.workMode != workMode)
            {
                this.workMode = workMode;

                foreach (SerialListener comController in this.m_listenerDictionary.Values)
                {
                    comController.ChangeWorkMode(workMode);
                }

                switch (this.workMode)
                {
                    case ReportWorkMode.Initiative:
                        this.RegisterComEvent();
                        //this.StopPoll();
                        this.pollRun = false;
                        break;

                    case ReportWorkMode.Passive:
                        this.UnRegisterComEvent();
                        //this.StartPoll();
                        this.pollRun = true;
                        break;

                    default:

                        break;
                }
            }

            return true;
        }

        public void Start()
        {
            if (!this.run)
            {
                foreach (SerialListener comController in this.m_listenerDictionary.Values)
                {
                    comController.Start();
                }

                switch (this.workMode)
                {
                    case ReportWorkMode.Initiative:
                        this.RegisterComEvent();
                        //this.StopPoll();
                        this.pollRun = false;
                        break;

                    case ReportWorkMode.Passive:
                        this.UnRegisterComEvent();
                        //this.StartPoll();
                        this.pollRun = true;
                        break;

                    default:

                        break;
                }
            }

            this.run = true;
        }

        public void Stop()
        {
            if (this.run)
            {
                foreach (SerialListener comController in this.m_listenerDictionary.Values)
                {
                    comController.Stop();
                }

                foreach (Senser senser in this.senserDictionary.Values)
                {
                    senser.Reset();
                }

                switch (this.workMode)
                {
                    case ReportWorkMode.Passive:
                        //this.RegisterComEvent();
                        //this.StopPoll();
                        this.pollRun = false;
                        break;

                    case ReportWorkMode.Initiative:
                        this.UnRegisterComEvent();
                        //this.StartPoll();
                        //this.pollRun = true;
                        break;

                    default:

                        break;
                }
            }

            this.run = false;
        }

        protected bool run;

        public bool Run
        {
            get { return run; }
            set { run = value; }
        }

        public void CloseThreads()
        {
            if (this.pollThread != null)
            {
                this.pollThread.Abort();
            }

            if (this.reportThread != null)
            {
                this.reportThread.Abort();
            }
        }

        public void Close()
        {
            this.Stop();

            foreach (SerialListener comController in this.m_listenerDictionary.Values)
            {
                comController.Close();
            }

            CloseThreads();
        }

        protected bool pollRun;

        public bool PollRun
        {
            get { return pollRun; }
            set { pollRun = value; }
        }

        public SenserController()
        {
            InitMembers();
        }

        protected abstract void InitMembers();

        public SenserController(ReportWorkMode workMode, params int[] pollInterval)
            : this()
        {
            if (workMode == ReportWorkMode.Passive && pollInterval == null)
            {
                throw new ArgumentNullException("pollInterval", "被动工作方式下 请指定轮询间隔");
            }

            this.workMode = workMode;

            if (0 != pollInterval.Length)
            {
                if (pollInterval[0] > 0)
                {
                    this.pollInterval = pollInterval[0];
                }
            }

            else
            {
                this.pollInterval = 1;
            }

            this.m_listenerDictionary = new Dictionary<string, SerialListener>();

            this.senserDictionary = new Dictionary<byte, ISenser>();

            LoadSetting();

            string[] settings = this.BaseSettings.Split(';');

            foreach (string setting in settings)
            {
                if (!string.IsNullOrEmpty(setting))
                {
                    string[] comSettings = setting.Split(':');
                    SerialListener serialController = (SerialListener)this.AddComController(comSettings[0]);

                    this.AddSenser(serialController, comSettings[1]);
                }
            }

            if (this.workMode == ReportWorkMode.Initiative)
            {
                this.RegisterComEvent();
            }

            this.currentSenser = null;

            this.indexSenser = 0;

            this.pollSignal = new object();

            this.reportSignal = new object();

            this.reportThread = new Thread(new ThreadStart(ReportEventHandler));

            this.reportThread.IsBackground = true;

            this.reportThread.Start();

            this.pollThread = new Thread(new ThreadStart(PollHandler));

            this.pollThread.IsBackground = true;

            this.pollThread.Start();

            if (this.workMode == ReportWorkMode.Passive)
            {
                this.pollRun = true;
            }

            this.run = true;

        }//End SenserController
    }
}
