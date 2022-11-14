using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Unit Asset/New Unit")]
    public class UnitSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: Header("Unit Data")]
        [field: SerializeField] public string unitID { get; private set; } //to be used with saving system
        [field: SerializeField] public string displayName { get; private set; }
        [field: SerializeField] public string unitDescription { get; private set; }
        [field: SerializeField] public Sprite unitThumbnail { get; private set; }//thumbnail icon sprite to be displayed in shop or other UIs
        [field: SerializeField] public GameObject unitPrefab { get; private set; }

        [field: Header("Unit Stats")]
        [field: SerializeField] [field: Min(0)] public int waterBars { get; private set; }
        [field: SerializeField] [field: Min(0.0f)] public float damage { get; private set; }
        [field: SerializeField] [field: Min(0.0f)] public float attackSpeed { get; private set; }

        [field: SerializeField] [field: Min(1)]
        [field: Tooltip("How many tiles this unit's attacks/abilities can reach?")] public int attackRangeInTiles { get; private set; } = 1;

        [field: SerializeField] [field: Min(0.0f)] public float humanMultiplier { get; private set; }
        [field: SerializeField] [field: Min(0.0f)] public float pollinatorMultiplier { get; private set; }
        [field: SerializeField] public bool isPlacableOnPath { get; private set; } = false;

        [field: Header("Unit Costs")]
        [field: SerializeField] [field: Min(0)] public int plantingCoinCost { get; private set; }
        [field: SerializeField] [field: Min(0)] public int waterUse { get; private set; }

        //INTERNAL.........................................................
        //Saving system related
        private static Dictionary<string, UnitSO> unitLookupCache;//to be used with saving system

        //PRIVATES:
        private static void GenerateUnitLookupCacheIfNull()
        {
            if (unitLookupCache != null) return;

            unitLookupCache = new Dictionary<string, UnitSO>();//create one
            
            var unitList = Resources.LoadAll<UnitSO>("");//get all scriptable objects under the resource folder

            foreach (var unit in unitList)
            {
                if (unitLookupCache.ContainsKey(unit.unitID))//if duplicate
                {
                    Debug.LogError(string.Format("Looks like there's a duplicate ID: " + unit.unitID + " for object: " + unitLookupCache[unit.unitID], unit) + ". Generating new ID for object.");
                    unit.GenerateNewUnitID();//re-generate a new ID to replace the duplicated one
                    ///continue;
                }

                unitLookupCache[unit.unitID] = unit;//if not duplicate -> set dictionary ID and corresponding item data
            }
        }

        private void GenerateNewUnitID()
        {
            unitID = System.Guid.NewGuid().ToString();
        }

        //PUBLICS.................................................................

        //This method returns the scriptable object if it has the same ID with the provided ID
        //To be used alongside saving system where the ID is saved and when the game is reload, items will be loaded based on which IDs have been saved
        public static UnitSO GetItemFromProvidedItemID(string unitID)
        {
            //GENERATE AND FILL ITEM CACHE DICTIONARY IF NULL OPERATION:

            GenerateUnitLookupCacheIfNull();

            //ELSE IF ITEM CACHE DICT EXISTS AND IS NOT NULL THEN JUMP STRAIGHT TO
            //RETURNING AN ITEM ACCORDING TO ID OPERATION:

            if (unitID == null || !unitLookupCache.ContainsKey(unitID)) return null;//edge case check (id is not provided or not found in cache dict)

            return unitLookupCache[unitID];
        }

        //ISerializationCallbackReceiver interface functions.............................
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Generate and save a new UUID if there is no ID on before serialize
            if (string.IsNullOrWhiteSpace(unitID))
            {
                GenerateNewUnitID();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }
    }
}
