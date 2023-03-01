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

        [field: SerializeField] 
        [field: Tooltip("The lowest possible amount of this resource")] 
        public float resourceAmountBase { get; private set; } = 0.0f;

        [field: SerializeField] 
        [field: Min(0)] 
        public float resourceAmount { get; private set; }

        [field: SerializeField]
        [field: Tooltip("The highest possible amount of this resource. If value is 0, this resource has an infinite cap.")]
        [field: Min(0)] 
        public float resourceAmountCap { get; private set; }

        //This event is sub by GameResourceUI.cs to update the UI display of resource amount
        public static event System.Action<GameResourceSO> OnResourceAmountUpdated;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            CheckResourceAmountMinMaxReached();

            OnResourceAmountUpdated?.Invoke(this);
        }
#endif

        /*protected virtual void Awake()
        {
            CheckResourceAmountMinMaxReached();

            OnResourceAmountUpdated?.Invoke(this);
        }*/

        public virtual void AddResourceAmount(float addedAmount)
        {
            resourceAmount += addedAmount;

            CheckResourceAmountMinMaxReached();

            OnResourceAmountUpdated?.Invoke(this);
        }

        public virtual void RemoveResourceAmount(float removedAmount)
        {
            resourceAmount -= removedAmount;

            CheckResourceAmountMinMaxReached();

            OnResourceAmountUpdated?.Invoke(this);
        }

        public virtual void IncreaseResourceAmountCap(float increaseAmount, bool matchResourceAmountToNewCapAmount)
        {
            resourceAmountCap += increaseAmount;

            if (matchResourceAmountToNewCapAmount) resourceAmount = resourceAmountCap;

            OnResourceAmountUpdated?.Invoke(this);
        }

        public virtual void DecreaseResourceAmountCap(float decreaseAmount)
        {
            resourceAmountCap -= decreaseAmount;

            CheckResourceAmountMinMaxReached();

            OnResourceAmountUpdated?.Invoke(this);
        }

        protected virtual void CheckResourceAmountMinMaxReached()
        {
            //if resource amount below 0 -> set = 0
            if(resourceAmount < resourceAmountBase)
            {
                resourceAmount = resourceAmountBase;
                return;
            }

            //if unlimited resource amount allowed -> exit function
            if (resourceAmountCap <= 0) return;

            //else if there's a cap and current amount is above it -> reset to cap amount
            if (resourceAmount > resourceAmountCap) resourceAmount = resourceAmountCap;
        }
    }
}
