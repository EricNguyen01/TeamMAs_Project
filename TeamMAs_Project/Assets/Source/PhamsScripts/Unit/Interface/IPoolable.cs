using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface IPoolable
    {
        public MonoBehaviour GetScriptCarriesThisIPoolable();
        public object GetPoolableType();
        public GameObject GetPoolablePrefabGameObject();
    }
}
