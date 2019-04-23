using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AdjacentData))]
public class AdjacentDataDrawer : PropertyDrawer
{
    private SerializedProperty directionsProp;

    private string name;
    private bool cache = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!cache)
        {
            name = property.displayName;

            property.Next(true);
            directionsProp = property.Copy();

            cache = true;
        }

        Rect contentPosition = EditorGUI.PrefixLabel(position, new GUIContent(name));

        position.height = 16f;
        EditorGUI.indentLevel += 1;
        contentPosition = EditorGUI.IndentedRect(position);
        contentPosition.y += 18f;

        EditorGUIUtility.labelWidth = 75f;
        EditorGUI.indentLevel = 0;

        EditorGUI.BeginProperty(contentPosition, label, directionsProp);
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(contentPosition, directionsProp, new GUIContent(""));
        }
        EditorGUI.EndProperty();
        contentPosition.y += 32f;
    }
}