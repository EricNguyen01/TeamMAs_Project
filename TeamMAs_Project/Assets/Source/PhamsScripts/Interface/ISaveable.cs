// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface ISaveable
    {
        public SaveDataSerializeBase SaveData(string saveName = "");

        public void LoadData(SaveDataSerializeBase savedDataToLoad);

        public static void GenerateSaveableComponentIfNull(MonoBehaviour mono)
        {
            if (!mono) return;

            Saveable saveable;

            if (!mono.TryGetComponent<Saveable>(out saveable))
            {
                saveable = mono.gameObject.AddComponent<Saveable>();
            }
        }

        /// <summary>
        /// Check if the loaded object's type matches the type of the object that is about to be loaded. Also checks for null object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="savedData"></param>
        /// <returns></returns>
        public static bool IsSavedObjectMatchObjectType<T>(SaveDataSerializeBase savedData)
        {
            if (savedData == null || savedData.LoadSavedObject() == null) return false;

            object savedObject = savedData.LoadSavedObject();

            if (savedObject.GetType() is not T) return false;

            return true;
        }
    }
}
