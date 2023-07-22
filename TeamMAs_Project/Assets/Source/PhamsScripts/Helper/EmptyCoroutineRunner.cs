using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    /*
     * An empty class that acts as a MonoBehaviour (since MonoBehaviour cant be added directly to game object).
     * Use by other classes to run their Coroutine functions externally when needed
     * (so that even if the class is destroyed or disabled, the already ran Coroutine can still run)
     * Other classes can create an empty gameobject on awake and attach this empty script and then call Coroutine functions from this class reference
     */
    [DisallowMultipleComponent]
    public class EmptyCoroutineRunner : MonoBehaviour
    {

    }
}
