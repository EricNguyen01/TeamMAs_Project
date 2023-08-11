// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * ONLY EXACTLY 1 GAME RESOURCE CLASS/GAME OBJECT INSTANCE CAN EXIST IN A SCENE!
     * GameResource is a singleton that exists through scene changes.
     * GameResource class is a central place for other classes/scripts to access the game's resources.
     * When a new game resource is made which is through making a new game resource scriptable object, the new game resource SO goes here (make a new field for it).
     * To access the game resource instance, call: GameResource.gameResourceInstance...
     */
    [DisallowMultipleComponent]
    public class GameResource : MonoBehaviour, ISaveable
    {
        [field: SerializeField] public CoinResourceSO coinResourceSO { get; private set; }

        [field: SerializeField] public List<EmotionalHealthGameResourceSO> emotionalHealthSOTypes { get; private set; } = new List<EmotionalHealthGameResourceSO>();

        //make new fields for new game resources SO here...

        public static GameResource gameResourceInstance;

        private void Awake()
        {
            //keep only 1 instance of game resource during runtime
            if(gameResourceInstance != null)
            {
                Debug.LogWarning("More than 1 instance of GameResource object exists! Destroying duplicated GameResource obj: " + name + "...");
                Destroy(gameObject);
                return;
            }

            gameResourceInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        //ISaveable interface implementations...........................................................................

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            SaveDataSerializeBase resourceSaveData = new SaveDataSerializeBase(this, transform.position, scene.name);

            return resourceSaveData;
        }

        public void LoadData(SaveDataSerializeBase savedDataToLoad)
        {
            if (!ISaveable.IsSavedObjectMatchObjectType<GameResource>(savedDataToLoad)) return;

            GameResource savedGameResource = (GameResource)savedDataToLoad.LoadSavedObject();

            if(savedGameResource.coinResourceSO != null) coinResourceSO = savedGameResource.coinResourceSO;

            coinResourceSO.InvokeGameResourceUpdateEvent();

            if (savedGameResource.emotionalHealthSOTypes == null || savedGameResource.emotionalHealthSOTypes.Count == 0) return;

            emotionalHealthSOTypes = savedGameResource.emotionalHealthSOTypes;

            if (emotionalHealthSOTypes == null && emotionalHealthSOTypes.Count == 0) return;
            
            for(int i = 0; i < emotionalHealthSOTypes.Count; i++)
            {
                emotionalHealthSOTypes[i].InvokeGameResourceUpdateEvent();
            }
        }
    }
}
