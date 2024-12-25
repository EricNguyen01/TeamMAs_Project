// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;

namespace TeamMAsTD
{
    public abstract class BuffDebuffAbilityEffect : AbilityEffect
    {
        [Header("Buff Received Connection Line Renderer")]

        [SerializeField] protected ConnectingLineRenderer buffConnectingLineRendererPrefab;

        [SerializeField] protected bool disableLineOnBuffStarted = false;

        [SerializeField] protected bool disableLineOnSelect = false;

        protected ConnectingLineRenderer buffConnectingLineRenderer;

        //INTERNALS....................................................................

        protected BuffAbilityEffectSO buffAbilityEffectSO { get; private set; }

        protected DeBuffAbilityEffectSO deBuffAbilityEffectSO { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (abilityEffectSO == null)
            {
                enabled = false;

                return;
            }

            if (abilityEffectSO.GetType() != typeof(BuffAbilityEffectSO) && abilityEffectSO.GetType() != typeof(DeBuffAbilityEffectSO))
            {
                Debug.LogError("PlantBuffAbilityEffect script on : " + name + " has unmatched AbilityEffectSO ability type." +
                "Ability effect won't work and will be destroyed!");

                DestroyEffectWithEffectEndedInvoked(false);

                return;
            }

            if (buffConnectingLineRendererPrefab)
            {
                buffConnectingLineRenderer = Instantiate(buffConnectingLineRendererPrefab,
                                                         transform.position,
                                                         Quaternion.identity, transform).GetComponent<ConnectingLineRenderer>();
            }
            else
            {
                buffConnectingLineRenderer = GetComponentInChildren<ConnectingLineRenderer>();
            }

            if(abilityEffectSO.GetType() == typeof(BuffAbilityEffectSO))
            {
                buffAbilityEffectSO = (BuffAbilityEffectSO)abilityEffectSO;
            }

            if(abilityEffectSO.GetType() == typeof(DeBuffAbilityEffectSO))
            {
                deBuffAbilityEffectSO = (DeBuffAbilityEffectSO)abilityEffectSO;
            }
        }

        protected override bool OnEffectStarted()
        {
            if (unitBeingAffectedUnitSO == null)
            {
                Debug.LogError("The unit: " + name + " being affected by this buff effect: " + name + " doesn't have a UnitSO data. " +
                "Destroying buff effect!");

                DestroyEffectWithEffectEndedInvoked(false);

                return false;
            }

            return true;
        }
    }
}
