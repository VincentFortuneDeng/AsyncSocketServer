using System;
using System.Collections.Generic;
using System.Text;
using SenserModels.Data;
using SenserModels.Entity;

namespace SenserModels
{
    public class NodeInfoProcess
    {
        public List<SenserModels.Entity.CatalogNode> GetAllCatalogNode()
        {
            return DatabaseProvider.GetInstance().GetAllCatalogNodes();
        }

        public List<SenserModels.Entity.CatalogNode> GetStationNodes(string nodeID)
        {
            List<SenserModels.Entity.CatalogNode> catalogNodeList = new List<CatalogNode>();
            //catalogNodeList.Add(DatabaseProvider.GetInstance().GetStationNode(nodeID));
            catalogNodeList.AddRange(DatabaseProvider.GetInstance().GetChildNodes(DatabaseProvider.GetInstance().GetStationNode(nodeID)));

            return catalogNodeList;
        }

        public List<SenserModels.Entity.StationInfoNode> GetStationInfoNode(string nodeID)
        {
            return DatabaseProvider.GetInstance().GetStationInfoNodes(nodeID);
        }

        public bool DeleteDateData(StationInfoNode stationInfo)
        {
            return DatabaseProvider.GetInstance().DeleteStationDateData(stationInfo);
        }

        public bool DeleteFactory(string catalogNodeID)
        {
            return DatabaseProvider.GetInstance().DeleteFactory(catalogNodeID);
        }

        public bool DeleteMotor(string catalogNodeID)
        {
            return DatabaseProvider.GetInstance().DeleteMotor(catalogNodeID);
        }

        public bool DeleteStation(string catalogNodeID)
        {
            return DatabaseProvider.GetInstance().DeleteStation(catalogNodeID);
        }

        public bool DeleteWell(string catalogNodeID)
        {
            return DatabaseProvider.GetInstance().DeleteWell(catalogNodeID);
        }

        public bool UpdateCatalog(CatalogNode catalogNode, string nodeName)
        {
            return DatabaseProvider.GetInstance().UpdateCatalog(catalogNode, nodeName);
        }
    }
}
