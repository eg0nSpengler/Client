using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConnectableGroup))]
public class ConnectableGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ConnectableGroup connectableGroup = (ConnectableGroup) target;
        
        if (GUILayout.Button("Update Grid"))
        {
            connectableGroup.UpdateGrid();
        }
        base.OnInspectorGUI();
    }
}