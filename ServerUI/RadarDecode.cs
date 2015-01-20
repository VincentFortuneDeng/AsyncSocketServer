using ASTERIXDecode;
//using ASTERIXDecode;
//using ASTERIXDecode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class RadarDecode : Form
    {
        Thread ListenForDataThread;
        public RadarDecode()
        {
            InitializeComponent();
        }

        private void RadarDecode_Load(object sender, EventArgs e)
        {
            ASTERIX.ReinitializeSocket();

            ListenForDataThread = new Thread(new ThreadStart(ASTERIX.ListenForData));
        }

        private void RadarDecode_DoubleClick(object sender, EventArgs e)
        {

        }

        private void btnUDP_Click(object sender, EventArgs e)
        {

            ListenForDataThread.Start();
        }



        private void btnFile_Click(object sender, EventArgs e)
        {
            ASTERIX.DecodeAsterixData(@"F:\文件交换\云同步\2015-01\真实数据点\R00_03.dat");
            //FrmDetailedView MyDetailedView = new FrmDetailedView();
            //MyDetailedView.WhatToDisplay = FrmDetailedView.DisplayType.CAT01I040;
            //MyDetailedView.Show();

            //MyDetailedView = new FrmDetailedView();
            //MyDetailedView.WhatToDisplay = FrmDetailedView.DisplayType.CAT01I070;
            //MyDetailedView.Show();

            //MyDetailedView = new FrmDetailedView();
            //MyDetailedView.WhatToDisplay = FrmDetailedView.DisplayType.CAT01I090;
            //MyDetailedView.Show();
            //Thread displayRadarThread = new Thread(new ThreadStart(Display));
            //displayRadarThread.Start();
            //Display();
        }

        private delegate void logTextDele(TextBox tbx, string text);

        private void logText(TextBox tbx, string text)
        {
            if(tbx.InvokeRequired) {
                logTextDele log = new logTextDele(logText);
                tbx.Invoke(log, new object[] { tbx, text });

            } else {

                tbx.AppendText(text);
            }
        }

        private void Display()
        {
            //ASTERIX.DecodeAsterixData(@".\index6.dat");
            //FrmDetailedView MyDetailedView = new FrmDetailedView();
            //MyDetailedView.WhatToDisplay = FrmDetailedView.DisplayType.CAT01I020;
            //MyDetailedView.Show();
            //listRadar.Items.Clear();

            //listRadar.Items.AddRange(SharedData.DataBox.Items);

            for(int i = 0; i < 1; i++) {
                //RadarLog.AppendText(SharedData.DataBox.Items[i].ToString() + Environment.NewLine);
                //RadarLog.AppendText(MainASTERIXDataStorage.CAT01Message[i].ToString() + Environment.NewLine);
                foreach(CAT01.CAT01DataItem item in MainASTERIXDataStorage.CAT01Message[i].CAT01DataItems) {
                    if(item.value != null) {

                        ////RadarLog.AppendText();
                        DisplayRadarDataItem(item);
                        //RadarLog.AppendText(Environment.NewLine);
                    }
                }
                logText(this.RadarLog, Environment.NewLine);
                logText(this.RadarLog, Environment.NewLine);
            }


        }

        private void DisplayRadarDataItem(CAT01.CAT01DataItem item)
        {
            //010
            //020
            //040
            //200
            //070
            //090
            switch(item.ID) {
                case "010":
                    ASTERIXDecode.ASTERIX.SIC_SAC_Time sic_sac_time = item.value as ASTERIXDecode.ASTERIX.SIC_SAC_Time;

                    logText(this.RadarLog, "SIC:" + sic_sac_time.SIC.ToString());
                    logText(this.RadarLog, " SAC:" + sic_sac_time.SAC.ToString());
                    logText(this.RadarLog, Environment.NewLine);
                    break;

                case "020":
                    CAT01I020UserData cat01I020 = item.value as CAT01I020UserData;
                    logText(this.RadarLog, " 报告类型:" + cat01I020.Type_Of_Report);
                    logText(this.RadarLog, " 模拟/真实报告:" + cat01I020.Simulated_Or_Actual_Report);
                    logText(this.RadarLog, " 雷达检测类型:" + cat01I020.Type_Of_Radar_Detection);
                    logText(this.RadarLog, " 天线源:" + cat01I020.Antena_Source);
                    logText(this.RadarLog, " 特殊位置标识:" + cat01I020.Special_Position_Ind);
                    logText(this.RadarLog, " 来自固定应答器:" + cat01I020.Data_Is_From_FFT);
                    logText(this.RadarLog, " 扩展标识1:" + cat01I020.Next_Extension_1);

                    logText(this.RadarLog, " 测试目标指示:" + cat01I020.Is_Test_Target_Indicator);
                    logText(this.RadarLog, " 特殊二次雷达代码:" + cat01I020.Special_SSR_Codes);
                    logText(this.RadarLog, " 军事紧急代码:" + cat01I020.Is_Military_Emergency);
                    logText(this.RadarLog, " 军事标识:" + cat01I020.Is_Military_Identification);
                    logText(this.RadarLog, " 扩展标识2:" + cat01I020.Next_Extension_2);
                    logText(this.RadarLog, Environment.NewLine);
                    break;

                case "161":
                    //CAT01I161UserData cat01I161 = item.value as CAT01I161UserData;
                    //logText(this.RadarLog," 报告类型:" + cat01I161);

                    break;

                case "040":
                    ASTERIXDecode.CAT01I040Types.CAT01I040MeasuredPosInPolarCoordinates cat01I040 = item.value as ASTERIXDecode.CAT01I040Types.CAT01I040MeasuredPosInPolarCoordinates;
                    //ASTERIXDecode.GeoCordSystemDegMinSecUtilities.LatLongClass latlong = cat01I040.LatLong as ASTERIXDecode.GeoCordSystemDegMinSecUtilities.LatLongClass;
                    logText(this.RadarLog, " 距离:" + cat01I040.Measured_Distance * 1.852 * 1000 + "米");
                    logText(this.RadarLog, " 方位:" + cat01I040.Measured_Azimuth + "度");
                    logText(this.RadarLog, " 纬度:" + cat01I040.LatLong.GetLatLongDecimal().LatitudeDecimal);
                    logText(this.RadarLog, " 经度:" + cat01I040.LatLong.GetLatLongDecimal().LongitudeDecimal);
                    logText(this.RadarLog, Environment.NewLine);
                    break;

                case "042":
                    break;

                case "200":
                    CAT01I200Types.CalculatedGSPandHDG_Type cat01I200 = item.value as CAT01I200Types.CalculatedGSPandHDG_Type;
                    logText(this.RadarLog, " 速度:" + cat01I200.GSPD * 1.852 + "公里/小时");
                    logText(this.RadarLog, " 航向:" + cat01I200.HDG + "度");
                    logText(this.RadarLog, Environment.NewLine);
                    break;

                case "070":
                    CAT01I070Types.CAT01070Mode3UserData cat01I070 = item.value as CAT01I070Types.CAT01070Mode3UserData;
                    logText(this.RadarLog, " Mode A代码有效:" + cat01I070.Code_Validated);
                    logText(this.RadarLog, " Mode A交织应答:" + cat01I070.Code_Garbled);
                    logText(this.RadarLog, " Mode A平滑应答:" + cat01I070.Code_Smothed_Or_From_Transponder);
                    logText(this.RadarLog, " Mode A应答代码:" + cat01I070.Mode3A_Code);
                    logText(this.RadarLog, Environment.NewLine);

                    break;

                case "090":
                    ASTERIXDecode.CAT01I090Types.CAT01I090FlightLevelUserData cat01I090 = item.value as ASTERIXDecode.CAT01I090Types.CAT01I090FlightLevelUserData;
                    logText(this.RadarLog, " Mode C高度有效:" + cat01I090.Code_Validated);
                    logText(this.RadarLog, " Mode C高度交织应答:" + cat01I090.Code_Garbled);
                    logText(this.RadarLog, " Mode C高度:" + cat01I090.FlightLevel * 100 * 0.3048 + "米");
                    logText(this.RadarLog, Environment.NewLine);

                    break;

                case "141":
                    break;

                case "130":
                    break;

                case "131":
                    break;

                case "120":
                    break;

                case "170":
                    break;

                case "210":
                    break;
            }
        }
    }
}
