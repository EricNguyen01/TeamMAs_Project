using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class PersistentSaveLoadIndicatorTextUI : MonoBehaviour
    {
        [SerializeField] private string savingIndicatorText = "Saving";

        [SerializeField] private string loadingIndicatorText = "Loading";

        [SerializeField] private TMP_FontAsset textFont;

        [SerializeField] private Color textColor;

        [SerializeField] private int textSize = 20;

        //INTERNALS.........................................................................................

        private GameObject saveLoadIndicatorObj;

        private TextMeshProUGUI saveLoadIndicatorTextMeshComp;

        private Canvas canvas;

        private CanvasGroup saveLoadIndicatorObjCanvasGroup;

        private UIFade UI_Fade;

        private bool alreadyDisplayingText = false;

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

            if(textFont) saveLoadIndicatorTextMeshComp.font = textFont;

            if (textColor != Color.clear && textColor != Color.black)
            {
                saveLoadIndicatorTextMeshComp.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a);
            }

            saveLoadIndicatorTextMeshComp.enableAutoSizing = true;

            saveLoadIndicatorTextMeshComp.alignment = TextAlignmentOptions.Center;

            saveLoadIndicatorTextMeshComp.fontSize = textSize;

            saveLoadIndicatorTextMeshComp.raycastTarget = false;
        }

        private void OnEnable()
        {
            alreadyDisplayingText = false;

            SaveLoadHandler.OnSavingStarted += () => DisplaySavingLoadIndicatorText(savingIndicatorText, true);

            SaveLoadHandler.OnSavingFinished += () => DisplaySavingLoadIndicatorText(savingIndicatorText, false);

            SaveLoadHandler.OnLoadingStarted += () => DisplaySavingLoadIndicatorText(loadingIndicatorText, true);

            SaveLoadHandler.OnLoadingFinished += () => DisplaySavingLoadIndicatorText(loadingIndicatorText, false);
        }

        private void OnDisable()
        {
            SaveLoadHandler.OnSavingStarted -= () => DisplaySavingLoadIndicatorText(savingIndicatorText, true);

            SaveLoadHandler.OnSavingFinished -= () => DisplaySavingLoadIndicatorText(savingIndicatorText, false);

            SaveLoadHandler.OnLoadingStarted -= () => DisplaySavingLoadIndicatorText(loadingIndicatorText, true);

            SaveLoadHandler.OnLoadingFinished -= () => DisplaySavingLoadIndicatorText(loadingIndicatorText, false);
        }

        private void ConstructSaveLoadIndicatorObject()
        {
            saveLoadIndicatorObj = new GameObject();

            saveLoadIndicatorObj.transform.SetParent(transform, false);

            saveLoadIndicatorObj.transform.position = Vector3.zero;

            saveLoadIndicatorTextMeshComp = saveLoadIndicatorObj.AddComponent<TextMeshProUGUI>();

            saveLoadIndicatorObjCanvasGroup = saveLoadIndicatorObj.AddComponent<CanvasGroup>();

            TryGetComponent<UIFade>(out UI_Fade);

            if (!UI_Fade) UI_Fade = saveLoadIndicatorObj.AddComponent<UIFade>();

            UI_Fade.SetTweenExecuteMode(UITweenBase.UITweenExecuteMode.Auto);

            UI_Fade.SetUITweenCanvasGroup(saveLoadIndicatorObjCanvasGroup);

            saveLoadIndicatorObj.SetActive(false);
        }

        private void DisplaySavingLoadIndicatorText(string text, bool isDisplayed)
        {
            if (!alreadyDisplayingText && isDisplayed)
            {
                alreadyDisplayingText = true;

                saveLoadIndicatorTextMeshComp.text = text;

                saveLoadIndicatorObj.SetActive(true);
            }
            else if(alreadyDisplayingText && !isDisplayed)
            {
                saveLoadIndicatorObj.SetActive(false);

                UI_Fade.StopAndResetUITweenImmediate();

                alreadyDisplayingText = false;
            }
        }
    }
}
