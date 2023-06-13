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

        //INTERNALS...............................................

        private Vector2 baseAnchoredPos;

        private bool alreadyShook = false;

        protected override void Awake()
        {
            base.Awake();

            baseAnchoredPos = rectTransform.anchoredPosition;
        }

        public override void RunTween()
        {
            ProcessUIShake();
        }

        protected virtual void ProcessUIShake()
        {
            if (!rectTransform || alreadyShook) return;

            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            alreadyShook = true;

            rectTransform.DOShakeAnchorPos(tweenDuration, shakeStrength, shakeVibrato, 50.0f, false, true, ShakeRandomnessMode.Harmonic);

            yield return new WaitForSeconds(tweenDuration);

            alreadyShook = false;
        }
    }
}
