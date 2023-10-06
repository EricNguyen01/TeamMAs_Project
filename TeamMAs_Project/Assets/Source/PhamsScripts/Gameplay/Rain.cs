// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class Rain : MonoBehaviour
    {
        [field: SerializeField] public int plantWaterBarsRefilledAfterRain { get; private set; } = 1;

        [SerializeField] private float rainDuration = 1.5f;

        [SerializeField] private ParticleSystem rainParticleSystem;

        //Unity Event
        [SerializeField] private UnityEvent OnRainStartedEvent;
        [SerializeField] private UnityEvent<int> OnRainEndedEvent;

        //the current wave that just ended right before rain started happening
        private int currentWaveBeforeRain = 0;

        private bool hasDisabledRain = false;

        //if there's rain animation -> add here...

        //sub by PlantWaterUsageSystem.cs for water refilling after rain
        //sub by TileMenu for temporary disabling menu UI interaction during rain
        public static event System.Action<Rain> OnRainStarted;
        public static event System.Action<Rain> OnRainEnded;

        private void Awake()
        {
            if(rainParticleSystem != null)
            {
                var rainFxMain = rainParticleSystem.main;

                rainFxMain.playOnAwake = false;

                if(rainParticleSystem.isPlaying) rainParticleSystem.Stop();

                if(!rainParticleSystem.gameObject.activeInHierarchy) rainParticleSystem.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            SaveLoadHandler.OnLoadingStarted += () => hasDisabledRain = true;

            SaveLoadHandler.OnLoadingFinished += () => hasDisabledRain = false;

            WaveSpawner.OnWaveFinished += RainOnWaveFinished;
        }

        private void OnDisable()
        {
            SaveLoadHandler.OnLoadingStarted -= () => hasDisabledRain = true;

            SaveLoadHandler.OnLoadingFinished -= () => hasDisabledRain = false;

            WaveSpawner.OnWaveFinished -= RainOnWaveFinished;

            StopCoroutine(RainSequenceCoroutine());
        }

        private void RainOnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (hasDisabledRain) return;

            if (stillHasOngoingWaves) return;

            currentWaveBeforeRain = waveNum;

            StartCoroutine(RainSequenceCoroutine());
        }

        private IEnumerator RainSequenceCoroutine()
        {
            OnRainStarted?.Invoke(this);

            yield return new WaitForSeconds(0.25f);

            OnRainStartedEvent?.Invoke();

            //if there's rain anim/particle fx -> play it here...
            if (rainParticleSystem != null)
            {
                if(!rainParticleSystem.isPlaying) rainParticleSystem.Play();
            }

            yield return new WaitForSeconds(rainDuration);

            if (rainParticleSystem != null)
            {
                if (rainParticleSystem.isPlaying) rainParticleSystem.Stop();
            }

            yield return new WaitForSeconds(0.27f);

            OnRainEnded?.Invoke(this);

            OnRainEndedEvent?.Invoke(currentWaveBeforeRain);

            yield break;
        }

        public void TestRainEvent(int waveNum)
        {
            //Debug.Log("Wave just finished: " + waveNum);
            DialogueManager.StartConversation("Wave/" + (waveNum + 2));
        }
    }
}
