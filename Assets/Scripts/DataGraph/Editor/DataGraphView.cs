using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataGraph.Editor
{
    public class DataGraphView : GraphView
    {
        private DataGraphAsset m_dataGraph;
        private SerializedObject m_serializedObject;
        private DataGraphEditorWindow m_graphEditorWindow;

        private Dictionary<string, DataGraphEditorNode> m_nodeDictionary;
        private Dictionary<Edge, DataGraphEdge> m_edgeDictionary;

        private DataGraphWindowSearchProvider m_searchProvider;

        public DataGraphEditorWindow GraphEditorWindow => m_graphEditorWindow;


		public DataGraphView(SerializedObject serializedObject, DataGraphEditorWindow dataGraphEditorWindow) 
        {
            m_serializedObject = serializedObject;
            m_dataGraph = serializedObject.targetObject as DataGraphAsset;
            m_graphEditorWindow = dataGraphEditorWindow;

            m_nodeDictionary = new Dictionary<string, DataGraphEditorNode>();
            m_edgeDictionary = new Dictionary<Edge, DataGraphEdge>();

            m_searchProvider = ScriptableObject.CreateInstance<DataGraphWindowSearchProvider>();
            m_searchProvider.graph = this;

            nodeCreationRequest = ShowSearchWindow;

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/DataGraph/Editor/USS/DataGraphEditor.uss");
            styleSheets.Add(styleSheet);

            var background = new GridBackground();
            background.name = "Grid";
            Add(background);
            background.SendToBack();

			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            DrawDataGraph();

            graphViewChanged += OnGraphViewChanged;
			RegisterCallback<MouseUpEvent>(evt =>
			{
				var selectedNodes = new List<DataGraphEditorNode>();

				foreach (var selectedItem in selection)
				{
					if (selectedItem is DataGraphEditorNode editorNode)
					{
						selectedNodes.Add(editorNode);
					}
				}

                m_graphEditorWindow.OnNodeSelectionChanged(selectedNodes);
			});
		}

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var result = new List<Port>();
            foreach(var (_, node) in m_nodeDictionary)
            {
                foreach (var port in node.Ports)
                {
                    if (port == startPort) { continue; }
                    if (port.node == startPort.node) { continue; }
                    if (port.direction == startPort.direction) { continue; }
                    if (port.portType == startPort.portType)
                    {
                        result.Add(port);
                    }
                }
            }
            return result;
        }

		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			if(graphViewChange.elementsToRemove != null)
            {
				Undo.RecordObject(m_serializedObject.targetObject, "Removed Elements");

                foreach (var editorNode in graphViewChange.elementsToRemove.OfType<DataGraphEditorNode>())
                {
                    RemoveNode(editorNode);
                }
				
                foreach (Edge edge in graphViewChange.elementsToRemove.OfType<Edge>()) 
                {
                    RemoveEdge(edge);
                }
            }

            if(graphViewChange.movedElements != null)
            {
                var nodesToMove = graphViewChange.movedElements.OfType<DataGraphEditorNode>().ToList();
                if(nodesToMove.Count > 0)
                {
					Undo.RecordObject(m_serializedObject.targetObject, "Moved Node");
					foreach (var editorNode in nodesToMove)
					{
						editorNode.SavePosition();
					}
				}
			}

            if(graphViewChange.edgesToCreate != null)
            {
				Undo.RecordObject(m_serializedObject.targetObject, "Created Edge");
                foreach(Edge edge in graphViewChange.edgesToCreate)
                {
                    CreateEdge(edge);
                }
			}

            return graphViewChange;
		}

		private void RemoveEdge(Edge edge)
		{
			if(m_edgeDictionary.TryGetValue(edge, out DataGraphEdge edgeToRemove))
            {
                m_dataGraph.Edges.Remove(edgeToRemove);
                m_edgeDictionary.Remove(edge);
            }
		}

		private void RemoveNode(DataGraphEditorNode editorNode)
		{
            foreach(Port port in editorNode.Ports)
            {
                foreach(Edge edge in port.connections.ToList())
                {
                    RemoveEdge(edge);
                }
            }
            editorNode.Node.OnDestroy();
			m_dataGraph.Nodes.Remove(editorNode.Node);
			m_nodeDictionary.Remove(editorNode.Node.Id);
			m_serializedObject.Update();
		}

		private void CreateEdge(Edge edge)
		{
            DataGraphEditorNode outputNode = (DataGraphEditorNode)(edge.output.node);
            int outputIndex = outputNode.Ports.IndexOf(edge.output);

            DataGraphEditorNode inputNode = (DataGraphEditorNode)(edge.input.node);
            int inputIndex = inputNode.Ports.IndexOf(edge.input);

            DataGraphEdge dataGraphEdge = new DataGraphEdge(outputNode.Node.Id, outputIndex, inputNode.Node.Id, inputIndex);
            m_dataGraph.Edges.Add(dataGraphEdge);
            m_edgeDictionary.Add(edge, dataGraphEdge);
		}

		private void DrawDataGraph()
		{
            if (m_dataGraph == null) { return; }
            if (m_dataGraph.Nodes != null)
            {
                foreach (DataGraphNode node in m_dataGraph.Nodes)
                {
                    AddNodeToGraphView(node);
                }
            }
            if(m_dataGraph.Edges != null)
            {
                foreach(DataGraphEdge edge in m_dataGraph.Edges)
                {
                    AddEdgeToGraphView(edge);
                }
            }
		}

		private void AddEdgeToGraphView(DataGraphEdge dataGraphEdge)
		{
			Port outputPort = GetPortFromEdge(dataGraphEdge.outputPort.nodeId, dataGraphEdge.outputPort.portIndex);
            Port inputPort = GetPortFromEdge(dataGraphEdge.inputPort.nodeId, dataGraphEdge.inputPort.portIndex);
            if(outputPort == null || inputPort == null) { return; }
            Edge edge = outputPort.ConnectTo(inputPort);
			AddElement(edge);
            m_edgeDictionary.Add(edge, dataGraphEdge);
		}

		private Port GetPortFromEdge(string nodeId, int portIndex)
		{
			m_nodeDictionary.TryGetValue(nodeId, out DataGraphEditorNode node);
            if(node == null) return null;
            else return node.Ports[portIndex];
		}

		private void ShowSearchWindow(NodeCreationContext context)
		{
            m_searchProvider.target = focusController.focusedElement as VisualElement;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_searchProvider);
		}

		public void AddNode(DataGraphNode node)
        {
            Undo.RecordObject(m_serializedObject.targetObject, "Added Node");

            m_dataGraph.Nodes.Add(node);
            m_serializedObject.Update();

            AddNodeToGraphView(node);
        }

        private void AddNodeToGraphView(DataGraphNode node)
        {
            node.typeName = node.GetType().AssemblyQualifiedName;
            node.OnValueChanged += OnNodeValueChanged;
            DataGraphEditorNode editorNode = new DataGraphEditorNode(node);
            editorNode.SetPosition(node.Position);
			editorNode.UpdateDescription(node.Name, node.Description);
			m_nodeDictionary.Add(node.Id, editorNode);

            AddElement(editorNode);
        }

        private void OnNodeValueChanged(DataGraphNode node)
        {
            m_graphEditorWindow.SetAssetDirty();

            DataGraphEditorNode editorNode = m_nodeDictionary[node.Id];
            editorNode.UpdateDescription(node.Name, node.Description);
        }
	}
}