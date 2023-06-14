using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class UIShakeFx : UITweenBase
    {
        [Header("UI Shake FX Settings")]

        [SerializeField] private Vector3 shakeStrength = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField] private int shakeVibrato = 5;

        public override void RunTween()
        {
            ProcessUIShake();
        }

        protected virtual void ProcessUIShake()
        {
            if (!rectTransform || alreadyPerformedTween) return;

            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            alreadyPerformedTween = true;

            rectTransform.DOShakeAnchorPos(tweenDuration, shakeStrength, shakeVibrato, 50.0f, false, true, ShakeRandomnessMode.Harmonic);

            yield return new WaitForSeconds(tweenDuration);

            alreadyPerformedTween = false;
        }
    }
}
