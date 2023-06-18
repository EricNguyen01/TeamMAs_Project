using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIJumpFx :UITweenBase
    {
        [Header("UI Jump FX Settings")]

        [SerializeField] private float jumpHeight = 5.0f;

        [SerializeField] private float jumpPower = 1.0f;

        public override void RunTweenInternal()
        {
            ProcessUIJump();
        }

        protected virtual void ProcessUIJump()
        {
            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(JumpCoroutine());
        }

        private IEnumerator JumpCoroutine()
        {
            alreadyPerformedTween = true;

            yield return rectTransform.DOJumpAnchorPos(new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + jumpHeight), 
                                                       jumpPower, 
                                                       2, 
                                                       tweenDuration).WaitForCompletion();

            alreadyPerformedTween = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {

            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!rectTransform) return;

            if (UI_TweenExecuteMode == UITweenExecuteMode.HoverOnly || UI_TweenExecuteMode == UITweenExecuteMode.ClickAndHover)
            {

            }
        }
    }
}
