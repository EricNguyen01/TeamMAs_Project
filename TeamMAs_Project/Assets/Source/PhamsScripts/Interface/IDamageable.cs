using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public interface IDamageable
    {
        public VisitorUnit TakeDamageFrom(object damageCauser, float damage);
    }
}
