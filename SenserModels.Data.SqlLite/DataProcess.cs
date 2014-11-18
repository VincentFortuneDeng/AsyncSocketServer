using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using CnTaxLawyer.Entity;
using SenserModels.Data;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using SenserModels.Entity;
namespace SenserModels.Data.SQLite
{
    public partial class DataProvider : IDataProvider
    {

        #region IDataProvider 成员

        public bool TestDataConnect()
        {
            string sql = "SELECT * FROM Catalog";
            try
            {
                DataSet ds = DbHelper.ExecuteDataset(sql);

                if (ds.Tables.Count != 0)
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }

            catch
            {
                throw new Exception("DB Error!");
            }
            /*
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@DimID", (DbType)OleDbType.WChar, 0, dimID)
                                              
                                              
                                          };

            int rltvalue = (int)DbHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) FROM [t_dmDim] WHERE [dimID]=@dimID ", parms);

            if (rltvalue != 0)
            {
                return true;
            }

            else
            {
                return false;
            }
             DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@isTax", (DbType)OleDbType.Boolean, 0, taxCategory.isTax),
                                              DbHelper.MakeInParam("@taxCatCaption", (DbType)OleDbType.WChar, 0, taxCategory.taxCatCaption),
                                              DbHelper.MakeInParam("@taxCatCode", (DbType)OleDbType.WChar, 0, taxCategory.taxCatCode)
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text, "INSERT INTO [t_taxCategory]([isTax],[taxCatCaption],[taxCatCode]) VALUES(@isTax,@taxCatCaption,@taxCatCode)", parms);
             */
        }

        #endregion

        public bool GenerateWellInfoSchema(Entity.StationKey stationKey)
        {
            List<string> wells = GetStationWells(stationKey.NodeID);

            foreach (string wellID in wells)
            {
                Entity.WellInfoKey wellInfoKey = new Entity.WellInfoKey(wellID, stationKey.TimeStamp);
                if (!ExistWellInfo(wellInfoKey))
                {
                    GenerateWellInfoSchema(wellInfoKey);
                }
            }

            return true;
        }

        public bool GenerateMotorUnitSchema(Entity.StationKey stationKey)
        {
            List<string> motors = GetStationMotors(stationKey.NodeID);

            foreach (string motorID in motors)
            {
                Entity.MotorUnitKey motorUnitKey = new Entity.MotorUnitKey(motorID, stationKey.TimeStamp);
                if (!ExistMotorUnit(motorUnitKey))
                {
                    GenerateMotorUnitSchema(motorUnitKey);
                }
            }

            return true;
        }

        public bool GenerateStationInfoData(Entity.StationKey stationKey)
        {

            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };
            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWellWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "([TimeStamp] LIKE @TimeStamp)";
            string sqlPumpEfficiency = "ROUND((SELECT SUM([OutputPower]) / SUM([InputPower]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + ")*100,3)";
            string sqlPNEfficiency = "ROUND((SELECT SUM([WIWP] * [WF]) FROM [WellInfo] WHERE " + sqlWellWhereID + " AND " + sqlWhereTime + ")/(SELECT SUM([PUOP] * [ULTF]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + ")*100,3)";

            DbHelper.ExecuteNonQuery(CommandType.Text,
                "UPDATE [StationInfo] SET [PumpEfficiency] = " + sqlPumpEfficiency + "," +
                "[PNEfficiency] = " + sqlPNEfficiency +
                " WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);

            string sqlWFPieceYardage = "ROUND((SELECT SUM([InputPower]) / SUM([ULTF]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + "),3)";
            string sqlWIRate = "ROUND((SELECT SUM([ULTF])*24 FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + "),3)";
            string sqlRemoteControl = "ROUND((SELECT SUM([InputPower])*24 FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + "),3)";
            DbHelper.ExecuteNonQuery(CommandType.Text,
                "UPDATE [StationInfo] SET [SystemEfficiency]=ROUND([PumpEfficiency]*[PNEfficiency]/100,3)," +
                "[WFPieceYardage] = " + sqlWFPieceYardage + "," +
                "[WIRate] = " + sqlWIRate + "," +
                "[RemoteControl] = " + sqlRemoteControl +
                " WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp ", parms);

            return true;
        }


        public bool ClearNullData(Entity.StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
                                          };
            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "([TimeStamp] LIKE @TimeStamp)";
            string sqlWhereNull = "[ActivePower] IS NULL OR [PUOP] IS NULL OR [PUIP] IS NULL OR [ULTF] IS NULL";
            string sqlWhereZero = "[ActivePower] = 0 OR [PUOP] = 0 OR [PUIP] = 0 OR [ULTF] = 0";
            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [CollectingData] WHERE " +
            sqlMotorWhereID + " AND " + sqlWhereTime + " AND (" +
            sqlWhereNull + " OR " + sqlWhereZero + ")", parms);

            return true;
        }

        public bool GenerateMotorUnitData(Entity.MotorUnitKey motorUnitKey)
        {
            List<double> avgValues = new List<double>();
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%")
            };

            SQLiteDataReader sldr = (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
            "SELECT ROUND(AVG(ActivePower),3) AS AVGActivePower,ROUND(AVG(PUOP),3) AS AVGPUOP,ROUND(AVG(PUIP),3) AS AVGPUIP,ROUND(AVG(ULTF),3) AS AVGULTF FROM [CollectingData] WHERE " +
            "[NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp AND " +
            "([ActivePower] IS NOT NULL AND [PUOP] IS NOT NULL AND [PUIP] IS NOT NULL AND [ULTF] IS NOT NULL) AND " +
            "([ActivePower] <> 0 AND [PUOP] <> 0 AND [PUIP] <> 0 AND [ULTF] <> 0)", parms);

            sldr.Read();
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    avgValues.Add(sldr.GetDouble(i));
                }
                sldr.Close();

                parms = new DbParameter[]{
                    DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                    DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%"),
                    DbHelper.MakeInParam("@AVGActivePower", DbType.Double, 0,  avgValues[0]),
                    DbHelper.MakeInParam("@AVGPUOP", DbType.Double, 0, avgValues[1]),
                    DbHelper.MakeInParam("@AVGPUIP", DbType.Double, 0, avgValues[2]),
                    DbHelper.MakeInParam("@AVGULTF", DbType.Double, 0, avgValues[3]),
                };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                    "UPDATE [MotorUnit] SET [InputPower]=@AVGActivePower,[PUOP]=@AVGPUOP,[PUIP]=@AVGPUIP,[ULTF]=@AVGULTF,[OutputPower]=ROUND(((@AVGPUOP-@AVGPUIP) *  @AVGULTF/3.6),3), [Efficiency] = ROUND((@AVGPUOP-@AVGPUIP) * @AVGULTF / (3.6 * @AVGActivePower)*100,3) WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp ", parms);
            }

            catch
            {
                sldr.Close();
                return false;
            }


            return true;
        }

        public bool GenerateMotorUnitData(Entity.StationKey stationKey)
        {
            List<string> motors = GetStationMotors(stationKey.NodeID);
            foreach (string nodeID in motors)
            {
                GenerateMotorUnitData(new Entity.MotorUnitKey(nodeID, stationKey.TimeStamp));
            }

            return true;
        }


        public bool ExistMotorUnit(Entity.MotorUnitKey motorUnitKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%")
            };

            long count = (long)DbHelper.ExecuteScalar(CommandType.Text,
                "SELECT COUNT(*) FROM [MotorUnit] WHERE " +
                "[NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp ", parms);
            return count != 0;
        }


        public bool ExistCatalog(string nodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID)
            };

            long count = (long)DbHelper.ExecuteScalar(CommandType.Text,
                "SELECT COUNT(*) FROM [Catalog] WHERE [NodeID]=@NodeID", parms);
            return count != 0;
        }

        public bool ExistStationInfo(Entity.StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };

            long count = (long)DbHelper.ExecuteScalar(CommandType.Text,
                "SELECT COUNT(*) FROM [StationInfo] WHERE " +
                "[NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp ", parms);
            return count != 0;
        }

        public bool ExistWellInfo(Entity.WellInfoKey wellInfoKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, wellInfoKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, wellInfoKey.TimeStamp+"%")
            };

            long count = (long)DbHelper.ExecuteScalar(CommandType.Text,
                "SELECT COUNT(*) FROM [WellInfo] WHERE " +
                "[NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp ", parms);
            return count != 0;
        }


        public bool GenerateMotorUnitSchema(Entity.MotorUnitKey motorUnitKey)
        {
            if (!ExistMotorUnit(motorUnitKey))
            {
                DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp)
                                              
                                          };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                "INSERT INTO [MotorUnit]([NodeID],[TimeStamp],[InputPower],[PUOP],[PUIP],[ULTF]) VALUES(@NodeID,@TimeStamp,0,0,0,0)", parms);
            }

            return true;
        }


        public bool GenerateStationInfoSchema(StationKey stationKey)
        {
            if (!ExistStationInfo(stationKey))
            {
                DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp)
                                              
                                          };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                "INSERT INTO [StationInfo]([NodeID],[TimeStamp],[PumpEfficiency],[PNEfficiency],[SystemEfficiency],[WFPieceYardage],[WIRate],[RemoteControl]) VALUES(@NodeID,@TimeStamp,0,0,0,0,0,0)", parms);
            }

            return true;
        }


        public bool GenerateWellInfoSchema(WellInfoKey wellInfoKey)
        {
            if (!ExistWellInfo(wellInfoKey))
            {
                DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, wellInfoKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, wellInfoKey.TimeStamp)
                                              
                                          };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                "INSERT INTO [WellInfo]([NodeID],[TimeStamp],[WIWP],[WF]) VALUES(@NodeID,@TimeStamp,0,0)", parms);
            }
            return true;
        }


        public bool CorrectWellInfo(StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };
            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWellWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "([TimeStamp] LIKE @TimeStamp)";

            string sqlSumMotorFlow = "(SELECT SUM([ULTF]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + ")";
            string sqlSumWellFlow = "(SELECT SUM([WF]) FROM [WellInfo] WHERE " + sqlWellWhereID + " AND " + sqlWhereTime + "),3)";

            DbHelper.ExecuteNonQuery(CommandType.Text,
                "UPDATE [WellInfo] SET [WF] = ROUND([WF]*" + sqlSumMotorFlow + "/" + sqlSumWellFlow +
                " WHERE " + sqlWellWhereID + " AND " + sqlWhereTime, parms);

            return true;
        }


        public DataTable GetMotorUnitData(MotorUnitKey motorUnitKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%")
            };



            return DbHelper.ExecuteDataset(CommandType.Text,
                "SELECT D.[NodeName] AS 机组名称,C.[TimeStamp] AS 日期,C.[InputPower] AS 输入功率,C.[PUOP] AS 出口压力,C.[PUIP] AS 入口压力,C.[ULTF] AS 出口流量,C.[OutputPower] AS 输出功率,C.[Efficiency] AS 泵机组效率 FROM [MotorUnit] C INNER JOIN [Catalog] D ON C.[NodeID]=D.[NodeID]" +
                " WHERE C.[NodeID]=@NodeID AND C.[TimeStamp] LIKE @TimeStamp", parms).Tables[0];
        }

        public DataTable GetCollectingData(MotorUnitKey motorUnitKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%")
            };


            return DbHelper.ExecuteDataset(CommandType.Text,
                "SELECT C.[TimeStamp] AS 时间,C.[ActivePowerA] AS A相功率,C.[ActivePowerB] AS B相功率,C.[ActivePowerC] AS C相功率,C.[VoltageA] AS A相电压,C.[VoltageB] AS B相电压,C.[VoltageC] AS C相电压,C.[CurrentA] AS A相电流,C.[CurrentB] AS B相电流,C.[CurrentC] AS C相电流,C.[ActivePower] AS 有功功率,C.[ReactivePower] AS 无功功率,C.[PowerFactor] AS 功率因数,C.[PUOP] AS 出口压力,C.[PUIP] AS 入口压力,C.[ULTF] AS 出口流量 FROM [CollectingData] C" +
                " WHERE C.[NodeID]=@NodeID AND C.[TimeStamp] LIKE @TimeStamp", parms).Tables[0];
        }

        public bool UpdateCollectingData(MotorUnitKey motorUnitKey,float[] updateData)
        {
            if (updateData.Length == 3)
            {
                DbParameter[] parms = new DbParameter[]{
                    DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                    DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%"),
                    DbHelper.MakeInParam("@PUOP", DbType.String, 0, updateData[0]),
                    DbHelper.MakeInParam("@PUIP", DbType.String, 0, updateData[1]),
                    DbHelper.MakeInParam("@ULTF", DbType.String, 0, updateData[2])
                };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                   "UPDATE [CollectingData] SET [PUOP] = @PUOP,[PUIP] = @PUIP,[ULTF] = @ULTF" +
                   " WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);
            }

            else if (updateData.Length == 2)
            {
                DbParameter[] parms = new DbParameter[]{
                    DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                    DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%"),
                    DbHelper.MakeInParam("@PUOP", DbType.String, 0, updateData[0]),
                    DbHelper.MakeInParam("@PUIP", DbType.String, 0, updateData[1])
                };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                   "UPDATE [CollectingData] SET [PUOP] = @PUOP,[PUIP] = @PUIP" +
                   " WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);
            }

            else if (updateData.Length == 1)
            {
                DbParameter[] parms = new DbParameter[]{
                    DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                    DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp+"%"),
                    DbHelper.MakeInParam("@ULTF", DbType.String, 0, updateData[0])
                };

                DbHelper.ExecuteNonQuery(CommandType.Text,
                   "UPDATE [CollectingData] SET [ULTF] = @ULTF" +
                   " WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);
            }

             return true;
        }

        public DataTable GetMotorUnitData(StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };
            string sqlMotorWhereID = "(C.[NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            //string sqlWellWhereID = "(C.[NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "(C.[TimeStamp] LIKE @TimeStamp)";

            //string sqlSumMotorFlow = "(SELECT SUM([ULTF]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + ")";
            //string sqlSumWellFlow = "(SELECT SUM([WF]) FROM [WellInfo] WHERE " + sqlWellWhereID + " AND " + sqlWhereTime + ")";

            return DbHelper.ExecuteDataset(CommandType.Text,
                "SELECT D.[NodeName] AS 机组名称,C.[TimeStamp] AS 日期,C.[InputPower] AS 输入功率,C.[PUOP] AS 出口压力,C.[PUIP] AS 入口压力,C.[ULTF] AS 出口流量,C.[OutputPower] AS 输出功率,C.[Efficiency] AS 泵机组效率 FROM [MotorUnit] C INNER JOIN [Catalog] D ON C.[NodeID]=D.[NodeID]" +
                " WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime, parms).Tables[0];
        }

        public DataTable GetWellInfo(StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };
            //string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWellWhereID = "(C.[NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "(C.[TimeStamp] LIKE @TimeStamp)";

            //string sqlSumMotorFlow = "(SELECT SUM([ULTF]) FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime + ")";
            //string sqlSumWellFlow = "(SELECT SUM([WF]) FROM [WellInfo] WHERE " + sqlWellWhereID + " AND " + sqlWhereTime + ")";

            return DbHelper.ExecuteDataset(CommandType.Text,
                "SELECT C.[NodeID],D.[NodeName] AS 注水井名称,C.[TimeStamp] AS 日期,C.[WIWP] AS 压力,C.[WF] AS 流量 FROM [WellInfo] C INNER JOIN [Catalog] D ON C.[NodeID]=D.[NodeID]" +
                " WHERE " + sqlWellWhereID + " AND " + sqlWhereTime, parms).Tables[0];

            //return true;
        }

        public DataTable GetStationInfoData(StationKey stationKey)
        {
            DbParameter[] parms = new DbParameter[]{
                DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationKey.NodeID),
                DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationKey.TimeStamp+"%")
            };


            return DbHelper.ExecuteDataset(CommandType.Text,
                "SELECT D.[NodeName] AS 注水站名称,C.[TimeStamp] AS 日期,[PumpEfficiency] AS 机泵效率,[PNEfficiency] AS 管网效率,[SystemEfficiency] AS 系统效率,[WFPieceYardage] 系统单耗,[WIRate] 日注水量,[RemoteControl] AS 日耗电量 FROM [StationInfo] C INNER JOIN [Catalog] D ON C.[NodeID]=D.[NodeID]" +
                " WHERE C.[NodeID]=@NodeID AND C.[TimeStamp] LIKE @TimeStamp ", parms).Tables[0];
        }


        public void UpdateWellInfo(DataTable wellInfo)
        {
            SQLiteParameter param = new SQLiteParameter();
            SQLiteCommand updateCommand = new SQLiteCommand();
            //this._adapter.UpdateCommand.Connection = this.Connection;
            updateCommand.CommandText = @"UPDATE [WellInfo] SET [NodeID] = @NodeID, [TimeStamp] = @TimeStamp, [WIWP] = @WIWP, [WF] = @WF WHERE (([NodeID] = @Original_NodeID) AND ([TimeStamp] = @Original_TimeStamp) AND ((@IsNull_WIWP = 1 AND [WIWP] IS NULL) OR ([WIWP] = @Original_WIWP)) AND ((@IsNull_WF = 1 AND [WF] IS NULL) OR ([WF] = @Original_WF)))";
            updateCommand.CommandType = global::System.Data.CommandType.Text;
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@NodeID";
            param.DbType = global::System.Data.DbType.String;
            param.SourceColumn = "NodeID";
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@TimeStamp";
            param.DbType = global::System.Data.DbType.String;
            param.SourceColumn = "日期";
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@WIWP";
            param.DbType = global::System.Data.DbType.Double;
            param.DbType = global::System.Data.DbType.Double;
            param.SourceColumn = "压力";
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@WF";
            param.DbType = global::System.Data.DbType.Double;
            param.DbType = global::System.Data.DbType.Double;
            param.SourceColumn = "流量";
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@Original_NodeID";
            param.DbType = global::System.Data.DbType.String;
            param.SourceColumn = "NodeID";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@Original_TimeStamp";
            param.DbType = global::System.Data.DbType.String;
            param.SourceColumn = "日期";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@IsNull_WIWP";
            param.DbType = global::System.Data.DbType.Int32;
            param.DbType = global::System.Data.DbType.Int32;
            param.SourceColumn = "压力";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            param.SourceColumnNullMapping = true;
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@Original_WIWP";
            param.DbType = global::System.Data.DbType.Double;
            param.DbType = global::System.Data.DbType.Double;
            param.SourceColumn = "压力";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@IsNull_WF";
            param.DbType = global::System.Data.DbType.Int32;
            param.DbType = global::System.Data.DbType.Int32;
            param.SourceColumn = "流量";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            param.SourceColumnNullMapping = true;
            updateCommand.Parameters.Add(param);
            param = new global::System.Data.SQLite.SQLiteParameter();
            param.ParameterName = "@Original_WF";
            param.DbType = global::System.Data.DbType.Double;
            param.DbType = global::System.Data.DbType.Double;
            param.SourceColumn = "流量";
            param.SourceVersion = global::System.Data.DataRowVersion.Original;
            updateCommand.Parameters.Add(param);


            DbHelper.UpdateDataset(new SQLiteCommand(), new SQLiteCommand(), updateCommand, wellInfo.DataSet, "Table");

            //throw new NotImplementedException();
        }
    }
}