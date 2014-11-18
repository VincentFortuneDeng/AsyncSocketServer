using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using SenserModels.Data;
using System.Windows.Forms;
using SenserModels.Entity;
//using Microsoft.Office.Interop.Excel;

namespace SenserModels
{
    public class DataCenter
    {
        private DataMonitor dataMonitor;

        public System.Data.DataTable CollectingData
        {
            get { return dataMonitor.CollectingData; }
            //set { dataMonitor = value; }
        }
        private DataMonitorTableAdapters.TableAdapterManager tableAdapterManager;


        //private LocalStoreEntities dataMonitor;
        //private DataTable dataTable;

        public DataCenter(RangeSaveType rangeEle)
        {
            ranges = rangeEle;
            this.dataMonitor = new DataMonitor();
            tableAdapterManager = new DataMonitorTableAdapters.TableAdapterManager();

            tableAdapterManager.CollectingDataTableAdapter = new DataMonitorTableAdapters.CollectingDataTableAdapter();
            tableAdapterManager.CollectingDataTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.CatalogTableAdapter = new DataMonitorTableAdapters.CatalogTableAdapter();
            tableAdapterManager.CatalogTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.MotorUnitTableAdapter = new DataMonitorTableAdapters.MotorUnitTableAdapter();
            tableAdapterManager.MotorUnitTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.WellInfoTableAdapter = new DataMonitorTableAdapters.WellInfoTableAdapter();
            tableAdapterManager.WellInfoTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.StationInfoTableAdapter = new DataMonitorTableAdapters.StationInfoTableAdapter();
            tableAdapterManager.StationInfoTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);
            //tableAdapterManager.CollectingDataTableAdapter.Adapter.UpdateBatchSize = 2;
            tableAdapterManager.Connection.ConnectionString = SenserModels.Properties.Settings.Default.LocalStoreConnectionString;
            //tableAdapterManager.CatalogTableAdapter.Fill(this.dataMonitor.Catalog);
            //LoadSettings();
        }

        public DataCenter()
        {
            this.dataMonitor = new DataMonitor();
            tableAdapterManager = new DataMonitorTableAdapters.TableAdapterManager();

            tableAdapterManager.CollectingDataTableAdapter = new DataMonitorTableAdapters.CollectingDataTableAdapter();
            tableAdapterManager.CollectingDataTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.CatalogTableAdapter = new DataMonitorTableAdapters.CatalogTableAdapter();
            tableAdapterManager.CatalogTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.MotorUnitTableAdapter = new DataMonitorTableAdapters.MotorUnitTableAdapter();
            tableAdapterManager.MotorUnitTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.WellInfoTableAdapter = new DataMonitorTableAdapters.WellInfoTableAdapter();
            tableAdapterManager.WellInfoTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);

            tableAdapterManager.StationInfoTableAdapter = new DataMonitorTableAdapters.StationInfoTableAdapter();
            tableAdapterManager.StationInfoTableAdapter.Adapter.RowUpdated += new EventHandler<System.Data.Common.RowUpdatedEventArgs>(Adapter_RowUpdated);
            //tableAdapterManager.CollectingDataTableAdapter.Adapter.UpdateBatchSize = 2;
            tableAdapterManager.Connection.ConnectionString = SenserModels.Properties.Settings.Default.LocalStoreConnectionString;
            //tableAdapterManager.CatalogTableAdapter.Fill(this.dataMonitor.Catalog);
            //LoadSettings();
        }

        private void Adapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        {
            if (e.Status == UpdateStatus.ErrorsOccurred)
            {
                e.Row.RowError = e.Errors.Message;
                e.Status = UpdateStatus.SkipCurrentRow;
            }
        }
        /*
        public DataTable DataTable
        {
            get { return dataTable; }
            set { dataTable = value; }
        }*/
        /*
        public DataTable Copy()
        {
            return this.dataTable.Copy();
        }*/

        public void OutputExcel(DataView dataView, string caption)
        {
            try
            {
                if (dataView.Count >= 1)
                {
                    //dv为要输出到Excel的数据，caption为标题名称 
                    //GC.Collect();
                    Microsoft.Office.Interop.Excel.Application excel;// = new Application(); 

                    int captionRowIndex = 2;
                    int captionColIndex = 1;

                    int dataRowStartIndex = captionRowIndex + 2;
                    int dataColStartIndex = 1;

                    int rowIndex = dataRowStartIndex;
                    int colIndex = dataColStartIndex;

                    Microsoft.Office.Interop.Excel.Workbook xBk;
                    Microsoft.Office.Interop.Excel.Worksheet xSt;

                    excel = new Microsoft.Office.Interop.Excel.Application();

                    // 
                    //显示效果 
                    // 
                    excel.Visible = true;

                    xBk = excel.Workbooks.Add(true);//添加工作簿

                    xSt = (Microsoft.Office.Interop.Excel.Worksheet)xBk.ActiveSheet;

                    // 
                    //取得标题 
                    // 
                    foreach (DataColumn col in dataView.Table.Columns)
                    {
                        if (col.ColumnName != "NodeID")
                        {
                            excel.Cells[dataRowStartIndex, colIndex] = col.ColumnName;
                            xSt.get_Range(excel.Cells[dataRowStartIndex, colIndex], excel.Cells[dataRowStartIndex, colIndex]).HorizontalAlignment =
                                Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//设置标题格式为居中对齐 
                            colIndex++;
                        }
                    }

                    // 
                    //取得表格中的数据 
                    // 
                    foreach (DataRowView row in dataView)
                    {
                        colIndex = dataColStartIndex;
                        rowIndex++;

                        foreach (DataColumn col in dataView.Table.Columns)
                        {
                            if (col.ColumnName != "NodeID")
                            {
                                if (row[col.ColumnName] != null)
                                {
                                    if (col.DataType == System.Type.GetType("System.DateTime"))
                                    {
                                        excel.Cells[rowIndex, colIndex] =
                                            (Convert.ToDateTime(row[col.ColumnName].ToString())).ToString("yyyy-MM-dd");

                                        xSt.get_Range(excel.Cells[rowIndex, colIndex], excel.Cells[rowIndex, colIndex]).HorizontalAlignment =
                                            Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//设置日期型的字段格式为居中对齐 
                                    }

                                    else if (col.DataType == System.Type.GetType("System.String"))
                                    {
                                        excel.Cells[rowIndex, colIndex] = "'" + row[col.ColumnName].ToString();
                                        xSt.get_Range(excel.Cells[rowIndex, colIndex], excel.Cells[rowIndex, colIndex]).HorizontalAlignment =
                                            Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//设置字符型的字段格式为居中对齐 
                                    }

                                    else
                                    {
                                        excel.Cells[rowIndex, colIndex] = row[col.ColumnName].ToString();
                                    }
                                }

                                else
                                {
                                    excel.Cells[rowIndex, colIndex] = "";
                                }
                                colIndex++;
                            }
                        }
                    }
                    colIndex--;
                    // 
                    //加一个合计行 
                    // 
                    int rowSum = rowIndex;// +1;

                    /*
                     int rowSum = rowIndex+1;
                     int colSum = 2;
                    excel.Cells[rowSum, colSum] = "合计";
                    xSt.get_Range(excel.Cells[rowSum, colSum], excel.Cells[rowSum, colSum]).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    // 
                    //设置选中的部分的颜色 
                    // 
                    xSt.get_Range(excel.Cells[rowSum, colSum], excel.Cells[rowSum, colIndex]).Select();
                    xSt.get_Range(excel.Cells[rowSum, colSum], excel.Cells[rowSum, colIndex]).Interior.ColorIndex = 19;//设置为浅黄色，共计有56种 
                    */

                    // 
                    //取得整个报表的标题 
                    // 
                    excel.Cells[captionRowIndex, captionColIndex] = caption;

                    // 
                    //设置整个报表的标题格式 
                    // 
                    xSt.get_Range(excel.Cells[captionRowIndex, captionColIndex], excel.Cells[captionRowIndex, captionColIndex]).Font.Bold = true;
                    xSt.get_Range(excel.Cells[captionRowIndex, captionColIndex], excel.Cells[captionRowIndex, captionColIndex]).Font.Size = 22;

                    // 
                    //设置报表表格为最适应宽度 
                    // 
                    xSt.get_Range(excel.Cells[dataRowStartIndex, dataColStartIndex], excel.Cells[rowSum, colIndex]).Select();
                    xSt.get_Range(excel.Cells[dataRowStartIndex, dataColStartIndex], excel.Cells[rowSum, colIndex]).Columns.AutoFit();

                    // 
                    //设置整个报表的标题为跨列居中 
                    // 
                    xSt.get_Range(excel.Cells[captionRowIndex, captionColIndex], excel.Cells[captionRowIndex, colIndex]).Select();
                    xSt.get_Range(excel.Cells[captionRowIndex, captionColIndex], excel.Cells[captionRowIndex, colIndex]).HorizontalAlignment =
                        Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenterAcrossSelection;

                    // 
                    //绘制边框 
                    // 
                    xSt.get_Range(excel.Cells[dataRowStartIndex, dataColStartIndex], excel.Cells[rowSum, colIndex]).Borders.LineStyle = 1;
                    xSt.get_Range(excel.Cells[dataRowStartIndex, dataColStartIndex], excel.Cells[rowSum, dataColStartIndex]).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Weight =
                        Microsoft.Office.Interop.Excel.XlBorderWeight.xlThick;//设置左边线加粗 
                    xSt.get_Range(excel.Cells[dataRowStartIndex, dataColStartIndex], excel.Cells[dataRowStartIndex, colIndex]).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Weight =
                        Microsoft.Office.Interop.Excel.XlBorderWeight.xlThick;//设置上边线加粗 
                    xSt.get_Range(excel.Cells[dataRowStartIndex, colIndex], excel.Cells[rowSum, colIndex]).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Weight =
                        Microsoft.Office.Interop.Excel.XlBorderWeight.xlThick;//设置右边线加粗 
                    xSt.get_Range(excel.Cells[rowSum, dataColStartIndex], excel.Cells[rowSum, colIndex]).Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight =
                        Microsoft.Office.Interop.Excel.XlBorderWeight.xlThick;//设置下边线加粗 

                    xSt.PageSetup.CenterHorizontally = true;
                    xSt.PrintPreview(Type.Missing);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ExportExcel(DataGridView gridView, bool isShowExcle)
        {
            try
            {
                if (gridView.Rows.Count >= 1)
                {
                    //建立Excel对象 
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Application.Workbooks.Add(true);
                    excel.Visible = isShowExcle;
                    //生成字段名称 
                    for (int i = 0; i < gridView.ColumnCount; i++)
                    {
                        excel.Cells[1, i + 1] = gridView.Columns[i].HeaderText;

                        //设置列宽
                        ((Microsoft.Office.Interop.Excel.Range)excel.Columns[i + 1, Type.Missing]).AutoFit();
                        ((Microsoft.Office.Interop.Excel.Range)excel.Columns[i + 1, Type.Missing]).NumberFormatLocal = "@";
                    }
                    //设置列宽 A为Excel列编号 

                    //((Microsoft.Office.Interop.Excel.Range)excel.Columns["A", Type.Missing]).AutoFit();
                    //填充数据 
                    for (int i = 0; i < gridView.RowCount; i++)
                    {
                        for (int j = 0; j < gridView.ColumnCount; j++)
                        {
                            if (gridView[j, i].Value != null)
                            {
                                excel.Cells[i + 2, j + 1] = gridView[j, i].Value.ToString();
                            }

                            else
                            {
                                excel.Cells[i + 2, j + 1] = "";
                            }
                        }
                    }
                    Microsoft.Office.Interop.Excel.Worksheet worksheet =
                        ((Microsoft.Office.Interop.Excel.Worksheet)excel.Workbooks[1].Worksheets[1]);
                    //worksheet.Columns.EntireColumn.AutoFit();
                    //worksheet.get_Range(worksheet.Cells[1, 1]).NumberFormatLocal = "@";
                    worksheet.PrintPreview(Type.Missing);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private RangeSaveType ranges;
        /*
        private void LoadSettings()
        {
            baseSettings = Config.Configs<Config.BaseSettings>.GetConfig();
        }*/

        private const float vRatio = 1f;
        private const float iRatio = 70f;

        public bool InsertRecord(RtuRecordKey recordKey, byte[] recordData)
        {
            List<int> values = GenerateIntValues(recordData);
            try
            {
                DataMonitor.CollectingDataRow collectingDataRow = this.dataMonitor.CollectingData.FindByNodeIDTimeStamp(recordKey.NodeID, recordKey.DataDateTime);
                //DataMonitor.CatalogRow catalogRow = this.dataMonitor.Catalog.FindByNodeID(recordKey.NodeID);
                if (collectingDataRow == null)
                {
                    collectingDataRow = dataMonitor.CollectingData.NewCollectingDataRow();
                    collectingDataRow.NodeID = recordKey.NodeID;
                    collectingDataRow.TimeStamp = recordKey.DataDateTime;
                    this.dataMonitor.CollectingData.AddCollectingDataRow(collectingDataRow);
                }

                switch (recordKey.DeviceType)
                {
                    case DeviceType.ELEP:

                        //LoadSettings();
                        int vRange = this.ranges.FirstValue;
                        int iRange = this.ranges.SecondValue;


                        /*
                         * 
                         * 三相电压：
                                 实际值 =  读取值 / 10000 * 250 * 1
                            即： 实际值＝（读取值）／10000*（电压量程）*（电压变比）V 

                            三相电流：
                                 实际值 =  读取值 / 10000 * 5 * 70
                            即： 实际值＝（读取值）／10000*（电流量程）*（电流变比）A

                            三相功率：
                                 实际值 =  读取值/10000 * 250 * 5 * 1* 70          
                            即： 实际值＝ +（读取值）／10000*（电压量程）*（电流量程）*（电压变比）*（电流变比） W

                            总有功功率：
                                 实际值 = (读取值 /10000  * 3 * 250 * 5 * 1 * 70
                            即： 实际值＝ +（读取值）／10000 * 3 *（电压量程）*（电流量程）*（电压变比）*（电流变比） W

                            总无功功率：
                                 实际值 =  读取值 /10000 * 3 * 250 * 5 * 1 * 70
                            即： 实际值 ＝ +（读取值）／10000 * 3 *（电压量程）*（电流量程）*（电压变比）*（电流变比） Var

                            功率因数：
                                显示值 =  读取值/10000
                         * 
                         */
                        for (int i = 0; i < 12; i++)
                        {
                            if (i >= 9 && i <= 10) //总功率 无用功率
                            {/*
                              * 
                              * 总有功功率：
                                 实际值 = (读取值 /10000  * 3 * 250 * 5 * 1 * 70
                                即： 实际值＝ +（读取值）／10000 * 3 *（电压量程）*（电流量程）*（电压变比）*（电流变比） W
                              */
                                collectingDataRow[i + 2] = Math.Round(values[i] / 10000f * 3f * vRange * iRange * vRatio * iRatio / 1000f, 3);
                                //collectingDataRow[i + 2] = values[i];
                            }

                            else if (i >= 3 && i <= 5)//电压
                            {
                                collectingDataRow[i + 2] = Math.Round(values[i] / 10000f * vRange * vRatio, 3);
                            }

                            else if (i >= 6 && i <= 8)//电流
                            {
                                collectingDataRow[i + 2] = Math.Round(values[i] / 10000f * iRange * iRatio, 3);
                            }

                            else if (i >= 0 && i <= 2)//三相功率
                            {
                                collectingDataRow[i + 2] = Math.Round(values[i] / 10000f * vRange * iRange * vRatio * iRatio / 1000f, 3);

                                /*if (Math.Abs((double)collectingDataRow[i + 2]) > 1000)
                                {
                                    MessageBox.Show(string.Format("{0}原始值:{1},{2},{3},{4},{5}", i, values[i], vRange, iRange, vRatio, iRatio));
                                }*/
                            }

                            else//功率因数
                            {
                                collectingDataRow[i + 2] = Math.Round(values[i] / 10000f, 3);

                            }
                            //collectingDataRow[i + 2] = (i == 9 || i == 10) ? Math.Round(values[i] / 10000f / 3f, 3) : Math.Round(values[i]* this.baseSettings.DeviceRange[ DeviceType.ELEP].SecondValue / 10000f, 3);
                        }
                        break;


                    case DeviceType.ULTF:
                        collectingDataRow[recordKey.DeviceType.ToString()] = Math.Round(values[0] / 10f, 3);
                        break;

                    case DeviceType.PUIP:
                    case DeviceType.PUOP:
                        collectingDataRow[recordKey.DeviceType.ToString()] = Math.Round(values[0] / 1000f, 3);

                        break;

                    default:
                        break;
                }
                //Trace.WriteLine("recordKey");
                return true;
            }

            catch //(Exception ex)
            {
                //MessageBox.Show(ex.StackTrace);
                //throw ex;
                return false;
            }
        }

        private List<int> GenerateIntValues(byte[] data)
        {

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            List<int> values = new List<int>();

            if (data.Length > 2)
            {
                /*
                string outByteString = "原始:";
                string outValueString = "转换:";*/
                for (int i = 0; i < data.Length; i += 2)
                {
                    //outByteString += "0x" + Convert.ToString(data[i], 16) + " " + "0x" + Convert.ToString(data[i + 1], 16) + " ";
                    if (i >= 6 && i <= 16)
                    {
                        values.Add((UInt16)(data[i] << 8 | data[i + 1]));//无符号
                    }

                    else
                    {
                        values.Add((Int16)(data[i] << 8 | data[i + 1]));//有符号
                    }
                    //outValueString += "Dec" + Convert.ToString(values[values.Count - 1], 10) + " ";
                }
                //MessageBox.Show(outValueString + outByteString);
            }

            else
            {
                values.Add((UInt16)(data[0] << 8 | data[1]));//
            }

            return values;
        }

        public int Update()
        {
            //throw new NotImplementedException();
            //tableAdapterManager.CollectingDataTableAdapter.Update(
            return tableAdapterManager.UpdateAll(this.dataMonitor);
        }

        public int UpdateCollectingData()
        {
            //throw new NotImplementedException();
            int retResult = tableAdapterManager.CollectingDataTableAdapter.Update(this.dataMonitor.CollectingData);
            //this.tableAdapterManager.CollectingDataTableAdapter.ClearBeforeFill = true;
            //this.tableAdapterManager.CollectingDataTableAdapter.Fill(this.dataMonitor.CollectingData);
            return retResult;
            //return tableAdapterManager.UpdateAll(this.dataMonitor);

        }

        public int UpdateWellInfoBySystem(DataTable dt)
        {
            //this.dataMonitor.WellInfo = dt;
            //throw new NotImplementedException();
            int retResult = tableAdapterManager.CollectingDataTableAdapter.Update(this.dataMonitor.CollectingData);
            //this.tableAdapterManager.CollectingDataTableAdapter.ClearBeforeFill = true;
            //this.tableAdapterManager.CollectingDataTableAdapter.Fill(this.dataMonitor.CollectingData);
            return retResult;
            //return tableAdapterManager.UpdateAll(this.dataMonitor);

        }

        public int UpdateCatalog()
        {
            //throw new NotImplementedException();
            return tableAdapterManager.CatalogTableAdapter.Update(this.dataMonitor.Catalog);
            //return tableAdapterManager.UpdateAll(this.dataMonitor);
        }

        public void TestDataBase()
        {
            MessageBox.Show(DatabaseProvider.GetInstance().TestDataConnect().ToString());
        }

        public bool GenerateMotorUnitSchema(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateMotorUnitSchema(stationKey);
        }

        public bool GenerateWellInfoSchema(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateWellInfoSchema(stationKey);
        }

        public bool GenerateStationInfoSchema(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateStationInfoSchema(stationKey);
        }

        public bool GenerateStationInfoData(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateStationInfoData(stationKey);
        }

        public DataTable GetStationInfoData(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GetStationInfoData(stationKey);
        }

        public bool GenerateMotorUnitData(MotorUnitKey motorUnitKey)
        {
            return DatabaseProvider.GetInstance().GenerateMotorUnitData(motorUnitKey);
        }

        public DataTable GetCollectingData(MotorUnitKey motorUnitKey)
        {
            return DatabaseProvider.GetInstance().GetCollectingData(motorUnitKey);
        }

        public DataTable GetMotorUnitData(MotorUnitKey motorUnitKey)
        {
            return DatabaseProvider.GetInstance().GetMotorUnitData(motorUnitKey);
        }

        public bool GenerateMotorUnitData(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateMotorUnitData(stationKey);
        }

        public DataTable GetMotorUnitData(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GetMotorUnitData(stationKey);
        }

        public bool ClearNullData(Entity.StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().ClearNullData(stationKey);
        }

        public bool CorrectWellInfo(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().CorrectWellInfo(stationKey);
        }

        public DataTable GetWellInfo(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GetWellInfo(stationKey);
        }

        public void UpdateWellInfo(DataTable wellInfo)
        {
            DatabaseProvider.GetInstance().UpdateWellInfo(wellInfo);
        }

        public bool UpdateCollectingData(MotorUnitKey motorUnitKey, float[] updateData)
        {
            return DatabaseProvider.GetInstance().UpdateCollectingData(motorUnitKey, updateData);
        }
    }
}
