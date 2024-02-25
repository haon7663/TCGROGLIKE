using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(CardInfo))]
public class UnitSOInspector : Editor
{
    /*CardInfo data;
    void OnEnable()
    {
        data = serializedObject.targetObject as CardInfo;
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();
        data.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", data.sprite, typeof(Sprite), true);

        EditorGUILayout.Space();

        base.OnInspectorGUI();

        EditorGUILayout.EndVertical();
    }*/
}
