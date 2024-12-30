using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataGraph
{
	[System.Serializable]
	public class QuestJsonData
	{
		public List<QuestJsonNode> quests;
	}

	[System.Serializable]
	public class QuestJsonNode
	{
		public int index;
		public QuestCondition condition;
		public QuestInfo info;
		public List<QuestStageJsonNode> stages;
	}

	[System.Serializable]
	public class QuestCondition
	{
		public List<int> parentIndices;
		public int delayToBeAcceptable;
	}

	[System.Serializable]
	public class QuestInfo
	{
		public string name;
		public string description;
		public List<ItemAmountPair> rewardItems;
		public int rewardLenbo;
	}

	[System.Serializable]
	public class QuestStageJsonNode
	{
		public int index;
		public string description;
		public List<ActiveDialogue> dialogues;
		public StageCondition condition;
		public List<int> children;
		public int eventIndex_OnStart;
		public int eventIndex_OnEnd;
		public List<ItemAmountPair> stageRewardItems;
		public bool isLastStage;
	}

	[System.Serializable]
	public class StageCondition
	{
		public List<ItemAmountPair> itemConditions;
		public List<int> dialogueConditions;
	}

	public class DataGraphToJsonConverter
	{
		public static void ConvertGraphToJson(DataGraphAsset graph)
		{
			if (graph == null)
			{
				Debug.LogError("No graph loaded to convert!");
				return;
			}

			string path = EditorUtility.SaveFilePanel("Save JSON File", "", "QuestGraph.json", "json");
			if (string.IsNullOrEmpty(path))
				return;

			string jsonData = GenerateJsonData(graph);

			System.IO.File.WriteAllText(path, jsonData);
			Debug.Log("Graph successfully converted and saved to: " + path);
		}



		private static string GenerateJsonData(DataGraphAsset graph)
		{
			var questJsonData = new QuestJsonData
			{
				quests = graph.Nodes
					.OfType<QuestNode>()
					.Select(questNode =>
					{
						var stages = GetStages((questNode as ISubGraph).SubGraph);

						return new QuestJsonNode
						{
							index = graph.Nodes.IndexOf(questNode),
							condition = new QuestCondition
							{
								parentIndices = GetParentIndices(graph, questNode),
								delayToBeAcceptable = questNode.DelayToBeAcceptable
							},
							info = new QuestInfo
							{
								name = questNode.Name,
								description = questNode.Description,
								rewardItems = questNode.rewardItems,
								rewardLenbo = questNode.RewardLenbo
							},
							stages = stages
						};
					})
					.ToList()
			};

			var settings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
			};

			return JsonConvert.SerializeObject(questJsonData, settings);
		}


	private static List<QuestStageJsonNode> GetStages(DataGraphAsset subGraph)
		{
			if (subGraph == null)
			{
				Debug.LogWarning("[GetStages] SubGraph is null. Skipping stages.");
				return new List<QuestStageJsonNode>();
			}

			return subGraph.Nodes
				.OfType<QuestStageNode>()
				.Select(stageNode =>
				{
					return new QuestStageJsonNode
					{
						index = subGraph.Nodes.IndexOf(stageNode),
						description = stageNode.Name,
						dialogues = stageNode.Dialogues,
						condition = new StageCondition
						{
							itemConditions = stageNode.ItemConditions,
							dialogueConditions = stageNode.DialogueConditions
						},
						children = GetStageChildren(subGraph, stageNode),
						eventIndex_OnStart = stageNode.EventIndex_OnStart,
						eventIndex_OnEnd = stageNode.EventIndex_OnEnd,
						stageRewardItems = stageNode.RewardItems,
						isLastStage = stageNode.IsLastStage
					};
				})
				.ToList();
		}


		private static List<int> GetParentIndices(DataGraphAsset graph, DataGraphNode node)
		{
			return graph.Edges
				.Where(edge => edge.inputPort.nodeId == node.Id)
				.Select(edge => graph.Nodes.FindIndex(n => n.Id == edge.outputPort.nodeId))
				.Where(index => index >= 0)
				.ToList();
		}

		private static List<int> GetStageChildren(DataGraphAsset subGraph, DataGraphNode stageNode)
		{
			return subGraph.Edges
				.Where(edge => edge.outputPort.nodeId == stageNode.Id)
				.Select(edge => subGraph.Nodes.FindIndex(n => n.Id == edge.inputPort.nodeId))
				.Where(index => index >= 0)
				.ToList();
		}
	}
}
