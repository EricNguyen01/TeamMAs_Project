using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UIMove : UITweenBase
    {
        private enum UIMoveDir { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }

        [Header("UI Move In Direction Settings")]

        [SerializeField] private UIMoveDir moveDir = UIMoveDir.None;

        [SerializeField] private float moveDistance = 1.0f;

        [Header("UI Move From To Settings")]

        [SerializeField]
        [Tooltip("Set the current position of this UI object as the position to start moving from")]
        private bool currentPositionAsStartPoint = true;

        [SerializeField]
        [Tooltip("The position to start moving from. " +
        "Overridden if the current position of UI obj is set as starting position")]
        private Vector2 startingAnchoredPosition = Vector2.zero;

        [SerializeField]
        [Tooltip("The position to move to")]
        private Vector2 endAnchoredPosition = Vector2.zero;

        protected override void Awake()
        {
            base.Awake();

            if (endAnchoredPosition != Vector2.zero) moveDir = UIMoveDir.None;

            //set start pos

            if(!currentPositionAsStartPoint && startingAnchoredPosition != Vector2.zero)
            {
                baseAnchoredPos = startingAnchoredPosition;
            }

            //set end pos if is using move distance instead of preset move-to pos

            if(moveDir != UIMoveDir.None)
            {
                endAnchoredPosition = FinalMoveToPositionFromSetDistance();
            }
        }

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            if(UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                ReverseMove();
            }

            alreadyPerformedTween = true;

            yield return rectTransform.DOAnchorPos(endAnchoredPosition, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        public void ReverseMove()
        {
            if (rectTransform.anchoredPosition == baseAnchoredPos) return;

            Vector2 tempStartPos = baseAnchoredPos;

            baseAnchoredPos = endAnchoredPosition;

            endAnchoredPosition = tempStartPos;

            if (moveDir == UIMoveDir.None) return;

            if(moveDir == UIMoveDir.Right) moveDir = UIMoveDir.Left;

            if(moveDir == UIMoveDir.Left) moveDir = UIMoveDir.Right;

            if (moveDir == UIMoveDir.Up) moveDir = UIMoveDir.Down;

            if (moveDir == UIMoveDir.Down) moveDir = UIMoveDir.Up;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DOAnchorPos(endAnchoredPosition, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }

        /*
         * Disclaimer for "FinalMoveToPosition Function:
         * IF is using move distance value instead of a preset "endAnchoredPostion" value,
         * this function should only be called in Awake (and never in the tween) 
         * to calc the end pos value from the provided move distance.
         */
        private Vector2 FinalMoveToPositionFromSetDistance()
        {
            if(endAnchoredPosition != Vector2.zero) return endAnchoredPosition;

            switch (moveDir)
            {
                case UIMoveDir.None:
                    return Vector2.zero;

                case UIMoveDir.Up:
                    return new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + moveDistance);

                case UIMoveDir.Down:
                    return new Vector2(baseAnchoredPos.x, baseAnchoredPos.y - moveDistance);

                case UIMoveDir.Left:
                    return new Vector2(baseAnchoredPos.x - moveDistance, baseAnchoredPos.y);

                case UIMoveDir.Right:
                    return new Vector2(baseAnchoredPos.x + moveDistance, baseAnchoredPos.y);

                default: return Vector2.zero;
            }
        }
    }
}
