using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using SenserModels.Data;
using SenserModels.Entity;

namespace SenserModels
{
    public class Analyzer
    {
        /// <summary>
        /// 生成分析数据
        /// </summary>
        /// <param name="dataBase">基础数据</param>
        public static AnalyticalData GenerateAnalyseData(BaseData dataBase)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 输出数据
        /// </summary>
        /// <param name="fileType">输出文件类型</param>
        /// <param name="filePath">文件位置</param>
        /// <param name="dataTable">数据内容</param>
        public bool Output(OutputFileType fileType, string filePath, DataTable dataTable)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 生成机组数据
        /// </summary>
        public bool GenerateMotorUnitData(MotorUnitKey motorUnitKey)
        {
            return DatabaseProvider.GetInstance().GenerateMotorUnitData(motorUnitKey);
        }

        public bool GenerateMotorUnitData(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateMotorUnitData(stationKey);
        }

        public bool GenerateStationInfo(StationKey stationKey)
        {
            return DatabaseProvider.GetInstance().GenerateStationInfoData(stationKey);
        }
    }


}
