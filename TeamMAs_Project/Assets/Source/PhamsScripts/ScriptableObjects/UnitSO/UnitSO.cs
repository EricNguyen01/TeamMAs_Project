using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class UnitSO : ScriptableObject
    {
        [field: Header("General Unit Data")]
        [field: SerializeField] public string displayName { get; private set; }
        [field: SerializeField] public GameObject unitPrefab { get; private set; }
    }
}
