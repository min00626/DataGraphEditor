using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace DataGraph
{
	[NodeInfo("Stage", "Quest Graph/Stage"), System.Serializable]
	public class QuestStageNode : DataGraphNode
	{
		public bool IsLastStage = false;

		[Section("Stage Dialogues")]
		public List<ActiveDialogue> Dialogues = new List<ActiveDialogue>();

		[Section("Stage Rewards")]
		public List<ItemAmountPair> RewardItems = new List<ItemAmountPair>();

		[Section("Stage Clear Conditions")]
		public List <ItemAmountPair> ItemConditions = new List<ItemAmountPair>();
		public List <int> DialogueConditions = new List<int>();

		[Section("Custom Events")]
		public int EventIndex_OnStart = -1;
		public int EventIndex_OnEnd = -1;
	}
}