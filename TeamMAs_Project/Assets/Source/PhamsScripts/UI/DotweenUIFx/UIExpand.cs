using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIExpand : UIHover, ILayoutSelfController
    {
        [Header("UI Hover Expand Settings")]

        [SerializeField] protected float expandValue = 1.0f;

        [SerializeField]
        [Tooltip("The delay in seconds before new expand cycle begins. " +
        "If set to 0, repeat expand immediately after current expand duration ends." +
        "Only works with Auto UITweenExecuteMode!")]
        protected float expandStartDelay = 0.0f;

        //INTERNALS............................................................................

        protected Vector2 expandedSize;


        protected override void OnEnable()
        {
            base.OnEnable();

            expandedSize = new Vector2(baseSizeDelta.x + expandValue, baseSizeDelta.y + expandValue);

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoExpandCycleLoopCoroutine());
            }
        }

        public override void RunTween()
        {
            //do nothing
            //this is a hover tween action so tween only executes on hovered (OnPointerEnter and OnPointerExit below)
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            //on pointer enter -> expand size

            rectTransform.DOSizeDelta(expandedSize, tweenDuration);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            //on pointer exit -> collapse to original size

            rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration);
        }

        protected virtual IEnumerator AutoExpandCycleLoopCoroutine()
        {
            //only do auto expand while in auto ui tween mode

            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                //wait for expand operation to finish async
                yield return rectTransform.DOSizeDelta(expandedSize, tweenDuration).WaitForCompletion();

                //collapse to original size and wait for collapse async operation to finish
                yield return rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration).WaitForCompletion();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (expandStartDelay > 0.0f) yield return new WaitForSeconds(expandStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            yield break;
        }

        public void SetLayoutHorizontal()
        {
            
        }

        public void SetLayoutVertical()
        {
            
        }
    }
}
