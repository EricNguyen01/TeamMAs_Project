// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        [SerializeField] private bool loadSaveAfterScene = false;

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

        public void LoadDefaultScene()
        {
            Scene defaultSc = SceneManager.GetSceneByBuildIndex(DEFAULT_SCENE_BUILD_INDEX);

            if (defaultSc == null)
            {
                Debug.LogWarning("Default Scene Not Found! No Scene Will Be Loaded.");

                return;
            }

            LoadScene(defaultSc.name);
        }

        public void LoadFirstGameScene()
        {
            Scene defaultSc = SceneManager.GetSceneByBuildIndex(FIRST_GAME_SCENE_BUILD_INDEX);

            if (defaultSc == null)
            {
                Debug.LogWarning("First Game Scene Not Found! Load Default Scene Instead!");

                LoadDefaultScene();

                return;
            }

            LoadScene(defaultSc.name);
        }

        public void LoadScene(string sceneToLoad)
        {
            SceneTransitionToScene(sceneToLoad);
        }

        public void AllowLoadSaveAfterSceneLoaded(bool allowedSaveToLoad)
        {
            loadSaveAfterScene = allowedSaveToLoad;
        }

        private void SceneTransitionToScene(string sceneTo)
        {
            if (string.IsNullOrEmpty(sceneTo) ||
                string.IsNullOrWhiteSpace(sceneTo) ||
                SceneManager.GetSceneByName(sceneTo) == null)
            {
                Debug.LogWarning("Trying To Transition To Scene: " + sceneTo + "But Scene Does Not Exist!\n" +
                "Loading Default Scene Instead.");

                LoadDefaultScene();

                return;
            }

            StartCoroutine(SceneTransitionCoroutine(sceneTo));
        }

        private IEnumerator SceneTransitionCoroutine(string sceneTo)
        {
            EnableSceneLoadUI(true);

            yield return SceneManager.LoadSceneAsync(sceneTo);

            PixelCrushers.DialogueSystem.DialogueManager.Pause();

            if (SaveLoadHandler.saveLoadHandlerInstance && loadSaveAfterScene)
            {
                if (SaveLoadHandler.LoadToAllSaveables()) 
                { 
                    yield return StartCoroutine(SceneSavedDataLoadCheckIntervalCoroutine()); 
                }
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
