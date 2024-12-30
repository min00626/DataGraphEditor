using System;
using UnityEngine;

namespace DataGraph
{
    public class NodeInfoAttribute : Attribute
    {
        private string m_nodeTitle;
        private string m_menuItem;
        private bool m_hasFlowInput;
        private bool m_hasFlowOutput;

        public string Title => m_nodeTitle;
        public string MenuItem => m_menuItem;
        public bool HasFlowInput => m_hasFlowInput;
        public bool HasFlowOutput => m_hasFlowOutput;

        public NodeInfoAttribute(string nodeTitle, string menuItem = "", bool hasFlowInput = true, bool hasFlowOutput = true)
        {
            m_nodeTitle = nodeTitle;
            m_menuItem = menuItem;
            m_hasFlowInput = hasFlowInput;
            m_hasFlowOutput = hasFlowOutput;
        }
    }
}