// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections.Generic;
using TeamMAsTD;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [DisallowMultipleComponent]

#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class Saveable : MonoBehaviour
    {
        [SerializeField] private bool disableSavingForThisObject = false;

        //generate new UUID(Universal unique ID) and convert it to string
        [ReadOnlyInspector]
        [SerializeField] private string UUID;

        SerializedObject serializedObject;

        SerializedProperty UUID_SerializedProperty;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            UUID_SerializedProperty = serializedObject.FindProperty("UUID");
        }

        //remove the block of codes below this if on build
#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;

            //only continue to execute if the object is dropped into a scene and not in asset prefab folder
            //if the below returns null or empty then object is in prefab and should stop execute
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;

            //if this does not have an UUID yet or has an ID that overlaps another object's ID -> provide new one
            if (UUID_SerializedProperty.stringValue == null ||
                string.IsNullOrEmpty(UUID_SerializedProperty.stringValue) ||
                !HelperFunctions.ObjectHasUniqueID(UUID_SerializedProperty.stringValue, this))
            {
                UUID_SerializedProperty.stringValue = System.Guid.NewGuid().ToString();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif

        public string GetSaveableID()
        {
            return UUID;
        }

        //capture whatever states that any ISaveable interface on any components attached to this game object has captured
        public object CaptureSaveableState()
        {
            Dictionary<ISaveable, object> state = new Dictionary<ISaveable, object>();

            if (disableSavingForThisObject) return null;

            //Get all ISaveable components in this game object and store the appropriate values in state dict
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                state[saveable] = saveable.SaveData();//SaveData() is a method of ISaveable interface
            }

            return state;
        }

        public void RestoreSaveableState(object state)
        {
            if (state is not Dictionary<ISaveable, object>) return;

            Dictionary<ISaveable, object> savedState = (Dictionary<ISaveable, object>)state;

            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                if (savedState.ContainsKey(saveable) && savedState[saveable] is SaveDataSerializeBase)
                {
                    saveable.LoadData((SaveDataSerializeBase)savedState[saveable]);//call the ISaveable method
                }
            }
        }
    }
}
