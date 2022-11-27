using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    public class StartWaveButtonUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The WaveSpawner script component instance that will start a wave when this button is pressed. " +
        "In case we have multiple WaveSpawners linked to multiple wave start buttons. This must be assigned for button to work!")]
        private WaveSpawner waveSpawnerLinkedToButton;

        [SerializeField] private TextMeshProUGUI startWaveButtonText;

        [SerializeField]
        [Tooltip("Should the button displays the first wave as Wave 0 or Wave 1?")]
        private bool startedAtWave_1 = true;

        private CanvasGroup startWaveCanvasGroup;

        private void Awake()
        {
            if(waveSpawnerLinkedToButton == null)
            {
                Debug.LogError("Start Wave Button: " + name + " has no WaveSpawner linked to it assigned! Disabling button!");
                gameObject.SetActive(false);
                return;
            }

            startWaveCanvasGroup = GetComponent<CanvasGroup>();

            if(startWaveCanvasGroup == null)
            {
                startWaveCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetStartWaveButtonWaveText();
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveStarted += OnWaveStarted;
            WaveSpawner.OnWaveFinished += OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned += OnAllWaveSpawned;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= OnWaveStarted;
            WaveSpawner.OnWaveFinished -= OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned -= OnAllWaveSpawned;
        }

        private void SetStartWaveButtonWaveText()
        {
            if(startWaveButtonText == null)
            {
                Debug.LogWarning("Start Wave Button UI Text component is not assigned! Button text displays won't work!");
                return;
            }

            if (waveSpawnerLinkedToButton == null) return;

            if (!startedAtWave_1)
            {
                startWaveButtonText.text = "Start Wave " + waveSpawnerLinkedToButton.currentWave;
                return;
            }

            startWaveButtonText.text = "Start Wave " + (waveSpawnerLinkedToButton.currentWave + 1).ToString();
        }

        private void EnableButton(bool enabled)
        {
            if (enabled)
            {
                startWaveCanvasGroup.alpha = 1.0f;
                startWaveCanvasGroup.interactable = true;
                startWaveCanvasGroup.blocksRaycasts = true;
                return;
            }

            startWaveCanvasGroup.alpha = 0.0f;
            startWaveCanvasGroup.interactable = false;
            startWaveCanvasGroup.blocksRaycasts = false;
        }

        //This function is a UI Button's UnityEvent callback, called when user pressed the startWaveButton.
        public void StartWave()
        {
            if (waveSpawnerLinkedToButton == null) return;

            waveSpawnerLinkedToButton.StartCurrentWave();
        }


        //The below 2 functions subscribed to WaveSpawner.cs' wave started/stopped events. Check WaveSpawner.cs for more info!
        //disable button on wave started
        public void OnWaveStarted(WaveSpawner waveSpawnerThatStartedWave, int waveNum)
        {
            //if not the same wave spawner as this button's linked wave spawner -> do nothing
            if (waveSpawnerThatStartedWave != waveSpawnerLinkedToButton) return;

            EnableButton(false);
        }

        //re-enable button on wave finished
        public void OnWaveFinished(WaveSpawner waveSpawnerThatStartedWave, int waveNum)
        {
            //if not the same wave spawner as this button's linked wave spawner -> do nothing
            if (waveSpawnerThatStartedWave != waveSpawnerLinkedToButton) return;

            //update wave number UI text
            SetStartWaveButtonWaveText();

            //re-enable button
            EnableButton(true);
        }

        public void OnAllWaveSpawned(WaveSpawner waveSpawnerThatStartedWave)
        {
            //if not the same wave spawner as this button's linked wave spawner -> do nothing
            if (waveSpawnerThatStartedWave != waveSpawnerLinkedToButton) return;

            EnableButton(false);
        }
    }
}
