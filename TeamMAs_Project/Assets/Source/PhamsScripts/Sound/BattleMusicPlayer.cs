// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class BattleMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        [field: SerializeField] public AudioClip audioClipToPlay { get; private set; }

        [SerializeField] private float musicFadesInDuration = 1.0f;

        [SerializeField] private float musicFadesOutDuration = 1.4f;

        private float audioSourceBaseVolume = 0.0f;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();

                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (audioClipToPlay == null)
            {
                Debug.LogWarning("Battle Music Player: " + name + " doesnt have an audio clip to play assigned!");
            }
            else audioSource.clip = audioClipToPlay;

            audioSourceBaseVolume = audioSource.volume;
        }

        private void OnEnable()
        {
            WaveSpawner.OnWaveStarted += EnableBattleThemeOnWaveStarted;
            WaveSpawner.OnWaveFinished += DisableBattleThemeOnAllWavesFinished;
            WaveSpawner.OnAllWaveSpawned += DisableBattleThemeOnAllWavesFinished;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= EnableBattleThemeOnWaveStarted;
            WaveSpawner.OnWaveFinished -= DisableBattleThemeOnAllWavesFinished;
            WaveSpawner.OnAllWaveSpawned -= DisableBattleThemeOnAllWavesFinished;

            StopAllCoroutines();
        }

        private void EnableBattleThemeOnWaveStarted(WaveSpawner waveSpawner, int waveNum)
        {
            EnableBattleMusic(true, true);
        }

        private void DisableBattleThemeOnAllWavesFinished(WaveSpawner waveSpawner, int waveNum, bool hasOngoingWaves)
        {
            if (hasOngoingWaves) return;

            EnableBattleMusic(false);
        }

        private void DisableBattleThemeOnAllWavesFinished(WaveSpawner waveSpawner, bool hasOngoingWaves)
        {
            DisableBattleThemeOnAllWavesFinished(waveSpawner, 0, hasOngoingWaves);
        }

        public void EnableBattleMusic(bool enabled, bool loop = true)
        {
            if (audioClipToPlay == null) return;

            audioSource.loop = loop;

            if (enabled)
            {
                if (!audioSource.isPlaying) audioSource.Play();

                StartCoroutine(MusicFadesLerpFromTo(0.0f, audioSourceBaseVolume, musicFadesInDuration));

                return;
            }

            if (!audioSource.isPlaying) return;

            StartCoroutine(MusicFadesLerpFromTo(audioSourceBaseVolume, 0.0f, musicFadesOutDuration, true));
        }

        private IEnumerator MusicFadesLerpFromTo(float volumeFrom, float volumeTo, float fadeDuration, bool stopMusicOnFinished = false)
        {
            audioSource.volume = volumeFrom;

            float fadeTime = 0.0f;

            float lerpedVolume = 0.0f;

            while (fadeTime < fadeDuration)
            {
                lerpedVolume = Mathf.Lerp(volumeFrom, volumeTo, fadeTime);

                audioSource.volume = lerpedVolume;

                yield return new WaitForFixedUpdate();

                fadeTime += Time.fixedDeltaTime;
            }

            audioSource.volume = volumeTo;

            if(stopMusicOnFinished && audioSource.isPlaying) audioSource.Stop();

            yield break;
        }
    }
}
