using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UIShakeFx : UITweenBase
    {
        [Header("UI Shake FX Settings")]

        [SerializeField] private Vector3 shakeStrength = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField] private int shakeVibrato = 5;

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;
            
            yield return rectTransform.DOShakeAnchorPos(tweenDuration, 
                                                        shakeStrength, 
                                                        shakeVibrato, 
                                                        50.0f, 
                                                        false, 
                                                        true, 
                                                        ShakeRandomnessMode.Harmonic).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                RunTweenInternal();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {

            }
        }
    }
}
