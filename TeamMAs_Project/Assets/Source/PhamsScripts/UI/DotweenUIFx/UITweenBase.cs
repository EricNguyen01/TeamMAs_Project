// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    public abstract class UITweenBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public enum UITweenExecuteMode { ClickOnly, HoverOnly, ClickAndHover, Auto, Internal }

        [Header("UI Tween General Settings")]

        [SerializeField]
        [Tooltip("Auto: the tween or tween cycle will be performed and looped automatically and independently on component enabled.\n" +
        "Internal: the tween will be called and processed internally.")]
        protected UITweenExecuteMode UI_TweenExecuteMode = UITweenExecuteMode.ClickOnly;

        [SerializeField] 
        protected Ease easeMode = Ease.OutBounce;

        [SerializeField] protected float tweenDuration = 0.5f;

        [SerializeField]
        [Tooltip("The delay in seconds before new expand cycle begins. " +
        "If set to 0, repeat expand immediately after current expand duration ends." +
        "Only works with Auto UITweenExecuteMode!")]
        protected float tweenAutoStartDelay = 0.0f;

        [SerializeField] protected bool isIndependentTimeScale = false;

        [SerializeField] protected bool disableUIFunctionDuringTween = false;

        [SerializeField] protected bool disableUIFunctionAfterTween = false;

        //INTERNALS......................................................................

        protected RectTransform rectTransform;

        protected CanvasGroup canvasGroup;

        protected Vector2 baseAnchoredPos;

        protected Vector2 baseRectRotation;

        protected Vector2 baseSizeDelta;

        protected bool alreadyPerformedTween = false;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if(!rectTransform) rectTransform = GetComponentInChildren<RectTransform>(true);

            if (!rectTransform)
            {
                enabled = false;

                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();

            if(!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);

            if(!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            baseAnchoredPos = rectTransform.anchoredPosition;

            baseRectRotation = rectTransform.rotation.eulerAngles;

            baseSizeDelta = rectTransform.sizeDelta;
        }

        protected virtual void OnEnable()
        {
            if (!enabled) return;

            SceneManager.activeSceneChanged += (Scene sc1, Scene sc2) => KillAllTweenOnSceneLoad();

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoTweenLoopCycleCoroutine());
            }
        }

        protected virtual void OnDisable()
        {
            SceneManager.activeSceneChanged -= (Scene sc1, Scene sc2) => KillAllTweenOnSceneLoad();
        }

        protected virtual void Start()
        {

        }

        public void RunTweenInternal()
        {
            if (!enabled) return;

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

        public void SetTweenExecuteMode(UITweenExecuteMode executeMode)
        {
            UI_TweenExecuteMode = executeMode;
        }

        public virtual void SetUITweenCanvasGroup(CanvasGroup canvasGrp)
        {
            if (!canvasGrp) return;

            canvasGroup = canvasGrp;
        }

        private IEnumerator RunTweenCycleOnceCoroutineBase()
        {
            if (!enabled) yield break;

            alreadyPerformedTween = true;

            if (disableUIFunctionDuringTween)
            {
                if(!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = false;

                canvasGroup.blocksRaycasts = false;
            }

            yield return RunTweenCycleOnceCoroutine();

            if (canvasGroup && disableUIFunctionDuringTween)
            {
                if (!disableUIFunctionAfterTween)
                {
                    canvasGroup.interactable = true;

                    canvasGroup.blocksRaycasts = true;
                }
            }

            alreadyPerformedTween = false;
        }

        protected abstract IEnumerator RunTweenCycleOnceCoroutine();

        protected virtual IEnumerator AutoTweenLoopCycleCoroutine()
        {
            if (!enabled) yield break;

            if (disableUIFunctionDuringTween)
            {
                if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = false;

                canvasGroup.blocksRaycasts = false;
            }

            while (enabled && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                alreadyPerformedTween = true;

                //if start delay is > 0.0f -> wait for this number of seconds before looping cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);

                yield return RunTweenCycleOnceCoroutine();

                //if start delay is > 0.0f -> wait for this number of seconds before looping cycle again
                //if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            if (canvasGroup && disableUIFunctionDuringTween)
            {
                if (!disableUIFunctionAfterTween)
                {
                    canvasGroup.interactable = true;

                    canvasGroup.blocksRaycasts = true;
                }
            }

            StopAndResetUITweenImmediate();

            alreadyPerformedTween = false;

            yield break;
        }

        protected IEnumerator ProcessCanvasGroupOnTweenStartStop(Tween tween)
        {
            if (tween == null || !disableUIFunctionDuringTween) yield break;
            
            if (disableUIFunctionDuringTween)
            {
                if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = false;

                canvasGroup.blocksRaycasts = false;
            }

            yield return tween.WaitForCompletion();

            if (canvasGroup && disableUIFunctionDuringTween)
            {
                if (!disableUIFunctionAfterTween)
                {
                    canvasGroup.interactable = true;

                    canvasGroup.blocksRaycasts = true;
                }
            }

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

        public virtual void StopAndResetUITweenImmediate()
        {
            StopAllCoroutines();

            alreadyPerformedTween = false;

            DOTween.Kill(transform);

            if (!rectTransform) return;

            rectTransform.anchoredPosition = baseAnchoredPos;

            rectTransform.rotation = Quaternion.Euler(baseRectRotation);

            rectTransform.sizeDelta = baseSizeDelta;
        }

        protected static void KillAllTweenOnSceneLoad()
        {
            DOTween.KillAll();
        }
    }
}
