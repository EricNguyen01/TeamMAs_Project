// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public interface ISaveable
    {
        public void GenerateSaveableComponentIfNull(MonoBehaviour mono)
        {
            if (!mono) return;

            Saveable saveable;

            if(!mono.TryGetComponent<Saveable>(out saveable))
            {
                saveable = mono.gameObject.AddComponent<Saveable>();
            }
        }

        public SaveDataSerializeBase SaveData(string saveName = "");

        public void LoadData(SaveDataSerializeBase saveDataToLoad);
    }
}
