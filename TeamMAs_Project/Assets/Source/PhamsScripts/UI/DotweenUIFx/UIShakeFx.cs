// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    //[DisallowMultipleComponent]
    public class UIShakeFx : UITweenBase
    {
        [Header("UI Shake FX Settings")]

        [SerializeField] private Vector3 shakeStrength = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField] private int shakeVibrato = 5;

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;

            Tween tween = rectTransform.DOShakeAnchorPos(tweenDuration,
                                                        shakeStrength,
                                                        shakeVibrato,
                                                        50.0f,
                                                        false,
                                                        true,
                                                        ShakeRandomnessMode.Harmonic);

            yield return tween.SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                RunTweenInternal();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {

            }
        }
    }
}
