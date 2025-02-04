// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    public class PlantProjectilePool : TD_GameObjectPoolBase
    {
        private PlantProjectileSO plantProjectileSO;

        private PlantUnit plantUnit;

        private PlantAimShootSystem plantAimShoot;

        private Transform projectileParentTransform;

        public List<PlantProjectile> plantProjectileList { get; private set; } = new List<PlantProjectile>();

        //PlantProjectilePool's constructor
        public PlantProjectilePool(PlantUnit plantUnit, PlantAimShootSystem plantAimShootSystem, PlantProjectileSO plantProjectileSO, Transform plantObjectTransform) : base(plantAimShootSystem, plantProjectileSO.plantProjectilePrefab, plantObjectTransform)
        {
            if(plantProjectileSO == null)
            {
                Debug.LogError("Projectile Pool spawned by PlantAimShoot: " + plantAimShootSystem.name + " received no plant projectile ScriptableObject data!");
                return;
            }

            this.plantUnit = plantUnit;

            plantAimShoot = plantAimShootSystem;

            this.plantProjectileSO = plantProjectileSO;

            projectileParentTransform = plantObjectTransform;
        }

        //Override CreateAndAddToPool in TD_GameObjectPoolBase
        protected override bool CreateAndAddToPool(GameObject objectToPool, int numberToPool, Transform transformCarriesPool, bool setInactive)
        {
            bool addedSuccessful = base.CreateAndAddToPool(objectToPool, numberToPool, transformCarriesPool, setInactive);

            if (!addedSuccessful) return addedSuccessful;

            if(plantUnit == null)
            {
                Debug.LogError("The Plant Unit linked to this plant projectile pool's Plant Aim Shoot System: " + plantAimShoot.name + " is null!");
                return false;
            }

            //initialize only the newly added plant projectile to pool (starting from the previous pool count up to the newly increased count)
            //only check for newly added projectile so that we don't have to loop through the whole pool everytime we add a new plant projectile
            for (int i = gameObjectsPool.Count - numberToPool; i < gameObjectsPool.Count; i++)
            {
                PlantProjectile plantProjectileComp = gameObjectsPool[i].GetComponent<PlantProjectile>();

                if (plantProjectileComp == null)
                {
                    Debug.LogError("Plant Projectile GameObject prefab of PlantProjectileSO: " + plantProjectileSO.name +
                    " has no PlantProjectile script component attached! Pooling of this visitor obj failed!");
                    continue;
                }

                plantProjectileComp.InitializePlantProjectile(plantUnit);

                if(!plantProjectileList.Contains(plantProjectileComp)) plantProjectileList.Add(plantProjectileComp);
            }

            return true;
        }

        public bool CreateAndAddInactivePlantProjectileToPool(int numberToPool)
        {
            //Calls this child class' modified version of the base version of this function
            return CreateAndAddToPool(plantProjectileSO.plantProjectilePrefab, numberToPool, projectileParentTransform, true);
        }

        public GameObject GetInactiveProjectileObjectFromPool()
        {
            return GetInactiveGameObjectFromPool();
        }

        public bool ReturnProjectileObjectToPool(GameObject projectileGO)
        {
            return ReturnGameObjectToPool(projectileGO);
        }

        public void UpdatePlantProjectilePoolData(PlantUnit plantUnit, PlantProjectileSO projectileSO)
        {
            if (plantUnit == null || plantUnit.plantAimShootSystem == null || projectileSO == null) return;

            DecommisionOldProjectiles();

            this.plantUnit = plantUnit;

            plantAimShoot = plantUnit.plantAimShootSystem;

            plantProjectileSO = projectileSO;

            CreateAndAddInactivePlantProjectileToPool(15);
        }

        private void DecommisionOldProjectiles()
        {
            if (plantUnit == null) return;

            if (plantProjectileList.Count == 0) return;

            for(int i = 0; i < plantProjectileList.Count; i++)
            {
                if (plantProjectileList[i] == null) continue;

                if (!plantProjectileList[i].gameObject.activeInHierarchy)
                {
                    MonoBehaviour.Destroy(plantProjectileList[i].gameObject);

                    continue;
                }

                RemoveGameObjectFromPool(plantProjectileList[i].gameObject);

                plantProjectileList[i].Completely_UnAssociate_Projectile_WithItsPlant_AndPool();
            }
        }
    }
}
