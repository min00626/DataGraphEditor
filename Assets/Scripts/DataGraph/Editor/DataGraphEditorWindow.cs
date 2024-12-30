using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataGraph.Editor
{
	public class DataGraphEditorWindow : EditorWindow
	{
		[SerializeField]
		private DataGraphAsset m_currentGraph;

		[SerializeField]
		private SerializedObject m_serializedObject;

		[SerializeField]
		private DataGraphView m_currentView;

		[SerializeField]
		private InspectorView m_inspectorView;

		public DataGraphAsset CurrentGraph => m_currentGraph;

		public static void Open(DataGraphAsset target)
		{
			DataGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DataGraphEditorWindow>();
			foreach (DataGraphEditorWindow window in windows)
			{
				if (window.CurrentGraph == target)
				{
					window.Focus();
					return;
				}
			}

			DataGraphEditorWindow newWindow = CreateWindow<DataGraphEditorWindow>(typeof(DataGraphEditorWindow), typeof(SceneView));
			newWindow.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(DataGraphAsset)).image);
			newWindow.DrawEditorWindow(target);
		}

		private void OnEnable()
		{
			if (m_currentGraph != null)
			{
				DrawEditorWindow(m_currentGraph);
			}
			rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnResize);
		}

		private void OnGUI()
		{
			if (m_currentGraph != null)
			{
				hasUnsavedChanges = EditorUtility.IsDirty(m_currentGraph);
			}
		}

		public void DrawEditorWindow(DataGraphAsset target)
		{
			m_currentGraph = target;
			m_serializedObject = new SerializedObject(m_currentGraph);

			rootVisualElement.Clear();

			m_currentView = new DataGraphView(m_serializedObject, this)
			{
				name = "graph-view"
			};
			m_currentView.graphViewChanged += OnGraphViewChanged;
			m_currentView.style.flexGrow = 1;

			var splitView = new TwoPaneSplitView(0, 800, TwoPaneSplitViewOrientation.Horizontal);
			splitView.Add(m_currentView);

			m_inspectorView = new InspectorView { name = "inspector-view" };
			m_inspectorView.style.flexGrow = 1;
			splitView.Add(m_inspectorView);

			rootVisualElement.Add(splitView);

			var saveButton = new Button(() => SaveGraph())
			{
				text = "Save"
			};
			saveButton.style.position = Position.Absolute;
			saveButton.style.top = 10;
			saveButton.style.left = 10;
			saveButton.style.width = 50;
			saveButton.style.height = 20;
			rootVisualElement.Add(saveButton);

			var convertButton = new Button(() => ConvertGraphToJson())
			{
				text = "Convert to JSON"
			};
			convertButton.style.position = Position.Absolute;
			convertButton.style.top = 10;
			convertButton.style.left = 70;
			convertButton.style.width = 120;
			convertButton.style.height = 20;
			rootVisualElement.Add(convertButton);
		}

		private void OnResize(GeometryChangedEvent evt)
		{
			var splitView = rootVisualElement.Q<TwoPaneSplitView>();
			if (splitView != null)
			{
				float totalWidth = rootVisualElement.resolvedStyle.width;
				splitView.fixedPaneInitialDimension = totalWidth * 0.73f;
			}
		}

		public void OnNodeSelectionChanged(List<DataGraphEditorNode> node)
		{
			m_inspectorView.UpdateInspector(node);
		}

		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			EditorUtility.SetDirty(m_currentGraph);
			return graphViewChange;
		}

		public void SetAssetDirty()
		{
			m_serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(m_currentGraph);
		}

		private void SaveGraph()
		{
			m_serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(m_currentGraph);
			AssetDatabase.SaveAssets();
		}

		private void ConvertGraphToJson()
		{
			if (m_currentGraph == null)
			{
				Debug.LogError("No graph loaded to convert!");
				return;
			}

			DataGraphToJsonConverter.ConvertGraphToJson(m_currentGraph);
		}
	}
}
