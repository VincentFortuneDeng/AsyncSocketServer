using System;
using System.Collections.Generic;
using System.Text;

namespace SenserModels.Entity
{
    public class MotorUnitKey
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeID;
        /// <summary>
        /// 时间戳
        /// </summary>
        public string TimeStamp;

        public MotorUnitKey(string nodeID, string timeStamp)
        {
            this.NodeID = nodeID;
            this.TimeStamp = timeStamp;
        }
    }
}
