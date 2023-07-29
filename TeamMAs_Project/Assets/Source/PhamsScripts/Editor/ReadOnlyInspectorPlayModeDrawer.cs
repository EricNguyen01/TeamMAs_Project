// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyInspectorPlayModeAttribute))]
public class ReadOnlyInspectorPlayModeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            GUI.enabled = true;

            EditorGUI.PropertyField(position, property, label);

            return;
        }

        // Disabling edit for property
        GUI.enabled = false;

        // Drawing Property
        EditorGUI.PropertyField(position, property, label);

        // Setting old GUI enabled value
        GUI.enabled = true;
    }
}
#endif
