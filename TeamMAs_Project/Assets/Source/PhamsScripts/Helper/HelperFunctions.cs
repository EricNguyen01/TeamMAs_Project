// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TeamMAsTD
{
    public static class HelperFunctions
    {
        private static Dictionary<string, Object> globalIDLookup = new Dictionary<string, Object>();

        //.................Layer Mask Comparison..................
        public static bool IsMaskEqual(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        //.....................Randomly Shuffle A List..................................
        public static List<T> RandomShuffleListElements<T>(List<T> list)
        {
            if (list == null || list.Count == 0 || list.Count == 1) return list;

            for (int i = 0; i < list.Count; i++)
            {
                //determine the random slot index to swap with the current slot (starting from slot 0)
                int rand = Random.Range(0, 2);

                int temp = i;

                if (rand == 1) temp = (list.Count - 1) - i;

                //the data in the slot that to be swappped to the current slot
                T elementToSwap = list[temp];

                //set the random slot's data to the current slot's
                list[temp] = list[i];

                //set the current index slot's data with the data in the random shuffled slot
                list[i] = elementToSwap;
            }

            return list;
        }

        //.....................Randomly Shuffle An Array..................................
        public static T[] RandomShuffleArrayElements<T>(T[] array)
        {
            if (array == null || array.Length == 0 || array.Length == 1) return array;

            for (int i = 0; i < array.Length; i++)
            {
                //determine the random slot index to swap with the current slot (starting from slot 0)
                int rand = Random.Range(0, 2);

                int temp = i;

                if (rand == 1) temp = (array.Length - 1) - i;

                //the data in the slot that to be swappped to the current slot
                T elementToSwap = array[temp];

                //set the random slot's data to the current slot's
                array[temp] = array[i];

                //set the current index slot's data with the data in the random shuffled slot
                array[i] = elementToSwap;
            }

            return array;
        }

        //................Get The Look Rotation From A Position Towards Another..................................
        public static Quaternion GetRotationToPos2D(Vector3 fromPos, Vector3 toPos)
        {
            Vector3 dir = toPos - fromPos;

            dir.Normalize();

            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            return Quaternion.AngleAxis(-angle, Vector3.forward);
        }

        /// <summary>
        /// Check if an object's generated UUID is unique or not among every other object that has an UUID registered in the global lookup. 
        /// </summary>
        /// <param name="ID"> The ID of the object that will be checked for uniqueness. </param> 
        /// <param name="objectWithId"> The object using the ID just provided in the first param. Both must match! </param>
        /// <returns></returns>
        public static bool ObjectHasUniqueID(string ID, Object objectWithId)
        {
            //if provided UUID NOT already existed => IS UNIQUE and Add ID to global IDs Lookup
            if (!globalIDLookup.ContainsKey(ID))
            {
                globalIDLookup.Add(ID, objectWithId);

                return true;
            }

            //if provided UUID already existed and not match with provided object
            if (globalIDLookup.ContainsKey(ID))
            {
                //if provided UUID belongs to another object and not the one its supposes to belong to => NOT UNIQUE
                if (globalIDLookup[ID] != objectWithId || !globalIDLookup[ID].Equals(objectWithId))
                {
                    Debug.Log("Object: " + globalIDLookup[ID].name + " with ID: " + ID + "\n" +
                    "does not matches its provided comparison object: " + objectWithId.name);

                    return false;
                }
                
            }

            //if an object is unloaded or destroyed -> remove its UUID from the static dict
            if (globalIDLookup[ID] == null)
            {
                //Debug.Log("ID: " + ID + " doesnt have associate object!");

                globalIDLookup.Remove(ID);

                //since this key in the static dict is now null -> it is unique
                return true;
            }
            
            //if passed all above checks => IS UNIQUE ID
            return true;
        }

        /// <summary>
        /// Match the provided UI Object (RectTransform) and it's local rect position to the provided world position. 
        /// </summary>
        /// <param name="UIObject"> The UI Object with a RectTransform component attached to reposition. </param> 
        /// <param name="worldPos"> The world position to snap the UIObject (in rect local pos) to. </param>
        /// <returns></returns>
        public static void MatchUIRectPosToWorldPos(RectTransform UIObject, Vector3 worldPos, bool keepZOffset = true)
        {
            if (!UIObject) return;

            Canvas parentCanvas = UIObject.GetComponentInParent<Canvas>();

            if (!parentCanvas) return;

            RectTransform parentCanvasRect;
            parentCanvas.TryGetComponent<RectTransform>(out parentCanvasRect);

            Camera cam = parentCanvas.worldCamera;

            if (!cam) return;

            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            Vector2 localPointInRect;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIObject, screenPos, cam, out localPointInRect);

            Vector3 finalWorldPos = UIObject.transform.TransformPoint(localPointInRect);

            if(!keepZOffset) UIObject.transform.position = finalWorldPos;
            else UIObject.transform.position = new Vector3(finalWorldPos.x, finalWorldPos.y, UIObject.transform.position.z);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Helps converting auto-implemented property display name to its actual property name 
        /// which can then be picked up by FindProperty function.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedObject obj, string propName)
        {
            return obj.FindProperty(string.Format("<{0}>k__BackingField", propName));
        }
#endif

    }
}
