using UnityEditor;
using UnityEngine;
using System;

namespace DataGraph
{
	public interface IGuidAssignable
	{
		void SetGuid(string guid);
	}

	public interface ISubGraph
	{
		public DataGraphAsset SubGraph { get; }
	}


	[System.Serializable ]
    public class DataGraphNode : IGuidAssignable
    {
        [SerializeField]
        private string m_guid;
        [SerializeField]
        private Rect m_position;

        [HideInInspectorView]
        public string typeName;
        [HideInInspectorView]
        public string Id => m_guid;
        [HideInInspectorView]
        public Rect Position => m_position;

        public event Action<DataGraphNode> OnValueChanged;

        [Section("Node Info")]
        public string Name;
        public string Description;

        public DataGraphNode( )
        {

        }

		public void SetGuid(string guid)
        {
            m_guid = guid;
        }

        public void SetPosition(Rect position)
        {
            m_position = position;
        }

        public virtual void OnCreate()
        {

        }

        public virtual void OnDestroy()
        {

        }

        public virtual void OnValueChangedEvent()
        {
            if ( OnValueChanged != null)
            {
                OnValueChanged.Invoke (this);
            }
        }
    }
}