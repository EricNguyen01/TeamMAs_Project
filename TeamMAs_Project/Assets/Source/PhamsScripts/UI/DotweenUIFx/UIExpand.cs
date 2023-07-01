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
            expandedSize = new Vector2(baseSizeDelta.x + expandValue, baseSizeDelta.y + expandValue);

            base.OnEnable();
        }

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;

            //wait for expand operation to finish async
            yield return rectTransform.DOSizeDelta(expandedSize, tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            //collapse to original size and wait for collapse async operation to finish
            yield return rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        //hover on
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            //on pointer enter -> expand size

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DOSizeDelta(expandedSize, tweenDuration).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
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
                Tween tween = rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }
    }
}
