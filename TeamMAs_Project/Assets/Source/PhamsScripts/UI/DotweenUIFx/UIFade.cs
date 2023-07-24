using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static CartoonFX.ExpressionParser;

namespace TeamMAsTD
{
    public class UIFade : UITweenBase
    {

        [Header("UI Fade Settings")]

        //if a canvas group is not assigned and
        //if no CanvasGroup is added to this UI obj, the UITweenBase parent class will add it automatically on awake and set the added CG as default CG to fade
        [SerializeField] private CanvasGroup canvasGroupToFade;

        [SerializeField] private Image imageToFade;

        [Range(0f, 1f)]
        [Tooltip("0f = fully fade.\n" +
                     "1f = no fade.")]
        public float fadeTo;

        //INTERNALS...............................................................................

        private float imageBaseAlphaVal = 255f;

        private bool canvasGroupFadeCompleted = true;

        private bool imageFadeCompleted = true;

        protected override void Start()
        {
            base.Start();

            if (!canvasGroupToFade)
            {
                if (canvasGroup) canvasGroupToFade = canvasGroup;
            }

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

                    canvasGroupTween = canvasGroupToFade.DOFade(fadeTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(canvasGroupTween));
                }

                if (imageToFade)
                {
                    Tween imageTween;

                    imageTween = imageToFade.DOFade(imageToFade.color.a * fadeTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

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

                    canvasGroupTween = canvasGroupToFade.DOFade(1f, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(canvasGroupTween));
                }

                if (imageToFade)
                {
                    Tween imageTween;

                    imageTween = imageToFade.DOFade(imageBaseAlphaVal, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                    StartCoroutine(ProcessCanvasGroupOnTweenStartStop(imageTween));
                }
            }
        }

        protected override IEnumerator RunTweenCycleOnceCoroutine()//internal call for fade
        {
            alreadyPerformedTween = true;

            if(canvasGroupToFade && !imageToFade) StartCoroutine(CanvasGroupFadeCoroutine());

            if(imageToFade) StartCoroutine(ImageFadeCoroutine());

            yield return new WaitUntil(() => (canvasGroupFadeCompleted && imageFadeCompleted));

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

            yield return imageToFade.DOFade(imageToFade.color.a * fadeTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

            imageFadeCompleted = true;

            yield break;
        }
    }
}
