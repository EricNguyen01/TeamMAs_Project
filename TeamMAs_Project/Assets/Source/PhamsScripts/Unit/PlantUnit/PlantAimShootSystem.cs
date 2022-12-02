using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantUnit))]
    public class PlantAimShootSystem : MonoBehaviour
    {
        private PlantUnit plantUnitLinked;

        private PlantProjectileSO plantProjectileSO;

        private TD_GameObjectPool plantProjectilePool;

        private List<VisitorUnit> visitorTargetsList = new List<VisitorUnit>();

        private bool enableAimShoot = false;

        private float currentShootWaitTime = 0.0f;

        private float baseTargetsUpdateWaitTime = 0.03f;

        private float currentTargetsUpdateWaitTime = 0.0f;

        private List<VisitorUnit> debugVisitorsInRange;

        private void OnEnable()
        {
            WaveSpawner.OnWaveStarted += OnWaveStarted;
            WaveSpawner.OnWaveFinished += OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned += OnAllWaveSpawned;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= OnWaveStarted;
            WaveSpawner.OnWaveFinished -= OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned -= OnAllWaveSpawned;
        }

        private void Start()
        {
            if(plantUnitLinked == null)
            {
                enabled = false;
                return;
            }

            //when a plant is planted and this script is enabled -> check if it is enabled after a wave has already started
            //if so, start the targetting and shooting of this plant
            bool alreadyHasOngoingWave = FindOnGoingWaves();

            if (alreadyHasOngoingWave) EnablePlantAimShoot(true);
        }

        private void Update()
        {
            if (enableAimShoot)
            {
                UpdateVisitorTargetsList();
                ProcessPlantAimShoot();
            }
        }

        private void InitializePlantProjectileInPool()
        {
            if (plantProjectilePool == null || plantProjectilePool.gameObjectsPool == null) return;

            for(int i = 0; i < plantProjectilePool.gameObjectsPool.Count; i++)
            {
                PlantProjectile plantProjectileComp = plantProjectilePool.gameObjectsPool[i].GetComponent<PlantProjectile>();

                if(plantProjectileComp == null)
                {
                    Debug.LogError("Plant Projectile GameObject prefab of PlantProjectileSO: " + plantProjectileSO.name +
                    " has no PlantProjectile script component attached! Pooling of this visitor obj failed!");
                    continue;
                }

                plantProjectileComp.InitializePlantProjectile(plantUnitLinked);
            }
        }

        private Quaternion CalculateProjectileRotatesTowardsTarget(GameObject projectileObj, VisitorUnit target)
        {
            if (projectileObj == null || target == null) return Quaternion.identity;

            Vector2 dirToTarget = (Vector2)target.transform.position - (Vector2)projectileObj.transform.position;

            dirToTarget.Normalize();

            return Quaternion.FromToRotation(projectileObj.transform.up, dirToTarget);
        }

        private void ProcessPlantAimShoot()
        {
            if(currentShootWaitTime <= 0.0f)
            {
                for (int i = 0; i < visitorTargetsList.Count; i++)
                {
                    if (visitorTargetsList[i] == null) continue;

                    GameObject projectileGO = plantProjectilePool.GetInactiveGameObjectFromPool();

                    projectileGO.transform.rotation = CalculateProjectileRotatesTowardsTarget(projectileGO, visitorTargetsList[i]);

                    if (!projectileGO.activeInHierarchy) projectileGO.SetActive(true);

                    Debug.Log("Projectile: " + projectileGO.name + " successfully launched from Plant: " + name + ".");
                }

                currentShootWaitTime = plantUnitLinked.plantUnitScriptableObject.attackSpeed;

                return;
            }

            currentShootWaitTime -= Time.deltaTime;
        }

        private void UpdateVisitorTargetsList()
        {
            if (currentTargetsUpdateWaitTime <= 0.0f)
            {
                UpdateVisitorTargetsList(GetVisitorsInRange());
                currentTargetsUpdateWaitTime = baseTargetsUpdateWaitTime;
                return;
            }

            currentTargetsUpdateWaitTime -= Time.deltaTime;
        }

        private List<VisitorUnit> GetVisitorsInRange()
        {
            Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(transform.position, plantUnitLinked.plantMaxAttackRange, LayerMask.GetMask(plantUnitLinked.plantUnitScriptableObject.plantTargetsLayerNames));
            
            List<VisitorUnit> visitorsInRange = null;

            if (collidersInRange.Length == 0) return visitorsInRange;

            visitorsInRange = new List<VisitorUnit>();

            for (int i = 0; i < collidersInRange.Length; i++)
            {
                VisitorUnit visitorUnit = collidersInRange[i].GetComponent<VisitorUnit>();

                if (visitorUnit == null) continue;

                if (!visitorUnit.gameObject.activeInHierarchy) continue;

                if (visitorUnit.currentVisitorHealth <= 0.0f) continue;

                visitorsInRange.Add(visitorUnit);
            }

            return visitorsInRange;
        }

        private void UpdateVisitorTargetsList(List<VisitorUnit> visitorsInRange)
        {
            debugVisitorsInRange = visitorsInRange;

            //if there's no visitors in range -> reset visitorTargetsList elements to null
            if (visitorsInRange == null || visitorsInRange.Count == 0)
            {
                for(int i = 0; i < visitorTargetsList.Count; i++)
                {
                    visitorTargetsList[i] = null;
                }

                return;
            }

            //if there are visitors in range and this is a 1 target per atk only plant:
            if(visitorTargetsList.Count == 1)
            {
                for(int i = 0; i < visitorsInRange.Count; i++)
                {
                    if (visitorTargetsList[0] == null)
                    {
                        visitorTargetsList[0] = visitorsInRange[i];
                        continue;
                    }

                    //use normal dist sort since its a 1 target only.
                    //using linq here would be too overkill and expensive
                    float distToPlantOld = Vector2.Distance(transform.position, visitorTargetsList[0].transform.position);
                    float distToPlantNew = Vector2.Distance(transform.position, visitorsInRange[i].transform.position);

                    if (distToPlantNew < distToPlantOld)
                    {
                        visitorTargetsList[0] = visitorsInRange[i];
                        continue;
                    }
                }

                return;
            }

            //if this is a multiple targets per atk plant:

            //use linq to sort the visitors in range by distance (from closest to furthest) first
            visitorsInRange = visitorsInRange.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).ToList();
            
            //match the sorted visitorsInRange list to the visitorTargetsList now that the distance are in order
            for(int i = 0; i < visitorTargetsList.Count; i++)
            {
                if(i <= visitorsInRange.Count - 1)
                {
                    visitorTargetsList[i] = visitorsInRange[i];
                }
                else
                {
                    visitorTargetsList[i] = null;
                }
            }
        }

        //This function gets Called in PlantUnit.cs
        public void InitializePlantAimShootSystem(PlantUnit plantUnit, PlantProjectileSO plantProjectileSO)
        {
            if (plantUnit == null || plantProjectileSO == null || plantProjectileSO.plantProjectilePrefab == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            this.plantProjectileSO = plantProjectileSO;

            plantProjectilePool = new TD_GameObjectPool(this, plantProjectileSO.plantProjectilePrefab, 30, transform, true);

            InitializePlantProjectileInPool();

            InitializeNullTargetsList();//no targets at first so target list only contains nulls
        }

        private void InitializeNullTargetsList()//only use this on intialization!
        {
            for(int i = 0; i < plantUnitLinked.plantUnitScriptableObject.targetsPerAttack; i++)
            {
                visitorTargetsList.Add(null);
            }
        }

        public void ReturnProjectileToPool(PlantProjectile projectile)
        {
            if (projectile == null) return;

            bool returnSuccessful = plantProjectilePool.ReturnGameObjectToPool(projectile.gameObject);

            if(returnSuccessful) Debug.Log("Plant projectile successfully returned to pool!");
        }

        public void EnablePlantAimShoot(bool enabled)
        {
            if (enabled) enableAimShoot = true;
            else enableAimShoot = false;

            //reset visitor target list elements to null
            for(int i = 0; i < visitorTargetsList.Count; i++)
            {
                visitorTargetsList[i] = null;
            }

            currentShootWaitTime = 0.0f;
            currentTargetsUpdateWaitTime = 0.0f;
        }

        //WaveSpawner events callback (check WaveSpawner.cs for more info)........................................................
        public void OnWaveStarted(WaveSpawner waveSpawner, int waveNum)
        {
            if(!enableAimShoot) EnablePlantAimShoot(true);
        }

        public void OnWaveFinished(WaveSpawner waveSpawner, int waveNum)
        {
            bool stillHasOtherOngoingWave = FindOnGoingWaves();

            if(!stillHasOtherOngoingWave) EnablePlantAimShoot(false);
        }

        public void OnAllWaveSpawned(WaveSpawner waveSpawner)
        {
            bool stillHasOtherOngoingWave = FindOnGoingWaves();

            if (!stillHasOtherOngoingWave) EnablePlantAimShoot(false);
        }

        //has looping -> should only call in 1 frame only -> dont use in Update()
        private bool FindOnGoingWaves()
        {
            foreach (WaveSpawner waveSpawner in FindObjectsOfType<WaveSpawner>())
            {
                if (waveSpawner.waveAlreadyStarted)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
