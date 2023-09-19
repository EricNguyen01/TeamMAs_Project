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
        private LineRenderer lineRenderer;

        private bool isRunningSequence = false;

        private Vector3[] defaultLinePoints = { Vector3.zero };

        private Vector3[] dynamicLinePoints = new Vector3[2];

        private void Awake()
        {
            TryGetComponent<LineRenderer>(out lineRenderer);
        }

        public void InitializeConnectingLine()
        {

        }

        public LineRenderer ActivateLine(Vector3 from, Vector3 to)
        {
            lineRenderer.enabled = true;

            dynamicLinePoints[0] = from;

            dynamicLinePoints[1] = to;

            lineRenderer.SetPositions(dynamicLinePoints);  

            return lineRenderer;
        }

        public LineRenderer ActivateLineWithSequence(bool deactivateOnSequenceEnded)
        {
            return lineRenderer;
        }

        private IEnumerator LineSequence(float sequenceDuration)
        {
            yield break;
        }

        public void DeactivateLineAndReturnToPool()
        {
            lineRenderer.SetPositions(defaultLinePoints);

            lineRenderer.enabled = false;
        }
    }
}
