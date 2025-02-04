// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace TeamMAsTD
{
    public class UIFade : UITweenBase
    {
        private enum FadeMode { FadeIn, FadeOut }

        [Header("UI Fade Settings")]

        //if a canvas group is not assigned and
        //if no CanvasGroup is added to this UI obj, the UITweenBase parent class will add it automatically on awake and set the added CG as default CG to fade
        [SerializeField] private CanvasGroup canvasGroupToFade;

        [SerializeField] private float canvasGroupAlphaToFadeInTo = 0.0f;

        [SerializeField] private float canvasGroupAlphaToFadeOutTo = 1.0f;

        [SerializeField] private Image imageToFade;

        [SerializeField] private float imageAlphaToFadeInTo = 0.0f;

        [SerializeField] private float imageAlphaToFadeOutTo = 255.0f;

        [SerializeField]
        [Tooltip("Fade Mode is only effective for Internal and Auto tween mode. " +
        "For Hover tween mode, hover on will be fade in and hover off will be fade out. " +
        "For Click-on mode, fade in/out will be toggled")]
        private FadeMode fadeMode = FadeMode.FadeIn;

        [SerializeField] public bool reverseFadeOnFadeCycleFinished { get; set; } = false;

        public enum UIFadeMode { FadeIn, FadeOut }

        //INTERNALS...............................................................................

        private float imageBaseAlphaVal = 255f;

        private float canvasGroupBaseAlpha = 1.0f;

        private bool canvasGroupFadeCompleted = true;

        private bool imageFadeCompleted = true;

        protected override void Start()
        {
            base.Start();

            if (!canvasGroupToFade)
            {
                if (canvasGroup) canvasGroupToFade = canvasGroup;
            }

            if(canvasGroupToFade) canvasGroupBaseAlpha = canvasGroupToFade.alpha;

            if (imageToFade)
            {
                imageBaseAlphaVal = imageToFade.color.a;
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)//fade in on hover on
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                if (canvasGroupToFade && !imageToFade)
                {
                    Tween canvasGroupTween;

                    canvasGroupTween = canvasGroupToFade.DOFade(canvasGroupAlphaToFadeInTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(canvasGroupTween));
                }

                if (imageToFade)
                {
                    Tween imageTween;

                    imageTween = imageToFade.DOFade(imageAlphaToFadeInTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(imageTween));
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)//fade out on hover off
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                if (canvasGroupToFade && !imageToFade)
                {
                    Tween canvasGroupTween;

                    canvasGroupTween = canvasGroupToFade.DOFade(canvasGroupAlphaToFadeOutTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(canvasGroupTween));
                }

                if (imageToFade)
                {
                    Tween imageTween;

                    imageTween = imageToFade.DOFade(imageAlphaToFadeOutTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(imageTween));
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (fadeMode == FadeMode.FadeIn) fadeMode = FadeMode.FadeOut;
            else if (fadeMode == FadeMode.FadeOut) fadeMode = FadeMode.FadeIn;
        }

        public override void SetUITweenCanvasGroup(CanvasGroup canvasGrp)
        {
            base.SetUITweenCanvasGroup(canvasGrp);

            if (!canvasGrp) return;

            canvasGroupToFade = canvasGrp;
        }

        public override void StopAndResetUITweenImmediate()
        {
            base.StopAndResetUITweenImmediate();

            if (canvasGroupToFade && !imageToFade)
            {
                canvasGroupToFade.alpha = canvasGroupBaseAlpha;
            }

            if (imageToFade)
            {
                imageToFade.color = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, imageBaseAlphaVal);
            }
        }

        protected override IEnumerator RunTweenCycleOnceCoroutine()//internal call for fade
        {
            alreadyPerformedTween = true;

            if (canvasGroupToFade && !imageToFade) 
            { 
                /*yield return */StartCoroutine(CanvasGroupFadeCoroutine()); 
            }

            if (imageToFade) 
            { 
                /*yield return */StartCoroutine(ImageFadeCoroutine()); 
            }

            if (canvasGroupToFade && !imageToFade) yield return new WaitUntil(() => canvasGroupFadeCompleted);
            else if(imageToFade) yield return new WaitUntil(() => imageFadeCompleted);

            if (reverseFadeOnFadeCycleFinished)
            {
                if (fadeMode == FadeMode.FadeIn) fadeMode = FadeMode.FadeOut;
                else fadeMode = FadeMode.FadeIn;
            }

            alreadyPerformedTween = false;

            yield break;
        }

        private IEnumerator CanvasGroupFadeCoroutine()
        {
            if (!canvasGroupToFade)
            {
                canvasGroupFadeCompleted = true;

                yield break;
            }

            canvasGroupFadeCompleted = false;

            float fadeTo = 0.0f;

            if (fadeMode == FadeMode.FadeIn) fadeTo = canvasGroupAlphaToFadeInTo;
            else fadeTo = canvasGroupAlphaToFadeOutTo;

            if(canvasGroupToFade.alpha == fadeTo)
            {
                canvasGroupFadeCompleted = true;

                yield break;
            }

            yield return canvasGroupToFade.DOFade(fadeTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            canvasGroupFadeCompleted = true;

            yield break;
        }

        private IEnumerator ImageFadeCoroutine()
        {
            if (!imageToFade)
            {
                imageFadeCompleted = true;

                yield break;
            }

            imageFadeCompleted = false;

            float fadeTo = 0.0f;

            if (fadeMode == FadeMode.FadeIn) fadeTo = imageAlphaToFadeInTo;
            else fadeTo = imageAlphaToFadeOutTo;

            if(imageToFade.color.a == fadeTo)
            {
                imageFadeCompleted = true;

                yield break;
            }

            yield return imageToFade.DOFade(fadeTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            imageFadeCompleted = true;

            yield break;
        }

        public void SetFadeMode(UIFadeMode fadeMode)
        {
            if (fadeMode == UIFadeMode.FadeIn) this.fadeMode = FadeMode.FadeIn;
            else this.fadeMode = FadeMode.FadeOut;
        }
    }
}
