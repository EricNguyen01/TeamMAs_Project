using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIHoverExpand : UIHover
    {
        [Header("UI Hover Expand Settings")]
        [SerializeField] private float expandValue = 1.0f;

        public override void RunTween()
        {
            //do nothing
            //this is a hover tween action so tween only executes on hovered (OnPointerEnter and OnPointerExit below)
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Vector2 expandedSize = new Vector2(baseSizeDelta.x + expandValue, baseSizeDelta.y + expandValue);

            rectTransform.DOSizeDelta(expandedSize, tweenDuration);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOSizeDelta(baseSizeDelta, tweenDuration);
        }
    }
}
