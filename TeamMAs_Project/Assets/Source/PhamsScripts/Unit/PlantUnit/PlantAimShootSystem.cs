using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlantUnit))]
    public class PlantAimShootSystem : MonoBehaviour
    {
        private PlantUnit plantUnitLinked;

        private PlantProjectileSO plantProjectileSO;

        private List<PlantProjectile> plantProjectilesPool = new List<PlantProjectile>();

        private VisitorUnit currentVisitorTargetted;

        private void Awake()
        {
            AddPlantProjectileToPool(30, true);
        }

        private void AddPlantProjectileToPool(int numberToAdd, bool setInactive)
        {
            if (plantProjectileSO == null || plantProjectileSO.plantProjectilePrefab == null) return;

            if (numberToAdd <= 0) return;

            for(int i = 0; i < numberToAdd; i++)
            {
                GameObject projectileGO = Instantiate(plantProjectileSO.plantProjectilePrefab, Vector2.zero, Quaternion.Euler(Vector3.zero), transform);
                PlantProjectile plantProjectileComp = projectileGO.GetComponent<PlantProjectile>();

                if(plantProjectileComp == null)
                {
                    Debug.LogError("Plant Projectile GameObject prefab of PlantProjectileSO: " + plantProjectileSO.name +
                    " has no PlantProjectile script component attached! Pooling of this visitor obj failed!");
                    continue;
                }

                plantProjectilesPool.Add(plantProjectileComp);

                plantProjectileComp.InitializePlantProjectile(plantUnitLinked);

                if (setInactive) projectileGO.SetActive(false);
            }
        }

        private Quaternion CalculateProjectileRotateTowardsTarget()
        {
            return Quaternion.identity;
        }

        public void InitializePlantAimShootSystem(PlantUnit plantUnit, PlantProjectileSO plantProjectileSO)
        {
            if (plantUnit == null || plantProjectileSO == null || plantProjectileSO.plantProjectilePrefab == null)
            {
                enabled = false;
                return;
            }

            plantUnitLinked = plantUnit;

            this.plantProjectileSO = plantProjectileSO;
        }
    }
}
