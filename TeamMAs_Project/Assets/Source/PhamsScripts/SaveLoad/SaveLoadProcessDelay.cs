// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamMAsTD
{
    [Serializable]
    public class SaveLoadProcessDelay
    {
        private SaveLoadHandler saveLoadHandler;

        private HashSet<Saveable> saveablesToSave = new HashSet<Saveable>();

        private HashSet<Saveable> saveablesToLoad = new HashSet<Saveable>();

        private bool wantsToSaveNow = false;

        private bool wantsToLoadNow = false;

        private bool isSavingSaveables = false;

        private bool isLoadingSaveables = false;

        public SaveLoadProcessDelay(SaveLoadHandler saveLoadHandlerInstance)
        {
            saveLoadHandler = saveLoadHandlerInstance;
        }

        public bool IsDelayingSave()
        {
            if (!wantsToSaveNow) return true;

            return false;
        }

        public void SaveDelayScheduled(Saveable saveable)
        {
            if (!saveable) return;

            if (saveLoadHandler == null) return;

            if (saveLoadHandler != SaveLoadHandler.saveLoadHandlerInstance) return;

            if (!saveablesToSave.Contains(saveable))
            {
                saveablesToSave.Add(saveable);
            }

            if (isSavingSaveables) return;

            SaveLoadHandler.saveLoadHandlerInstance.StartCoroutine(SaveDelay());
        }

        public void SaveDelayScheduled(Saveable[] saveables)
        {
            if (saveables == null || saveables.Length == 0) return;

            if (saveLoadHandler == null) return;

            if (saveLoadHandler != SaveLoadHandler.saveLoadHandlerInstance) return;

            for (int i = 0; i < saveables.Length; i++)
            {
                if (!saveables[i]) continue;

                if (!saveablesToSave.Contains(saveables[i]))
                {
                    saveablesToSave.Add(saveables[i]);
                }
            }

            if (isSavingSaveables) return;

            SaveLoadHandler.saveLoadHandlerInstance.StartCoroutine(SaveDelay());
        }

        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private IEnumerator SaveDelay()
        {
            if (saveablesToSave == null || saveablesToSave.Count == 0) yield break;

            if (isSavingSaveables) yield break;

            isSavingSaveables = true;
            
            yield return new WaitForSecondsRealtime(0.4f);

            yield return waitForEndOfFrame;

            if(isLoadingSaveables || wantsToLoadNow)
            {
                yield return new WaitUntil(() => (isLoadingSaveables == false && wantsToLoadNow == false));
            }

            wantsToSaveNow = true;

            SaveLoadHandler.SaveMultiSaveables(saveablesToSave.ToArray());

            saveablesToSave.Clear();

            wantsToSaveNow = false;

            isSavingSaveables = false;
        }

        public bool IsDelayingLoad()
        {
            if (!wantsToLoadNow) return true;

            return false;
        }

        public void LoadDelayScheduled(Saveable saveable)
        {
            if (!saveable) return;

            if (saveLoadHandler == null) return;

            if (saveLoadHandler != SaveLoadHandler.saveLoadHandlerInstance) return;

            if (!saveablesToLoad.Contains(saveable))
            {
                saveablesToLoad.Add(saveable);
            }

            if (isLoadingSaveables) return;

            SaveLoadHandler.saveLoadHandlerInstance.StartCoroutine(LoadDelay());
        }

        public void LoadDelayScheduled(Saveable[] saveables)
        {
            if (saveables == null || saveables.Length == 0) return;

            if (saveLoadHandler == null) return;

            if (saveLoadHandler != SaveLoadHandler.saveLoadHandlerInstance) return;

            for (int i = 0; i < saveables.Length; i++)
            {
                if (!saveables[i]) continue;

                if (!saveablesToLoad.Contains(saveables[i]))
                {
                    saveablesToLoad.Add(saveables[i]);
                }
            }

            if (isLoadingSaveables) return;

            SaveLoadHandler.saveLoadHandlerInstance.StartCoroutine(LoadDelay());
        }

        private IEnumerator LoadDelay()
        {
            if (saveablesToLoad == null || saveablesToLoad.Count == 0) yield break;

            if (isLoadingSaveables) yield break;

            isLoadingSaveables = true;

            yield return new WaitForSecondsRealtime(0.4f);

            yield return waitForEndOfFrame;

            if (isSavingSaveables || wantsToSaveNow)
            {
                yield return new WaitUntil(() => (isSavingSaveables == false && wantsToSaveNow == false));
            }

            wantsToLoadNow = true;

            SaveLoadHandler.LoadMultiSaveables(saveablesToLoad.ToArray());

            saveablesToLoad.Clear();

            wantsToLoadNow = false;

            isLoadingSaveables = false;
        }
    }
}
