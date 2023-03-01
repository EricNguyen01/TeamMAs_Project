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

        [SerializeField] private Canvas statPopupWorldUICanvas;

        [SerializeField] private Image statPopupUIImage;

        [SerializeField] private TextMeshProUGUI statPopupTextMesh;

        private RectTransform statPopupWorldUIRectTransform;

        [Header("Stat Popup Config")]

        [SerializeField] private Sprite positiveStatPopupSprite;

        [SerializeField] private Sprite negativeStatPopupSprite;

        [SerializeField] private Animator statPopupAnimator;

        [SerializeField] private AnimatorOverrideController positiveStatPopupAnimatorOverride;

        [SerializeField] private AnimatorOverrideController negativeStatPopupAnimatorOverride;

        [SerializeField] private string positiveStatText;

        [SerializeField] private string negativeStatText;

        [SerializeField] private Color positiveStatPopupTextColor;

        [SerializeField] private Color negativeStatPopupTextColor;

        private StatPopupPool statPopupPoolSpawnedThisPopup;

        private StatPopupSpawner statPopupSpawnerSpawnedThisPopup;

        private Vector3 startPos = Vector3.zero;
        
        private Vector3 endPos = Vector3.zero;

        private float popupTravelTime;

        private float currentTravelTime = 0.0f;

        private bool hasFinishedPoppingUp = false;

        private void Awake()
        {
            if(statPopupWorldUICanvas == null)
            {
                statPopupWorldUICanvas = GetComponentInChildren<Canvas>(true);
            }
            if(statPopupWorldUICanvas != null)
            {
                if(statPopupWorldUICanvas.worldCamera == null) statPopupWorldUICanvas.worldCamera = Camera.main;
            }

            if(statPopupUIImage == null)
            {
                statPopupUIImage = GetComponentInChildren<Image>(true);
            }
            if(statPopupTextMesh == null)
            {
                statPopupTextMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if(statPopupAnimator == null)
            {
                statPopupAnimator = GetComponent<Animator>();

                if(statPopupAnimator == null) statPopupAnimator = gameObject.AddComponent<Animator>();
            }

            //this script and its object's enabled/disabled status can only be controlled by StatPopupPool.
            //if this script is not spawned by a StatPopupPool, it will always disable the whole gameobject by default on awake.
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (statPopupPoolSpawnedThisPopup == null) gameObject.SetActive(false);

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
        }

        private void OnDisable()
        {
            //reset everything on disable
            currentTravelTime = 0.0f;
            hasFinishedPoppingUp = false;
        }

        private void Update()
        {
            if (hasFinishedPoppingUp) return;
            
            //process popup moving from its start to end position using lerp
            if (currentTravelTime < popupTravelTime)
            {
                Vector3 lerpedPos = Vector3.Lerp(startPos, endPos, currentTravelTime / popupTravelTime);

                transform.position = lerpedPos;

                currentTravelTime += Time.deltaTime;
            }
            else//on lerp finished
            {
                hasFinishedPoppingUp = true;

                transform.position = endPos;

                currentTravelTime = popupTravelTime;

                //if finished popping up, return this stat popup object to pool through calling below function from its stat popup spawner
                if (statPopupPoolSpawnedThisPopup != null) statPopupPoolSpawnedThisPopup.ReturnStatPopupGameObjectToPool(gameObject);
                //if the stat popup spawner of this stat popup is null->destroy this stat popup game object
                else Destroy(gameObject);
            }
        }

        public void InitializeStatPopup(StatPopupSpawner statPopupSpawner, StatPopupPool statPopupPool)
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
        }

        public void SetNewStatPopupSprite(Sprite spritePopup)
        {
            if (statPopupUIImage != null && spritePopup != null) statPopupUIImage.sprite = spritePopup;
        }

        public void SetStatPopupText(string textPopup)
        {
            if (statPopupTextMesh != null && !string.IsNullOrEmpty(textPopup)) statPopupTextMesh.text = textPopup;
        }

        public void UseDefaultStatPopupText(bool usePositiveText)
        {
            if (usePositiveText)
            {
                if (statPopupTextMesh != null) statPopupTextMesh.text = positiveStatText;

                return;
            }

            if (statPopupTextMesh != null) statPopupTextMesh.text = negativeStatText;
        }

        public void SetStatPopupTextColor(Color colorToSet)
        {
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

        public void SetStatPopupPositiveTextColor()
        {
            statPopupTextMesh.color = positiveStatPopupTextColor;
        }

        public void SetStatPopupNegativeTextColor()
        {
            statPopupTextMesh.color = negativeStatPopupTextColor;
        }

        public void SetStatPopupScaleMultipliers(float multipliers)
        {
            if (statPopupWorldUICanvas == null) return;

            if(statPopupWorldUIRectTransform == null)
            {
                statPopupWorldUIRectTransform = statPopupWorldUICanvas.GetComponent<RectTransform>();
            }

            if (statPopupWorldUIRectTransform == null) return;

            if (multipliers == 0) return;

            float sizeXMultiplied = statPopupWorldUIRectTransform.sizeDelta.x * multipliers;

            float sizeYMultiplied = statPopupWorldUIRectTransform.sizeDelta.y * multipliers;

            statPopupWorldUIRectTransform.sizeDelta = new Vector2(sizeXMultiplied, sizeYMultiplied);
        }
    }
}
