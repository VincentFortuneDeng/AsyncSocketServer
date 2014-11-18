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
        public List<Entity.CatalogNode> GetAllCatalogNodes()
        {
            List<Entity.CatalogNode> listCatalogNode = new List<Entity.CatalogNode>();

            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID],[NodeName],[ParentNodeID],[NodeType] FROM [Catalog]");

            while (sldr.Read())
            {
                listCatalogNode.Add(new Entity.CatalogNode((string)sldr["NodeID"],
                    (string)sldr["NodeName"], (string)sldr["ParentNodeID"],
                    (SenserModels.Entity.NodeTypeEnum)Enum.Parse(typeof(SenserModels.Entity.NodeTypeEnum),
                    (string)sldr["NodeType"])));
            }
            sldr.Close();

            return listCatalogNode;
        }

        public List<Entity.CatalogNode> GetChildNodes(CatalogNode parentCatalogNode)
        {
            List<Entity.CatalogNode> listCatalogNode = new List<Entity.CatalogNode>();
            listCatalogNode.Add(parentCatalogNode);
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, parentCatalogNode.NodeID) 
                                          };
            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID],[NodeName],[ParentNodeID],[NodeType] FROM [Catalog] WHERE [ParentNodeID]=@NodeID", parms);

            CatalogNode catalogNode = null;
            while (sldr.Read())
            {
                catalogNode = new Entity.CatalogNode((string)sldr["NodeID"],
                       (string)sldr["NodeName"], (string)sldr["ParentNodeID"],
                       (SenserModels.Entity.NodeTypeEnum)Enum.Parse(typeof(SenserModels.Entity.NodeTypeEnum),
                       (string)sldr["NodeType"]));

                //listCatalogNode.Add(catalogNode);
                listCatalogNode.AddRange(GetChildNodes(catalogNode));
            }
            sldr.Close();

            return listCatalogNode;
        }

        public List<Entity.StationInfoNode> GetStationInfoNodes(string nodeID)
        {
            List<Entity.StationInfoNode> listStationInfoNode = new List<Entity.StationInfoNode>();

            //listCatalogNode.Add(GetStationNode(nodeID));//本节点

            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };
            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [TimeStamp] FROM [StationInfo] WHERE [NodeID]=@NodeID", parms);

            StationInfoNode stationInfoNode = null;
            while (sldr.Read())
            {
                stationInfoNode = new Entity.StationInfoNode(nodeID, (string)sldr["TimeStamp"]);

                listStationInfoNode.Add(stationInfoNode);
            }
            sldr.Close();

            return listStationInfoNode;
        }

        public Entity.CatalogNode GetStationNode(string nodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };
            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID],[NodeName],[ParentNodeID],[NodeType] FROM [Catalog] WHERE [NodeID]=@NodeID", parms);

            CatalogNode catalogNode = null;

            sldr.Read();

            catalogNode = new Entity.CatalogNode((string)sldr["NodeID"],
                (string)sldr["NodeName"], (string)sldr["ParentNodeID"],
                (SenserModels.Entity.NodeTypeEnum)Enum.Parse(typeof(SenserModels.Entity.NodeTypeEnum),
                (string)sldr["NodeType"]));
            sldr.Close();

            return catalogNode;
        }

        public bool DeleteStationDateData(Entity.StationInfoNode stationInfo)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationInfo.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationInfo.TimeStamp +"%")
                                          };

            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWellWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWhereTime = "([TimeStamp] LIKE @TimeStamp)";

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [MotorUnit] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime, parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [CollectingData] WHERE " + sqlMotorWhereID + " AND " + sqlWhereTime, parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [WellInfo] WHERE " + sqlWellWhereID + " AND " + sqlWhereTime, parms);

            DeleteStationInfoDateData(stationInfo);

            return true;
        }

        public bool DeleteFactory(string catalogNodeID)
        {
            List<string> stationList = GetFatoryStations(catalogNodeID);

            foreach (string nodeID in stationList)
            {
                DeleteStation(nodeID);
            }
            /*
            while (sldr.Read())
            {
                nodeID = (string)sldr["NodeID"];
                DeleteStation(nodeID);
            }*/

            DeleteCatalogNode(catalogNodeID);

            return true;
        }

        public bool DeleteMotor(string catalogNodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, catalogNodeID)
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [MotorUnit] WHERE [NodeID]=@NodeID", parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [CollectingData] WHERE [NodeID]=@NodeID", parms);

            DeleteCatalogNode(catalogNodeID);

            return true;
        }

        public bool DeleteStation(string catalogNodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, catalogNodeID)
                                          };
            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";
            string sqlWellWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";

            //Motor
            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [MotorUnit] WHERE " + sqlMotorWhereID, parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [CollectingData] WHERE " + sqlMotorWhereID, parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [Catalog] WHERE " + sqlMotorWhereID, parms);

            //Well
            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [WellInfo] WHERE " + sqlWellWhereID, parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [Catalog] WHERE " + sqlWellWhereID, parms);

            //Station
            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [StationInfo] WHERE [NodeID]=@NodeID", parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [Catalog] WHERE [ParentNodeID]=@NodeID", parms);

            DeleteCatalogNode(catalogNodeID);

            return true;
        }

        public bool DeleteWell(string catalogNodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, catalogNodeID)
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [WellInfo] WHERE [NodeID]=@NodeID", parms);

            DeleteCatalogNode(catalogNodeID);

            return true;
        }

        public bool UpdateCatalog(CatalogNode catalogNode, string nodeName)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, catalogNode.NodeID), 
                                              DbHelper.MakeInParam("@NodeName", DbType.String, 0, catalogNode.NodeName), 
                                              DbHelper.MakeInParam("@NodeType", DbType.String, 0, catalogNode.NodeType), 
                                              DbHelper.MakeInParam("@ParentNodeID", DbType.String, 0, catalogNode.ParentNodeID),
                                              DbHelper.MakeInParam("@NewNodeName", DbType.String, 0, string.IsNullOrEmpty(nodeName)?catalogNode.NodeName:nodeName), 
                                          };
            bool isExist = ExistCatalog(catalogNode.NodeID);
            if (isExist && !catalogNode.NodeName.Equals(nodeName))//update
            {
                DbHelper.ExecuteNonQuery(CommandType.Text,
                "UPDATE [Catalog] SET [NodeName]=@NewNodeName WHERE [NodeID]=@NodeID", parms);
            }

            else if (!isExist)
            {
                DbHelper.ExecuteNonQuery(CommandType.Text,
                "INSERT INTO [Catalog]([NodeID],[NodeName],[ParentNodeID],[NodeType]) VALUES(@NodeID,@NewNodeName,@ParentNodeID,@NodeType)", parms);
            }

            return true;
        }


        public bool DeleteCatalogNode(string nodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID)
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [Catalog] WHERE [NodeID]=@NodeID", parms);

            return true;
        }


        public List<string> GetStationMotors(string nodeID)
        {
            List<string> motorNodeIDList = new List<string>();

            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };

            string sqlMotorWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'MOTORGROUP') AND ([ParentNodeID] = @NodeID)))))";

            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID] FROM [Catalog] WHERE " + sqlMotorWhereID, parms);


            while (sldr.Read())
            {
                motorNodeIDList.Add((string)sldr["NodeID"]);
            }
            sldr.Close();

            return motorNodeIDList;
        }

        public List<string> GetStationWells(string nodeID)
        {
            List<string> wellNodeIDList = new List<string>();

            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };
            string sqlWellWhereID = "([NodeID] IN (SELECT [NodeID] FROM [Catalog] A WHERE ([ParentNodeID] IN (SELECT [NodeID] FROM [Catalog] B WHERE ([NodeType] = 'WELLGROUP') AND ([ParentNodeID] = @NodeID)))))";
            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID] FROM [Catalog] WHERE " + sqlWellWhereID, parms);


            while (sldr.Read())
            {
                wellNodeIDList.Add((string)sldr["NodeID"]);
            }
            sldr.Close();

            return wellNodeIDList;
        }


        public bool DeleteStationInfoDateData(StationInfoNode stationInfo)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, stationInfo.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, stationInfo.TimeStamp +"%")
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [StationInfo] WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);

            return true;
        }

        public bool DeleteMotorDateData(MotorUnitKey motorUnitKey)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, motorUnitKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, motorUnitKey.TimeStamp +"%")
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [MotorUnit] WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [CollectingData] WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);

            return true;
        }

        public bool DeleteWellDateData(WellInfoKey wellInfoKey)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, wellInfoKey.NodeID),
                                              DbHelper.MakeInParam("@TimeStamp", DbType.String, 0, wellInfoKey.TimeStamp +"%")
                                          };

            DbHelper.ExecuteNonQuery(CommandType.Text,
            "DELETE FROM [WellInfo] WHERE [NodeID]=@NodeID AND [TimeStamp] LIKE @TimeStamp", parms);

            return true;
        }


        public List<string> GetFatoryStations(string nodeID)
        {
            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };
            SQLiteDataReader sldr =
                (SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [NodeID] FROM [Catalog] WHERE [ParentNodeID]=@NodeID", parms);

            List<string> stationNodeIDList = new List<string>();
            while (sldr.Read())
            {
                stationNodeIDList.Add((string)sldr["NodeID"]);
            }
            sldr.Close();

            return stationNodeIDList;
        }


        public List<string> GetStationDateList(string nodeID)
        {
            List<string> listDate = new List<string>();

            DbParameter[] parms = new DbParameter[]{
                                              DbHelper.MakeInParam("@NodeID", DbType.String, 0, nodeID) 
                                          };
            SQLiteDataReader sldr =(SQLiteDataReader)DbHelper.ExecuteReader(CommandType.Text,
                "SELECT [TimeStamp] FROM [StationInfo] WHERE [NodeID]=@NodeID", parms);

            while (sldr.Read())
            {
                listDate.Add((string)sldr["TimeStamp"]);
            }
            sldr.Close();

            return listDate;
        }
    }
}