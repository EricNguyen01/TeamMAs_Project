using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveVisitorTypesLookAheadSlot : MonoBehaviour
    {
        [SerializeField] private Image slotUIImage;

        private void Awake()
        {
            if(slotUIImage == null)
            {
                slotUIImage = GetComponent<Image>();
            }

            if(slotUIImage == null)
            {
                Debug.LogWarning("Wave Visitor Types Look Ahead Slot: " + name + " is missing UI Image reference!");
            }
        }

        public void UpdateVisitorTypeLookAheadSlot(VisitorUnitSO visitorUnitSO)
        {
            UpdateSlotVisitorTypeVisualFrom(visitorUnitSO);
        }

        public void EnableLookAheadUISlot(bool shouldEnable)
        {
            if (shouldEnable)
            {
                if(!gameObject.activeInHierarchy) gameObject.SetActive(true);

                return;
            }

            if(gameObject.activeInHierarchy) gameObject.SetActive(false);
        }

        private void UpdateSlotVisitorTypeVisualFrom(VisitorUnitSO visitorUnitSO)
        {
            if (slotUIImage == null) return;

            if (visitorUnitSO == null) return;

            if (visitorUnitSO.unitPrefab == null) return;

            SpriteRenderer visitorSpriteRenderer = visitorUnitSO.unitPrefab.GetComponent<SpriteRenderer>();

            if (visitorSpriteRenderer == null) return;

            Sprite visitorSprite = visitorSpriteRenderer.sprite;

            if(visitorSprite == null) return;

            slotUIImage.sprite = visitorSprite;

            if (!slotUIImage.preserveAspect) slotUIImage.preserveAspect = true;
        }
    }
}
