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

        [SerializeField] private float expandDuration = 0.25f;

        //INTERNALS........................................................

        private Vector2 baseSizeDelta;

        protected override void Awake()
        {
            base.Awake();

            baseSizeDelta = rectTransform.sizeDelta;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Vector2 expandedSize = new Vector2(baseSizeDelta.x + expandValue, baseSizeDelta.y + expandValue);

            rectTransform.DOSizeDelta(expandedSize, expandDuration);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOSizeDelta(baseSizeDelta, expandDuration);
        }
    }
}
