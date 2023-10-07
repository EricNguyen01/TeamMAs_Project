// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PersistentSceneLoadUI : MonoBehaviour
    {
        [Header("Scene Transition Settings")]

        //default scene (build index 0) should always be menu scene
        private const int DEFAULT_SCENE_BUILD_INDEX = 0;

        //first game scene shoud be the scene after menu scene or load transition scene (if has one) in the build index
        private const int FIRST_GAME_SCENE_BUILD_INDEX = 1;

        [SerializeField] private float additionalTransitionTime = 1.0f;

        [SerializeField] private bool performAdditionalTransitionTime = false;

        //INTERNALS..............................................................

        public static PersistentSceneLoadUI persistentSceneLoadUIInstance;

        private Canvas sceneLoadCanvas;

        private CanvasGroup sceneLoadCanvasGroup;

        private bool loadSaveAfterScene = false;

        private bool isLoadingSavedData = false;

        private bool isPerformingSceneLoad = false;

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

        public void LoadDefaultScene()
        {
            LoadScene(DEFAULT_SCENE_BUILD_INDEX);
        }

        public void LoadFirstGameScene()
        {
            LoadScene(FIRST_GAME_SCENE_BUILD_INDEX);
        }

        public void LoadScene(string sceneNameToLoad)
        {
            SceneTransitionToScene(sceneNameToLoad);
        }

        public void LoadScene(int sceneNumToLoad)
        {
            SceneTransitionToScene(sceneNumToLoad);
        }

        public void AllowLoadSaveAfterSceneLoaded(bool allowedSaveToLoad)
        {
            loadSaveAfterScene = allowedSaveToLoad;
        }

        public bool IsPerformingSceneLoad()
        {
            return isPerformingSceneLoad;
        }

        private void SceneTransitionToScene(string sceneNameTo)
        {
            if (string.IsNullOrEmpty(sceneNameTo) ||
                string.IsNullOrWhiteSpace(sceneNameTo) ||
                sceneNameTo == null)
            {
                Debug.LogWarning("Trying To Transition To Scene: " + sceneNameTo + "But Scene Does Not Exist!\n" +
                "Loading Default Scene Instead.");

                LoadDefaultScene();

                return;
            }

            StartCoroutine(SceneTransitionCoroutine(sceneNameTo, -1));
        }

        private void SceneTransitionToScene(int sceneNumToLoad)
        {
            StartCoroutine(SceneTransitionCoroutine("", sceneNumToLoad));
        }

        private IEnumerator SceneTransitionCoroutine(string sceneNameTo = "", int sceneNumTo = -1)
        {
            isPerformingSceneLoad = true;

            EnableSceneLoadUI(true);

            if (!string.IsNullOrEmpty(sceneNameTo) &&
                !string.IsNullOrWhiteSpace(sceneNameTo) &&
                sceneNameTo != null && sceneNameTo != "")
            {
                yield return SceneManager.LoadSceneAsync(sceneNameTo, LoadSceneMode.Single);
            }
            else if(sceneNumTo >= 0)
            {
                yield return SceneManager.LoadSceneAsync(sceneNumTo, LoadSceneMode.Single);
            }

            if (SaveLoadHandler.saveLoadHandlerInstance && SaveLoadHandler.HasSavedData() && loadSaveAfterScene)
            {
                if (PixelCrushers.DialogueSystem.DialogueManager.IsConversationActive)
                {
                    PixelCrushers.DialogueSystem.DialogueManager.StopAllConversations();
                }

                if (SaveLoadHandler.LoadToAllSaveables()) 
                { 
                    yield return StartCoroutine(SceneSavedDataLoadCheckIntervalCoroutine()); 
                }
            }

            if(performAdditionalTransitionTime) yield return new WaitForSeconds(additionalTransitionTime);

            EnableSceneLoadUI(false);

            isPerformingSceneLoad = false;
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
