// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * This class contains custom drawer for ReadOnlyInspectorPlayModeAttribute.cs.
 */

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyInspectorPlayModeAttribute))]
public class ReadOnlyInspectorPlayModeDrawer : ReadOnlyInspectorDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!Application.isPlaying) GUI.enabled = true;
        else GUI.enabled = false;

        // Drawing Property
        EditorGUI.PropertyField(position, property, label, true);

        // Setting old GUI enabled value
        GUI.enabled = true;
    }
}
#endif
