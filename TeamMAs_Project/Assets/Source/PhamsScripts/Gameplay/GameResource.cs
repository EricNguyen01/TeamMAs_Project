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
    public class GameResource : MonoBehaviour
    {
        [field: SerializeField] public CoinResourceSO coinResourceSO { get; private set; }

        [field: SerializeField] public EmotionalHealthGameResourceSO emotionalHealthSO { get; private set; }
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

            CheckGameResourceDependencies();
        }

        private void CheckGameResourceDependencies()
        {
            if(coinResourceSO == null)
            {
                Debug.LogError("Coin Resource SO reference is missing in GameResource: " + name + "!");
            }

            if(emotionalHealthSO == null)
            {
                Debug.LogError("Emotional Health SO reference is missing in GameResource: " + name + "!");
            }
        }
    }
}
