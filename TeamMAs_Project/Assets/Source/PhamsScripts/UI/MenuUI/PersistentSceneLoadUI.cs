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

        [SerializeField] private float additionalTransitionTime = 1.0f;

        //INTERNALS..............................................................

        public static PersistentSceneLoadUI persistentSceneLoadUIInstance;

        private Canvas sceneLoadCanvas;

        private CanvasGroup sceneLoadCanvasGroup;

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
                Debug.LogWarning("PersistentSceneLoadUI is implemented but a scene load canvas is not assigned! Scene Load UI won't work!");

                gameObject.SetActive(false);

                return;
            }

            sceneLoadCanvasGroup = sceneLoadCanvas.GetComponent<CanvasGroup>();

            if (sceneLoadCanvasGroup == null) sceneLoadCanvasGroup = sceneLoadCanvas.gameObject.AddComponent<CanvasGroup>();

            EnableSceneLoadUI(false);
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

            if (Time.timeScale > 0.0f) Time.timeScale = 0.0f;

            yield return new WaitForSeconds(additionalTransitionTime);

            PixelCrushers.DialogueSystem.DialogueManager.Unpause();

            if (Time.timeScale == 0.0f) Time.timeScale = 1.0f;

            EnableSceneLoadUI(false);
        }

        private void EnableSceneLoadUI(bool enabled)
        {
            if (sceneLoadCanvasGroup == null) return;

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
    }
}
