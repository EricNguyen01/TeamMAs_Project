using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeamMAsTD
{
    /* A Class for creating a customizable list of tweening effects (by adding tween components) and then run them either individually in order or simultaneously.
     * Have nothing to do with DoTween Sequence functionality.
     */
    public class UITweenSequenceSetup : MonoBehaviour
    {
        private enum UITweenSequenceRunMode { InOrder, Simultaneously }

        private struct TweenStruct
        {
            public UITweenBase tween;

            public float startDelaySec;
        }

        [Header("Tween Sequence Setup")]

        [SerializeField]
        private UITweenSequenceRunMode UI_TweenSequenceRunMode = UITweenSequenceRunMode.InOrder;

        [SerializeField]
        private TweenStruct[] tweensInSequence;

        [Header("Tween Sequence Unity Event")]

        [SerializeField]
        private UnityEvent OnTweenSequenceCompleted;

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
                totalDuration += tweensInSequence[i].tween.GetTweenDuration();
            }

            return totalDuration;
        }

        private IEnumerator TweenSequenceCoroutine()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) yield break;

            for(int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                if (tweensInSequence[i].startDelaySec > 0.0f)
                {
                    yield return new WaitForSeconds(tweensInSequence[i].startDelaySec);
                }

                tweensInSequence[i].tween.RunTween();

                if (UI_TweenSequenceRunMode == UITweenSequenceRunMode.InOrder)
                {
                    yield return new WaitForSeconds(tweensInSequence[i].tween.GetTweenDuration());
                }
            }

            OnTweenSequenceCompleted?.Invoke();

            yield break;
        }

        public void RunTweenSequence()
        {
            StartCoroutine(TweenSequenceCoroutine());
        }
    }
}