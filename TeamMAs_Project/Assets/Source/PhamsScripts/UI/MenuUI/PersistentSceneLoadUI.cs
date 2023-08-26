// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    public class PersistentSceneLoadUI : MonoBehaviour
    {
        [Header("Scene Transition Settings")]

        [SerializeField] private const string DEFAULT_SCENE_TO_LOAD = "GameScene";

        [SerializeField] private float additionalTransitionTime = 1.0f;

        //INTERNALS..............................................................

        public static PersistentSceneLoadUI persistentSceneLoadUIInstance;

        private Canvas sceneLoadCanvas;

        private CanvasGroup sceneLoadCanvasGroup;

        private bool isLoadingSavedData = false;

        private void Awake()
        {
            if (persistentSceneLoadUIInstance != null)
            {
                Destroy(gameObject);
                return;
            }

            persistentSceneLoadUIInstance = this;

            DontDestroyOnLoad(gameObject);

            sceneLoadCanvas = GetComponent<Canvas>();

            if (sceneLoadCanvas == null) sceneLoadCanvas = GetComponentInChildren<Canvas>();

            if (sceneLoadCanvas == null)
            {
                //Debug.LogWarning("PersistentSceneLoadUI is implemented but a scene load canvas is not assigned! Scene Load UI won't work!");

            }

            if(sceneLoadCanvas) sceneLoadCanvasGroup = sceneLoadCanvas.GetComponent<CanvasGroup>();

            if (sceneLoadCanvasGroup == null && sceneLoadCanvas) sceneLoadCanvasGroup = sceneLoadCanvas.gameObject.AddComponent<CanvasGroup>();

            EnableSceneLoadUI(false);
        }

        private void OnEnable()
        {
            SaveLoadHandler.OnLoadingStarted += () => isLoadingSavedData = true;

            SaveLoadHandler.OnLoadingFinished += () => isLoadingSavedData = false;
        }

        private void OnDisable()
        {
            SaveLoadHandler.OnLoadingStarted -= () => isLoadingSavedData = true;

            SaveLoadHandler.OnLoadingFinished -= () => isLoadingSavedData = false;
        }

        public void SceneTransitionToScene(string sceneTo)
        {
            if (SceneManager.GetSceneByName(sceneTo) == null)
            {
                Debug.LogWarning("Trying To Transition To Scene: " + sceneTo + "But Scene Does Not Exist!");

                return;
            }

            StartCoroutine(SceneTransitionCoroutine(sceneTo));
        }

        private IEnumerator SceneTransitionCoroutine(string sceneTo)
        {
            EnableSceneLoadUI(true);

            yield return SceneManager.LoadSceneAsync(sceneTo);

            PixelCrushers.DialogueSystem.DialogueManager.Pause();

            if (SaveLoadHandler.LoadToAllSaveables())
            {
                yield return StartCoroutine(SceneSavedDataLoadCheckIntervalCoroutine());
            }

            yield return new WaitForSeconds(additionalTransitionTime);

            PixelCrushers.DialogueSystem.DialogueManager.Unpause();

            EnableSceneLoadUI(false);
        }

        private void EnableSceneLoadUI(bool enabled)
        {
            if (!sceneLoadCanvas || !sceneLoadCanvasGroup) return;

            sceneLoadCanvasGroup.ignoreParentGroups = true;

            sceneLoadCanvasGroup.interactable = false;

            if (enabled)
            {
                sceneLoadCanvasGroup.alpha = 1.0f;

                sceneLoadCanvasGroup.blocksRaycasts = true;

                return;
            }

            sceneLoadCanvasGroup.alpha = 0.0f;

            sceneLoadCanvasGroup.blocksRaycasts = false;
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();//cache wait for fixed update
        private IEnumerator SceneSavedDataLoadCheckIntervalCoroutine(float intervalDuration = 1.0f)
        {
            if (intervalDuration <= 0.0f) yield break;

            float t = 0.0f;

            while(t <= intervalDuration)
            {
                t += Time.fixedDeltaTime;

                if (isLoadingSavedData) t = 0.0f;

                yield return waitForFixedUpdate;
            }

            yield break;
        }
    }
}
