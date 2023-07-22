using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeamMAsTD
{
    /* A Class for creating a customizable list of tweening effects (by adding tween components) and then run them either individually in order or simultaneously.
     * Have nothing to do with DoTween Sequence functionality.
     */
    [DisallowMultipleComponent]
    public class UITweenSequenceSetup : MonoBehaviour
    {
        private enum UITweenSequenceRunMode { Sequential, Simultaneously }

        [Serializable]
        private struct TweenStruct
        {
            public UITweenBase tween;

            public float startDelaySec;
        }

        [Header("Tween Sequence Setup")]

        [SerializeField]
        private UITweenSequenceRunMode UI_TweenSequenceRunMode = UITweenSequenceRunMode.Sequential;

        [SerializeField]
        private TweenStruct[] tweensInSequence;

        [Header("Tween Sequence Unity Event")]

        [SerializeField]
        private UnityEvent OnTweenSequenceStarted;

        [SerializeField]
        private UnityEvent OnTweenSequenceCompleted;

        //INTERNALS.......................................................

        private List<UITweenBase> runningTweenList = new List<UITweenBase>();

        private int toggleState = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (tweensInSequence == null || tweensInSequence.Length <= 1) return;

            Remove_OverlappingTweens_OnSameGameObject_InSimultanousMode();
        }
#endif

        private void Awake()
        {
            if(tweensInSequence == null || tweensInSequence.Length == 0)
            {
                enabled = false;

                return;
            }

            SetTweensInSequenceToExecuteInternalOnly();

            Remove_OverlappingTweens_OnSameGameObject_InSimultanousMode();
        }

        private IEnumerator SequentialTweenSequenceCoroutine()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) yield break;

            if (UI_TweenSequenceRunMode != UITweenSequenceRunMode.Sequential) yield break;
            
            for (int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                if (runningTweenList.Contains(tweensInSequence[i].tween)) continue;

                StartCoroutine(ProcessTweenSlotCoroutine(tweensInSequence[i]));
            }

            yield return new WaitUntil(() => runningTweenList.Count == 0);
            
            OnTweenSequenceCompleted?.Invoke();

            yield break;
        }

        private IEnumerator ProcessTweenSlotCoroutine(TweenStruct tweenSlot)
        {
            if (tweenSlot.Equals(null) || !tweenSlot.tween || runningTweenList.Contains(tweenSlot.tween)) yield break;

            runningTweenList.Add(tweenSlot.tween);

            //Debug.Log("StartProcessTweenSlot");

            if (tweenSlot.startDelaySec > 0.0f)
            {
                //Debug.Log("TweenSlotHasDelay!");
                yield return new WaitForSecondsRealtime(tweenSlot.startDelaySec);
            }

            //Debug.Log("SlotDelayCompleted! StartingTween!");

            tweenSlot.tween.RunTweenInternal();

            yield return new WaitUntil(() => !tweenSlot.tween.IsTweenRunning());

            //Debug.Log("SlotTweenFinishedRunning!");

            runningTweenList.Remove(tweenSlot.tween);

            yield break;
        }

        private IEnumerator SimultanousTweenSequenceCoroutine()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) yield break;

            if (UI_TweenSequenceRunMode != UITweenSequenceRunMode.Simultaneously) yield break;

            for (int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                tweensInSequence[i].tween.RunTweenInternal();
            }

            OnTweenSequenceCompleted?.Invoke();

            yield break;
        }

        private void Remove_OverlappingTweens_OnSameGameObject_InSimultanousMode()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) return;

            if (UI_TweenSequenceRunMode != UITweenSequenceRunMode.Simultaneously) return;

            //set null to overlaps

            int overlappingCounts = 0;

            for(int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                for(int j = 0; j < tweensInSequence.Length; j++)
                {
                    if (tweensInSequence[j].Equals(null) || !tweensInSequence[j].tween) continue;

                    if (tweensInSequence[j].tween == tweensInSequence[i].tween) continue;

                    if (tweensInSequence[j].tween.gameObject == tweensInSequence[i].tween.gameObject)
                    {
                        tweensInSequence[j].tween = null;

                        overlappingCounts++;
                    }
                }
            }

            //remove null elements from array by swapping with temp arr

            TweenStruct[] temp = new TweenStruct[tweensInSequence.Length - overlappingCounts];

            int tempCurrentElement = 0;

            for(int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                temp[tempCurrentElement] = tweensInSequence[i];

                tempCurrentElement++;
            }

            tweensInSequence = temp;
        }

        private void SetTweensInSequenceToExecuteInternalOnly()
        {
            if (tweensInSequence == null || tweensInSequence.Length == 0) return;

            for (int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                tweensInSequence[i].tween.SetTweenExecuteMode(UITweenBase.UITweenExecuteMode.Internal);
            }
        }

        public void RunTweenSequence()
        {
            if(toggleState == 1)
            {
                StopAndResetTweenSequence();

                if(toggleState != 0) toggleState = 0;

                return;
            }
            
            if (UI_TweenSequenceRunMode == UITweenSequenceRunMode.Sequential)
            {
                StartCoroutine(SequentialTweenSequenceCoroutine());
            }
            else if(UI_TweenSequenceRunMode == UITweenSequenceRunMode.Simultaneously)
            {
                StartCoroutine(SimultanousTweenSequenceCoroutine());
            }

            OnTweenSequenceStarted?.Invoke();

            toggleState = 1;
        }

        public void StopAndResetTweenSequence()
        {
            StopAllCoroutines();

            runningTweenList.Clear();

            if (tweensInSequence == null || tweensInSequence.Length == 0) return;

            for (int i = 0; i < tweensInSequence.Length; i++)
            {
                if (tweensInSequence[i].Equals(null) || !tweensInSequence[i].tween) continue;

                tweensInSequence[i].tween.ResetUITween();
            }

            OnTweenSequenceCompleted?.Invoke();

            toggleState = 0;
        }
    }
}