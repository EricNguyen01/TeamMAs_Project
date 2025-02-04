// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class TimeSpeedUpButton : MonoBehaviour
    {
        [Header("Time Skip Config")]

        [SerializeField] private List<float> timeSpeedUpStages = new List<float>();

        [Header("Required Components")]

        [SerializeField] private CanvasGroup timeSpeedUpCanvasGroup;

        [SerializeField] private TextMeshProUGUI timeSpeedUpTextMeshComp;

        private int currentTimeSpeedUpStage = 0;

        private bool isTemporaryDisabled = false;

        private bool hasWaveAlreadyStarted = false;

        private bool hasDialogueStarted = false;

        private void Awake()
        {
            if(timeSpeedUpCanvasGroup == null)
            {
                timeSpeedUpCanvasGroup = GetComponent<CanvasGroup>();
            }

            if(timeSpeedUpCanvasGroup == null) timeSpeedUpCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            DisplayTimeSpeedUpTextUI();

            TemporaryDisableTimeSpeedUp(true, false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //on changes made to time speed up stages
            //the chunk below makes sure the timeSkipStages list always have
            //the base and default time (1.0f means normal time speed, no slo-mo, no fastforward) as its first element
            //it also checks for valid time stages

            if (timeSpeedUpStages.Count == 0)
            {
                timeSpeedUpStages.Add(1.0f);
            }
            else if(timeSpeedUpStages.Count > 0)
            {
                //remove any time stage that is smaller than 0.0f (invalid time stage)
                timeSpeedUpStages.RemoveAll(x => x < 0.0f);

                //if time speed up stages are missing 1.0f (the default time) -> add to list
                if (!timeSpeedUpStages.Contains(1.0f))
                {
                    timeSpeedUpStages.Insert(0, 1.0f);
                }
            }
            
        }
#endif

        private void Start()
        {
            //always
            //sort time stages ascending in-place using List.Sort() with a provided Comparison<T> delegate
            //on start
            if (timeSpeedUpStages.Count > 1)
            {
                timeSpeedUpStages.Sort((x, y) => x.CompareTo(y));
            }

            //reset time speed to default must be called after the sort function above to avoid bugs!!!
            ResetTimeSpeedToDefault();
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += (WaveSpawner wp, int waveNum, bool ongoingWave) => TemporaryDisableTimeSpeedUp(true);

            WaveSpawner.OnWaveFinished += (WaveSpawner wp, int waveNum, bool ongoingWave) => hasWaveAlreadyStarted = false;

            WaveSpawner.OnWaveStarted += (WaveSpawner wp, int waveNum) => hasWaveAlreadyStarted = true;

            WaveSpawner.OnWaveStarted += (WaveSpawner wp, int waveNum) => TemporaryDisableTimeSpeedUp(false);

            Rain.OnRainStarted += (Rain r) => TemporaryDisableTimeSpeedUp(true);
        }

        private void OnDisable()
        {
            ResetTimeSpeedToDefault();

            WaveSpawner.OnWaveFinished -= (WaveSpawner wp, int waveNum, bool ongoingWave) => TemporaryDisableTimeSpeedUp(true);

            WaveSpawner.OnWaveFinished -= (WaveSpawner wp, int waveNum, bool ongoingWave) => hasWaveAlreadyStarted = false;

            WaveSpawner.OnWaveStarted -= (WaveSpawner wp, int waveNum) => hasWaveAlreadyStarted = true;

            WaveSpawner.OnWaveStarted -= (WaveSpawner wp, int waveNum) => TemporaryDisableTimeSpeedUp(false);

            Rain.OnRainStarted -= (Rain r) => TemporaryDisableTimeSpeedUp(true);
        }

        public void ToNextTimeSpeedUpStage()
        {
            if (isTemporaryDisabled) return;

            if (timeSpeedUpStages == null || timeSpeedUpStages.Count == 0) return;

            if (currentTimeSpeedUpStage == timeSpeedUpStages.Count - 1) 
            { 
                currentTimeSpeedUpStage = 0; 
            }
            else currentTimeSpeedUpStage++;

            DisplayTimeSpeedUpTextUI();

            Time.timeScale = timeSpeedUpStages[currentTimeSpeedUpStage];
        }

        public void ToPreviousTimeSpeedUpStage()
        {
            if (isTemporaryDisabled) return;

            if (timeSpeedUpStages == null || timeSpeedUpStages.Count == 0) return;

            if (currentTimeSpeedUpStage == 0)
            {
                currentTimeSpeedUpStage = timeSpeedUpStages.Count - 1;
            }
            else currentTimeSpeedUpStage--;

            DisplayTimeSpeedUpTextUI();

            Time.timeScale = timeSpeedUpStages[currentTimeSpeedUpStage];
        }

        public void ResetTimeSpeedToDefault()
        {
            if (timeSpeedUpStages == null || timeSpeedUpStages.Count == 0) return;

            int defaultTimeStage = 0;

            if(timeSpeedUpStages.Count > 1)
            {
                while (defaultTimeStage < timeSpeedUpStages.Count)
                {
                    if (timeSpeedUpStages[defaultTimeStage] == 1.0f) break;

                    defaultTimeStage++;
                }
            }

            currentTimeSpeedUpStage = defaultTimeStage;

            DisplayTimeSpeedUpTextUI();

            Time.timeScale = timeSpeedUpStages[currentTimeSpeedUpStage];
        }

        public void TemporaryDisableTimeSpeedUpOnDialogueStarts(bool shouldDisable)
        {
            if (shouldDisable)
            {
                hasDialogueStarted = true;

                Time.timeScale = 0.0f;

                return;
            }

            hasDialogueStarted = false;

            TemporaryDisableTimeSpeedUp(false, false);

            Time.timeScale = timeSpeedUpStages[currentTimeSpeedUpStage];
        }

        public void TemporaryDisableTimeSpeedUp(bool shouldDisable, bool shouldResetTimeSpeed = true)
        {
            if (timeSpeedUpCanvasGroup && shouldDisable)
            {
                if (isTemporaryDisabled) return;

                isTemporaryDisabled = true;

                if(shouldResetTimeSpeed) ResetTimeSpeedToDefault();

                timeSpeedUpCanvasGroup.alpha = 0.5f;

                timeSpeedUpCanvasGroup.interactable = false;

                return;
            }

            if (!timeSpeedUpCanvasGroup || !isTemporaryDisabled || !hasWaveAlreadyStarted || hasDialogueStarted) return;

            isTemporaryDisabled = false;

            if (shouldResetTimeSpeed) ResetTimeSpeedToDefault();

            timeSpeedUpCanvasGroup.alpha = 1.0f;

            timeSpeedUpCanvasGroup.blocksRaycasts = true;

            timeSpeedUpCanvasGroup.interactable = true;
        }

        private void DisplayTimeSpeedUpTextUI()
        {
            if (timeSpeedUpTextMeshComp == null) return;

            if (timeSpeedUpStages == null || timeSpeedUpStages.Count == 0)
            {
                timeSpeedUpTextMeshComp.text = "n/a";

                return;
            }

            float timeSpeedUpValue = (float)System.Math.Round(timeSpeedUpStages[currentTimeSpeedUpStage], 1);

            timeSpeedUpTextMeshComp.text = timeSpeedUpValue.ToString() + "x";
        }
    }
}
