// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
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
    }
}
