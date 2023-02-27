using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveVisitorTypesLookAheadSlot : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private Image slotUIImage;

        public UnitInfoTooltipEnabler unitInfoTooltipEnabler { get; private set; }

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

            unitInfoTooltipEnabler = GetComponent<UnitInfoTooltipEnabler>();
        }

        public void UpdateVisitorTypeLookAheadSlot(VisitorUnitSO visitorUnitSO)
        {
            UpdateSlotVisitorTypeVisualFrom(visitorUnitSO);

            UpdateVisitorInfoTooltipIfApplicable(visitorUnitSO);
        }

        public void EnableLookAheadUISlot(bool shouldEnable)
        {
            if (shouldEnable)
            {
                if(!gameObject.activeInHierarchy) gameObject.SetActive(true);

                return;
            }

            //disable tooltip (in case it's opened) on visitor type look ahead slot is disabled
            if (unitInfoTooltipEnabler != null) 
            { 
                unitInfoTooltipEnabler.EnableUnitInfoTooltipImage(false);

                unitInfoTooltipEnabler.EnableTooltipClickOnReminder(false);
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

        private void UpdateVisitorInfoTooltipIfApplicable(VisitorUnitSO visitorSO)
        {
            if (unitInfoTooltipEnabler == null) return;

            unitInfoTooltipEnabler.UpdateUnitInfoTooltipDataFrom(visitorSO);
        }

        public void RegisterThisSlotUnitTooltipToTooltipClickOnReminder(UnitInfoTooltipClickReminderDisplayTimer reminderTimer)
        {
            if(reminderTimer == null) return;

            if(unitInfoTooltipEnabler == null)
            {
                unitInfoTooltipEnabler = GetComponent<UnitInfoTooltipEnabler>();

                if (unitInfoTooltipEnabler == null) return;
            }

            reminderTimer.RegisterTooltipEnablerInChildListOnly(unitInfoTooltipEnabler);
        }
    }
}
