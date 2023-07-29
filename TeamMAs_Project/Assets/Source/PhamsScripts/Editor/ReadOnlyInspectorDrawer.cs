// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * This class contain custom drawer for ReadOnlyAttribute.
 */
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Disabling edit for property
        GUI.enabled = false;

        // Drawing Property
        EditorGUI.PropertyField(position, property, label);

        // Setting old GUI enabled value
        GUI.enabled = true;
    }
}
#endif
