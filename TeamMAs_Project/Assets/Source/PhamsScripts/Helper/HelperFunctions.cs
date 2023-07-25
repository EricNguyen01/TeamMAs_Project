// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public static class HelperFunctions
    {
        //.................Layer Mask Comparison..................
        public static bool IsMaskEqual(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        //.....................Randomly Shuffle A List..................................
        public static List<T> RandomShuffleListElements<T>(List<T> list)
        {
            if (list == null || list.Count == 0 || list.Count == 1) return list;

            for (int i = 0; i < list.Count; i++)
            {
                //determine the random slot index to swap with the current slot (starting from slot 0)
                int rand = Random.Range(0, 2);

                int temp = i;

                if (rand == 1) temp = (list.Count - 1) - i;

                //the data in the slot that to be swappped to the current slot
                T elementToSwap = list[temp];

                //set the random slot's data to the current slot's
                list[temp] = list[i];

                //set the current index slot's data with the data in the random shuffled slot
                list[i] = elementToSwap;
            }

            return list;
        }

        //.....................Randomly Shuffle An Array..................................
        public static T[] RandomShuffleArrayElements<T>(T[] array)
        {
            if (array == null || array.Length == 0 || array.Length == 1) return array;

            for (int i = 0; i < array.Length; i++)
            {
                //determine the random slot index to swap with the current slot (starting from slot 0)
                int rand = Random.Range(0, 2);

                int temp = i;

                if (rand == 1) temp = (array.Length - 1) - i;

                //the data in the slot that to be swappped to the current slot
                T elementToSwap = array[temp];

                //set the random slot's data to the current slot's
                array[temp] = array[i];

                //set the current index slot's data with the data in the random shuffled slot
                array[i] = elementToSwap;
            }

            return array;
        }

        //................Get The Look Rotation From A Position Towards Another..................................
        public static Quaternion GetRotationToPos2D(Vector3 fromPos, Vector3 toPos)
        {
            Vector3 dir = toPos - fromPos;

            dir.Normalize();

            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            return Quaternion.AngleAxis(-angle, Vector3.forward);
        }

        //................Cast A 2D Physics Raycast With Specific Params.........................................

        public static RaycastHit2D PerformSingleHit2DRaycastInDirection(Vector2 origin, Vector2 dir, float dist, string layerName)
        {
            RaycastHit2D hit2D = Physics2D.Raycast(origin, dir, dist, LayerMask.GetMask(layerName));

            return hit2D;
        }
    }
}
