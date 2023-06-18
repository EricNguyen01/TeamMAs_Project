using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UIHoverMove : UITweenBase
    {
        private enum UIHoverMoveDir { None, Up, Down, Left, Right }

        [Header("UI Hover Move Settings")]

        [SerializeField] private UIHoverMoveDir hoverMoveDir = UIHoverMoveDir.None;

        [SerializeField] private float moveDistance = 1.0f;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoMoveCycleLoopCoroutine());
            }
        }

        public override void RunTweenInternal()
        {
            ProcessUIMoveTweenCycleOnce();
        }

        protected virtual void ProcessUIMoveTweenCycleOnce()
        {
            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(UIMoveCycleCoroutine());
        }

        protected IEnumerator UIMoveCycleCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration).WaitForCompletion();

            yield return rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        protected IEnumerator AutoMoveCycleLoopCoroutine()
        {
            //only do move cycle while in auto ui tween mode

            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                yield return UIMoveCycleCoroutine();

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
                rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if(!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration);
            }
        }

        protected Vector2 FinalMoveToPosition()
        {
            switch (hoverMoveDir)
            {
                case UIHoverMoveDir.None:
                    return Vector2.zero;

                case UIHoverMoveDir.Up:
                    return new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + moveDistance);

                case UIHoverMoveDir.Down:
                    return new Vector2(baseAnchoredPos.x, baseAnchoredPos.y - moveDistance);

                case UIHoverMoveDir.Left:
                    return new Vector2(baseAnchoredPos.x - moveDistance, baseAnchoredPos.y);

                case UIHoverMoveDir.Right:
                    return new Vector2(baseAnchoredPos.x + moveDistance, baseAnchoredPos.y);

                default: return Vector2.zero;
            }
        }
    }
}
