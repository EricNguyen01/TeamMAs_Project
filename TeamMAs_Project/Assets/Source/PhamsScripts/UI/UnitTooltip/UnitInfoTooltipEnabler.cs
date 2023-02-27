using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UnitInfoTooltipEnabler : MonoBehaviour, IPointerDownHandler, IDeselectHandler
    {
        [field: Header("Required Components")]

        [field: SerializeField] public UnitSO unitScriptableObjectToDisplayTooltip { get; private set; }

        [SerializeField] private UnitInfoTooltip unitInfoTooltipPrefab;

        private UnitInfoTooltip unitInfoTooltip;

        [field: SerializeField] public Transform unitInfoTooltipSpawnTransformRef { get; private set; }

        [field: SerializeField] public Vector2 unitInfoTooltipSpawnOffset { get; private set; }

        [field: SerializeField] public AnimatorOverrideController clickReminderAnimOverride { get; private set; }

        private PointerEventData pointerEventData;

        private UnitInfoTooltipClickReminderDisplayTimer clickReminderDisplayTimer;

        private void OnEnable()
        {
            //check for an existing EventSystem and disble script if null
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogError("Cannot find an EventSystem in the scene. " +
                "An EventSystem is required for tile interaction menu to function. Disabling tile interaction menu!");

                enabled = false;

                return;
            }

            clickReminderDisplayTimer = GetComponentInParent<UnitInfoTooltipClickReminderDisplayTimer>();

            if (clickReminderDisplayTimer != null) clickReminderDisplayTimer.RegisterTooltipEnablerAsChildOfReminderTimerOnly(this);
        }

        private void OnDisable()
        {
            if(clickReminderDisplayTimer != null) clickReminderDisplayTimer.DeregisterTooltipEnablerFromReminderTotally(this);
        }

        private void Start()
        {
            //This function must be in Start() to avoid execution conflicts with WaveVisitorsLookAhead.cs
            //where WaveVisitorLookAhead has not finished initialized this visitor look ahead slot yet,
            //hence, this slot tooltip could receive a null data
            CreateAndInitUnitInfoTooltip();

            EnableUnitInfoTooltipImage(false);

            EnableTooltipClickOnReminder(false);
        }

        private void CreateAndInitUnitInfoTooltip()
        {
            Vector2 tooltipSpawnPos;

            if (unitInfoTooltipSpawnTransformRef != null) tooltipSpawnPos = (Vector2)unitInfoTooltipSpawnTransformRef.position;
            else tooltipSpawnPos = (Vector2)transform.position + unitInfoTooltipSpawnOffset;

            GameObject tooltipGO = Instantiate(unitInfoTooltipPrefab.gameObject, tooltipSpawnPos, Quaternion.identity);

            unitInfoTooltip = tooltipGO.GetComponent<UnitInfoTooltip>();

            unitInfoTooltip.InitializeUnitInfoTooltip(this, unitScriptableObjectToDisplayTooltip, Vector2.zero);
        }

        public void UpdateUnitInfoTooltipDataFrom(UnitSO unitSO)
        {
            //disable tooltip and tooltip click reminder on update
            EnableUnitInfoTooltipImage(false);

            EnableTooltipClickOnReminder(false);

            //update SO data from external scripts/sources
            unitScriptableObjectToDisplayTooltip = unitSO;
        }

        public void UnitInfoTooltipImageToggle()
        {
            if (unitInfoTooltip == null) return;

            if (!unitInfoTooltip.isTooltipActive) EnableUnitInfoTooltipImage(true);
            else EnableUnitInfoTooltipImage(false);
        }

        public void EnableUnitInfoTooltipImage(bool enabled)
        {
            if (unitInfoTooltip == null) return;

            if (enabled)
            {
                unitInfoTooltip.EnableUnitInfoTooltipImage(true);

                if (clickReminderDisplayTimer != null) clickReminderDisplayTimer.SetReminderInactiveAndStopTimerOnTooltipOpened(this);

                return;
            }

            unitInfoTooltip.EnableUnitInfoTooltipImage(false);

            if (clickReminderDisplayTimer != null) clickReminderDisplayTimer.StartClickOnReminderTimerOnTooltipClosed(this);
        }

        public void EnableTooltipClickOnReminder(bool enabled)
        {
            if (unitInfoTooltip == null) return;

            if (enabled)
            {
                unitInfoTooltip.EnableTooltipClickOnReminder(true);

                return;
            }

            unitInfoTooltip.EnableTooltipClickOnReminder(false);
        }

        public void SetUnitTooltipClickReminderDisplayTimer(UnitInfoTooltipClickReminderDisplayTimer clickReminderTimer)
        {
            clickReminderDisplayTimer = clickReminderTimer;
        }

        //Unity's EventSystem interfaces implementation.......................................................

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerEventData = eventData;

            UnitInfoTooltipImageToggle();

            //set the selected game object in the current event system so that
            //when the event system detects a newly selected game obj whether null or not,
            //it will trigger OnDeselect() on this script which can be used to close the tile menu.
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            //Only close if pointer is not over anything, not over an object, or over an object that is not the same as this obj.
            //This is done so that when clicking again on the same unit after opening the tooltip of that unit,
            //we don't want to close the tooltip in this OnDeselect function (which gets called even on selecting the same obj again)
            //just so we can open it again in OnPointerDown (OnPointerDown happens after OnDeselect) -> it essentially does nothing if happens.
            //We want to close the menu instead of keeping it open after re-clicking on the same unit.

            if (pointerEventData.pointerEnter == null || pointerEventData.pointerEnter.gameObject == null || pointerEventData.pointerEnter.gameObject != gameObject)
            {
                EnableUnitInfoTooltipImage(false);
            }

            //after OnDeselect is called, EventSystem's selected object is set to null again so we don't have to reset it manually.
        }
    }
}
