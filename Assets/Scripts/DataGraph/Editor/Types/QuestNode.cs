using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DataGraph
{
	[NodeInfo("Quest", "Quest Graph/Quest"), System.Serializable]
	public class QuestNode : DataGraphNode, ISubGraph
	{
		[SerializeField, HideInInspectorView] private DataGraphAsset m_stageGraph;

		DataGraphAsset ISubGraph.SubGraph => m_stageGraph;

		//[Section("Quest Start Condition")]
		[HideInInspectorView] public int DelayToBeAcceptable = 0;

		[Section("Quest Rewards")]
		public List<ItemAmountPair> rewardItems = new List<ItemAmountPair>();
		[HideInInspectorView] public int RewardLenbo = 0;
		public bool ShowReward  = true;
		
		
		


		public override void OnCreate()
		{
			base.OnCreate();

			string path = $"Assets/Resources/QuestStageGraphs/{Id}.asset";
			CreateDirectoryIfNeeded("Assets/Resources/QuestStageGraphs");

			m_stageGraph = ScriptableObject.CreateInstance<DataGraphAsset>();
			AssetDatabase.CreateAsset(m_stageGraph, path);
			AssetDatabase.SaveAssets();

			Debug.Log($"QuestNode: Created sub-graph asset at {path}");
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if (m_stageGraph != null)
			{
				string path = AssetDatabase.GetAssetPath(m_stageGraph);
				if (!string.IsNullOrEmpty(path))
				{
					AssetDatabase.DeleteAsset(path);
					AssetDatabase.SaveAssets();
					Debug.Log($"QuestNode: Deleted sub-graph asset at {path}");
				}
			}
		}

		public override void OnValueChangedEvent()
		{
			base.OnValueChangedEvent();


		}

		private void CreateDirectoryIfNeeded(string directoryPath)
		{
			if (!AssetDatabase.IsValidFolder(directoryPath))
			{
				string parent = System.IO.Path.GetDirectoryName(directoryPath);
				string folderName = System.IO.Path.GetFileName(directoryPath);
				AssetDatabase.CreateFolder(parent, folderName);
				AssetDatabase.SaveAssets();
			}
		}
	}
}
