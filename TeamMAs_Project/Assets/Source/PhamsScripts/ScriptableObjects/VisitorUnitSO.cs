using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Visitor Data Asset/New Visitor")]
    public class VisitorUnitSO : UnitSO
    {
        public enum VisitorType { None, Human, Pollinator }

        [field: Header("Visitor Unit Stats")]
        [field: SerializeField] public VisitorType visitorType { get; private set; } = VisitorType.Human;
        [field: SerializeField] [field: Min(0.0f)] public float happinessAsHealth { get; private set; } = 100.0f;
        [field: SerializeField][field: Min(0.0f)] public float moveSpeed { get; private set; } = 1.0f;
        [field: SerializeField] [field: Min(0.0f)] public float emotionalAttackDamage { get; private set; } = 1.0f;
        [field: SerializeField] public bool isBoss { get; private set; } = false;

        [field: Header("Visitor Taking Hits Settings")]
        [field: SerializeField] public Color visitorHitColor { get; private set; }

        [field: Header("Visitor Appeasement Settings")]
        [field: SerializeField] public AnimationClip visitorAppeasementAnimClip { get; private set; }
        [field: SerializeField] public float visitorAppeasementTime { get; private set; } = 2.5f;
    }
}
