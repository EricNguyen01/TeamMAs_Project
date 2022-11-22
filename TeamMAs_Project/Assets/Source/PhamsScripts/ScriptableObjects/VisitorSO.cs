using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Visitor Data Asset/New Visitor")]
    public class VisitorSO : ScriptableObject
    {
        [field: Header("Visitor Stats")]
        [field: SerializeField] public string displayName { get; private set; }
        public enum VisitorType { Human, Pollinator }
        [field: SerializeField] public VisitorType visitorType { get; private set; } = VisitorType.Human;
        [field: SerializeField] [field: Min(0.0f)] public float happinessAsHealth { get; private set; } = 100.0f;
        [field: SerializeField][field: Min(0.0f)] public float moveSpeed { get; private set; } = 1.0f;
        [field: SerializeField] [field: Min(0.0f)] public float emotionalAttackDamage { get; private set; } = 1.0f;
        [field: SerializeField] public bool isBoss { get; private set; } = false;
        [field: SerializeField] public GameObject visitorPrefab { get; private set; }
    }
}
