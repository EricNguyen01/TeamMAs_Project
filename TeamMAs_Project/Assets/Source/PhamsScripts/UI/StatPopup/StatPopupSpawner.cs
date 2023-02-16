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

        [field: Header("Popup Config")]

        [field: SerializeField]
        [Tooltip("Scale multipliers to the width and height of the stat popup's Canvas UI Component.")]
        public float statPopupScaleMultiplier { get; private set; } = 0.0f;

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

        [SerializeField]
        [Tooltip("The number of stat popup objects to spawn into a pool before runtime so that they can be enable when needed later")]
        protected int popupNumberToPool = 1;

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

            statPopupPool.CreateAndAddStatPopupsToPool(popupNumberToPool);
        }

        public void SetStatPopupSpawnerConfig(float startVertOffset = 0.0f, float startHorOffset = 0.0f, float vertDistFromStart = 0.0f, float horDistFromStart = 0.0f, float popupScaleMultiplier = 0.0f, float popupTime = 0.0f, bool randomTravelVert = false, bool randomTravelHor = true)
        {
            if(startVertOffset > 0.0f)
            {
                startVerticalOffset = startVertOffset;
            }
            if(startHorOffset > 0.0f)
            {
                startHorizontalOffset = startHorOffset;
            }
            if(vertDistFromStart > 0.0f)
            {
                verticalDistanceFromStart = vertDistFromStart;
            }
            if(horDistFromStart > 0.0f)
            {
                horizontalDistFromStart = horDistFromStart;
            }
            if(popupScaleMultiplier > 0.0f)
            {
                statPopupScaleMultiplier = popupScaleMultiplier;
            }

            randomHorizontalTravelDistFromStart = randomTravelHor;

            randomVerticalTravelDistFromStart = randomTravelVert;

            if(popupTime > 0.0f)
            {
                this.popupTime = popupTime;
            }
        }
        
        public virtual void PopUp(Sprite spriteToPopup, string textToPopup, bool isPositivePopup)
        {
            Vector3 popupStartPos = transform.position + new Vector3(startHorizontalOffset, startVerticalOffset, transform.position.z);

            float horDist = horizontalDistFromStart;
            float vertDist = verticalDistanceFromStart;

            if (randomHorizontalTravelDistFromStart) horDist = Random.Range(0.01f, horizontalDistFromStart);
            
            if(randomVerticalTravelDistFromStart) vertDist = Random.Range(0.01f, verticalDistanceFromStart);

            Vector3 popupEndPos = popupStartPos + new Vector3(horDist, vertDist, popupStartPos.z);

            //This EnableStatPopupGameObjectFromPool in StatPopupPool script both enables and initializes the StatPopup obj at the same time.
            GameObject statPopupObj = statPopupPool.Init_And_Enable_StatPopup_GameObject_FromPool(spriteToPopup, 
                                                                                                  textToPopup, 
                                                                                                  isPositivePopup,
                                                                                                  popupStartPos, 
                                                                                                  popupEndPos, 
                                                                                                  popupTime);

            if(statPopupObj == null)
            {
                Debug.LogWarning("StatPopup GameObjects spawned from StatPopupPrefab by StatPopupSpawner: " + name + " is missing StatPopup script! " +
                    "Please check if a StatPopup script is attached to the StatPopupPrefab.");
            }
        }
    }
}
