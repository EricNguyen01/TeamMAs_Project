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

        public abstract UnitSO CloneThisUnitSO(UnitSO unitSO);

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

            if (effectAnimator == null) yield break;

            //Debug.Log("NormTime: " + effectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + " | Dur: " + effectAnimator.GetCurrentAnimatorStateInfo(0).length);
            
            yield return new WaitUntil(() => (effectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= effectAnimator.GetCurrentAnimatorStateInfo(0).length && !effectAnimator.IsInTransition(0)));
            
            //Debug.Log("NormTime: " + effectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + " | Dur: " + effectAnimator.GetCurrentAnimatorStateInfo(0).length);
            
            Destroy(effectGO);

            yield break;
        }
    }
}
