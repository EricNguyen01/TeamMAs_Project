using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Plant Unit Data Asset/New Plant Unit")]
    public class PlantUnitSO : UnitSO, ISerializationCallbackReceiver
    {
        [field: Header("Plant Unit Data")]
        [field: SerializeField] public string unitID { get; private set; } //to be used with saving system
        [field: SerializeField] public string unitDescription { get; private set; }
        [field: SerializeField] public Sprite unitThumbnail { get; private set; }//thumbnail icon sprite to be displayed in shop or other UIs

        [field: Header("Plant Unit Stats")]
        [field: SerializeField] public PlantProjectileSO plantProjectileSO { get; private set; }
        [field: SerializeField] [field: Min(0)] public int waterBars { get; private set; }
        [field: SerializeField] [field: Min(0)] public int wavesSurviveWithoutWater { get; private set; } = 1;
        [field: SerializeField] [field: Min(0.0f)] public float damage { get; private set; }
        [field: SerializeField] [field: Min(0.0f)] public float attackSpeed { get; private set; }

        [field: SerializeField] [field: Min(1)]
        [field: Tooltip("How many tiles this unit's attacks/abilities can reach?")] 
        public int attackRangeInTiles { get; private set; } = 1;

        [field: SerializeField] [field: Min(1)]
        [field: Tooltip("How many targets this plant unit can attack per attack? 1 = no aoe atk while higher number = aoe.")]
        public int targetsPerAttack { get; private set; } = 1;

        [field: SerializeField] [field: Min(0.0f)] public float humanMultiplier { get; private set; }
        [field: SerializeField] [field: Min(0.0f)] public float pollinatorMultiplier { get; private set; }
        [field: SerializeField] public bool isPlacableOnPath { get; private set; } = false;
        [field: SerializeField] public string[] plantTargetsLayerNames;

        [field: Header("Plant Unit Water Usage and Costs")]
        [field: SerializeField] [field: Min(0)] public int plantingCoinCost { get; private set; }
        [field: SerializeField] [field: Min(0)] public int uprootRefundAmount { get; private set; }
        [field: SerializeField] [field: Min(0)] public int uprootCost { get; private set; }
        [field: SerializeField] [field: Min(0)] public int waterUse { get; private set; }
        [field: SerializeField] [field: Min(0)] public int waterBarsRefilledPerWatering { get; private set; } = 1;
        [field: SerializeField][field: Min(0)] public int wateringCoinsCost { get; private set; } = 1;

        //INTERNAL.........................................................
        //Saving system related
        private static Dictionary<string, PlantUnitSO> unitLookupCache;//to be used with saving system

        //PRIVATES:
        private static void GenerateUnitLookupCacheIfNull()
        {
            if (unitLookupCache != null) return;

            unitLookupCache = new Dictionary<string, PlantUnitSO>();//create one
            
            var unitList = Resources.LoadAll<PlantUnitSO>("ScriptableObjects/UnitSO");//get all scriptable objects under the resource folder

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
        public static PlantUnitSO GetItemFromProvidedItemID(string unitID)
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
