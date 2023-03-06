using System.Collections;
using System.Collections.Generic;
using TeamMAsTD;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [field: Header("Ability Data")]

    [field: SerializeField] public AbilitySO abilityScriptableObject { get; private set; }
}
