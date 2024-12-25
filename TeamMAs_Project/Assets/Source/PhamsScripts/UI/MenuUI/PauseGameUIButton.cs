// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace TeamMAsTD
{
    public class PauseGameUIButton : MonoBehaviour
    {
        [Header("Pause Menu Button Required Components")]

        [SerializeField]
        [Tooltip("The top most canvas group that is handling the displaying of the pause menu panel and its children UI elements." +
        "This canvas group will be toggled by this button when player turning pause menu on/off")]
        private CanvasGroup pauseMenuCanvasGroup;

        //INTERNALS.....................................................

        private CanvasGroup pauseMenuButtonCanvasGroup;

        private bool pauseMenuOpened = false;

        private float timeScaleBeforePause = 0.0f;

        private void Awake()
        {
            if (!pauseMenuCanvasGroup)
            {
                Debug.LogWarning("Pause Menu UI Button doesn't have a pause menu canvas group reference assigned. Disabling button...");

                gameObject.SetActive(false);

                return;
            }

            EnablePauseMenuCanvasGroup(false);

            TryGetComponent<CanvasGroup>(out pauseMenuButtonCanvasGroup);

            if (!pauseMenuButtonCanvasGroup)
            {
                pauseMenuButtonCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            SaveLoadHandler.OnSavingStarted += () => EnablePauseMenuButtonCanvasGroup(false);

            SaveLoadHandler.OnSavingFinished += () => EnablePauseMenuButtonCanvasGroup(true);

            SaveLoadHandler.OnLoadingStarted += () => EnablePauseMenuButtonCanvasGroup(false);

            SaveLoadHandler.OnLoadingFinished += () => EnablePauseMenuButtonCanvasGroup(true);
        }

        private void OnDisable()
        {
            SaveLoadHandler.OnSavingStarted -= () => EnablePauseMenuButtonCanvasGroup(false);

            SaveLoadHandler.OnSavingFinished -= () => EnablePauseMenuButtonCanvasGroup(true);

            SaveLoadHandler.OnLoadingStarted -= () => EnablePauseMenuButtonCanvasGroup(false);

            SaveLoadHandler.OnLoadingFinished -= () => EnablePauseMenuButtonCanvasGroup(true);

            StopCoroutine(CheckAndStopTimeCoroutine());

            if (Time.timeScale != timeScaleBeforePause) Time.timeScale = timeScaleBeforePause;
        }

        private void EnablePauseMenuCanvasGroup(bool enabled, float enableAlpha = 1.0f, float disableAlpha = 0.0f)
        {
            if (!pauseMenuCanvasGroup) return;

            if (enabled)
            {
                pauseMenuOpened = true;

                pauseMenuCanvasGroup.alpha = enableAlpha;

                pauseMenuCanvasGroup.interactable = true;

                pauseMenuCanvasGroup.blocksRaycasts = true;

                return;
            }

            pauseMenuOpened = false;

            pauseMenuCanvasGroup.alpha = disableAlpha;

            pauseMenuCanvasGroup.interactable = false;

            pauseMenuCanvasGroup.blocksRaycasts = false;
        }

        public void EnablePauseMenuButtonCanvasGroup(bool enabled, float enableAlpha = 1.0f, float disableAlpha = 0.4f)
        {
            if (!pauseMenuButtonCanvasGroup) return;

            if (enabled)
            {
                pauseMenuButtonCanvasGroup.alpha = enableAlpha;

                pauseMenuButtonCanvasGroup.interactable = true;

                pauseMenuButtonCanvasGroup.blocksRaycasts = true;

                return;
            }

            pauseMenuButtonCanvasGroup.alpha = disableAlpha;

            pauseMenuButtonCanvasGroup.interactable = false;

            pauseMenuButtonCanvasGroup.blocksRaycasts = false;
        }

        public void TogglePauseMenu()
        {
            if(pauseMenuOpened) //if opened -> then close
            {
                EnablePauseMenuCanvasGroup(false);

                DialogueManager.Unpause();

                if (DialogueManager.isConversationActive) return;

                if(Time.timeScale != timeScaleBeforePause) Time.timeScale = timeScaleBeforePause;

                return;
            }

            //else if closed -> then open 
            
            EnablePauseMenuCanvasGroup(true);

            DialogueManager.Pause();

            if (DialogueManager.isConversationActive) return;

            StartCoroutine(CheckAndStopTimeCoroutine());
        }

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();//cache wait for fixed update to avoid GC

        private IEnumerator CheckAndStopTimeCoroutine()
        {
            timeScaleBeforePause = Time.timeScale;

            if (Time.timeScale > 0.0f) Time.timeScale = 0.0f;

            while (pauseMenuOpened)
            {
                if(Time.timeScale > 0.0f) Time.timeScale = 0.0f;

                yield return waitForFixedUpdate;//use cached wait for fixed update above.
            }

            if (!DialogueManager.isConversationActive && Time.timeScale != timeScaleBeforePause)
            {
                Time.timeScale = timeScaleBeforePause;
            }

            yield break;
        }
    }
}
