using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DataGraph
{
    [CreateAssetMenu(fileName = "DataGraphAsset", menuName = "Data Graph/DataGraphAsset")]
    public class DataGraphAsset : ScriptableObject
    {
        [SerializeReference]
        private List<DataGraphNode> m_nodes;

        [SerializeField]
        private List<DataGraphEdge> m_edges;

        public List<DataGraphNode> Nodes => m_nodes;

        public List<DataGraphEdge> Edges => m_edges;

        public DataGraphAsset()
        {
            m_nodes = new List<DataGraphNode>(); 
            m_edges = new List<DataGraphEdge>();
        }
    }
}
