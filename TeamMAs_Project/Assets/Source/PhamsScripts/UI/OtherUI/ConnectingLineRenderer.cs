// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TeamMAsTD
{
    /*
     * Generate a rendered connecting from/to line for anything that needs it (like a Gizmo DrawLine but for runtime using LineRenderer component).
     * Have option to lerp the the line so the start position gradually catches up to the end position after a duration.
     */

    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class ConnectingLineRenderer : MonoBehaviour
    {
        [SerializeField] private float lerpingLineSequenceDuration = 0.8f;

        private LineRenderer lineRenderer;

        private bool isRunningSequence = false;

        private Vector3[] defaultLinePoints = { Vector3.zero, Vector3.zero };

        private Vector3[] dynamicLinePoints = new Vector3[2];

        private void Awake()
        {
            TryGetComponent<LineRenderer>(out lineRenderer);

            if(lineRenderer) lineRenderer.enabled = false;
        }

        public void ActivateLine(Vector3 from, Vector3 to)
        {
            lineRenderer.enabled = true;

            dynamicLinePoints[0] = from;

            dynamicLinePoints[1] = to;

            lineRenderer.SetPositions(dynamicLinePoints);
        }

        public void ActivateLineWithSequence(Vector3 from, Vector3 to, bool deactivateOnSequenceEnded)
        {
            StartCoroutine(LineSequence(from, to, lerpingLineSequenceDuration, deactivateOnSequenceEnded));
        }

        private IEnumerator LineSequence(Vector3 from, Vector3 to, float sequenceDuration, bool deactivateOnSequenceEnded)
        {
            isRunningSequence = true;

            lineRenderer.enabled = true;

            dynamicLinePoints[0] = from;

            dynamicLinePoints[1] = from;

            StartCoroutine(SetLineRendererPointsInSequence());

            yield return DOTween.To(() => dynamicLinePoints[1], x => dynamicLinePoints[1] = x, to, sequenceDuration).SetEase(Ease.InOutFlash).WaitForCompletion();

            yield return DOTween.To(() => dynamicLinePoints[0], x => dynamicLinePoints[0] = x, to, sequenceDuration).SetEase(Ease.InOutExpo).WaitForCompletion();

            isRunningSequence = false;

            StopCoroutine(SetLineRendererPointsInSequence());

            if (deactivateOnSequenceEnded) DeactivateLine();

            yield break;
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();   
        private IEnumerator SetLineRendererPointsInSequence()
        {
            while (isRunningSequence)
            {
                lineRenderer.SetPositions(dynamicLinePoints);

                yield return waitForFixedUpdate;
            }

            yield break;
        }

        public void DeactivateLine()
        {
            lineRenderer.SetPositions(defaultLinePoints);

            if(lineRenderer.enabled) lineRenderer.enabled = false;
        }

        public LineRenderer GetLineRenderer()
        {
            return lineRenderer;
        }
    }
}
