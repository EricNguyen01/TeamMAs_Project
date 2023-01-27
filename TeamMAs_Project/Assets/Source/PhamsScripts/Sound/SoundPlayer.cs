using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if(audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();

                if(audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        public void PlayAudioOneShot(AudioClip audioClip)
        {
            if (audioSource == null)
            {
                Debug.LogError("AudioSource on gameobject: " + name + " not found!");
                return;
            }

            if (audioClip == null)
            {
                Debug.LogWarning("AudioClip to be set to AudioSource on gameobject: " + name + " is null! No audioclip wll be played!");
                return;
            }

            if (audioSource.isPlaying) audioSource.Stop();

            audioSource.PlayOneShot(audioClip);
        }

        public float GetCurrentAudioClipLengthIfNotNull()
        {
            if (audioSource == null) return 0.0f;

            if (audioSource.clip == null) return 0.0f;

            return audioSource.clip.length;
        }
    }
}
