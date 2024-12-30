using UnityEngine;

namespace DataGraph
{
    [System.Serializable ]
    public struct DataGraphEdge
    {
        public DataGraphEdgePort outputPort; 
        public DataGraphEdgePort inputPort;

        public DataGraphEdge(DataGraphEdgePort outputPort, DataGraphEdgePort inputPort)
        {
            this.outputPort = outputPort;
            this.inputPort = inputPort;
        }

        public DataGraphEdge(string startPortId, int startPortIndex, string endPortId, int endPortIndex)
        {
            outputPort = new DataGraphEdgePort(startPortId, startPortIndex);
            inputPort = new DataGraphEdgePort(endPortId, endPortIndex);
        }
    }

    [System.Serializable ]
    public struct DataGraphEdgePort
    {
        public string nodeId;
        public int portIndex;

        public DataGraphEdgePort( string nodeId, int portIndex)
        {
            this.nodeId = nodeId;
            this.portIndex = portIndex;
        }
    }
}