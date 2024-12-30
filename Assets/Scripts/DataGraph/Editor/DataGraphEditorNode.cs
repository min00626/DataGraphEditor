using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataGraph.Editor
{
    public class DataGraphEditorNode : Node
    {
        private DataGraphNode m_graphNode;
        private Port m_outputPort;
        private List<Port> m_ports;

		private string m_name;
		private string m_description;
		private VisualElement m_descriptionContainer;

        public DataGraphNode Node => m_graphNode;
        public List<Port> Ports => m_ports;
		public event Action<DataGraphNode> OnNodeSelected;
		public string Name => m_name;
		public string Description => m_description;

        public DataGraphEditorNode(DataGraphNode dataGraphNode) 
        {
            AddToClassList("data-graph-node");

            m_graphNode = dataGraphNode;

            Type type = dataGraphNode.GetType();
            NodeInfoAttribute nodeInfoAttribute = type.GetCustomAttribute<NodeInfoAttribute>();

            title = nodeInfoAttribute.Title;
            name = type.Name;

            string[] depths = nodeInfoAttribute.MenuItem.Split('/');
            foreach (string depth in depths)
            {
                AddToClassList(depth.ToLower().Replace(" ", "-"));
            }

            m_ports = new List<Port>();
            if (nodeInfoAttribute.HasFlowInput)
            {
                CreateFlowInputPort();
            }
            if (nodeInfoAttribute.HasFlowOutput)
            {
                CreateFlowOutputPort();
            }

			RegisterCallback<MouseDownEvent>(evt =>
			{
				if (evt.clickCount == 2)
				{
					OnDoubleClick();
				}
			});

			var separator = new VisualElement
			{
				name = "port-label-separator",
				style =
				{
					height = 1,
					backgroundColor = new StyleColor(new Color(.137f, 0.137f, .137f)),
				}
			};
			
			var borderElement = this.Q<VisualElement>("node-border");
			if (borderElement != null)
			{
				borderElement.Add(separator);
				borderElement.Add(CreateDescriptionLabel());
			}
			else
			{
				Debug.LogWarning("node-border not found!");
			}
		}

		private void OnDoubleClick()
		{
			if (Node is ISubGraph subGraphNode)
			{
				if (subGraphNode.SubGraph != null)
				{
					DataGraphEditorWindow.Open(subGraphNode.SubGraph);
				}
			}
		}

		private void CreateFlowOutputPort()
		{
			m_outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
            m_outputPort.portName = "Next";
            m_outputPort.tooltip = "The flow output";
            m_ports.Add(m_outputPort);
            outputContainer.Add(m_outputPort);
		}

		private void CreateFlowInputPort()
		{
			Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inputPort.portName = "Prev";
			inputPort.tooltip = "The flow input";
			m_ports.Add(inputPort);
			inputContainer.Add(inputPort);
		}

		public void SavePosition()
        {
            m_graphNode.SetPosition(GetPosition());
        }

		public override void OnSelected()
		{
			base.OnSelected();
			if(OnNodeSelected != null)
			{
				OnNodeSelected.Invoke(m_graphNode);
			}
		}

		private VisualElement CreateDescriptionLabel()
		{
			if(m_descriptionContainer == null)
			{
				m_descriptionContainer = new VisualElement();
				m_descriptionContainer.AddToClassList("node-description-container");
			}
			else
			{
				m_descriptionContainer.Clear();
			}
			m_descriptionContainer.style.flexDirection = FlexDirection.Column;



			var nameLabel = new Label($"{Name}")
			{
				style = { 
					unityFontStyleAndWeight = FontStyle.Bold ,
					whiteSpace = WhiteSpace.Normal,
				}
			};

			var descLabel = new Label($"{Description}")
			{
				style = { 
					unityTextAlign = TextAnchor.UpperLeft ,
					whiteSpace = WhiteSpace.Normal,
				}
			};

			m_descriptionContainer.Add(nameLabel);
			m_descriptionContainer.Add(descLabel);

			return m_descriptionContainer;
		}

		public void UpdateDescription(string newName, string newDescription)
		{
			m_name = newName;
			m_description = newDescription;

			CreateDescriptionLabel();
		}
	}
}