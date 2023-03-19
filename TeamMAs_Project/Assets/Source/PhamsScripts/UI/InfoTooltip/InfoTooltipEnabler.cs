using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class InfoTooltipEnabler : MonoBehaviour, IPointerDownHandler, IDeselectHandler
    {
        [field: Header("Required Components")]

        [field: SerializeField] public UnitSO unitScriptableObjectToDisplayTooltip { get; private set; }

        [SerializeField] private InfoTooltip infoTooltipPrefab;

        private InfoTooltip infoTooltip;

        [field: SerializeField] public Transform infoTooltipSpawnTransformRef { get; private set; }

        [field: SerializeField] public Vector2 infoTooltipSpawnOffset { get; private set; }

        [field: SerializeField] public AnimatorOverrideController clickReminderAnimOverride { get; private set; }

        [field: SerializeField] public bool autoEnableTooltipOnStart { get; private set; } = false;

        [SerializeField] private WaveSO waveToShowClickReminderAfterFinished;

        private WaveSO currentWave;

        [field: SerializeField] public bool toggleTooltipOnClick { get; private set; } = true;

        private PointerEventData pointerEventData;

        public InfoTooltipClickReminderDisplayTimer clickReminderDisplayTimer { get; private set; }

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

            //This function must be in here to avoid execution conflicts with WaveVisitorsLookAhead.cs
            //where WaveVisitorLookAhead has not finished initialized this visitor look ahead slot yet,
            //hence, this slot tooltip could receive a null data
            CreateAndInitInfoTooltip();

            WaveSpawner.OnWaveFinished += GetCurrentWaveOnWaveEndedEvent;

            Rain.OnRainEnded += EnableTooltipAfterRainIfApplicable;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= GetCurrentWaveOnWaveEndedEvent;

            Rain.OnRainEnded -= EnableTooltipAfterRainIfApplicable;
        }

        private void Start()
        {
            if(waveToShowClickReminderAfterFinished == null && autoEnableTooltipOnStart)
            {
                EnableInfoTooltipImage(true);

                EnableTooltipClickOnReminder(false);
            }
        }

        private void CreateAndInitInfoTooltip()
        {
            //if already created -> do nothing and exit
            if (infoTooltip != null) return;

            Vector2 tooltipSpawnPos;

            if (infoTooltipSpawnTransformRef != null) tooltipSpawnPos = (Vector2)infoTooltipSpawnTransformRef.position;
            else tooltipSpawnPos = (Vector2)transform.position + infoTooltipSpawnOffset;

            GameObject tooltipGO = Instantiate(infoTooltipPrefab.gameObject, tooltipSpawnPos, Quaternion.identity);

            infoTooltip = tooltipGO.GetComponent<InfoTooltip>();

            infoTooltip.InitializeInfoTooltip(this, unitScriptableObjectToDisplayTooltip);
        }

        public void UpdateInfoTooltipDataFrom(UnitSO unitSO)
        {
            //disable tooltip and tooltip click reminder on update
            EnableInfoTooltipImage(false);

            EnableTooltipClickOnReminder(false);

            //update SO data from external scripts/sources
            unitScriptableObjectToDisplayTooltip = unitSO;

            if(infoTooltip != null) infoTooltip.InitializeInfoTooltip(this, unitScriptableObjectToDisplayTooltip);
        }

        public void InfoTooltipImageToggle()
        {
            if (infoTooltip == null) return;

            if (!infoTooltip.isTooltipActive) EnableInfoTooltipImage(true);
            else EnableInfoTooltipImage(false);
        }

        public void EnableInfoTooltipImage(bool enabled, bool setTooltipClickReminderStatus = true)
        {
            if (infoTooltip == null) return;

            if (enabled)
            {
                infoTooltip.EnableInfoTooltipImage(true, setTooltipClickReminderStatus);

                /*The if below has been moved to UnitInfoTooltip.cs' EnableUnitInfoTooltipImage()
                if (clickReminderDisplayTimer != null && setTooltipClickReminderStatus) 
                { 
                    clickReminderDisplayTimer.SetReminderInactiveAndStopTimerOnTooltipOpened(this); 
                }*/

                return;
            }

            infoTooltip.EnableInfoTooltipImage(false, setTooltipClickReminderStatus);

            /*The if below has been moved to UnitInfoTooltip.cs' EnableUnitInfoTooltipImage()
            if (clickReminderDisplayTimer != null && setTooltipClickReminderStatus) 
            { 
                clickReminderDisplayTimer.StartClickOnReminderTimerOnTooltipClosed(this); 
            }*/
        }

        public void EnableTooltipClickOnReminder(bool enabled)
        {
            if (infoTooltip == null) return;

            if (enabled)
            {
                infoTooltip.EnableTooltipClickOnReminder(true);

                return;
            }

            infoTooltip.EnableTooltipClickOnReminder(false);
        }

        public void SetUnitTooltipClickReminderDisplayTimer(InfoTooltipClickReminderDisplayTimer clickReminderTimer)
        {
            clickReminderDisplayTimer = clickReminderTimer;
        }

        //Unity's EventSystem interfaces implementation.......................................................

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerEventData = eventData;

            if(toggleTooltipOnClick) InfoTooltipImageToggle();

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
                EnableInfoTooltipImage(false);
            }

            //after OnDeselect is called, EventSystem's selected object is set to null again so we don't have to reset it manually.
        }

        private void GetCurrentWaveOnWaveEndedEvent(WaveSpawner ws, int wNum, bool hasOngoingWave)
        {
            if (ws == null) return;

            if (hasOngoingWave) return;

            currentWave = ws.GetCurrentWave().waveSO;
        }

        private void EnableTooltipAfterRainIfApplicable(Rain r)
        {
            if (waveToShowClickReminderAfterFinished == null) return;

            if (currentWave == null) return;

            if (currentWave != waveToShowClickReminderAfterFinished) return;

            EnableInfoTooltipImage(true);
        }
    }
}
