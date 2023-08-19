// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeamMAsTD
{
    [CreateAssetMenu(menuName = "GameResource Data Asset/New Game Resource")]
    public class GameResourceSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: SerializeField] public string resourceName { get; private set; }

        [SerializeField]
        [Tooltip("The lowest possible amount of this resource")]
        private float initialResourceAmountMin;//in-editor static only data

        [field: NonSerialized]
        public float resourceAmountMin { get; private set; } = 0.0f;//runtime non-static data

        [SerializeField]
        [Min(0)]
        private float initialResourceAmount;//in-editor static only SO data

        [field: NonSerialized]
        public float resourceAmount { get; private set; }//runtime non-static data

        [ReadOnlyInspector]
        [SerializeField] private float currentResourceAmount;

        [SerializeField]
        [Tooltip("The highest possible amount of this resource. If value is 0, this resource has an infinite cap.")]
        [Min(0)]
        private float initialResourceAmountCap;//in-editor static only SO data

        [field: NonSerialized]
        public float resourceAmountCap { get; private set; }//runtime non-static data

        //This event is sub by GameResourceUI.cs to update the UI display of resource amount
        public static event System.Action<GameResourceSO> OnResourceAmountUpdated;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            CheckResourceAmountMinMaxReached();

            if(currentResourceAmount != resourceAmount) currentResourceAmount = resourceAmount;

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

            currentResourceAmount = resourceAmount;

            OnResourceAmountUpdated?.Invoke(this);
        }

        public virtual void SetSpecificResourceAmount(float setAmount)
        {
            resourceAmount = setAmount;

            CheckResourceAmountMinMaxReached();

            currentResourceAmount = resourceAmount;

            OnResourceAmountUpdated?.Invoke(this);
        }

        public virtual void RemoveResourceAmount(float removedAmount)
        {
            resourceAmount -= removedAmount;

            CheckResourceAmountMinMaxReached();

            currentResourceAmount = resourceAmount;

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
            if(resourceAmount < resourceAmountMin)
            {
                resourceAmount = resourceAmountMin;
                return;
            }

            //if unlimited resource amount allowed -> exit function
            if (resourceAmountCap <= 0) return;

            //else if there's a cap and current amount is above it -> reset to cap amount
            if (resourceAmount > resourceAmountCap) resourceAmount = resourceAmountCap;
        }

        //ISerializationCallbackReceiver interface implementation....................................................
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            resourceAmountMin = initialResourceAmountMin;

            resourceAmount = initialResourceAmount;

            currentResourceAmount = resourceAmount;

            resourceAmountCap = initialResourceAmountCap;
        }
    }
}
