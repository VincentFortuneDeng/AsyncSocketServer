using System;
using System.Collections.Generic;
using System.Text;

namespace SenserModels.Entity
{
    public class StationInfoNode:IComparable
    {
        public string NodeID;
        public string TimeStamp;

        public StationInfoNode(string nodeID, string timeStamp)
        {
            this.NodeID = nodeID;
            this.TimeStamp = timeStamp;
        }

        public int CompareTo(object obj)
        {
            StationInfoNode comPareB = ( StationInfoNode)obj;
            if (comPareB.TimeStamp == this.TimeStamp && comPareB.NodeID == this.NodeID)
            {
                return 0;
            }

            else
            {
                return -1;
            }
        }
    }
}
