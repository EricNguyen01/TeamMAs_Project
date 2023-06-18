using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        }

        private void OnDisable()
        {
            StopCoroutine(CheckAndStopTimeCoroutine());

            if (Time.timeScale != timeScaleBeforePause) Time.timeScale = timeScaleBeforePause;
        }

        private void EnablePauseMenuCanvasGroup(bool enabled)
        {
            if (!pauseMenuCanvasGroup) return;

            if (enabled)
            {
                pauseMenuOpened = true;

                pauseMenuCanvasGroup.alpha = 1.0f;

                pauseMenuCanvasGroup.interactable = true;

                pauseMenuCanvasGroup.blocksRaycasts = true;

                return;
            }

            pauseMenuOpened = false;

            pauseMenuCanvasGroup.alpha = 0.0f;

            pauseMenuCanvasGroup.interactable = false;

            pauseMenuCanvasGroup.blocksRaycasts = false;
        }

        public void TogglePauseMenu()
        {
            if(pauseMenuOpened) //if opened -> then close
            {
                EnablePauseMenuCanvasGroup(false);

                StopCoroutine(CheckAndStopTimeCoroutine());

                if(Time.timeScale != timeScaleBeforePause) Time.timeScale = timeScaleBeforePause;

                return;
            }

            //else if closed -> then open 

            EnablePauseMenuCanvasGroup(true);

            StartCoroutine(CheckAndStopTimeCoroutine());
        }

        private IEnumerator CheckAndStopTimeCoroutine()
        {
            timeScaleBeforePause = Time.timeScale;

            if (Time.timeScale > 0.0f) Time.timeScale = 0.0f;

            while (pauseMenuOpened)
            {
                if(Time.timeScale > 0.0f) Time.timeScale = 0.0f;

                yield return new WaitForFixedUpdate();
            }

            Time.timeScale = timeScaleBeforePause;

            yield break;
        }
    }
}
