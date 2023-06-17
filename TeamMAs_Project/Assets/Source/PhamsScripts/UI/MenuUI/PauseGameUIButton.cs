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
            if(pauseMenuOpened)
            {
                EnablePauseMenuCanvasGroup(false);

                return;
            }

            EnablePauseMenuCanvasGroup(true);
        }
    }
}
