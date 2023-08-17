// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
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
    public class GameResource : MonoBehaviour, ISaveable
    {
        [field: SerializeField] public CoinResourceSO coinResourceSO { get; private set; }

        [field: SerializeField] public List<EmotionalHealthGameResourceSO> emotionalHealthSOTypes { get; private set; } = new List<EmotionalHealthGameResourceSO>();

        //make new fields for new game resources SO here...

        public static GameResource gameResourceInstance;

        private Saveable saveable;

        [Serializable]
        private class GameResourceSaveData
        {
            public int coinResourceAmountSave { get; private set; }

            public Dictionary<string, float> emotionalHealthTypesAmountSave { get; private set; }

            public GameResourceSaveData(int coinAmountSave, Dictionary<string, float> emotionalHealthTypesSave)
            {
                coinResourceAmountSave = coinAmountSave;

                emotionalHealthTypesAmountSave = emotionalHealthTypesSave;
            }
        }

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

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            Dictionary<string, float> emotionalHealthTypesSaveDict = new Dictionary<string, float>();

            if(emotionalHealthSOTypes != null && emotionalHealthSOTypes.Count > 0 )
            {
                for(int i = 0; i < emotionalHealthSOTypes.Count; i++)
                {
                    if (emotionalHealthSOTypes[i] == null) continue;

                    if (!emotionalHealthTypesSaveDict.ContainsKey(emotionalHealthSOTypes[i].name))
                    {
                        emotionalHealthTypesSaveDict.Add(emotionalHealthSOTypes[i].name, emotionalHealthSOTypes[i].resourceAmount);
                    }
                }
            }

            SaveDataSerializeBase resourceSaveData;

            GameResourceSaveData resourceSaveDataObject = new GameResourceSaveData((int)coinResourceSO.resourceAmount, emotionalHealthTypesSaveDict);

            resourceSaveData = new SaveDataSerializeBase(resourceSaveDataObject, transform.position, scene.name);

            return resourceSaveData;
        }

        public void LoadData(SaveDataSerializeBase savedDataToLoad)
        {
            if (savedDataToLoad == null) return;

            GameResourceSaveData savedGameResource = (GameResourceSaveData)savedDataToLoad.LoadSavedObject();
            
            coinResourceSO.SetSpecificResourceAmount(savedGameResource.coinResourceAmountSave);

            if (savedGameResource.emotionalHealthTypesAmountSave == null || savedGameResource.emotionalHealthTypesAmountSave.Count == 0) return;

            if (emotionalHealthSOTypes == null && emotionalHealthSOTypes.Count == 0) return;
            
            for(int i = 0; i < emotionalHealthSOTypes.Count; i++)
            {
                if (emotionalHealthSOTypes[i] == null) continue;

                if(savedGameResource.emotionalHealthTypesAmountSave.ContainsKey(emotionalHealthSOTypes[i].name))
                {
                    float emotionalHealthAmount = savedGameResource.emotionalHealthTypesAmountSave[emotionalHealthSOTypes[i].name];

                    emotionalHealthSOTypes[i].SetSpecificResourceAmount(emotionalHealthAmount);
                }
            }
        }
    }
}
