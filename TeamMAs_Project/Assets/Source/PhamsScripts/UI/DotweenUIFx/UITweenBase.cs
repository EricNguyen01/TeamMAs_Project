using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

            if (!rectTransform)
            {
                enabled = false;

                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();

            if(!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            baseAnchoredPos = rectTransform.anchoredPosition;

            baseRectRotation = rectTransform.rotation.eulerAngles;

            baseSizeDelta = rectTransform.sizeDelta;
        }

        protected virtual void OnEnable()
        {
            SceneManager.activeSceneChanged += KillAllTweenOnSceneLoad;

            if (rectTransform && UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                StartCoroutine(AutoTweenLoopCycleCoroutine());
            }
        }

        protected virtual void OnDisable()
        {
            SceneManager.activeSceneChanged -= KillAllTweenOnSceneLoad;
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

        public void SetTweenExecuteMode(UITweenExecuteMode executeMode)
        {
            UI_TweenExecuteMode = executeMode;
        }

        private IEnumerator RunTweenCycleOnceCoroutineBase()
        {
            alreadyPerformedTween = true;

            if (disableUIFunctionDuringTween)
            {
                if(!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = false;

                canvasGroup.blocksRaycasts = false;
            }

            yield return RunTweenCycleOnceCoroutine();

            alreadyPerformedTween = false;

            if (canvasGroup && !canvasGroup.interactable)
            {
                canvasGroup.interactable = true;

                canvasGroup.blocksRaycasts = true;
            }
        }

        protected abstract IEnumerator RunTweenCycleOnceCoroutine();

        protected virtual IEnumerator AutoTweenLoopCycleCoroutine()
        {
            if (disableUIFunctionDuringTween)
            {
                if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = false;
            }

            while (UI_TweenExecuteMode == UITweenExecuteMode.Auto)
            {
                alreadyPerformedTween = true;

                yield return RunTweenCycleOnceCoroutine();

                //if expand start delay is > 0.0f -> wait for this number of seconds before looping expand cycle again
                if (tweenAutoStartDelay > 0.0f) yield return new WaitForSeconds(tweenAutoStartDelay);
            }

            //if not in auto mode -> break and exit coroutine

            if (canvasGroup && !canvasGroup.interactable) canvasGroup.interactable = true;

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
            }

            yield return tween.WaitForCompletion();

            if (canvasGroup && !canvasGroup.interactable) canvasGroup.interactable = true;

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

        public void ResetUITween()
        {
            StopAllCoroutines();

            alreadyPerformedTween = false;

            DOTween.Kill(transform);

            rectTransform.anchoredPosition = baseAnchoredPos;

            rectTransform.rotation = Quaternion.Euler(baseRectRotation);

            rectTransform.sizeDelta = baseSizeDelta;
        }

        private static void KillAllTweenOnSceneLoad(Scene sc1, Scene sc2)
        {
            DOTween.KillAll();
        }
    }
}
