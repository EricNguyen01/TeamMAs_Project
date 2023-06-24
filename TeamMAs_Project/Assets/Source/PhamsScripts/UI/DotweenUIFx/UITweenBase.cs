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

        [SerializeField] protected bool isIndependentTimeScale = false;

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
            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoTweenLoopCycleCoroutine());
            }
        }

        protected virtual void OnDisable()
        {

        }

        protected virtual void Start()
        {

        }

        public void RunTweenInternal()
        {
            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(RunTweenCycleOnceCoroutineBase());
        }

        public float GetTweenDuration()
        {
            return tweenDuration;
        }

        public bool IsTweenRunning()
        {
            return alreadyPerformedTween;
        }

        private IEnumerator RunTweenCycleOnceCoroutineBase()
        {
            alreadyPerformedTween = true;

            yield return RunTweenCycleOnceCoroutine();

            alreadyPerformedTween = false;
        }

        protected abstract IEnumerator RunTweenCycleOnceCoroutine();

        protected virtual IEnumerator AutoTweenLoopCycleCoroutine()
        {
            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                yield return RunTweenCycleOnceCoroutine();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            yield break;
        }

        public abstract void OnPointerEnter(PointerEventData eventData);//hover on
        public abstract void OnPointerExit(PointerEventData eventData);//hover off

        public virtual void OnPointerDown(PointerEventData eventData)//click on
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.ClickOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                RunTweenInternal();
            }
        }
    }
}
