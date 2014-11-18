using System;
using System.Data;
using System.Text;



//using CnTaxLawyer.Entity;
using System.Data.Common;
using SenserModels.Entity;
using System.Collections.Generic;

namespace SenserModels.Data
{
    public interface IDataProvider
    {
        /// <summary>
        /// 测试
        /// </summary>
        /// <returns></returns>
        bool TestDataConnect();

        //bool GenerateStationInfo(StationKey stationKey);

        bool GenerateMotorUnitData(MotorUnitKey motorUnitKey);

        bool GenerateMotorUnitData(StationKey stationKey);

        bool GenerateWellInfoSchema(StationKey stationKey);

        bool GenerateWellInfoSchema(WellInfoKey motorUnitKey);

        bool GenerateMotorUnitSchema(StationKey stationKey);

        bool GenerateStationInfoData(StationKey stationKey);

        bool GenerateStationInfoSchema(StationKey stationKey);

        List<CatalogNode> GetAllCatalogNodes();

        List<CatalogNode> GetChildNodes(CatalogNode parentCatalogNode);

        CatalogNode GetStationNode(string nodeID);

        List<StationInfoNode> GetStationInfoNodes(string nodeID);

        bool DeleteStationDateData(StationInfoNode stationInfo);

        bool DeleteFactory(string catalogNodeID);

        bool DeleteMotor(string catalogNodeID);

        bool DeleteStation(string catalogNodeID);

        bool DeleteWell(string catalogNodeID);

        bool DeleteStationInfoDateData(StationInfoNode stationInfo);

        bool DeleteMotorDateData(MotorUnitKey motorUnitKey);

        bool DeleteWellDateData(WellInfoKey wellInfoKey);

        bool UpdateCatalog(CatalogNode catalogNode, string nodeName);

        bool DeleteCatalogNode(string nodeID);

        List<string> GetStationMotors(string nodeID);

        List<string> GetStationWells(string nodeID);

        List<string> GetFatoryStations(string nodeID);

        bool ClearNullData(Entity.StationKey stationKey);

        bool ExistMotorUnit(MotorUnitKey motorUnitKey);

        bool ExistCatalog(string nodeID);

        bool ExistStationInfo(StationKey stationKey);

        bool ExistWellInfo(WellInfoKey wellInfoKey);

        bool GenerateMotorUnitSchema(MotorUnitKey motorUnitKey);

        bool CorrectWellInfo(StationKey stationKey);

        List<string> GetStationDateList(string nodeID);

        DataTable GetMotorUnitData(MotorUnitKey motorUnitKey);

        DataTable GetMotorUnitData(StationKey stationKey);

        DataTable GetWellInfo(StationKey stationKey);

        DataTable GetStationInfoData(StationKey stationKey);

        DataTable GetCollectingData(MotorUnitKey motorUnitKey);

        void UpdateWellInfo(DataTable wellInfo);

        bool UpdateCollectingData(MotorUnitKey motorUnitKey, float[] updateData);
    }
}
