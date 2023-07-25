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
        [field: SerializeField] public GameObject plantProjectilePrefab { get; private set; }

        [field: Header("Plant Projectile Stats")]
        [field: SerializeField] public float plantProjectileSpeed { get; private set; } = 100.0f;
        [field: SerializeField] public bool isHoming { get; private set; } = false;
    }
}
