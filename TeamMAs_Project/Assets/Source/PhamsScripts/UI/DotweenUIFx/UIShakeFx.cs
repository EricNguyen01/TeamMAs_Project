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

        protected override void OnEnable()
        {
            base.OnEnable();

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoShakeCycleLoopCoroutine());//auto loop tween cycle
            }
        }

        public override void RunTweenInternal()
        {
            ProcessUIShake();
        }

        protected virtual void ProcessUIShake()
        {
            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(ShakeTweenCycleCoroutine());
        }

        protected IEnumerator ShakeTweenCycleCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOShakeAnchorPos(tweenDuration, 
                                                        shakeStrength, 
                                                        shakeVibrato, 
                                                        50.0f, 
                                                        false, 
                                                        true, 
                                                        ShakeRandomnessMode.Harmonic).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        protected IEnumerator AutoShakeCycleLoopCoroutine()
        {
            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                yield return ShakeTweenCycleCoroutine();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            yield break;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                ProcessUIShake();
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
