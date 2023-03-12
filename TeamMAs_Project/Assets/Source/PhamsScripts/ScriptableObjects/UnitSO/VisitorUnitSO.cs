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
        [field: SerializeField] [field: Min(0.0f)] public float moveSpeed { get; private set; } = 1.0f;
        [field: SerializeField][field: Min(0.0f)] public float appeasementHealAmount { get; private set; } = 0.0f;
        [field: SerializeField] [field: Min(0.0f)] public float dissapointmentDamage { get; private set; } = 0.0f;
        [field: SerializeField] public bool isBoss { get; private set; } = false;

        [field: Header("Visitor Taking Hits Settings")]
        [field: SerializeField] public Color visitorHitColor { get; private set; }

        [field: Header("Visitor Appeasement Settings")]
        [field: SerializeField] [field: Min(0)] public float visitorAppeasementTime { get; private set; } = 2.5f;
        [field: SerializeField] public CoinResourceSO coinResourceToDrop { get; private set; }
        [field: SerializeField][field: Min(0)] public int visitorsAppeasedCoinsDrop { get; private set; }
        [field: SerializeField][field: Range(1, 100)] public int chanceToDropCoins { get; private set; }
        [field: SerializeField][field: Range(0, 100)] public int chanceToNotDropCoins { get; private set; }

        public override UnitSO CloneThisUnitSO(UnitSO unitSO)
        {
            UnitSO visitorSO = Instantiate(unitSO);

            return visitorSO;
        }

        public void SetSpecificVisitorHealth(float health)
        {
            happinessAsHealth = health;
        }

        public void AddVisitorHealth(float addedHealth)
        {
            happinessAsHealth += addedHealth;
        }

        public void RemoveVisitorHealth(float removedHealth)
        {
            happinessAsHealth -= removedHealth;

            if (happinessAsHealth <= 0.0f) happinessAsHealth = 0.0f;
        }

        public void SetSpecificVisitorMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        public void AddVisitorMoveSpeed(float moveSpeedIncreased)
        {
            moveSpeed += moveSpeedIncreased;
        }

        public void RemoveVisitorMoveSpeed(float moveSpeedDecreased)
        {
            moveSpeed -= moveSpeedDecreased;

            if(moveSpeed <= 0.0f) moveSpeed = 0.0f;
        }
    }
}
