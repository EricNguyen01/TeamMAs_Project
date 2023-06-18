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

        [SerializeField]
        [Tooltip("The delay in seconds before new expand cycle begins. " +
        "If set to 0, repeat expand immediately after current expand duration ends." +
        "Only works with Auto UITweenExecuteMode!")]
        protected float tweenAutoStartDelay = 0.0f;

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

        public virtual void OnPointerDown(PointerEventData eventData)//click on
        {
            if (UI_TweenExecuteMode != UITweenExecuteMode.ClickOnly || UI_TweenExecuteMode != UITweenExecuteMode.ClickAndHover) return;

            RunTweenInternal();
        }
    }
}
