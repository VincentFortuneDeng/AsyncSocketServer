using System;
using System.Collections.Generic;
using System.Text;

namespace SenserModels.Entity
{
    public class CatalogNode
    {
        public string NodeID;
        public string NodeName;
        public string ParentNodeID;
        public NodeTypeEnum NodeType;

        public CatalogNode(string nodeID, string nodeName, string parentNodeID, NodeTypeEnum nodeType)
        {
            NodeID = nodeID;
            NodeName = nodeName;
            ParentNodeID = parentNodeID;
            NodeType = nodeType;
        }
    }
}
