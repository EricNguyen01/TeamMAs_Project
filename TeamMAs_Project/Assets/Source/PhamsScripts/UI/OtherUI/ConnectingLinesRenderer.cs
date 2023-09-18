// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class ConnectingLineRenderer : MonoBehaviour
    {
        public LineRenderer lineRenderer { get; private set; }

        public bool isRunningSequence { get; private set; } = false;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void InitializeConnectingLine()
        {

        }

        public LineRenderer ActivateLine(Vector3 from, Vector3 to)
        {
            LineRenderer line = null;

            return line;
        }

        public LineRenderer ActivateLineWithSequence(bool deactivateOnSequenceEnded)
        {
            LineRenderer line = null;

            return line;
        }

        private IEnumerator LineSequence(float sequenceDuration)
        {
            yield break;
        }

        public void DeactivateLineAndReturnToPool()
        {

        }
    }
}
