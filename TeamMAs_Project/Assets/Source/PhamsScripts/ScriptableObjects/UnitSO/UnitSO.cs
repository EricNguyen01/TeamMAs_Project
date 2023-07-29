// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    public abstract class UnitSO : ScriptableObject
    {
        [field: Header("General Unit Data")]
        [field: SerializeField] public string displayName { get; private set; }

        [field: SerializeField]
        public GameObject unitPrefab { get; private set; }

        [field: SerializeField] public Sprite unitInfoTooltipImageSprite { get; private set; }

        [field: Header("Unit Effects")]
        [field: SerializeField] public GameObject unitSpawnEffectPrefab { get; private set; }
        [field: SerializeField] public GameObject unitDestroyEffectPrefab { get; private set; }

        public UnitSO CloneThisUnitSO()
        {
            return CloneUnitSO(this);
        }

        protected virtual UnitSO CloneUnitSO(UnitSO unitSOToClone)
        {
            UnitSO instantiatedUnitSO = Instantiate(unitSOToClone);

            return instantiatedUnitSO;
        }

        public GameObject SpawnUnitEffectGameObject(GameObject effectGO, Transform parentTransform, bool makeChildren, bool activeOnSpawn)
        {
            if (effectGO == null || parentTransform == null) return null;

            GameObject effectObjSpawned = null;

            if (makeChildren) effectObjSpawned = Instantiate(effectGO, parentTransform.position, Quaternion.identity, parentTransform);
            else effectObjSpawned = Instantiate(effectGO, parentTransform.position, Quaternion.identity);

            if (activeOnSpawn)
            {
                if (!effectObjSpawned.activeInHierarchy) effectObjSpawned.SetActive(true);
            }
            else
            {
                if (effectObjSpawned.activeInHierarchy) effectObjSpawned.SetActive(false);
            }

            return effectObjSpawned;
        }

        public IEnumerator DestroyOnUnitEffectAnimFinishedCoroutine(GameObject effectGO)
        {
            if (effectGO == null) yield break;

            Animator effectAnimator = effectGO.GetComponent<Animator>();

            ParticleSystem[] particleSystems = effectGO.GetComponentsInChildren<ParticleSystem>();

            if (effectAnimator != null)
            {
                yield return new WaitUntil(() => (effectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= effectAnimator.GetCurrentAnimatorStateInfo(0).length && !effectAnimator.IsInTransition(0)));
            }

            bool particleSystemsFinished = false;

            if (particleSystems != null && particleSystems.Length > 0)
            {
                while (!particleSystemsFinished)
                {
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        if(i < particleSystems.Length - 1)
                        {
                            if (particleSystems[i].main.loop) continue;
                            else 
                            { 
                                if (particleSystems[i].isEmitting) break; 
                            }
                        }

                        if (i == particleSystems.Length - 1)
                        {
                            if(particleSystems[i].main.loop || !particleSystems[i].isEmitting) particleSystemsFinished = true;
                        }
                    }

                    if(!particleSystemsFinished) yield return new WaitForSeconds(0.5f);
                }
            }
            else particleSystemsFinished = true;
            
            Destroy(effectGO);

            yield break;
        }
    }
}
