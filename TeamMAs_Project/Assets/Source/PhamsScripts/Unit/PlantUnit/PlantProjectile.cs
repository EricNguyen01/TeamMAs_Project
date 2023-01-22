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

        public PlantUnitSO plantUnitSO { get; private set; }

        private VisitorUnit targettedVisitorOfProjectile;

        private Collider2D projectileCollider2D;

        private Rigidbody2D projectileRigidbody2D;

        private Vector2 projectileStartPos;

        private float maxTravelDistance = 5.0f;

        private float travelSpeed = 1.0f;

        private bool alreadyHitAVisitor = false;

        private bool isHoming = false;

        private void Awake()
        {
            if(LayerMask.LayerToName(gameObject.layer) != "PlantProjectile")
            {
                Debug.LogError("PlantProjectile GameObject: " + name + " is not in PlantProjectile layer!");
            }
            
            CheckProjectileColliderAndRigidbody();
        }

        private void OnDisable()
        {
            //reset data on returning to pool and getting disabled
            alreadyHitAVisitor = false;

            projectileRigidbody2D.velocity = Vector2.zero;

            projectileRigidbody2D.angularVelocity = 0.0f;
        }

        private void Update()
        {
            DespawnOnOutOfTravelDistance();

            //if projectile is a homing type
            if (isHoming)
            {

                return;
            }
            //else
            transform.position += transform.up * travelSpeed * Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (alreadyHitAVisitor) return;

            //find if the object collided is an IDamageable -> only IDamageable can receive damage from projectile
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            //if not IDamageable -> do nothing!
            if (damageable == null) return;

            VisitorUnit visitorHit = null;

            //check for the type of object hit
            if(damageable.ObjectTakenDamage().GetType() == typeof(VisitorUnit))
            {
                visitorHit = (VisitorUnit)damageable.ObjectTakenDamage();
            }

            //check if a valid visitor is hit and whether it is the same as the visitor this projectile and plant is targetting
            if(visitorHit != null && targettedVisitorOfProjectile != null)
            {
                //if hit a non-targetted visitor -> do nothing!
                if (visitorHit != targettedVisitorOfProjectile) return;
            }

            //else deals damage to visitor
            damageable.TakeDamageFrom(this, plantUnitSO.damage);

            //if(visitorHit != null) Debug.Log("Projectile Hit Visitor: " + visitorHit.name);

            alreadyHitAVisitor = true;

            DespawnProjectile();//despawn projectile on hit.
        }

        private void DespawnOnOutOfTravelDistance()
        {
            //use cached start pos and travel dist instead of getting from plant unit and plant unit SO
            //this is done so that even if plant was uprooted, the bullet can still check for current travel distance
            if(Vector2.Distance(projectileStartPos, (Vector2)transform.position) >= maxTravelDistance)
            {
                DespawnProjectile();
                //Debug.Log("Projectile has despawned from being out of travel dist!");
            }
        }

        private void DespawnProjectile()
        {
            //if plant that spawned this projectile was uprooted -> destroy this projectile on despawn
            if(plantUnitOfProjectile == null)
            {
                Destroy(gameObject);

                return;
            }

            //else

            //return this projectile to plant's projectile pool
            plantUnitOfProjectile.ReturnProjectileToPool(this);
        }

        private void CheckProjectileColliderAndRigidbody()
        {
            projectileCollider2D = GetComponent<Collider2D>();

            if(projectileCollider2D == null)
            {
                projectileCollider2D = gameObject.AddComponent<CircleCollider2D>();
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
                Debug.LogError("Initialize Plant Projectile: " + name + " failed! Some required components are missing!");
                enabled = false;
                gameObject.SetActive(false);
                return;
            }

            plantProjectileSO = plantUnitSpawnedThisProjectile.plantUnitScriptableObject.plantProjectileSO;

            plantUnitOfProjectile = plantUnitSpawnedThisProjectile;

            plantUnitSO = plantUnitSpawnedThisProjectile.plantUnitScriptableObject;

            projectileStartPos = (Vector2)plantUnitOfProjectile.transform.position;

            maxTravelDistance = plantUnitOfProjectile.plantMaxAttackRange;

            travelSpeed = plantProjectileSO.plantProjectileSpeed;

            isHoming = plantProjectileSO.isHoming;
        }

        public void SetTargettedVisitorUnit(VisitorUnit visitor)
        {
            targettedVisitorOfProjectile = visitor;
        }
    }
}
