// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class InfoTooltipClickReminderDisplayTimer : MonoBehaviour
    {
        [Header("Component Config")]

        [SerializeField] private List<InfoTooltipEnabler> tooltipsToDisplayClickReminder = new List<InfoTooltipEnabler>();

        private List<InfoTooltipEnabler> childTooltipsToCheckForReminders = new List<InfoTooltipEnabler>();

        [SerializeField] [Min(1.0f)] private float timeUntilNextReminderMin = 1000f;

        [SerializeField] [Min(2.0f)] private float timeUntilNextReminderMax = 2000f;

        private float timeUntilNextReminder = 0.0f;

        private float currentTimeUntilNextReminder = 0.0f;

        [SerializeField] [Min(1.0f)] private float timeToCloseReminder = 300.0f;

        private float currentTimeToCloseReminder = 0.0f;

        [field: SerializeField] public bool showTooltipClickReminderOnStart { get; private set; } = true;

        [SerializeField] private WaveSO waveToShowClickReminderAfterFinished;

        private WaveSO currentWave;

        private bool hasATooltipBeenOpenedRecently = false;

        private bool shouldStartClickReminderTimer = false;

        private bool timerIsPaused = false;

        private void Awake()
        {
            if (timeUntilNextReminderMax <= timeUntilNextReminderMin) timeUntilNextReminderMax += timeUntilNextReminderMin;

            timeUntilNextReminder = Random.Range(timeUntilNextReminderMin, timeUntilNextReminderMax);
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += GetCurrentWaveOnWaveEndedEvent;

            Rain.OnRainEnded += EnableClickOnReminderAfterRainIfApplicable;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= GetCurrentWaveOnWaveEndedEvent;

            Rain.OnRainEnded -= EnableClickOnReminderAfterRainIfApplicable;

            StopAllCoroutines();
        }

        private void Start()
        {
            HasValidSelectedAndChildrenTooltips();

            //provide this component reference to its children unit info tool tip script comps
            SetReminderTimerReferenceToChildrenTooltipsOnStart();

            if (waveToShowClickReminderAfterFinished == null)
            {
                if (showTooltipClickReminderOnStart) StartCoroutine(ShowClickOnReminderDelayCoroutine(1.5f));
            }
            else 
            {
                shouldStartClickReminderTimer = true;

                showTooltipClickReminderOnStart = false; 
            }
        }

        private void Update()
        {
            UpdateTimeUntilNextReminder();

            UpdateTimeToCloseReminder();
        }

        private bool HasValidSelectedAndChildrenTooltips()
        {
            if (tooltipsToDisplayClickReminder == null || tooltipsToDisplayClickReminder.Count == 0) return false;

            childTooltipsToCheckForReminders = GetComponentsInChildren<InfoTooltipEnabler>(true).ToList();

            if (childTooltipsToCheckForReminders.Count == 0) return false;

            bool hasTooltipsThatAreNotChildren = false;

            for (int i = 0; i < tooltipsToDisplayClickReminder.Count; i++)
            {
                if (childTooltipsToCheckForReminders.Contains(tooltipsToDisplayClickReminder[i])) continue;

                hasTooltipsThatAreNotChildren = true;

                tooltipsToDisplayClickReminder.RemoveAt(i);
            }

            if (hasTooltipsThatAreNotChildren)
            {
                Debug.LogWarning("One or more tooltips to display click reminder " +
                    "are not children of this unit tooltip click reminder display timer: " + name +
                    " they will not display click reminder. " +
                    "Make sure they are children of a reminder display timer component.");
            }

            if (tooltipsToDisplayClickReminder.Count == 0) return false;

            return true;
        }

        private void SetReminderTimerReferenceToChildrenTooltipsOnStart()
        {
            for(int i = 0; i < childTooltipsToCheckForReminders.Count; i++)
            {
                childTooltipsToCheckForReminders[i].SetUnitTooltipClickReminderDisplayTimer(this);
            }
        }

        private void UpdateTimeUntilNextReminder()
        {
            if(timerIsPaused) return;

            //if a tooltip has been opened recently OR smth is preventing clickon reminder timer to start
            //dont start timer
            if (hasATooltipBeenOpenedRecently || !shouldStartClickReminderTimer)
            {
                //if (currentTimeUntilNextReminder > 0.0f) currentTimeUntilNextReminder = 0.0f;
                return;
            }

            if(currentTimeUntilNextReminder < timeUntilNextReminder)
            {
                currentTimeUntilNextReminder += Time.deltaTime;

                return;
            }

            if(currentTimeUntilNextReminder >= timeUntilNextReminder)
            {
                DisplayTooltipClickReminderForSelectedTooltips();
            }
        }

        private void UpdateTimeToCloseReminder()
        {
            //if a tooltip is in use OR NO tooltip-clickon is being opened
            //which means that we cant close that which is not opened
            //should not update time to close clickon reminder cause there's no click-on reminder being opened...
            if (hasATooltipBeenOpenedRecently || shouldStartClickReminderTimer) return;

            if(currentTimeToCloseReminder < timeToCloseReminder)
            {
                currentTimeToCloseReminder += Time.deltaTime;

                return;
            }

            if(currentTimeToCloseReminder >= timeToCloseReminder)
            {
                CloseTooltipClickReminderForSelectedTooltips();
            }
        }

        //auto-closes all the selected click-on reminders after having displaying them for a certain amount of time (timeToCloseReminder var)
        public void CloseTooltipClickReminderForSelectedTooltips()
        {
            if (tooltipsToDisplayClickReminder == null || tooltipsToDisplayClickReminder.Count == 0) return;

            bool shouldResetClickReminderTimer = false;

            for (int i = 0; i < tooltipsToDisplayClickReminder.Count; i++)
            {
                if (tooltipsToDisplayClickReminder[i] == null) continue;

                if (!tooltipsToDisplayClickReminder[i].gameObject.activeInHierarchy) continue;

                //if there were click on reminder tooltips opened that were closed -> reset reminder timer
                //if loop finshed and should reset timer is still false (none reminders were opened and should not reset timer)
                shouldResetClickReminderTimer = true;

                tooltipsToDisplayClickReminder[i].EnableTooltipClickOnReminder(false);
            }

            if(shouldResetClickReminderTimer) ResetTimer();

            shouldStartClickReminderTimer = true;
        }

        //displays all selected click-on reminders after a while of not displaying one AND players have not clicked on any tooltip since
        //(timeUntilNextReminder var)
        public void DisplayTooltipClickReminderForSelectedTooltips()
        {
            if (tooltipsToDisplayClickReminder == null || tooltipsToDisplayClickReminder.Count == 0) return;
            
            for(int i = 0; i < tooltipsToDisplayClickReminder.Count; i++)
            {
                if (tooltipsToDisplayClickReminder[i] == null) continue;

                if (!tooltipsToDisplayClickReminder[i].gameObject.activeInHierarchy) continue;

                tooltipsToDisplayClickReminder[i].EnableTooltipClickOnReminder(true);
            }

            //because we just started displaying tooltip click-on reminder -> should not start timer yet
            //timer is now paused indefinitely as long as this reminder stays on until 
            //either click-on reminder is closed (timeToCloseReminder is reached)
            //or players clicked on a tooltip then closed it and with that, the click-on reminder is also closed and reset.
            shouldStartClickReminderTimer = false;

            ResetTimer();
        }

        //stops and resets click-on reminder timer everytime a tooltip that is a child of this timer is opened manually
        public void SetReminderInactiveAndStopTimerOnTooltipOpened(InfoTooltipEnabler tooltipEnabler)
        {
            if(tooltipEnabler == null) return;

            if(!childTooltipsToCheckForReminders.Contains(tooltipEnabler)) return;

            hasATooltipBeenOpenedRecently = true;

            shouldStartClickReminderTimer = false;

            tooltipEnabler.EnableTooltipClickOnReminder(false);

            CloseTooltipClickReminderForSelectedTooltips();

            ResetTimer();
        }

        //reverse of the above function: "SetReminderInactiveAndStopTimerOnTooltipOpened()"
        //when a tooltip that is a child of this timer is closed manually -> timer will start
        //if the players open any tooltip that is a child of this timer within the timer (above function), it's reset
        //else if the timer is finished -> redisplay click-on reminder
        public void StartClickOnReminderTimerOnTooltipClosed(InfoTooltipEnabler tooltipEnabler)
        {
            if (tooltipEnabler == null) return;

            if (!childTooltipsToCheckForReminders.Contains(tooltipEnabler)) return;

            hasATooltipBeenOpenedRecently = false;

            shouldStartClickReminderTimer = true;

            //do not reset timer here!!!
        }

        private void ResetTimer()
        {
            timeUntilNextReminder = Random.Range(timeUntilNextReminderMin, timeUntilNextReminderMax);

            currentTimeUntilNextReminder = 0.0f;

            currentTimeToCloseReminder = 0.0f;
        }

        //to add to the "unitTooltipsToDisplayClickReminder" list during runtime in addition to in the editor pre-runtime.
        //use this function in WaveVisitorLookAhead.cs to manage which recently updated LookAhead slot will get to display the reminder!
        public void SetTooltipThatWillDisplayClickOnReminder(InfoTooltipEnabler tooltipEnabler)
        {
            if (!tooltipsToDisplayClickReminder.Contains(tooltipEnabler))
            {
                tooltipsToDisplayClickReminder.Add(tooltipEnabler);
            }

            //also add to children list if not already done so
            RegisterTooltipEnablerInChildListOnly(tooltipEnabler);
        }

        //same as above function but for removing
        public void RemoveTooltipThatWillDisplayClickOnReminder(InfoTooltipEnabler tooltipEnabler)
        {
            if (tooltipsToDisplayClickReminder.Contains(tooltipEnabler))
            {
                tooltipsToDisplayClickReminder.Remove(tooltipEnabler);
            }

            //dont remove from children list because we still need this tooltip enabler in children for checking the timer
            //(its just not displaying the reminder thats all)
        }

        public void ClearTooltipsThatWillDisplayClickOnReminderList()
        {
            tooltipsToDisplayClickReminder.Clear();
        }

        public void PauseTimer(bool shouldPause)
        {
            timerIsPaused = shouldPause;
        }

        public void RegisterTooltipEnablerInChildListOnly(InfoTooltipEnabler tooltipEnabler)
        {
            if (!childTooltipsToCheckForReminders.Contains(tooltipEnabler))
            {
                childTooltipsToCheckForReminders.Add(tooltipEnabler);

                tooltipEnabler.SetUnitTooltipClickReminderDisplayTimer(this);
            }
        }

        private IEnumerator ShowClickOnReminderDelayCoroutine(float delaySec = 1.0f)
        {
            if (delaySec <= 0.0f) delaySec = 1.0f;

            yield return new WaitForSeconds(delaySec);

            DisplayTooltipClickReminderForSelectedTooltips();

            yield break;
        }

        private void GetCurrentWaveOnWaveEndedEvent(WaveSpawner ws, int wNum, bool hasOngoingWave)
        {
            if (ws == null) return;

            if (hasOngoingWave) return;

            currentWave = ws.GetCurrentWave().waveSO;
        }

        private void EnableClickOnReminderAfterRainIfApplicable(Rain r)
        {
            if (waveToShowClickReminderAfterFinished == null) return;

            if (currentWave == null) return;

            if (currentWave != waveToShowClickReminderAfterFinished) return;

            StartCoroutine(ShowClickOnReminderDelayCoroutine(0.3f));
        }
    }
}
