using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UIHoverMove : UIHover
    {
        private enum UIHoverMoveDir { None, Up, Down, Left, Right }

        [Header("UI Hover Move Settings")]

        [SerializeField] private UIHoverMoveDir hoverMoveDir = UIHoverMoveDir.None;

        [SerializeField] private float moveDistance = 1.0f;

        public override void RunTween()
        {
            UIMoveOnClick();
        }

        protected virtual void UIMoveOnClick()
        {

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOAnchorPos(FinalMoveToPosition(), tweenDuration);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOAnchorPos(baseAnchoredPos, tweenDuration);
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
