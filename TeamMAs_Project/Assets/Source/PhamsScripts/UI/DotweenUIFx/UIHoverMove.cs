using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UIHoverMove : UITweenBase
    {
        private enum UIHoverMoveDir { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }

        [Header("UI Hover Move Settings")]

        [SerializeField] private UIHoverMoveDir hoverMoveDir = UIHoverMoveDir.None;

        [SerializeField] private float moveDistance = 1.0f;

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            //yield return rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        //the set dir functions below are to be used in UnityEvent calls (such as UI Buttons)
        public void SetMoveDirUp()
        {
            hoverMoveDir = UIHoverMoveDir.Up;
        }

        public void SetMoveDirDown()
        {
            hoverMoveDir = UIHoverMoveDir.Down;
        }

        public void SetMoveDirRight()
        {
            hoverMoveDir = UIHoverMoveDir.Right;
        }

        public void SetMoveDirLeft()
        {
            hoverMoveDir = UIHoverMoveDir.Left;
        }

        public void SetInverseCurrentDir()
        {
            if(hoverMoveDir == UIHoverMoveDir.Right) hoverMoveDir = UIHoverMoveDir.Left;

            if(hoverMoveDir == UIHoverMoveDir.Left) hoverMoveDir = UIHoverMoveDir.Right;

            if (hoverMoveDir == UIHoverMoveDir.Up) hoverMoveDir = UIHoverMoveDir.Down;

            if (hoverMoveDir == UIHoverMoveDir.Down) hoverMoveDir = UIHoverMoveDir.Up;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if(!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
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
