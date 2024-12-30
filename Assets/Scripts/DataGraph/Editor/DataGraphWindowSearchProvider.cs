using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataGraph.Editor
{
	public struct SearchContextElement
	{
		public object target { get; private set; }
		public string title { get; private set; }

		public SearchContextElement(object target, string title)
		{
			this.target = target;
			this.title = title;
		}
	}

	public class DataGraphWindowSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		public DataGraphView graph;
		public VisualElement target;

		public static List<SearchContextElement> elements;

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
			tree.Add(new SearchTreeGroupEntry(new GUIContent("Nodes"), 0));

			elements = new List<SearchContextElement>();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.CustomAttributes.ToList() != null)
					{
						var attribute = type.GetCustomAttribute(typeof(NodeInfoAttribute));
						if (attribute != null)
						{
							NodeInfoAttribute attr = (NodeInfoAttribute)attribute;
							var node = Activator.CreateInstance(type);
							if (node is IGuidAssignable guidAssignable)
                            {
								guidAssignable.SetGuid(GUID.Generate().ToString());
                            }

                            if (string.IsNullOrEmpty(attr.MenuItem)) continue;
							elements.Add(new SearchContextElement(node, attr.MenuItem));
						}
					}
				}
			}

			elements.Sort((entry1, entry2) =>
			{
				string[] splits1 = entry1.title.Split('/');
				string[] splits2 = entry2.title.Split('/');
				for (int i = 0; i < splits1.Length; i++)
				{
					if (i >= splits2.Length)
					{
						return 1;
					}
					int value = splits1[i].CompareTo(splits2[i]);
					if (value != 0)
					{
						if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
						{
							return splits2.Length < splits2.Length ? 1 : -1;
						}
						return value;
					}
				}
				return 0;
			});

			List<string> groups = new List<string>();

			foreach (SearchContextElement element in elements)
			{
				string[] entryTitle = element.title.Split("/");
				string groupName = "";

				for (int i = 0; i < entryTitle.Length - 1; i++)
				{
					groupName += entryTitle[i];
					if (!groups.Contains(groupName))
					{
						tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
						groups.Add(groupName);
					}
					groupName += "/";
				}

				SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
				entry.level = entryTitle.Length;
				entry.userData = new SearchContextElement(element.target, element.title);
				tree.Add(entry);
			}

			return tree;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			var windowMousePosition = graph.ChangeCoordinatesTo(graph, context.screenMousePosition - graph.GraphEditorWindow.position.position);
			var graphMousePosition = graph.contentViewContainer.WorldToLocal(windowMousePosition);

			SearchContextElement element = (SearchContextElement)SearchTreeEntry.userData;

			DataGraphNode node = (DataGraphNode)element.target;
			node.SetPosition(new Rect(graphMousePosition, Vector2.zero));
			node.OnCreate();
			graph.AddNode(node);
			

			return true;
		}
	}
}