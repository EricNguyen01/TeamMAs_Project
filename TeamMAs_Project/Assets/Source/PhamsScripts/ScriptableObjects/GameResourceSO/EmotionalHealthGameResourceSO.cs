using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "GameResource Data Asset/New Emotional Health Game Resource")]
    public class EmotionalHealthGameResourceSO : GameResourceSO
    {
        [field: SerializeField] public VisitorUnitSO.VisitorType visitorTypeAffectingThisHealth { get; private set; } = VisitorUnitSO.VisitorType.None;

        /* DEPRACATED!!!
        [field: SerializeField] public float totalBaseHealthIncreaseOnWaveEnd { get; private set; } = 1.0f;
        [field: SerializeField] public float healthHealedAfterBossWave { get; private set; } = 5.0f;
        */
    }
}
