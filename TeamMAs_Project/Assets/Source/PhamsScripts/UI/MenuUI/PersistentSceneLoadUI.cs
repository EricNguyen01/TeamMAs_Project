// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

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

        [SerializeField] private Canvas loadingScreenCanvas;

        [SerializeField] private UIFade loadingScreenUIFadeComponent;

        [SerializeField] private Slider loadingScreenSlider;

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

            if (loadingScreenCanvas) sceneLoadCanvas = loadingScreenCanvas;

            if (sceneLoadCanvas == null) sceneLoadCanvas = GetComponentInChildren<Canvas>(true);

            if (sceneLoadCanvas == null)
            {
                //Debug.LogWarning("PersistentSceneLoadUI is implemented but a scene load canvas is not assigned! Scene Load UI won't work!");
            }

            if(sceneLoadCanvas) sceneLoadCanvasGroup = sceneLoadCanvas.GetComponent<CanvasGroup>();

            if (!sceneLoadCanvasGroup && sceneLoadCanvas) sceneLoadCanvasGroup = sceneLoadCanvas.gameObject.AddComponent<CanvasGroup>();

            sceneLoadCanvasGroup.alpha = 0.0f;

            if(!loadingScreenUIFadeComponent) loadingScreenUIFadeComponent = sceneLoadCanvas.GetComponent<UIFade>();

            if(!loadingScreenUIFadeComponent) loadingScreenUIFadeComponent = sceneLoadCanvas.gameObject.AddComponent<UIFade>();

            loadingScreenUIFadeComponent.SetTweenExecuteMode(UITweenBase.UITweenExecuteMode.Internal);

            loadingScreenUIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeIn);

            loadingScreenUIFadeComponent.StopAndResetUITweenImmediate();

            if(!loadingScreenSlider) loadingScreenSlider = GetComponentInChildren<Slider>(true);

            if (loadingScreenSlider) 
            { 
                loadingScreenSlider.minValue = 0.0f; 
                
                loadingScreenSlider.maxValue = 1.0f; 
            }
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

        private void Start()
        {
            EnableSceneLoadUIImmediate(false);
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

            yield return StartCoroutine(EnableSceneLoadUISequence(true));

            if (performAdditionalTransitionTime && loadingScreenSlider) loadingScreenSlider.DOValue(UnityEngine.Random.Range(0.3f, 0.4f), additionalTransitionTime);

            if (performAdditionalTransitionTime) yield return new WaitForSecondsRealtime(additionalTransitionTime);

            if (loadingScreenSlider) loadingScreenSlider.DOValue(UnityEngine.Random.Range(0.7f, 0.8f), 0.5f);

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

            if (loadingScreenSlider)
            {
                if(performAdditionalTransitionTime) loadingScreenSlider.DOValue(1.0f, additionalTransitionTime + 0.1f);
                else yield return loadingScreenSlider.DOValue(1.0f, 0.3f).WaitForCompletion();
            }

            if (performAdditionalTransitionTime) yield return new WaitForSecondsRealtime(additionalTransitionTime);

            yield return StartCoroutine(EnableSceneLoadUISequence(false));

            if (loadingScreenSlider && loadingScreenSlider.value < 1.0f) loadingScreenSlider.value = 1.0f;

            isPerformingSceneLoad = false;
        }

        private void EnableSceneLoadUIImmediate(bool enabled)
        {
            if (!sceneLoadCanvas || !sceneLoadCanvasGroup) return;

            sceneLoadCanvasGroup.ignoreParentGroups = true;

            sceneLoadCanvasGroup.blocksRaycasts = true;

            if (enabled)
            {
                sceneLoadCanvas.gameObject.SetActive(true);

                sceneLoadCanvasGroup.alpha = 1.0f;

                return;
            }

            sceneLoadCanvasGroup.alpha = 0.0f;

            sceneLoadCanvas.gameObject.SetActive(false);
        }

        private IEnumerator EnableSceneLoadUISequence(bool enabled)
        {
            if (!sceneLoadCanvas || !sceneLoadCanvasGroup) yield break;

            sceneLoadCanvasGroup.ignoreParentGroups = true;

            sceneLoadCanvasGroup.blocksRaycasts = true;

            if (loadingScreenSlider) loadingScreenSlider.value = 0.0f;

            if (enabled)
            {
                sceneLoadCanvas.gameObject.SetActive(true);

                if (loadingScreenUIFadeComponent)
                {
                    loadingScreenUIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeIn);

                    loadingScreenUIFadeComponent.RunTweenInternal();
                }
                else sceneLoadCanvasGroup.alpha = 1.0f;

                yield break;
            }
            else
            {
                if (loadingScreenUIFadeComponent)
                {
                    loadingScreenUIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeOut);

                    loadingScreenUIFadeComponent.RunTweenInternal();
                }
                else sceneLoadCanvasGroup.alpha = 0.0f;

                if (loadingScreenUIFadeComponent) yield return new WaitForSecondsRealtime(loadingScreenUIFadeComponent.GetTweenDuration());

                loadingScreenUIFadeComponent.StopAndResetUITweenImmediate();

                sceneLoadCanvas.gameObject.SetActive(false);
            }

            yield break;
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
