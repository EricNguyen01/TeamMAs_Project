using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIJumpFx :UITweenBase
    {
        [Header("UI Jump FX Settings")]

        [SerializeField] private float jumpHeight = 5.0f;

        [SerializeField] private float jumpPower = 1.0f;

        public override void RunTween()
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

            rectTransform.DOJumpAnchorPos(new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + jumpHeight), jumpPower, 2, tweenDuration);

            yield return new WaitForSeconds(tweenDuration);

            alreadyPerformedTween = false;
        }
    }
}
