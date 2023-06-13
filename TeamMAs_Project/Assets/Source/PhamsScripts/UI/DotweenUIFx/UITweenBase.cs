using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TeamMAsTD
{
    public abstract class UITweenBase : MonoBehaviour
    {
        protected enum UITweenExecuteMode { ClickOnly, HoverOnly, ClickAndHover, Internal }

        [Header("UI Tween General Settings")]

        [SerializeField]
        protected UITweenExecuteMode UI_TweenExecuteMode = UITweenExecuteMode.ClickOnly;

        [SerializeField] protected float tweenDuration = 0.5f;

        protected RectTransform rectTransform;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (!rectTransform) enabled = false;
        }

        public abstract void RunTween();

        public float GetTweenDuration()
        {
            return tweenDuration;
        }
    }
}
