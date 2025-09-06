// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        private float baseTargetsUpdateWaitTime = 0.05f;

        private float currentTargetsUpdateWaitTime = 0.0f;

        private List<VisitorUnit> visitorsInRangeList = new List<VisitorUnit>();

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

            //attempt to calculate and get targetted visitor's look ahead position
            Vector2 targetLookAheadPos = LookAheadTargetPosition(target);

            Vector2 dirToTarget = Vector2.zero;

            //if there's a viable target look ahead position -> use it to calculate dirToTarget
            if (targetLookAheadPos != Vector2.zero)
            {
                //Debug.Log("Look Ahead Aiming!");
                dirToTarget = targetLookAheadPos - (Vector2)projectileObj.transform.position;
            }
            //else use the target's position itself to calculate dirToTarget
            else 
            { 
                dirToTarget = (Vector2)target.transform.position - (Vector2)projectileObj.transform.position; 
            }

            dirToTarget.Normalize();

            return Quaternion.FromToRotation(projectileObj.transform.up, dirToTarget);
        }

        private void ProcessPlantAimShoot()
        {
            if(currentShootWaitTime <= 0.0f)
            {
                currentShootWaitTime = 0.0f;

                if (visitorsInRangeList == null || visitorsInRangeList.Count == 0) return;

                bool shootSuccessful = PlantPeformsAimShoot();

                //only reset attack timer (atk spd) if shooting was succesful
                if(shootSuccessful) currentShootWaitTime = plantUnitLinked.plantUnitScriptableObject.attackSpeed;

                return;
            }

            currentShootWaitTime -= Time.deltaTime;
        }

        private bool PlantPeformsAimShoot()
        {
            //doing some checks to see if there is one or more valid visitors to shoot
            
            //for 1 visitor target only:
            //dont shoot if the visitor target in target list is null, dead, or inactive in scene
            //if such situation occurs, immediately update target list and reset its update timer
            if(visitorTargetsList.Count == 1)
            {
                if (visitorTargetsList[0] == null ||
                    visitorTargetsList[0].currentVisitorHealth <= 0.0f ||
                    !visitorTargetsList[0].gameObject.activeInHierarchy)
                {
                    currentTargetsUpdateWaitTime = 0.0f;

                    return false;
                }
            }
            //for multiple visitors target only:
            //if there's at least 1 valid target -> can proceed to shoot
            //else can't shoot, return false, and update target list + reset timer
            else if(visitorTargetsList.Count > 1)
            {
                int validTargetCount = visitorTargetsList.Count;

                for (int i = 0; i < visitorTargetsList.Count; i++)
                {
                    if (visitorTargetsList[i] == null ||
                        visitorTargetsList[i].currentVisitorHealth <= 0.0f ||
                        !visitorTargetsList[i].gameObject.activeInHierarchy)
                    {
                        validTargetCount--;
                    }
                }

                if(validTargetCount <= 0)
                {
                    currentTargetsUpdateWaitTime = 0.0f;

                    return false;
                }
            }

            //else if all targets in target list are valid -> performs aim shoot

            for (int i = 0; i < visitorTargetsList.Count; i++)
            {
                //this if check is
                //in case the number of visitors in range or in scene are less than the number of visitors this plant can multi-target
                //projectiles will not be launched for any null target list element
                if (visitorTargetsList[i] == null ||
                    visitorTargetsList[i].currentVisitorHealth <= 0.0f ||
                    !visitorTargetsList[i].gameObject.activeInHierarchy) continue;

                GameObject projectileGO = plantProjectilePool.GetInactiveProjectileObjectFromPool();

                projectileGO.transform.rotation = CalculateProjectileRotatesTowardsTarget(projectileGO, visitorTargetsList[i]);

                //get the plant projectile script component attached to the projectile game object just taken from projectile pool
                PlantProjectile plantProjectile; projectileGO.TryGetComponent<PlantProjectile>(out plantProjectile);

                //set the visitor target data for the plant projectile script if one is found
                if (plantProjectile) plantProjectile.SetTargettedVisitorUnit(visitorTargetsList[i]);

                if (!projectileGO.activeInHierarchy) projectileGO.SetActive(true);

                //Debug.Log("Projectile: " + projectileGO.name + " successfully launched from Plant: " + name + ".");
            }

            return true;
        }

        private void UpdateVisitorTargetsListInUpdate()
        {
            if (currentTargetsUpdateWaitTime <= 0.0f)
            {
                currentTargetsUpdateWaitTime = 0.0f;

                UpdateVisitorTargetsList();//update timer is reset in this function

                return;
            }

            currentTargetsUpdateWaitTime -= Time.deltaTime;
        }

        //This function immediately updates visitor targets list and reset the update timer
        //use in OnVisitorAppeased event to re-update targets list immediately because a visitor just dissapeared in scene
        //also use in update visitor targets list in update after update timer has finished
        private void UpdateVisitorTargetsList()
        {
            visitorsInRangeList = GetVisitorsInRange();

            UpdateVisitorTargetsList(visitorsInRangeList);

            currentTargetsUpdateWaitTime = baseTargetsUpdateWaitTime;
        }

        private Collider2D[] collidersInRange = new Collider2D[100];//watch this arr's length
        private List<VisitorUnit> GetVisitorsInRange()
        {
            int colliderHitsCount = Physics2D.OverlapCircleNonAlloc(transform.position, 
                                                                   plantUnitLinked.plantMaxAttackRange, 
                                                                   collidersInRange, 
                                                                   LayerMask.GetMask(plantUnitLinked.plantUnitScriptableObject.plantTargetsLayerNames));
            
            List<VisitorUnit> visitorsInRange = null;

            if (collidersInRange.Length == 0 || colliderHitsCount == 0) return visitorsInRange;

            visitorsInRange = new List<VisitorUnit>();

            for (int i = 0; i < colliderHitsCount; i++)
            {
                if (!collidersInRange[i]) continue;

                VisitorUnit visitorUnit = collidersInRange[i].GetComponent<VisitorUnit>();

                if (visitorUnit == null) continue;

                if (!visitorUnit.gameObject.activeInHierarchy) continue;

                if (visitorUnit.currentVisitorHealth <= 0.0f) continue;

                //dont register the current visitor as in range if it is not the specific type that this plant can target
                if(plantUnitLinked != null && plantUnitLinked.plantUnitScriptableObject.plantTargetsSpecifically != VisitorUnitSO.VisitorType.None)
                {
                    if (plantUnitLinked.plantUnitScriptableObject.plantTargetsSpecifically != visitorUnit.visitorUnitSO.visitorType) continue;
                }

                visitorsInRange.Add(visitorUnit);
            }

            return visitorsInRange;
        }

        private void UpdateVisitorTargetsList(List<VisitorUnit> visitorsInRange)
        {
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
                if(i < visitorsInRange.Count)
                {
                    visitorTargetsList[i] = visitorsInRange[i];
                }
                else
                {
                    visitorTargetsList[i] = null;
                }
            }
        }

        //calculate where the targetted visitor unit will be based on plant projectile speed
        private Vector2 LookAheadTargetPosition(VisitorUnit targettedVisitor)
        {
            if (targettedVisitor == null || targettedVisitor.visitorUnitSO == null) return Vector2.zero;

            if (plantProjectileSO == null || plantProjectileSO.plantProjectileSpeed == 0) return Vector2.zero;

            //dont look ahead if projectile is of homing type
            if(plantProjectileSO.isHoming) return Vector2.zero;

            //get distance from this plant unit to targetted visitor unit
            float distToTarget = Vector2.Distance((Vector2)targettedVisitor.transform.position, (Vector2)transform.position);

            //dist/time = spd so time = dist/spd
            float timeTakenToTarget = distToTarget / plantProjectileSO.plantProjectileSpeed;

            //calculate where the target will be in its current movement direction after the above "timeTakenToTarget" value
            //dist/time = spd so dist = spd * time
            float targetLookAheadDist = targettedVisitor.visitorUnitSO.moveSpeed * timeTakenToTarget;

            //look ahead pos = target current pos + target current move dir(normalized) * look ahead distance
            Vector2 targetLookAheadPos = (Vector2)targettedVisitor.transform.position + targettedVisitor.GetVisitorUnitMoveDir(true) * targetLookAheadDist;

            //Debug.Log("VisitorUnit: " + targettedVisitor.name + " (" + targettedVisitor.GetInstanceID() + ") Look Ahead Pos: " + targetLookAheadPos);

            return targetLookAheadPos;
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
                foreach (WaveSpawner waveSpawner in FindObjectsByType<WaveSpawner>(FindObjectsSortMode.None))
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
            EnablePlantAimShoot(false);

            if (plantUnit == null || plantProjectileSO == null || plantProjectileSO.plantProjectilePrefab == null)
            {
                Debug.LogError("PlantAimShootSystem on PlantUnit: " + name + " is missing vital references upon initialization. Disabling script...");

                enabled = false;

                return;
            }

            plantUnitLinked = plantUnit;

            this.plantProjectileSO = plantProjectileSO;

            if(plantProjectilePool == null)
            {
                plantProjectilePool = new PlantProjectilePool(plantUnit, this, plantProjectileSO, transform);

                plantProjectilePool.CreateAndAddInactivePlantProjectileToPool(15);
            }
            else
            {
                plantProjectilePool.UpdatePlantProjectilePoolData(plantUnit, plantProjectileSO);
            }

            InitializeNullTargetsList();//no targets at first so target list only contains nulls

            EnablePlantAimShoot(true);
        }

        private void InitializeNullTargetsList()//only use this on intialization!
        {
            if (visitorTargetsList.Count > 0) return;

            for(int i = 0; i < plantUnitLinked.plantUnitScriptableObject.targetsPerAttack; i++)
            {
                visitorTargetsList.Add(null);
            }
        }

        public void ReturnProjectileToPool(PlantProjectile projectile)
        {
            if (projectile == null) return;

            bool returnSuccessful = plantProjectilePool.ReturnProjectileObjectToPool(projectile.gameObject);

            //if(returnSuccessful) Debug.Log("Plant projectile successfully returned to pool!");
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
