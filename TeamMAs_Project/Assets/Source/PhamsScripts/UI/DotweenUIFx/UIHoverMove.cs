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

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            yield return rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration).SetUpdate(isIndependentTimeScale);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if(!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration).SetUpdate(isIndependentTimeScale);
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
