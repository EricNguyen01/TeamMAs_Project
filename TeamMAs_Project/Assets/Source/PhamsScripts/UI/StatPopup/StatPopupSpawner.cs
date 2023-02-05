using System.Collections;
using System.Collections.Generic;
using TeamMAsTD;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class StatPopupSpawner : MonoBehaviour
    {
        [Header("Popup Components")]

        [SerializeField] protected StatPopup statPopupPrefab;

        [Header("Popup Config")]

        [SerializeField]
        [Tooltip("Vertical offset from this object's position that this popup will appear from.")]
        protected float startVerticalOffset = 0.0f;

        [SerializeField]
        [Tooltip("Horizontal offset from this object's position that this popup will appear from.")]
        protected float startHorizontalOffset = 0.0f;

        [SerializeField]
        [Tooltip("Vertical distance from this popup's start position that this popup will travel.")]
        protected float verticalDistanceFromStart = 0.7f;

        [SerializeField]
        [Tooltip("Horizontal distance from this popup's start position that this popup will travel.")]
        protected float horizontalDistFromStart = 0.3f;

        [SerializeField]
        [Tooltip("Calculate a random horizontal distance offset from start pos.")]
        protected bool randomHorizontalTravelDistFromStart = true;

        [SerializeField]
        [Tooltip("Same with random horizontal travel dist but vertical.")]
        protected bool randomVerticalTravelDistFromStart = false;

        [SerializeField]
        [Tooltip("The time it takes for popup to move from start to end position.")]
        protected float popupTime = 0.7f;

        protected StatPopupPool statPopupPool;

        protected virtual void Awake()
        {
            if (statPopupPrefab == null)
            {
                Debug.LogError("Stat Popup Prefab GameObject is not assigned on StatPopup: " + name + " " +
                    "with HashID: " + GetHashCode().ToString() + ". Disabling StatPopup...!");

                enabled = false;
                return;
            }

            statPopupPool = new StatPopupPool(this, statPopupPrefab.gameObject, transform);

            statPopupPool.CreateAndAddStatPopupsToPool(5);
        }

        public virtual void PopUp(Sprite spriteToPopup, string textToPopup)
        {
            Vector3 popupStartPos = transform.position + new Vector3(startHorizontalOffset, startVerticalOffset, transform.position.z);

            float horDist = horizontalDistFromStart;
            float vertDist = verticalDistanceFromStart;

            if (randomHorizontalTravelDistFromStart) horDist = Random.Range(0.01f, horizontalDistFromStart);
            
            if(randomVerticalTravelDistFromStart) vertDist = Random.Range(0.01f, verticalDistanceFromStart);

            Vector3 popupEndPos = popupStartPos + new Vector3(horDist, vertDist, popupStartPos.z);

            //This EnableStatPopupGameObjectFromPool in StatPopupPool script both enables and initializes the StatPopup obj at the same time.
            GameObject statPopupObj = statPopupPool.EnableStatPopupGameObjectFromPool(this, spriteToPopup, textToPopup, popupStartPos, popupEndPos, popupTime);

            if(statPopupObj == null)
            {
                Debug.LogWarning("StatPopup GameObjects spawned from StatPopupPrefab by StatPopupSpawner: " + name + " is missing StatPopup script! " +
                    "Please check if a StatPopup script is attached to the StatPopupPrefab.");
            }
        }

        public virtual void ReturnStatPopupToPool(StatPopup statPopup)
        {
            if (statPopup == null) return;

            statPopupPool.ReturnStatPopupGameObjectToPool(statPopup.gameObject);
        }
    }
}
