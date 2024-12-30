using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

namespace DataGraph.Editor
{
    [CustomEditor(typeof(DataGraphAsset))]
    public class DataGraphAssetEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            object asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() == typeof(DataGraphAsset))
            {
                DataGraphEditorWindow.Open(asset as DataGraphAsset);
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                DataGraphEditorWindow.Open((DataGraphAsset)target);
            }
        }
    }
}
