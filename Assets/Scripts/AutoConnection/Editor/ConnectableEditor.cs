using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Connectable))]
[CanEditMultipleObjects]
public class ConnectableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Connectable connectable = (Connectable) target;

        if (GUILayout.Button("Construct (connect)"))
        {
            Selection.gameObjects[0].GetComponent<Connectable>().Construct(true);
            for (int i = 1; i < Selection.gameObjects.Length; i++)
            {
                Selection.gameObjects[i].GetComponent<Connectable>().Construct(false);
            }
        }

        base.OnInspectorGUI();
    }
}