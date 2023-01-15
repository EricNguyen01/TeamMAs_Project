using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "GameResource Data Asset/New Game Resource")]
    public class GameResourceSO : ScriptableObject
    {
        [field: SerializeField] public string resourceName { get; private set; }
        [field: SerializeField] [field: Min(0)] public float resourceAmount { get; private set; }
        [field: SerializeField] [field: Min(0)] public float resourceAmountCap { get; private set; } = 2000.0f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            CheckAmountMinMaxReached();
        }
#endif

        private void Awake()
        {
            CheckAmountMinMaxReached();
        }

        public void AddResourceAmount(float addedAmount)
        {
            resourceAmount += addedAmount;
            CheckAmountMinMaxReached();
        }

        public void RemoveResourceAmount(float removedAmount)
        {
            resourceAmount -= removedAmount;

            CheckAmountMinMaxReached();
        }

        private void CheckAmountMinMaxReached()
        {
            //if resource amount below 0 -> set = 0
            if(resourceAmount < 0.0f)
            {
                resourceAmount = 0.0f;
                return;
            }

            //if unlimited resource amount allowed -> exit function
            if (resourceAmountCap <= 0) return;

            //else if there's a cap and current amount is above it -> reset to cap amount
            if (resourceAmount > resourceAmountCap) resourceAmount = resourceAmountCap;
        }
    }
}
