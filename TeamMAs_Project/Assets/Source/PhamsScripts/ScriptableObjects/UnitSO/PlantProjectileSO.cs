// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Plant Projectile Data Asset/New Plant Projectile")]
    public class PlantProjectileSO : ScriptableObject
    {
        [field: Header("Plant Projectile General Data")]

        [field: ReadOnlyInspectorPlayMode]
        [field: SerializeField] 
        public GameObject plantProjectilePrefab { get; private set; }

        [field: Header("Plant Projectile Stats")]
        [field: SerializeField] public float plantProjectileSpeed { get; private set; } = 100.0f;
        [field: SerializeField] public bool isHoming { get; private set; } = false;

        [field: SerializeField] public ParticleSystem projectileHitEffect { get; private set; }

        public ParticleSystem SpawnProjectileHitEffect(PlantProjectile projectileUsingEffect, Vector3 spawnOffsetFromProjectile, bool makeEffectChild, bool playEffectOnSpawn)
        {
            if (!projectileHitEffect) return null;

            var fxMain = projectileHitEffect.main;

            fxMain.loop = false;

            fxMain.playOnAwake = false;

            GameObject effectGO;

            ParticleSystem spawnedFx = null;

            if (!projectileUsingEffect)
            {
                if (!playEffectOnSpawn) return null;

                effectGO = Instantiate(projectileHitEffect.gameObject, spawnOffsetFromProjectile, Quaternion.identity);

                effectGO.TryGetComponent<ParticleSystem>(out spawnedFx);

                if(spawnedFx) spawnedFx.Play(); 

                Destroy(effectGO, fxMain.duration + 0.5f);

                return spawnedFx;
            }

            Vector3 projectilePos = projectileUsingEffect.transform.position;

            Vector3 effectSpawnPos = new Vector3(projectilePos.x + spawnOffsetFromProjectile.x, 
                                                 projectilePos.y + spawnOffsetFromProjectile.y, 
                                                 projectilePos.z);

            effectGO = Instantiate(projectileHitEffect.gameObject, effectSpawnPos, Quaternion.identity);

            if (makeEffectChild) effectGO.transform.SetParent(projectileUsingEffect.transform, true);

            effectGO.TryGetComponent<ParticleSystem>(out spawnedFx);

            if (playEffectOnSpawn && spawnedFx) spawnedFx.Play();

            return spawnedFx;
        }
    }
}
