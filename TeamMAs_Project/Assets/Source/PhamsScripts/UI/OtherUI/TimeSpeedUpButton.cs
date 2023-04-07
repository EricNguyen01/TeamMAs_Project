using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
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
            //make sure the timeSkipStages list always have
            //the base and default time (1.0f means normal time speed, no slo-mo, no fastforward)
            //as its first element
            if(timeSpeedUpStages == null) timeSpeedUpStages = new List<float>();

            if (timeSpeedUpStages.Count == 0)
            {
                timeSpeedUpStages.Add(1.0f);

                ResetTimeSpeedToDefault();
            }
            else if(timeSpeedUpStages.Count > 0)
            {
                if(timeSpeedUpStages.Contains(1.0f))
                {
                    timeSpeedUpStages.Remove(1.0f);
                }

                timeSpeedUpStages.Insert(0, 1.0f);
            }
        }
#endif

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += (WaveSpawner wp, int waveNum, bool ongoingWave) => TemporaryDisableTimeSpeedUp(true);

            WaveSpawner.OnWaveFinished += (WaveSpawner wp, int waveNum, bool ongoingWave) => hasWaveAlreadyStarted = false;

            WaveSpawner.OnWaveStarted += (WaveSpawner wp, int waveNum) => hasWaveAlreadyStarted = true;

            WaveSpawner.OnWaveStarted += (WaveSpawner wp, int waveNum) => TemporaryDisableTimeSpeedUp(false);

            Rain.OnRainStarted += (Rain r) => TemporaryDisableTimeSpeedUp(true);

            //Rain.OnRainEnded += (Rain r) => TemporaryDisableTimeSpeedUp(false);
        }

        private void OnDisable()
        {
            ResetTimeSpeedToDefault();

            WaveSpawner.OnWaveFinished -= (WaveSpawner wp, int waveNum, bool ongoingWave) => TemporaryDisableTimeSpeedUp(true);

            WaveSpawner.OnWaveFinished -= (WaveSpawner wp, int waveNum, bool ongoingWave) => hasWaveAlreadyStarted = false;

            WaveSpawner.OnWaveStarted -= (WaveSpawner wp, int waveNum) => hasWaveAlreadyStarted = true;

            WaveSpawner.OnWaveStarted -= (WaveSpawner wp, int waveNum) => TemporaryDisableTimeSpeedUp(false);

            Rain.OnRainStarted -= (Rain r) => TemporaryDisableTimeSpeedUp(true);

            //Rain.OnRainEnded -= (Rain r) => TemporaryDisableTimeSpeedUp(false);
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

            if (currentTimeSpeedUpStage == 0 && Time.timeScale == timeSpeedUpStages[0]) return;

            currentTimeSpeedUpStage = 0;

            DisplayTimeSpeedUpTextUI();

            Time.timeScale = timeSpeedUpStages[currentTimeSpeedUpStage];
        }

        public void TemporaryDisableTimeSpeedUpUnityEvent(bool shouldDisable)
        {
            TemporaryDisableTimeSpeedUp(shouldDisable);
        }

        public void TemporaryDisableTimeSpeedUp(bool shouldDisable, bool shouldResetTimeSpeed = true)
        {
            if (shouldDisable)
            {
                if (isTemporaryDisabled) return;

                isTemporaryDisabled = true;

                if(shouldResetTimeSpeed) ResetTimeSpeedToDefault();

                timeSpeedUpCanvasGroup.alpha = 0.5f;

                timeSpeedUpCanvasGroup.blocksRaycasts = false;

                timeSpeedUpCanvasGroup.interactable = false;

                return;
            }

            if (!isTemporaryDisabled || !hasWaveAlreadyStarted) return;

            isTemporaryDisabled = false;

            if (shouldResetTimeSpeed) ResetTimeSpeedToDefault();

            timeSpeedUpCanvasGroup.alpha = 1.0f;

            timeSpeedUpCanvasGroup.blocksRaycasts = true;

            timeSpeedUpCanvasGroup.interactable = true;
        }

        private void DisplayTimeSpeedUpTextUI()
        {
            if (timeSpeedUpTextMeshComp == null) return;

            if (timeSpeedUpStages == null || timeSpeedUpStages.Count == 0) return;

            float timeSpeedUpValue = (float)System.Math.Round(timeSpeedUpStages[currentTimeSpeedUpStage], 1);

            timeSpeedUpTextMeshComp.text = timeSpeedUpValue.ToString();
        }
    }
}
