using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlantProjectile : MonoBehaviour
    {
        private PlantProjectileSO plantProjectileSO;

        private PlantUnit plantUnitOfProjectile;

        public PlantUnitSO plantUnitSO { get; private set; }

        private VisitorUnit targettedVisitorOfProjectile;

        private Collider2D projectileCollider2D;

        private Rigidbody2D projectileRigidbody2D;

        public SpriteRenderer projectileSpriteRenderer { get; private set; }

        private Vector2 projectileStartPos;

        private float maxTravelDistance = 5.0f;

        private float travelSpeed = 1.0f;

        private bool isHoming = false;

        private void Awake()
        {
            if(LayerMask.LayerToName(gameObject.layer) != "PlantProjectile")
            {
                Debug.LogError("PlantProjectile GameObject: " + name + " is not in PlantProjectile layer!");
            }

            projectileSpriteRenderer = GetComponent<SpriteRenderer>();
            
            CheckProjectileColliderAndRigidbody();
        }

        private void OnDisable()
        {
            //reset data on returning to pool and getting disabled

            projectileRigidbody2D.velocity = Vector2.zero;

            projectileRigidbody2D.angularVelocity = 0.0f;
        }

        private void Update()
        {
            DespawnOnOutOfTravelDistance();
        }

        private void FixedUpdate()
        {
            ProcessProjectileMovement();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //find if the object collided is an IDamageable -> only IDamageable can receive damage from projectile
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            //if not IDamageable -> do nothing!
            if (damageable == null) return;

            VisitorUnit visitorHit = null;

            //check for the type of object hit -> if IDamageable is of VisitorUnit type, set visitorHit
            if(damageable.ObjectTakenDamage().GetType() == typeof(VisitorUnit))
            {
                visitorHit = (VisitorUnit)damageable.ObjectTakenDamage();
            }

            //check if a valid visitor is hit and whether it is the same as the visitor this projectile and plant is targetting
            //also check if targetted visitor is not null or disabled(returned to visitor pool) and has health > 0.0f
            if(visitorHit != null && 
                targettedVisitorOfProjectile != null && 
                targettedVisitorOfProjectile.currentVisitorHealth > 0.0f && 
                targettedVisitorOfProjectile.gameObject.activeInHierarchy)
            {
                //if hit a non-targetted visitor
                if (visitorHit != targettedVisitorOfProjectile)
                {
                    //check whether the projectile has passed the intended target and is now hitting a different one further
                    if (plantUnitOfProjectile != null)
                    {
                        float distFromPlantToVisitorHit = Vector2.Distance((Vector2)visitorHit.transform.position, (Vector2)plantUnitOfProjectile.transform.position);

                        float distFromPlantToTargettedVisitor = Vector2.Distance((Vector2)targettedVisitorOfProjectile.transform.position, (Vector2)plantUnitOfProjectile.transform.position);

                        //if still moving to intended target and hit another visitor along the way -> do nothing and exit hit event
                        if (distFromPlantToVisitorHit <= distFromPlantToTargettedVisitor) return;
                    }
                    //else
                    //if plant unit of this projectile is null or
                    //if this projectile has passed the intended target and is hitting another visitor further ->
                    //continue to execute hit event below
                }
                //else if hit the intended target -> also continue execute hit event below
            }

            //if above checks have not caused the function to exit meaning that a valid IDamageable is hit -> deals damage accordingly
            damageable.TakeDamageFrom(this, plantUnitSO.damage);

            //if(visitorHit != null) Debug.Log("Projectile Hit Visitor: " + visitorHit.name);

            DespawnProjectile();//despawn projectile on hit.
        }

        private void ProcessProjectileMovement()
        {
            //if projectile is a homing type
            if (isHoming)
            {
                //if target visitor unit given to this projectile is not null, not dead, and not disabled -> home onto target
                if (targettedVisitorOfProjectile != null && 
                    targettedVisitorOfProjectile.currentVisitorHealth > 0.0f && 
                    targettedVisitorOfProjectile.gameObject.activeInHierarchy)
                {
                    Vector3 targetPosSameZ = new Vector3(targettedVisitorOfProjectile.transform.position.x, targettedVisitorOfProjectile.transform.position.y, transform.position.z);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, HelperFunctions.GetRotationToPos2D(transform.position, targetPosSameZ), 180.0f * Time.fixedDeltaTime);

                    transform.position = Vector3.MoveTowards(transform.position, targettedVisitorOfProjectile.transform.position, travelSpeed * Time.fixedDeltaTime);

                    return;
                }
            }
            //else if not homing or target is null/disable ->
            //just fly forward with direction based on starting rotation (rotation upon projectile enabled)
            transform.position += transform.up * travelSpeed * Time.fixedDeltaTime;
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

        public void SetPlantProjectileNewPlantUnitSOData(PlantUnitSO plantUnitSO)
        {
            if (plantUnitSO == null) return;

            this.plantUnitSO = plantUnitSO;
        }

        public void SetNewPlantProjectileSOData(PlantProjectileSO plantProjectileSO)
        {
            if(plantProjectileSO == null) return;

            this.plantProjectileSO = plantProjectileSO;
        }
    }
}
