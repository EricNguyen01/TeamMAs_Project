// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

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
    public class GameResource : MonoBehaviour, ISaveable<GameResource>
    {
        [field: SerializeField] public CoinResourceSO coinResourceSO { get; private set; }

        [field: SerializeField] public List<EmotionalHealthGameResourceSO> emotionalHealthSOTypes { get; private set; } = new List<EmotionalHealthGameResourceSO>();

        //make new fields for new game resources SO here...

        public static GameResource gameResourceInstance;

        private Saveable saveable;

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

        private void OnEnable()
        {
            TryGetComponent<Saveable>(out saveable);

            GameResourceSO.OnResourceAmountUpdated += (resourceSO) => SaveLoadHandler.SaveThisSaveableOnly(saveable);
        }

        private void OnDisable()
        {
            GameResourceSO.OnResourceAmountUpdated -= (resourceSO) => SaveLoadHandler.SaveThisSaveableOnly(saveable);
        }

        //ISaveable interface implementations...........................................................................

        public SaveDataSerializeBase<GameResource> SaveData(string saveName = "")
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            SaveDataSerializeBase<GameResource> resourceSaveData; 

            resourceSaveData = new SaveDataSerializeBase<GameResource>(this, transform.position, scene.name);

            return resourceSaveData;
        }

        public void LoadData(SaveDataSerializeBase<GameResource> savedDataToLoad)
        {
            if (savedDataToLoad == null) return;

            GameResource savedGameResource = savedDataToLoad.LoadSavedObject();
            
            if(savedGameResource.coinResourceSO != null) coinResourceSO.SetSpecificResourceAmount(savedGameResource.coinResourceSO.resourceAmount);

            if (savedGameResource.emotionalHealthSOTypes == null || savedGameResource.emotionalHealthSOTypes.Count == 0) return;

            if (emotionalHealthSOTypes == null && emotionalHealthSOTypes.Count == 0) return;
            
            for(int i = 0; i < emotionalHealthSOTypes.Count; i++)
            {
                emotionalHealthSOTypes[i].SetSpecificResourceAmount(savedGameResource.emotionalHealthSOTypes[i].resourceAmount);
            }
        }
    }
}
