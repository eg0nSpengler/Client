using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Directions))]
public class DirectionsDrawer : PropertyDrawer
{
    private Directions directions;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        directions = (Directions) property.intValue;
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        bool[] values = DirectionsToBools(directions);

        EditorGUI.indentLevel += 1;

        EditorGUILayout.BeginHorizontal();
        values[0] = EditorGUILayout.Toggle(values[0], GUILayout.MaxWidth(13));
        values[1] = EditorGUILayout.Toggle(values[1], GUILayout.MaxWidth(13));
        values[2] = EditorGUILayout.Toggle(values[2], GUILayout.MaxWidth(13));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        values[7] = EditorGUILayout.Toggle(values[7], GUILayout.MaxWidth(13));
        EditorGUILayout.LabelField("C", GUILayout.MaxWidth(13));
        values[3] = EditorGUILayout.Toggle(values[3], GUILayout.MaxWidth(13));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        values[6] = EditorGUILayout.Toggle(values[6], GUILayout.MaxWidth(13));
        values[5] = EditorGUILayout.Toggle(values[5], GUILayout.MaxWidth(13));
        values[4] = EditorGUILayout.Toggle(values[4], GUILayout.MaxWidth(13));
        EditorGUILayout.EndHorizontal();

        property.intValue = (int) BoolsToDirections(values);

        EditorGUI.indentLevel -= 1;
    }

    static bool[] DirectionsToBools(Directions input)
    {
        bool[] output = new bool[8];
        output[0] = input.HasFlag(Directions.NorthWest);
        output[1] = input.HasFlag(Directions.North);
        output[2] = input.HasFlag(Directions.NorthEast);
        output[3] = input.HasFlag(Directions.East);
        output[4] = input.HasFlag(Directions.SouthEast);
        output[5] = input.HasFlag(Directions.South);
        output[6] = input.HasFlag(Directions.SouthWest);
        output[7] = input.HasFlag(Directions.West);
        return output;
    }

    static Directions BoolsToDirections(bool[] input)
    {
        Directions output = Directions.None;
        if (input[0]) output |= Directions.NorthWest;
        if (input[1]) output |= Directions.North;
        if (input[2]) output |= Directions.NorthEast;
        if (input[3]) output |= Directions.East;
        if (input[4]) output |= Directions.SouthEast;
        if (input[5]) output |= Directions.South;
        if (input[6]) output |= Directions.SouthWest;
        if (input[7]) output |= Directions.West;
        return output;
    }
}