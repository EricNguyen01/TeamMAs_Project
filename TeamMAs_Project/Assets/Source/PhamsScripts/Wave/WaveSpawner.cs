using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private List<WaveSO> wavesList = new List<WaveSO>();

        [Header("Debug Only!")]
        [SerializeField]
        [Tooltip("For fast-forwarding to a specific wave. Set to 0 to disable this debug setting.")]
        private int startAtWave = 0;

        //INTERNALS............................................................................

        //Each type of visitor will have their own visitor pool in which all the pools are stored in this list
        private List<VisitorPool> visitorPools = new List<VisitorPool>();
    }
}
