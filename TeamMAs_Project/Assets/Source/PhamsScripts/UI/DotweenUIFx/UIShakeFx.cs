using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class UIShakeFx : MonoBehaviour
    {
        [Header("UI Shake FX Settings")]

        [SerializeField] private Vector3 shakeStrength = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField] private int shakeVibrato = 5;

        [SerializeField] private float shakeDuration = 0.5f;

        //INTERNALS...............................................

        private RectTransform rectTransform;

        private Vector2 baseAnchoredPos;

        private bool alreadyShook = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (!rectTransform) enabled = false;

            baseAnchoredPos = rectTransform.anchoredPosition;
        }

        public void ProcessUIShake()
        {
            if (!rectTransform || alreadyShook) return;

            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            alreadyShook = true;

            rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, 50.0f, false, true, ShakeRandomnessMode.Harmonic);

            yield return new WaitForSeconds(shakeDuration);

            alreadyShook = false;
        }
    }
}
