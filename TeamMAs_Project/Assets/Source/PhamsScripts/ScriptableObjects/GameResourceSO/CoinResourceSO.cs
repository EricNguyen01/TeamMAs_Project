// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "GameResource Data Asset/New Coin Game Resource")]
    public class CoinResourceSO : GameResourceSO
    {
        [field: SerializeField] [field: Min(0)] public int coinsGainOnWaveEnded { get; private set; } = 5;
    }
}
