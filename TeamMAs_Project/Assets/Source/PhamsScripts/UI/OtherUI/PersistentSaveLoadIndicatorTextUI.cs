// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PersistentSaveLoadIndicatorTextUI : MonoBehaviour
    {
        [SerializeField] private string savingIndicatorText = "Saving";

        [SerializeField] private string loadingIndicatorText = "Loading";

        [SerializeField] private TMP_FontAsset textFont;

        [SerializeField] private Color textColor;

        [SerializeField] private float textSize = 0.5f;

        [SerializeField] private bool constructSaveLoadIndicatorObject = true;

        [SerializeField] private PauseGameUIButton pauseGameUIButton;

        //INTERNALS.........................................................................................

        private GameObject saveLoadIndicatorObj;

        private TextMeshProUGUI saveLoadIndicatorTextMeshComp;

        private Canvas canvas;

        private CanvasGroup saveLoadIndicatorObjCanvasGroup;

        private UIFade UI_Fade;

        private bool alreadyDisplayingText = false;

        private bool isDisablingSaveLoadTextDelay = false;

        private bool isDisablingPauseGameUIButton = false;

        private static PersistentSaveLoadIndicatorTextUI saveLoadIndicatorTextInstance;

        private void Awake()
        {
            if (!saveLoadIndicatorTextInstance)
            {
                saveLoadIndicatorTextInstance = this;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);

                return;
            }

            TryGetComponent<Canvas>(out canvas);

            if(!canvas) canvas = gameObject.AddComponent<Canvas>();

            if(canvas.renderMode != RenderMode.WorldSpace) canvas.renderMode = RenderMode.WorldSpace;

            if(!canvas.worldCamera) canvas.worldCamera = Camera.main;

            ConstructSaveLoadIndicatorObject();

            if(!pauseGameUIButton) pauseGameUIButton = FindObjectOfType<PauseGameUIButton>();
        }

        private void OnEnable()
        {
            alreadyDisplayingText = false;

            SceneManager.sceneLoaded += (Scene sc, LoadSceneMode loadMode) => FindPauseGameMenuUIButton();

            SaveLoadHandler.OnSavingStarted += () => DisplaySavingLoadIndicatorText(savingIndicatorText, true);

            SaveLoadHandler.OnSavingFinished += () => DisplaySavingLoadIndicatorText(savingIndicatorText, false);

            SaveLoadHandler.OnLoadingStarted += () => DisplaySavingLoadIndicatorText(loadingIndicatorText, true);

            SaveLoadHandler.OnLoadingFinished += () => DisplaySavingLoadIndicatorText(loadingIndicatorText, false);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= (Scene sc, LoadSceneMode loadMode) => FindPauseGameMenuUIButton();

            SaveLoadHandler.OnSavingStarted -= () => DisplaySavingLoadIndicatorText(savingIndicatorText, true);

            SaveLoadHandler.OnSavingFinished -= () => DisplaySavingLoadIndicatorText(savingIndicatorText, false);

            SaveLoadHandler.OnLoadingStarted -= () => DisplaySavingLoadIndicatorText(loadingIndicatorText, true);

            SaveLoadHandler.OnLoadingFinished -= () => DisplaySavingLoadIndicatorText(loadingIndicatorText, false);
        }

        private void FindPauseGameMenuUIButton()
        {
            if (!pauseGameUIButton) pauseGameUIButton = FindObjectOfType<PauseGameUIButton>();
        }

        private void ConstructSaveLoadIndicatorObject()
        {
            if (!constructSaveLoadIndicatorObject) return;

            saveLoadIndicatorObj = new GameObject();

            saveLoadIndicatorObj.name = "SaveLoadIndicator";

            saveLoadIndicatorObj.transform.SetParent(transform);

            saveLoadIndicatorObj.transform.localPosition = Vector3.zero;

            saveLoadIndicatorTextMeshComp = saveLoadIndicatorObj.AddComponent<TextMeshProUGUI>();

            if (textFont) saveLoadIndicatorTextMeshComp.font = textFont;

            if (textColor != Color.clear && textColor != Color.black)
            {
                saveLoadIndicatorTextMeshComp.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a);
            }

            //saveLoadIndicatorTextMeshComp.enableAutoSizing = true;

            saveLoadIndicatorTextMeshComp.fontSize = textSize;

            saveLoadIndicatorTextMeshComp.horizontalAlignment = HorizontalAlignmentOptions.Center;

            saveLoadIndicatorTextMeshComp.verticalAlignment = VerticalAlignmentOptions.Middle;

            saveLoadIndicatorTextMeshComp.raycastTarget = false;

            saveLoadIndicatorObjCanvasGroup = saveLoadIndicatorObj.AddComponent<CanvasGroup>();

            saveLoadIndicatorObjCanvasGroup.blocksRaycasts = false;

            TryGetComponent<UIFade>(out UI_Fade);

            if (!UI_Fade) UI_Fade = saveLoadIndicatorObj.AddComponent<UIFade>();

            UI_Fade.isLooped = true;

            UI_Fade.isIndependentTimeScale = true;

            UI_Fade.reverseFadeOnFadeCycleFinished = true;

            UI_Fade.SetTweenExecuteMode(UITweenBase.UITweenExecuteMode.Auto);

            UI_Fade.SetUITweenCanvasGroup(saveLoadIndicatorObjCanvasGroup);

            saveLoadIndicatorObj.SetActive(false);
        }

        private void DisplaySavingLoadIndicatorText(string text, bool isDisplayed)
        {
            if (isDisplayed)
            {
                if (isDisablingSaveLoadTextDelay)
                {
                    StopCoroutine(DisableSaveLoadIndicatorTextDelay(1.5f));

                    isDisablingSaveLoadTextDelay = false;
                }

                alreadyDisplayingText = true;

                saveLoadIndicatorTextMeshComp.text = text;

                if(!isDisablingPauseGameUIButton) 
                    StartCoroutine(DisablePauseGameUIButtonDuringSaveLoadIndicator());

                if (saveLoadIndicatorObj) saveLoadIndicatorObj.SetActive(true);
            }
            else if(alreadyDisplayingText && !isDisplayed)
            {
                if (!isDisablingSaveLoadTextDelay)
                {
                    StartCoroutine(DisableSaveLoadIndicatorTextDelay(1.5f));

                    return;
                }

                StopCoroutine(DisableSaveLoadIndicatorTextDelay(1.5f));

                isDisablingSaveLoadTextDelay = false;

                StartCoroutine(DisableSaveLoadIndicatorTextDelay(1.5f));
            }
        }

        private IEnumerator DisableSaveLoadIndicatorTextDelay(float delay)
        {
            if (isDisablingSaveLoadTextDelay) yield break;

            if (delay <= 0.0f)
            {
                goto DisableSaveLoadIndicatorText;
            }

            isDisablingSaveLoadTextDelay = true;

            yield return new WaitForSecondsRealtime(delay);

        DisableSaveLoadIndicatorText:

            UI_Fade.StopAndResetUITweenImmediate();

            alreadyDisplayingText = false;

            isDisablingSaveLoadTextDelay = false;

            if (saveLoadIndicatorObj) saveLoadIndicatorObj.SetActive(false);

            yield break;
        }

        private IEnumerator DisablePauseGameUIButtonDuringSaveLoadIndicator()
        {
            if (!pauseGameUIButton) yield break;

            if (!alreadyDisplayingText) yield break;

            if (isDisablingPauseGameUIButton) yield break;

            isDisablingPauseGameUIButton = true;

            while (alreadyDisplayingText)
            {
                if (pauseGameUIButton) pauseGameUIButton.EnablePauseMenuButtonCanvasGroup(false, 1, 0.0f);

                yield return new WaitForSecondsRealtime(0.04f);
            }

            if (pauseGameUIButton) pauseGameUIButton.EnablePauseMenuButtonCanvasGroup(true);

            isDisablingPauseGameUIButton = false;
        }
    }
}
