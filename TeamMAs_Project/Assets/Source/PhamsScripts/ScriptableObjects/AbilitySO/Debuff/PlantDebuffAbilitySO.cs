using PixelCrushers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Ability Data Asset/DeBuff Ability/Plant DeBuff Ability")]
    public class PlantDebuffAbilitySO : GeneralDebuffAbilitySO
    {
        protected override void Awake()
        {
            base.Awake();

            abilityUseReservedFor = AbilityUseReservedFor.VisitorOnly;

            abilityOnlyAffect = AbilityOnlyAffect.PlantOnly;

            abilityAffectsSpecificVisitorType = VisitorUnitSO.VisitorType.None;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            abilityUseReservedFor = AbilityUseReservedFor.VisitorOnly;

            abilityOnlyAffect = AbilityOnlyAffect.PlantOnly;

            abilityAffectsSpecificVisitorType = VisitorUnitSO.VisitorType.None;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PlantDebuffAbilitySO))]
        private class PlantDebuffAbilitySOEditor : Editor
        {
            PlantDebuffAbilitySO plantDebuffAbilitySO;

            private void OnEnable()
            {
                plantDebuffAbilitySO = target as PlantDebuffAbilitySO;
            }

            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox(
                "This debuff ability is used by VisitorUnits ONLY to apply debuffs onto PlantUnits", MessageType.Warning);

                DrawDefaultInspector();

                EditorGUILayout.HelpBox(
                "This debuff ability is used by VisitorUnits ONLY to apply debuffs onto PlantUnits", MessageType.Warning);
            }
        }
    }
#endif
}
