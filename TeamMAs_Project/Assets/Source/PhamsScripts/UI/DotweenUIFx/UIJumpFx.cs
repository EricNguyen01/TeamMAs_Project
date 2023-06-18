using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIJumpFx :UITweenBase
    {
        [Header("UI Jump FX Settings")]

        [SerializeField] private float jumpHeight = 5.0f;

        [SerializeField] private float jumpPower = 1.0f;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoJumpLoopCoroutine());
            }
        }

        public override void RunTweenInternal()
        {
            ProcessUIJumpCycleOnce();
        }

        protected virtual void ProcessUIJumpCycleOnce()
        {
            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(JumpCoroutine());
        }

        protected IEnumerator JumpCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOJumpAnchorPos(new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + jumpHeight), 
                                                       jumpPower, 
                                                       2, 
                                                       tweenDuration).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        protected IEnumerator AutoJumpLoopCoroutine()
        {
            //only do auto jump while in auto ui tween mode

            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                yield return JumpCoroutine();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            yield break;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                ProcessUIJumpCycleOnce();
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
