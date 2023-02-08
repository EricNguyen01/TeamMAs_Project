using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveVisitorTypesLookAheadSlot : MonoBehaviour
    {
        [SerializeField] private VisitorUnitSO visitorUnitSO;

        public bool shouldDisplayThisSlot { get; set; } = false;

        private void Awake()
        {
            
        }

        private void DisplayLookAheadUISlot(bool shouldDisplay)
        {

        }
    }
}
