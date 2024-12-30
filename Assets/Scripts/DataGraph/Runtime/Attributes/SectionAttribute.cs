using System;
using UnityEngine;

namespace DataGraph
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
	public class SectionAttribute : PropertyAttribute
	{
		public string Label { get; }
		public SectionAttribute(string label)
		{
			Label = label;
		}
	}
}