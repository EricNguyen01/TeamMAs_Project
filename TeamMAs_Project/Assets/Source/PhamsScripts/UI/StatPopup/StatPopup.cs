// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class StatPopup : MonoBehaviour
    {
        [Header("Stat Popup UI Components")]

        [SerializeField] private Image statPopupUIImage;

        [SerializeField] private TextMeshProUGUI statPopupTextMesh;

        [Header("Stat Popup Config")]

        [SerializeField] private Sprite positiveStatPopupSprite;

        [SerializeField] private Sprite negativeStatPopupSprite;

        [SerializeField] private Sprite neutralStatPopupSprite;

        [SerializeField] private Animator statPopupAnimator;

        [SerializeField] private AnimatorOverrideController positiveStatPopupAnimatorOverride;

        [SerializeField] private AnimatorOverrideController negativeStatPopupAnimatorOverride;

        [SerializeField] private AnimatorOverrideController neutralStatPopupAnimatorOverride;

        [SerializeField] private string positiveStatText;

        [SerializeField] private string negativeStatText;

        [SerializeField] private string neutralStatText;    

        [SerializeField] private Color positiveStatPopupTextColor;

        [SerializeField] private Color negativeStatPopupTextColor;

        [SerializeField] private Color neutralStatPopupTextColor = Color.gray;

        public Canvas statPopupCanvas { get; private set; }

        //INTERNALS.................................................................

        private StatPopupPool statPopupPoolSpawnedThisPopup;

        private StatPopupSpawner statPopupSpawnerSpawnedThisPopup;

        private Vector3 startPos = Vector3.zero;
        
        private Vector3 endPos = Vector3.zero;

        private Vector3 startingLocalScale;

        private Vector3 popupSpawnerInitPos = Vector3.zero;

        public enum PopUpType { Neutral = 0, Positive = 1, Negative = 2 }

        private float popupTravelTime;

        private float currentTravelTime = 0.0f;

        private bool hasFinishedPoppingUp = false;

        private void Awake()
        {
            if(statPopupUIImage == null)
            {
                statPopupUIImage = GetComponentInChildren<Image>(true);
            }
            if(statPopupTextMesh == null)
            {
                statPopupTextMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (statPopupUIImage != null) statPopupUIImage.raycastTarget = false;

            if(statPopupTextMesh != null) statPopupTextMesh.raycastTarget = false;

            if(statPopupAnimator == null)
            {
                statPopupAnimator = GetComponent<Animator>();

                if(statPopupAnimator == null) statPopupAnimator = gameObject.AddComponent<Animator>();
            }

            startingLocalScale = transform.localScale;

            Canvas popupCanvas = GetComponent<Canvas>();

            if (!popupCanvas) popupCanvas = GetComponentInChildren<Canvas>();

            if (popupCanvas && !popupCanvas.worldCamera)
            {
                statPopupCanvas = popupCanvas;

                popupCanvas.worldCamera = Camera.main;
            }

            //this script and its object's enabled/disabled status can only be controlled by StatPopupPool.
            //if this script is not spawned by a StatPopupPool, it will always disable the whole gameobject by default on awake.
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if(statPopupUIImage != null)
            {
                Color color = statPopupUIImage.color;

                if(statPopupUIImage.sprite == null)
                {
                    color.a = 0.0f;
                    statPopupUIImage.color = color;
                }
                else
                {
                    color.a = 100.0f;
                    statPopupUIImage.color = color;
                }
            }

            Canvas popupCanvas = GetComponentInChildren<Canvas>();

            if (popupCanvas && !popupCanvas.worldCamera)
            {
                popupCanvas.worldCamera = Camera.main;
            }
        }

        private void OnDisable()
        {
            //reset everything on disable
            currentTravelTime = 0.0f;

            hasFinishedPoppingUp = false;
        }

        private void Update()
        {
            if (!enabled || !gameObject.activeInHierarchy) return;

            if(statPopupPoolSpawnedThisPopup == null || !statPopupSpawnerSpawnedThisPopup)
            {
                Destroy(gameObject);

                return;
            }

            if (hasFinishedPoppingUp) return;

            float spawnerXDiff = statPopupSpawnerSpawnedThisPopup.transform.position.x - popupSpawnerInitPos.x;

            //process popup moving from its start to end position using lerp
            if (currentTravelTime < popupTravelTime)
            {
                Vector3 lerpedPos = Vector3.Lerp(startPos, endPos, currentTravelTime / popupTravelTime);

                lerpedPos = new Vector3(lerpedPos.x + spawnerXDiff, lerpedPos.y, lerpedPos.z);

                transform.position = lerpedPos;

                currentTravelTime += Time.deltaTime;
            }
            else//on lerp finished
            {
                hasFinishedPoppingUp = true;

                Vector3 finalEndPos = new Vector3(endPos.x + spawnerXDiff, endPos.y, endPos.z);

                transform.position = finalEndPos;

                currentTravelTime = popupTravelTime;

                //if finished popping up, return this stat popup object to pool through calling below function from its stat popup spawner
                if (statPopupPoolSpawnedThisPopup != null && statPopupSpawnerSpawnedThisPopup)
                {
                    statPopupPoolSpawnedThisPopup.ReturnStatPopupGameObjectToPool(gameObject);
                }
                //if the stat popup spawner of this stat popup is null->destroy this stat popup game object
                else 
                { 
                    Destroy(gameObject); 
                }

                return;
            }
        }

        public void InitializeStatPopup(StatPopupPool statPopupPool)
        {
            statPopupPoolSpawnedThisPopup = statPopupPool;

            if (statPopupPoolSpawnedThisPopup != null)
            {
                statPopupSpawnerSpawnedThisPopup = statPopupPoolSpawnedThisPopup.statPopupSpawnerSpawnedThisPool;

                if (statPopupSpawnerSpawnedThisPopup != null)
                {
                    SetStatPopupScaleMultipliers(statPopupSpawnerSpawnedThisPopup.statPopupScaleMultiplier);
                }
            }
        }

        public void InitializeStatPopup(Vector3 startPos, Vector3 endPos, float travelTime)
        {
            this.startPos = startPos;

            transform.position = this.startPos;

            this.endPos = endPos;

            popupTravelTime = travelTime;

            if(statPopupSpawnerSpawnedThisPopup) popupSpawnerInitPos = statPopupSpawnerSpawnedThisPopup.transform.position;
        }

        public void SetNewStatPopupSprite(Sprite spritePopup)
        {
            if (statPopupUIImage != null && spritePopup != null) statPopupUIImage.sprite = spritePopup;
        }

        public void SetStatPopupText(string textPopup)
        {
            if (statPopupTextMesh != null && !string.IsNullOrEmpty(textPopup)) 
            {
                statPopupTextMesh.text = textPopup;
            }
        }

        public void UseDefaultStatPopupText(PopUpType popUpType)
        {
            if (popUpType == PopUpType.Positive)
            {
                if (statPopupTextMesh != null) 
                {
                    statPopupTextMesh.text = positiveStatText;
                }

                return;
            }

            else if (popUpType == PopUpType.Negative)
            {
                if (statPopupTextMesh != null)
                {
                    statPopupTextMesh.text = negativeStatText;
                }

                return;
            }

            else if(popUpType == PopUpType.Neutral)
            {
                if (statPopupTextMesh != null)
                {
                    statPopupTextMesh.text = neutralStatText;
                }
            }
        }

        public void SetStatPopupTextColor(Color colorToSet)
        {
            if (!statPopupTextMesh) return;

            statPopupTextMesh.color = colorToSet;
        }

        public void SetPositiveStatPopupSprite()
        {
            if(statPopupAnimator != null && positiveStatPopupAnimatorOverride != null)
            {
                statPopupAnimator.runtimeAnimatorController = positiveStatPopupAnimatorOverride;

                return;
            }

            if (positiveStatPopupSprite == null) return;

            statPopupUIImage.sprite = positiveStatPopupSprite;
        }

        public void SetNegativeStatPopupSprite()
        {
            if(statPopupAnimator != null && negativeStatPopupAnimatorOverride != null)
            {
                statPopupAnimator.runtimeAnimatorController = negativeStatPopupAnimatorOverride;

                return;
            }

            if(negativeStatPopupSprite == null) return;

            statPopupUIImage.sprite = negativeStatPopupSprite;
        }

        public void SetNeutralStatPopupSprite()
        {
            if (statPopupAnimator != null && neutralStatPopupAnimatorOverride != null)
            {
                statPopupAnimator.runtimeAnimatorController = neutralStatPopupAnimatorOverride;

                return;
            }

            if (neutralStatPopupSprite == null) return;

            statPopupUIImage.sprite = neutralStatPopupSprite;
        }

        public void SetStatPopupPositiveTextColor()
        {
            if (!statPopupTextMesh) return;

            statPopupTextMesh.color = positiveStatPopupTextColor;
        }

        public void SetStatPopupNegativeTextColor()
        {
            if (!statPopupTextMesh) return;

            statPopupTextMesh.color = negativeStatPopupTextColor;
        }

        public void SetStatPopupNeutralTextColor()
        {
            if (!statPopupTextMesh) return;

            statPopupTextMesh.color = neutralStatPopupTextColor;
        }

        public void SetStatPopupScaleMultipliers(float multipliers)
        {
            if (multipliers <= 0.0f) return;

            transform.localScale = startingLocalScale;

            transform.localScale = new Vector3(transform.localScale.x * multipliers, transform.localScale.y * multipliers, transform.localScale.z);
        }
    }
}
