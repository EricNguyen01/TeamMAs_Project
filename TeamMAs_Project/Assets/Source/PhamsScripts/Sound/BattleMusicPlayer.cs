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
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= EnableBattleThemeOnWaveStarted;
            WaveSpawner.OnWaveFinished -= DisableBattleThemeOnAllWavesFinished;

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

        public void EnableBattleMusic(bool enabled, bool loop = true)
        {
            if (audioClipToPlay == null) return;

            audioSource.loop = loop;

            if (enabled)
            {
                //audioSource.volume = audioSourceBaseVolume;

                StartCoroutine(FadeInMusic(0.0f, audioSourceBaseVolume, musicFadeDuration / 3)); // feel free to refactor -sarita

                if (!audioSource.isPlaying) audioSource.Play();

                return;
            }

            if (!audioSource.isPlaying) return;

            StartCoroutine(FadeOutMusic(audioSourceBaseVolume, 0.0f, musicFadeDuration));
        }

        private IEnumerator FadeOutMusic(float volumeBeginsFade, float volumeEndsFade, float fadeDuration)
        {
            if (fadeDuration <= 0.0f)
            {
                if (audioSource.isPlaying) audioSource.Stop();

                yield break;
            }

            float fadeTime = 0.0f;

            float lerpedVolume = 0.0f;

            while (fadeTime < fadeDuration)
            {
                lerpedVolume = Mathf.Lerp(volumeBeginsFade, volumeEndsFade, fadeTime);

                audioSource.volume = lerpedVolume;

                yield return new WaitForFixedUpdate();

                fadeTime += Time.fixedDeltaTime;
            }

            audioSource.Stop();

            yield break;
        }

        private IEnumerator FadeInMusic(float volumeBeginsFade, float volumeEndsFade, float fadeDuration) // i'm lazy. feel free to refactor -sarita
        {
            float fadeTime = 0.0f;

            float lerpedVolume = 0.0f;

            while (lerpedVolume < volumeEndsFade)
            {
                lerpedVolume = Mathf.Lerp(volumeBeginsFade, volumeEndsFade, fadeTime / fadeDuration);

                audioSource.volume = lerpedVolume;

                yield return new WaitForFixedUpdate();

                fadeTime += Time.fixedDeltaTime;
            }

            yield break;
        }
    }
}
