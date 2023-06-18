using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public abstract class UITweenBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        protected enum UITweenExecuteMode { ClickOnly, HoverOnly, ClickAndHover, Auto, Internal }

        [Header("UI Tween General Settings")]

        [SerializeField]
        [Tooltip("Auto: the tween or tween cycle will be performed and looped automatically on component enabled.\n" +
        "Internal: the tween will be processed internally.")]
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

        public abstract void RunTweenInternal();

        public float GetTweenDuration()
        {
            return tweenDuration;
        }

        public abstract void OnPointerEnter(PointerEventData eventData);//hover on
        public abstract void OnPointerExit(PointerEventData eventData);//hover off

        public abstract void OnPointerDown(PointerEventData eventData);//click on
    }
}
