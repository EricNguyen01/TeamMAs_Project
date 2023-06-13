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

        //INTERNALS....................................................

        private Vector2 baseAnchorPos;

        protected override void Awake()
        {
            base.Awake();

            baseAnchorPos = rectTransform.anchoredPosition;
        }

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
            rectTransform.DOAnchorPos(baseAnchorPos, tweenDuration);
        }

        protected Vector2 FinalMoveToPosition()
        {
            switch (hoverMoveDir)
            {
                case UIHoverMoveDir.None:
                    return Vector2.zero;

                case UIHoverMoveDir.Up:
                    return new Vector2(baseAnchorPos.x, baseAnchorPos.y + moveDistance);

                case UIHoverMoveDir.Down:
                    return new Vector2(baseAnchorPos.x, baseAnchorPos.y - moveDistance);

                case UIHoverMoveDir.Left:
                    return new Vector2(baseAnchorPos.x - moveDistance, baseAnchorPos.y);

                case UIHoverMoveDir.Right:
                    return new Vector2(baseAnchorPos.x + moveDistance, baseAnchorPos.y);

                default: return Vector2.zero;
            }
        }
    }
}
