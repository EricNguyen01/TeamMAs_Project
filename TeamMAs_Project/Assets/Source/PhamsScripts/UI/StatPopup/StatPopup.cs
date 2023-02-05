using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    public class StatPopup : MonoBehaviour
    {
        [SerializeField] private Canvas statPopupWorldUICanvas;

        [SerializeField] private Image statPopupUIImage;

        [SerializeField] private TextMeshProUGUI statPopupTextMesh;

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

            //this script and its object's enabled/disabled status can only be controlled by StatPopupPool.
            //if this script is not spawned by a StatPopupPool, it will always disable the whole gameobject by default on awake.
            gameObject.SetActive(false);
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
                if (statPopupSpawnerSpawnedThisPopup != null) statPopupSpawnerSpawnedThisPopup.ReturnStatPopupToPool(this);
                //if the stat popup spawner of this stat popup is null->destroy this stat popup game object
                else Destroy(gameObject);
            }
        }

        public void InitializeStatPopup(StatPopupSpawner statPopupSpawner, Vector3 startPos, Vector3 endPos, float travelTime)
        {
            statPopupSpawnerSpawnedThisPopup = statPopupSpawner;
            
            this.startPos = startPos;

            transform.position = this.startPos;

            this.endPos = endPos;

            popupTravelTime = travelTime;
        }

        public void SetStatPopupImageAndText(Sprite imagePopup, string textPopup)
        {
            if (statPopupUIImage != null && imagePopup != null) statPopupUIImage.sprite = imagePopup;

            if(statPopupTextMesh != null && !string.IsNullOrEmpty(textPopup)) statPopupTextMesh.text = textPopup;
        }
    }
}
