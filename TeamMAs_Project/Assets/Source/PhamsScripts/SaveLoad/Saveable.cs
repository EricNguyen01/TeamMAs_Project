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
    [ExecuteInEditMode]
#endif
    public class Saveable : MonoBehaviour
    {
        [SerializeField] private bool disableSaveLoadForThisSaveable = false;

        //generate new UUID(Universal unique ID) and convert it to string
        [ReadOnlyInspector]
        [SerializeField] private string UUID;

        private string currentIdentification;

        private SerializedObject serializedObject;

        private SerializedProperty UUID_SerializedProperty;

        private void OnEnable()
        {
#if UNITY_EDITOR

            GenerateID_If_None();
#endif
        }

        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(currentIdentification) && !string.IsNullOrWhiteSpace(currentIdentification) && currentIdentification != "")
            {
                serializedObject = new SerializedObject(this);

                UUID_SerializedProperty = serializedObject.FindProperty("UUID");

                if (UUID_SerializedProperty.stringValue != currentIdentification)
                {
                    UUID_SerializedProperty.stringValue = currentIdentification;

                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void GenerateID_If_None()
        {
            if (Application.isPlaying) return;

            if (!gameObject.scene.isLoaded) return;

            //only continue to execute if the object is dropped into a scene and not in asset prefab folder
            //if the below returns null or empty then object is in prefab and should stop execute
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;

            serializedObject = new SerializedObject(this);

            UUID_SerializedProperty = serializedObject.FindProperty("UUID");

            //if this does not have an UUID yet or has an ID that overlaps another object's ID -> provide new one
            while (UUID_SerializedProperty.stringValue == "" ||
                string.IsNullOrEmpty(UUID_SerializedProperty.stringValue) ||
                string.IsNullOrWhiteSpace(UUID_SerializedProperty.stringValue) || 
                !HelperFunctions.ObjectHasUniqueID(UUID_SerializedProperty.stringValue, this))
            {
                UUID_SerializedProperty.stringValue = System.Guid.NewGuid().ToString();

                serializedObject.ApplyModifiedProperties();
            }

            currentIdentification = UUID;
        }

        public string GetSaveableID()
        {
            return UUID;
        }

        //capture the state of any ISaveable interface on the same game object that this Saveable component attached to
        public object CaptureSaveableState()
        {
            if (disableSaveLoadForThisSaveable) return null;

            Dictionary<string, SaveDataSerializeBase> state;

            state = new Dictionary<string, SaveDataSerializeBase>();

            //Get all ISaveable components in the same game object that this Saveable component is attached to
            //and store the appropriate values into state dict
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                if (!state.ContainsKey(saveable.GetType().ToString()))
                {
                    //call the ISaveable's SaveData method on each ISaveable component of the same object
                    //that this Saveable component is attached to
                    state.Add(saveable.GetType().ToString(), saveable.SaveData());
                }
            }

            return state;
        }

        //load and restore the state of any ISaveable interface on the same game object that this Saveable component attached to
        public void RestoreSaveableState(object state)
        {
            if (disableSaveLoadForThisSaveable || state == null) return;
            
            Dictionary<string, SaveDataSerializeBase> savedState;

            //cast "state" to dictionary type of "savedState"
            savedState = (Dictionary<string, SaveDataSerializeBase>)state;
            
            //Get all ISaveable components in the same game object that this Saveable component is attached to
            //and load the appropriate values from state dict
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                if (savedState.ContainsKey(saveable.GetType().ToString()))
                {
                    //call the ISaveable's LoadData method on each ISaveable component of the same object
                    //that this Saveable component is attached to
                    saveable.LoadData(savedState[saveable.GetType().ToString()]);
                }
            }
        }
    }
}
