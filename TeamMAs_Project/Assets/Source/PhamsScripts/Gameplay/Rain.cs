using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class Rain : MonoBehaviour
    {
        [field: SerializeField] public int plantWaterBarsRefilledAfterRain { get; private set; } = 1;

        [SerializeField] private float rainDuration = 1.5f;

        //if there's rain animation -> add here...

        //sub by PlantWaterUsageSystem.cs for water refilling after rain
        //sub by TileMenu for temporary disabling menu UI interaction during rain
        public static event System.Action<Rain> OnRainStarted;
        public static event System.Action<Rain> OnRainEnded;

        private void OnEnable()
        {
            WaveSpawner.OnWaveFinished += OnWaveFinished;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveFinished -= OnWaveFinished;

            StopCoroutine(RainSequenceCoroutine());
        }

        private void OnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOngoingWaves)
        {
            if (stillHasOngoingWaves) return;
            
            StartCoroutine(RainSequenceCoroutine());
        }

        private IEnumerator RainSequenceCoroutine()
        {
            OnRainStarted?.Invoke(this);

            //if there's rain anim -> play it here...

            yield return new WaitForSeconds(rainDuration);

            OnRainEnded?.Invoke(this);
        }
    }
}
