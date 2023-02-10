using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveVisitorTypesLookAheadSlot : MonoBehaviour
    {
        private VisitorUnitSO visitorUnitSO;

        private void Awake()
        {
            
        }

        public void InitializeVisitorTypeLookAheadSlot(VisitorUnitSO visitorUnitSO)
        {
            this.visitorUnitSO = visitorUnitSO;
        }

        public void DisplayLookAheadUISlot(bool shouldDisplay)
        {
            if (shouldDisplay)
            {
                gameObject.SetActive(true);

                return;
            }

            gameObject.SetActive(false);
        }
    }
}
