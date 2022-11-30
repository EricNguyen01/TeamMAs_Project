using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PlantProjectile : MonoBehaviour
    {
        private PlantProjectileSO plantProjectileSO;

        private PlantUnit plantUnitOfProjectile;

        private PlantUnitSO plantUnitSO;

        private Collider2D projectileCollider2D;

        private Rigidbody2D projectileRigidbody2D;

        private void Awake()
        {
            CheckProjectileColliderAndRigidbody();
        }

        private void Update()
        {
            //if projectile is a homing type
            if (plantProjectileSO.isHoming)
            {

                return;
            }
            //elese
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            if (damageable == null) return;

            damageable.TakeDamageFrom(this, plantUnitSO.damage);
        }

        private void CheckProjectileColliderAndRigidbody()
        {
            projectileCollider2D = GetComponent<Collider2D>();

            if(projectileCollider2D == null)
            {
                projectileCollider2D = gameObject.AddComponent<CapsuleCollider2D>();
            }

            projectileRigidbody2D = GetComponent<Rigidbody2D>();

            if(projectileRigidbody2D == null)
            {
                projectileRigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            }

            projectileRigidbody2D.gravityScale = 0.0f;
            projectileRigidbody2D.freezeRotation = true;
        }

        public void InitializePlantProjectile(PlantUnit plantUnitSpawnedThisProjectile)
        {
            if(plantUnitSpawnedThisProjectile == null || 
            plantUnitSpawnedThisProjectile.plantUnitScriptableObject == null || 
            plantUnitSpawnedThisProjectile.plantUnitScriptableObject.plantProjectileSO == null)
            {
                gameObject.SetActive(false);
                return;
            }

            plantProjectileSO = plantUnitSpawnedThisProjectile.plantUnitScriptableObject.plantProjectileSO;
            plantUnitOfProjectile = plantUnitSpawnedThisProjectile;
            plantUnitSO = plantUnitSpawnedThisProjectile.plantUnitScriptableObject;
        }
    }
}
