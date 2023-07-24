using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class UIRotate : UITweenBase
    {
        private enum RotateMode { PingPong, FullLoop }

        [Header("UI Rotate General Settings")]

        [SerializeField] private float rotateDegrees;

        [SerializeField] private RotateMode rotateMode = RotateMode.PingPong;

        //INTERNALS...............................................................

        private Vector3 eulerRotationFrom;

        private Vector3 eulerRotationTo;

        private void OnValidate()
        {
            if (rotateDegrees <= 179.0f) rotateMode = RotateMode.PingPong;//if less than 180 degrees, can only do ping pong rotation (rotate to then reverse not loop around).
        }

        protected override void OnEnable()
        {
            if (!rectTransform) return;

            eulerRotationFrom = rectTransform.rotation.eulerAngles;

            eulerRotationTo = new Vector3(eulerRotationFrom.x, eulerRotationFrom.y, eulerRotationFrom.z + rotateDegrees);

            if (rotateDegrees <= 179.0f) rotateMode = RotateMode.PingPong;//if less than 180 degrees, can only do ping pong rotation (rotate to then reverse not loop around).

            base.OnEnable();
        }

        protected override IEnumerator RunTweenCycleOnceCoroutine()
        {
            alreadyPerformedTween = true;

            if(rotateMode == RotateMode.PingPong)
            {
                yield return rectTransform.DORotate(eulerRotationTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();

                yield return rectTransform.DORotate(eulerRotationFrom, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale).WaitForCompletion();
            }
            else if(rotateMode == RotateMode.FullLoop)
            {

            }

            alreadyPerformedTween = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DORotate(eulerRotationTo, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!enabled) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.Auto) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {
                Tween tween = rectTransform.DORotate(eulerRotationFrom, tweenDuration).SetEase(easeMode).SetUpdate(isIndependentTimeScale);

                StartCoroutine(ProcessCanvasGroupOnTweenStartStop(tween));
            }
        }
    }
}
