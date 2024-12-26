// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;

namespace TeamMAsTD
{
    public interface ISaveable
    {
        public SaveDataSerializeBase SaveData(string saveName = "");

        public void LoadData(SaveDataSerializeBase savedDataToLoad);

        public static Saveable GetOrGenerateSaveableComponentIfNull(MonoBehaviour mono)
        {
            if (!mono) return null;

            Saveable saveable;

            if (mono.TryGetComponent<Saveable>(out saveable)) return saveable;

            if (!saveable && Application.isEditor && !Application.isPlaying)
            {
                saveable = mono.gameObject.AddComponent<Saveable>();
            }

            return saveable;
        }

        public Saveable GetSaveable();
    }
}
