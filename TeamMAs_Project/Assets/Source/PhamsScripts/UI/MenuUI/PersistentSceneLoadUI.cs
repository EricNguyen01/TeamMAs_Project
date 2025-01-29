// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

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

        //[SerializeField] private float transitionStartDelay = 0.0f;

        private bool performAdditionalTransitionTime = true;

        [SerializeField]
        [DisableIf("performAdditionalTransitionTime", false)]
        private float additionalTransitionTimeToGame = 4.0f;

        [SerializeField]
        [DisableIf("performAdditionalTransitionTime", false)]
        private float additionalTransitionTimeToMenu = 1.5f;

        //INTERNALS..............................................................

        public static PersistentSceneLoadUI persistentSceneLoadUIInstance;

        private Canvas sceneLoadCanvas;

        private CanvasGroup sceneLoadCanvasGroup;

        private bool loadSaveAfterScene = false;

        private bool isLoadingSavedData = false;

        private bool isPerformingSceneLoad = false;

        private bool isLoadingBarTweening = false;

        private void Awake()
        {
            if (persistentSceneLoadUIInstance && persistentSceneLoadUIInstance != this)
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
            isLoadingBarTweening = false;

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
            if (isPerformingSceneLoad) return;

            SceneTransitionToScene(sceneNameToLoad);
        }

        public void LoadScene(int sceneNumToLoad)
        {
            if (isPerformingSceneLoad) return;

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
            if (isPerformingSceneLoad) yield break;

            MemoryUsageLogger.LogMemoryUsageAsText("SceneTransitionStarted");

            isPerformingSceneLoad = true;

            isLoadingBarTweening = false;

            foreach (Button button in FindObjectsOfType<Button>())
            {
                if (!button) continue;

                button.interactable = false;
            }

            float additionalTransitionTime = 2.0f;

            yield return StartCoroutine(EnableSceneLoadUISequence(true));

            if (performAdditionalTransitionTime && loadingScreenSlider)
            {
                StartCoroutine(LoadingScreenBarSliderCoroutine(UnityEngine.Random.Range(0.23f, 0.35f), additionalTransitionTime));
            }

            if (performAdditionalTransitionTime) yield return new WaitForSecondsRealtime(additionalTransitionTime);

            yield return new WaitUntil(() => isLoadingBarTweening == false);

            if (loadingScreenSlider) 
            {
                StartCoroutine(LoadingScreenBarSliderCoroutine(UnityEngine.Random.Range(0.76f, 0.9f), 3.0f));

                //loadingScreenSlider.DOValue(UnityEngine.Random.Range(0.8f, 0.9f), 0.8f).SetUpdate(true); 
            }

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

            Time.timeScale = 1.0f;

            yield return new WaitForSecondsRealtime(0.1f);

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

            yield return new WaitUntil(() => isLoadingBarTweening == false);

            if (SceneManager.GetActiveScene().name.Contains("Game")) additionalTransitionTime = additionalTransitionTimeToGame;
            else if (SceneManager.GetActiveScene().name.Contains("Menu")) additionalTransitionTime = additionalTransitionTimeToMenu;

            if (loadingScreenSlider)
            {
                if (performAdditionalTransitionTime)
                {
                    StartCoroutine(LoadingScreenBarSliderCoroutine(1.0f, additionalTransitionTime));

                    //loadingScreenSlider.DOValue(1.0f, additionalTransitionTime).SetUpdate(true);
                }
                else 
                {
                    yield return StartCoroutine(LoadingScreenBarSliderCoroutine(1.0f, 1.0f));

                    //yield return loadingScreenSlider.DOValue(1.0f, 0.35f).SetUpdate(true).WaitForCompletion(); 
                }
            }

            if (performAdditionalTransitionTime) yield return new WaitForSecondsRealtime(additionalTransitionTime);

            yield return new WaitUntil(() => isLoadingBarTweening == false);

            yield return StartCoroutine(EnableSceneLoadUISequence(false));

            isPerformingSceneLoad = false;

            MemoryUsageLogger.LogMemoryUsageAsText("SceneTransitionFinished");
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

            if (enabled)
            {
                if (loadingScreenSlider) loadingScreenSlider.value = 0.0f;

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

                if (loadingScreenSlider && loadingScreenSlider.value < 1.0f) loadingScreenSlider.value = 1.0f;

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

        private IEnumerator LoadingScreenBarSliderCoroutine(float endVal, float duration)
        {
            if (!loadingScreenSlider)
            {
                isLoadingBarTweening = false;

                yield break;
            }

            if(loadingScreenSlider.value >= endVal)
            {
                isLoadingBarTweening = false;

                yield break;
            }

            isLoadingBarTweening = true;

            yield return loadingScreenSlider.DOValue(endVal, duration).SetUpdate(true).WaitForCompletion();

            isLoadingBarTweening = false;

            yield break;
        }

        public static void CreatePersistentSceneLoadUIInstance()
        {
            if (persistentSceneLoadUIInstance) return;

            if (FindObjectOfType<PersistentSceneLoadUI>()) return;

            GameObject go = new GameObject("PersistentSceneLoad(1InstanceOnly)");

            PersistentSceneLoadUI pSceneLoad = go.AddComponent<PersistentSceneLoadUI>();

            if (!persistentSceneLoadUIInstance) persistentSceneLoadUIInstance = pSceneLoad;
        }
    }
}
