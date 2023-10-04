// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Plant Unit Data Asset/New Plant Unit")]
    public class PlantUnitSO : UnitSO
    {
        [field: Header("Plant Unit Data")]

        //dynamic ID is used to identify specific SO INSTANCE of this SO (different for each instance)
        [field: ReadOnlyInspector]
        [field: SerializeField]
        public string unitDynamicID { get; private set; }

        //static ID is used to identify if an SO INSTANCE is of this specific SO (same for all instances and the base SO in folder)
        [field: ReadOnlyInspector]
        [field: SerializeField]
        public string unitStaticID { get; private set; }
        [field: SerializeField] public string unitDescription { get; private set; }
        [field: SerializeField] public Sprite unitThumbnail { get; private set; }//thumbnail icon sprite to be displayed in shop or other UIs

        [field: Header("Plant Unit Stats")]

        [field: ReadOnlyInspectorPlayMode]
        [field: SerializeField] 
        public PlantProjectileSO plantProjectileSO { get; private set; }
        [field: SerializeField][field: Min(0)] public int waterBars { get; private set; }
        [field: SerializeField][field: Min(0)] public int wavesSurviveWithoutWater { get; private set; } = 1;
        [field: SerializeField][field: Min(0.0f)] public float damage { get; private set; }
        [field: SerializeField][field: Min(0.0f)] public float attackSpeed { get; private set; }

        [field: SerializeField][field: Min(1)]
        [field: Tooltip("How many tiles this unit's attacks/abilities can reach?")]
        public int attackRangeInTiles { get; private set; } = 1;

        [field: SerializeField]
        public PlantRangeCircle plantRangeCirclePrefab { get; private set; }

        [field: SerializeField][field: Min(1)]
        [field: Tooltip("How many targets this plant unit can attack per attack? 1 = no aoe atk while higher number = aoe.")]
        public int targetsPerAttack { get; private set; } = 1;

        [field: SerializeField][field: Min(0.0f)] public float humanMultiplier { get; private set; }
        [field: SerializeField][field: Min(0.0f)] public float pollinatorMultiplier { get; private set; }
        [field: SerializeField] public bool isPlacableOnPath { get; private set; } = false;

        [field: SerializeField] public string[] plantTargetsLayerNames;

        [SerializeField] public VisitorUnitSO.VisitorType plantTargetsSpecifically = VisitorUnitSO.VisitorType.None;//default

        [field: Header("Plant Unit Water Usage and Costs")]

        [field: SerializeField][field: Min(0)] public int plantingCoinCost { get; private set; }
        [field: SerializeField][field: Min(0)] public int uprootRefundAmount { get; private set; }
        [field: SerializeField][field: Min(0)] public int uprootCost { get; private set; }
        [field: SerializeField][field: Min(0)] public int uprootHealthCost { get; private set; }
        [field: SerializeField][field: Min(0)] public int waterUse { get; private set; }
        [field: SerializeField][field: Min(0)] public int waterBarsRefilledPerWatering { get; private set; } = 1;
        [field: SerializeField][field: Min(0)] public int wateringCoinsCost { get; private set; } = 1;

        [field: Header("Plant Purchase Settings")]

        [field: SerializeField] public bool canPurchasePlant { get; private set; } = true;
        [field: SerializeField] public bool plantPurchaseLockOnStart { get; private set; } = true;
        [field: SerializeField] public WaveSO waveToUnlockPlantPurchaseOnWaveFinished { get; private set; }
        [field: SerializeField] public WaveSO waveToUnlockPlantPurchaseOnWaveStarted { get; private set; }

        //INTERNALS................................................................................................

        public Sprite plantThumbnailPlaceholderSpr { get; private set; }

        private void OnEnable()
        {
            SetDynamicID_IfNull_NonSerializable();

#if UNITY_EDITOR

            SetStaticID_IfNull_Serializable();

            plantThumbnailPlaceholderSpr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
        }

        private void OnValidate()
        {
            SetDynamicID_IfNull_NonSerializable();

#if UNITY_EDITOR

            SetStaticID_IfNull_Serializable();
#endif
        }

        public override UnitSO CloneUnitSO(UnitSO unitSO)
        {
            if (unitSO.GetType() != typeof(PlantUnitSO)) return null;

            PlantUnitSO instantiatedPlantUnitSO = Instantiate((PlantUnitSO)unitSO);

            instantiatedPlantUnitSO.SetDynamicID_IfNull_NonSerializable();

            return instantiatedPlantUnitSO;
        }

        private void SetDynamicID_IfNull_NonSerializable()
        {
            if (string.IsNullOrEmpty(unitDynamicID) || 
                string.IsNullOrWhiteSpace(unitDynamicID) || 
                unitDynamicID == "" //|| 
                /*!HelperFunctions.ObjectHasUniqueID(unitDynamicID, this)*/)
            {
                unitDynamicID = System.Guid.NewGuid().ToString();
            }
        }

        private void SetStaticID_IfNull_Serializable()
        {
            if (Application.isPlaying) return;

            SerializedObject serializedObject = new SerializedObject(this);

            //since unitStaticID is an auto-implemented property, its name has to be converted to the format that is find-able by 
            //the FindProperty function.
            //If we open Inspector DebugMode and hover over the unitStaticID field, the actual name will be displayed as tooltip.
            SerializedProperty staticIDProperty = HelperFunctions.FindPropertyByAutoPropertyName(serializedObject, "unitStaticID");
            
            //if this does not have an UUID yet or has an ID that overlaps another object's ID -> provide new one
            if (staticIDProperty.stringValue == "" ||
                string.IsNullOrEmpty(staticIDProperty.stringValue) ||
                string.IsNullOrWhiteSpace(staticIDProperty.stringValue) //||
                /*!HelperFunctions.ObjectHasUniqueID(staticIDProperty.stringValue, this)*/)
            {
                staticIDProperty.stringValue = System.Guid.NewGuid().ToString();

                serializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssetIfDirty(this);
            }
        }

        public void SetSpecificPlantUnitDamage(float damage)
        {
            this.damage = damage;

            if (damage <= 0.0f) damage = 0.0f;
        }

        public void AddPlantUnitDamage(float addedDamage)
        {
            damage += addedDamage;
        }

        public void RemovePlantUnitDamage(float removedDamage)
        {
            damage -= removedDamage;

            if (damage <= 0.0f) damage = 0.0f;
        }

        public void SetSpecificPlantUnitAttackSpeed(float atkSpeed)
        {
            attackSpeed = atkSpeed;

            if (attackSpeed <= 0.0f) attackSpeed = 0.0f;
        }

        public void AddPlantAttackSpeed(float atkSpdIncreaseAmount)
        {
            attackSpeed -= atkSpdIncreaseAmount;

            if (attackSpeed <= 0.0f) attackSpeed = 0.0f;
        }

        public void RemovePlantAttackSpeed(float atkSpdDecreaseAmount)
        {
            attackSpeed += atkSpdDecreaseAmount;
        }
    }
}
