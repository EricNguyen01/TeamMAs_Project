// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [Serializable]
    public class SaveDataSerializeBase
    {
        [SerializeField] protected object objectToSave;

        protected float posX, posY, posZ;

        protected string sceneNameSave;

        public SaveDataSerializeBase(object objectToSave, Vector3 posToSave, string sceneNameToSave)
        {
            this.objectToSave = objectToSave;

            posX = posToSave.x;

            posY = posToSave.y;

            posZ = posToSave.z;

            sceneNameSave = sceneNameToSave;
        }

        public object LoadSavedObject()
        {
            return objectToSave;
        }

        public Vector3 LoadPosition()
        {
            return new Vector3(posX, posY, posZ);
        }

        public string LoadSceneName()
        {
            return sceneNameSave;
        }
    }
}
