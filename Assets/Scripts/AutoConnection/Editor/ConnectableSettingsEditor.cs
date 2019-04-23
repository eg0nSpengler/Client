using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;

[CustomEditor(typeof(ConnectableSettings))]
[CanEditMultipleObjects]
public class ConnectableSettingsEditor : Editor
{
    private SerializedProperty connectsWithProp;

    private ConnectableSettings connectableSettings;

    private SerializedProperty globalRotationOffsetProp;
    
    private SerializedProperty endMeshProp;
    private SerializedProperty endRotationOffsetProp;
    private SerializedProperty straightMeshProp;
    private SerializedProperty straightRotationOffsetProp;
    private SerializedProperty cornerMeshProp;
    private SerializedProperty cornerRotationOffsetProp;
    private SerializedProperty splitMeshProp;
    private SerializedProperty splitRotationOffsetProp;
    private SerializedProperty crossMeshProp;
    private SerializedProperty crossRotationOffsetProp;

    private SerializedProperty quadColorMaterialProp;
    private SerializedProperty triColorMaterialProp;
    private SerializedProperty doubleColorMaterialProp;
    private SerializedProperty singleColorMaterialProp;
    private SerializedProperty noColorMaterialProp;

    private void OnEnable()
    {
        connectableSettings = (ConnectableSettings) target;

        connectsWithProp = serializedObject.FindProperty("ConnectsWith");

        globalRotationOffsetProp = serializedObject.FindProperty("GlobalRotationOffset");

        endMeshProp = serializedObject.FindProperty("EndMesh");
        endRotationOffsetProp = serializedObject.FindProperty("EndRotationOffset");
        straightMeshProp = serializedObject.FindProperty("StraightMesh");
        straightRotationOffsetProp = serializedObject.FindProperty("StraightRotationOffset");
        cornerMeshProp = serializedObject.FindProperty("CornerMesh");
        cornerRotationOffsetProp = serializedObject.FindProperty("CornerRotationOffset");
        splitMeshProp = serializedObject.FindProperty("SplitMesh");
        splitRotationOffsetProp = serializedObject.FindProperty("SplitRotationOffset");
        crossMeshProp = serializedObject.FindProperty("CrossMesh");        
        crossRotationOffsetProp = serializedObject.FindProperty("CrossRotationOffset");

        quadColorMaterialProp = serializedObject.FindProperty("QuadColorMaterial");
        triColorMaterialProp = serializedObject.FindProperty("TriColorMaterial");
        doubleColorMaterialProp = serializedObject.FindProperty("DoubleColorMaterial");
        singleColorMaterialProp = serializedObject.FindProperty("SingleColorMaterial");
        noColorMaterialProp = serializedObject.FindProperty("NoColorMaterial");
    }

    private void OnValidate()
    {
        EditorUtility.SetDirty(connectableSettings);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        connectableSettings.connectableType =
            (ConnectableType) EditorGUILayout.EnumPopup("Connectable Type",
                connectableSettings.connectableType);
        
        connectableSettings.GlobalRotationOffset = EditorGUILayout.Vector3Field("Global Rot. Offset", connectableSettings.GlobalRotationOffset);

        switch (connectableSettings.connectableType)
        {
            case ConnectableType.MeshBased:
                EditorGUILayout.LabelField("End");
                EditorGUI.indentLevel += 1;
                connectableSettings.EndMesh = (Mesh) EditorGUILayout.ObjectField(new GUIContent("Mesh"),
                    endMeshProp.objectReferenceValue, typeof(Mesh), false);
                connectableSettings.EndRotationOffset = EditorGUILayout.Vector3Field("Rot. Offset", connectableSettings.EndRotationOffset);
                EditorGUI.indentLevel -= 1;
                
                EditorGUILayout.LabelField("Straight");
                EditorGUI.indentLevel += 1;
                connectableSettings.StraightMesh = (Mesh) EditorGUILayout.ObjectField(new GUIContent("Mesh"),
                    straightMeshProp.objectReferenceValue, typeof(Mesh), false);
                connectableSettings.StraightRotationOffset = EditorGUILayout.Vector3Field("Rot. Offset", connectableSettings.StraightRotationOffset);
                EditorGUI.indentLevel -= 1;
                
                EditorGUILayout.LabelField("Corner");
                EditorGUI.indentLevel += 1;
                connectableSettings.CornerMesh = (Mesh) EditorGUILayout.ObjectField(new GUIContent("Mesh"),
                    cornerMeshProp.objectReferenceValue, typeof(Mesh), false);
                connectableSettings.CornerRotationOffset = EditorGUILayout.Vector3Field("Rot. Offset", connectableSettings.CornerRotationOffset);
                EditorGUI.indentLevel -= 1;
                
                EditorGUILayout.LabelField("Split");
                EditorGUI.indentLevel += 1;
                connectableSettings.SplitMesh = (Mesh) EditorGUILayout.ObjectField(new GUIContent("Mesh"),
                    splitMeshProp.objectReferenceValue, typeof(Mesh), false);
                connectableSettings.SplitRotationOffset= EditorGUILayout.Vector3Field("Rot. Offset", connectableSettings.SplitRotationOffset);
                EditorGUI.indentLevel -= 1;
                
                EditorGUILayout.LabelField("Cross");
                EditorGUI.indentLevel += 1;
                connectableSettings.CrossMesh = (Mesh) EditorGUILayout.ObjectField(new GUIContent("Mesh"),
                    crossMeshProp.objectReferenceValue, typeof(Mesh), false);
                connectableSettings.CrossRotationOffset= EditorGUILayout.Vector3Field("Rot. Offset", connectableSettings.CrossRotationOffset);
                EditorGUI.indentLevel -= 1;
                break;
            case ConnectableType.MaterialBased:
                connectableSettings.QuadColorMaterial = (Material) EditorGUILayout.ObjectField(new GUIContent("Quad"),
                    quadColorMaterialProp.objectReferenceValue, typeof(Material), false);
                connectableSettings.TriColorMaterial = (Material) EditorGUILayout.ObjectField(new GUIContent("Tri"),
                    triColorMaterialProp.objectReferenceValue, typeof(Material), false);
                connectableSettings.DoubleColorMaterial = (Material) EditorGUILayout.ObjectField(
                    new GUIContent("Double"), doubleColorMaterialProp.objectReferenceValue, typeof(Material), false);
                connectableSettings.SingleColorMaterial = (Material) EditorGUILayout.ObjectField(
                    new GUIContent("Single"), singleColorMaterialProp.objectReferenceValue, typeof(Material), false);
                connectableSettings.NoColorMaterial = (Material) EditorGUILayout.ObjectField(new GUIContent("None"),
                    noColorMaterialProp.objectReferenceValue, typeof(Material), false);
                break;
        }

        EditorGUILayout.PropertyField(connectsWithProp, true);
        serializedObject.ApplyModifiedProperties();
    }
}