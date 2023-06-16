using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TeamMAsTD
{
    public abstract class UITweenBase : MonoBehaviour
    {
        protected enum UITweenExecuteMode { ClickOnly, HoverOnly, ClickAndHover, Auto }

        [Header("UI Tween General Settings")]

        [SerializeField]
        protected UITweenExecuteMode UI_TweenExecuteMode = UITweenExecuteMode.ClickOnly;

        [SerializeField] protected float tweenDuration = 0.5f;

        //INTERNALS......................................................................

        protected RectTransform rectTransform;

        protected Vector2 baseAnchoredPos;

        protected Vector2 baseSizeDelta;

        protected bool alreadyPerformedTween = false;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (!rectTransform)
            {
                enabled = false;

                return;
            }

            baseAnchoredPos = rectTransform.anchoredPosition;

            baseSizeDelta = rectTransform.sizeDelta;
        }

        protected virtual void OnEnable() 
        { 

        }

        protected virtual void OnDisable() 
        {

        }

        protected virtual void Start()
        {

        }

        public abstract void RunTween();

        public float GetTweenDuration()
        {
            return tweenDuration;
        }
    }
}
