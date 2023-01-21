using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface IDamageable
    {
        public object ObjectTakenDamage();
        public void TakeDamageFrom(object damageCauser, float damage);
    }
}
