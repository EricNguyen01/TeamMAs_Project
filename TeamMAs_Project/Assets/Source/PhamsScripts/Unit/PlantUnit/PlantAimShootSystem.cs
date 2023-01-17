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
        public PlantUnit plantUnitLinked { get; private set; }

        private PlantProjectileSO plantProjectileSO;

        private PlantProjectilePool plantProjectilePool;

        private List<VisitorUnit> visitorTargetsList = new List<VisitorUnit>();

        private bool enableAimShoot = false;

        private float currentShootWaitTime = 0.0f;

        private float baseTargetsUpdateWaitTime = 0.1f;

        private float currentTargetsUpdateWaitTime = 0.0f;

        private List<VisitorUnit> debugVisitorsInRange;

        private void OnEnable()
        {
            WaveSpawner.OnWaveStarted += OnWaveStarted;
            WaveSpawner.OnWaveFinished += OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned += OnAllWaveSpawned;

            VisitorUnit.OnVisitorAppeased += UpdateVisitorTargetsList;
        }

        private void OnDisable()
        {
            WaveSpawner.OnWaveStarted -= OnWaveStarted;
            WaveSpawner.OnWaveFinished -= OnWaveFinished;
            WaveSpawner.OnAllWaveSpawned -= OnAllWaveSpawned;

            VisitorUnit.OnVisitorAppeased -= UpdateVisitorTargetsList;
        }

        private void Start()
        {
            if(plantUnitLinked == null || plantProjectileSO == null)
            {
                Debug.LogError("PlantUnitAimShootSystem script component on PlantUnit: " + name + " is missing crucial references. Disabling script!");
                enabled = false;

                return;
            }

            //when a plant is planted and this script is enabled -> check if it is enabled after a wave has already started
            //if so, start the targetting and shooting of this plant
            CheckOngoingWavesExist();
        }

        private void Update()
        {
            if (enableAimShoot)
            {
                UpdateVisitorTargetsListInUpdate();
                ProcessPlantAimShoot();
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

                    GameObject projectileGO = plantProjectilePool.GetInactiveProjectileObjectFromPool();

                    projectileGO.transform.rotation = CalculateProjectileRotatesTowardsTarget(projectileGO, visitorTargetsList[i]);

                    if (!projectileGO.activeInHierarchy) projectileGO.SetActive(true);

                    Debug.Log("Projectile: " + projectileGO.name + " successfully launched from Plant: " + name + ".");
                }

                currentShootWaitTime = plantUnitLinked.plantUnitScriptableObject.attackSpeed;

                return;
            }

            currentShootWaitTime -= Time.deltaTime;
        }

        private void UpdateVisitorTargetsListInUpdate()
        {
            if (currentTargetsUpdateWaitTime <= 0.0f)
            {
                UpdateVisitorTargetsList();
                return;
            }

            currentTargetsUpdateWaitTime -= Time.deltaTime;
        }

        private void UpdateVisitorTargetsList()
        {
            UpdateVisitorTargetsList(GetVisitorsInRange());
            currentTargetsUpdateWaitTime = baseTargetsUpdateWaitTime;
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

        private void CheckOngoingWavesExist()
        {
            bool alreadyHasOngoingWave = false;

            if (WaveSpawnerManager.waveSpawnerManagerInstance != null)
            {
                alreadyHasOngoingWave = WaveSpawnerManager.waveSpawnerManagerInstance.HasActiveWaveSpawnersExcept(null);
            }
            else
            {
                foreach (WaveSpawner waveSpawner in FindObjectsOfType<WaveSpawner>())
                {
                    if (waveSpawner.waveAlreadyStarted)
                    {
                        alreadyHasOngoingWave = true;
                        break;
                    }
                }
            }

            if (alreadyHasOngoingWave) EnablePlantAimShoot(true);
        }

        //This function is called in the Awake method of the PlantUnit that this script attached to
        public void InitializePlantAimShootSystem(PlantUnit plantUnit, PlantProjectileSO plantProjectileSO)
        {
            if (plantUnit == null || plantProjectileSO == null || plantProjectileSO.plantProjectilePrefab == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            this.plantProjectileSO = plantProjectileSO;

            plantProjectilePool = new PlantProjectilePool(plantUnit, this, plantProjectileSO, transform);

            plantProjectilePool.CreateAndAddInactivePlantProjectileToPool(30);

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

            bool returnSuccessful = plantProjectilePool.ReturnProjectileObjectToPool(projectile.gameObject);

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
        private void OnWaveStarted(WaveSpawner waveSpawner, int waveNum)
        {
            if(!enableAimShoot) EnablePlantAimShoot(true);
        }

        private void OnWaveFinished(WaveSpawner waveSpawner, int waveNum, bool stillHasOtherOngoingWaves)
        {
            if(!stillHasOtherOngoingWaves) EnablePlantAimShoot(false);
        }

        private void OnAllWaveSpawned(WaveSpawner waveSpawner, bool stillHasOtherOngoingWaves)
        {
            if (!stillHasOtherOngoingWaves) EnablePlantAimShoot(false);
        }
    }
}
