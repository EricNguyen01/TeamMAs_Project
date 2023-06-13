using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /* A Class for creating a customizable list of tweening effects (by adding tween components) and then run them either individually in order or simultaneously.
     * Have nothing to do with DoTween Sequence functionality.
     */
    public class UITweenSequenceSetup : MonoBehaviour
    {
        private enum UITweenSequenceRunMode { InOrder, Simultaneously }

        [Header("Tween Sequence Setup")]

        [SerializeField]
        private UITweenSequenceRunMode UI_TweenSequenceRunMode = UITweenSequenceRunMode.InOrder;

        [SerializeField]
        private UITweenBase[] tweensInSequence;

        //INTERNALS..............................................................................

        public float tweenSequenceDuration { get; private set; } = 0.0f;

        private void Awake()
        {
            if(tweensInSequence == null || tweensInSequence.Length == 0)
            {
                enabled = false;

                return;
            }

            tweenSequenceDuration = GetTweenSequenceTotalDuration();
        }

        private float GetTweenSequenceTotalDuration()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) return 0.0f;

            float totalDuration = 0.0f;

            for(int i = 0; i < tweensInSequence.Length; i++)
            {
                totalDuration += tweensInSequence[i].GetTweenDuration();
            }

            return totalDuration;
        }
    }
}