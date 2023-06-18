using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIExpand : UITweenBase
    {
        [Header("UI Hover Expand Settings")]

        [SerializeField] protected float expandValue = 1.0f;

        //INTERNALS............................................................................

        protected Vector2 expandedSize;


        protected override void OnEnable()
        {
            base.OnEnable();

            expandedSize = new Vector2(baseSizeDelta.x + expandValue, baseSizeDelta.y + expandValue);

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoExpandCycleLoopCoroutine());//auto loop tween cycle
            }
        }

        public override void RunTweenInternal()
        {
            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(AutoExpandCycleCoroutine());//do tween cycle once without loop
        }

        //hover on
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            //on pointer enter -> expand size

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                rectTransform.DOSizeDelta(expandedSize, tweenDuration);
            }
        }

        //hover off
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            //on pointer exit -> collapse to original size

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration);
            }
        }

        protected IEnumerator AutoExpandCycleCoroutine()
        {
            alreadyPerformedTween = true;

            //wait for expand operation to finish async
            yield return rectTransform.DOSizeDelta(expandedSize, tweenDuration).WaitForCompletion();

            //collapse to original size and wait for collapse async operation to finish
            yield return rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        protected IEnumerator AutoExpandCycleLoopCoroutine()
        {
            //only do auto expand while in auto ui tween mode

            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                yield return AutoExpandCycleCoroutine();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            yield break;
        }
    }
}
