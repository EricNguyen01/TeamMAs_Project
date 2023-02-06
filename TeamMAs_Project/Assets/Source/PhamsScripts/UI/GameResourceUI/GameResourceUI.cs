using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Language.Lua;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class GameResourceUI : MonoBehaviour
    {
        [Header("Game Resource ScriptableObject Data")]

        [SerializeField] private GameResourceSO gameResourceSO;

        [Header("Game Resource UI Components")]

        [SerializeField] private TextMeshProUGUI resourceNameText;

        [SerializeField] private TextMeshProUGUI resourceAmountText;

        [SerializeField] private bool displayResourceAmountCap = false;

        [Header("Game Resource Stat Popup Components")]

        [SerializeField] private StatPopupSpawner gameResourceUIStatPopupSpawner;

        private int currentResourceAmount;

        [Header("Debug Section")]

        [SerializeField] private bool showDebugAndErrorLog = false;

        private void Awake()
        {
            if(gameResourceSO == null)
            {
                Debug.LogError("GameResourceSO reference is missing on GameResourceUI: " + name + ". Disabling script...");

                enabled = false;

                return;
            }

            currentResourceAmount = Mathf.RoundToInt(gameResourceSO.resourceAmount);

            DisplayResourceNameText();

            DisplayResourceAmountText(gameResourceSO);
        }

        private void OnEnable()
        {
            GameResourceSO.OnResourceAmountUpdated += GameResourceUpdateStatPopupOnUI;
            GameResourceSO.OnResourceAmountUpdated += DisplayResourceAmountText;
        }

        private void OnDisable()
        {
            GameResourceSO.OnResourceAmountUpdated -= GameResourceUpdateStatPopupOnUI;
            GameResourceSO.OnResourceAmountUpdated -= DisplayResourceAmountText;
        }

        public void DisplayResourceNameText()
        {
            if (gameResourceSO == null) return;

            if (resourceNameText == null)
            {
                if(showDebugAndErrorLog) Debug.LogError("GameResourceUI: " + name + " is missing resource name TextMeshProUGUI component reference!");

                return;
            }

            resourceNameText.text = gameResourceSO.resourceName;
        }

        public void DisplayResourceAmountText(GameResourceSO gameResourceSO)
        {
            if (this.gameResourceSO == null || gameResourceSO == null) return;

            if (this.gameResourceSO != gameResourceSO) return;

            if (resourceAmountText == null)
            {
                if (showDebugAndErrorLog) Debug.LogError("GameResourceUI: " + name + " is missing resource amount TextMeshProUGUI component reference!");

                return;
            }

            if (!displayResourceAmountCap) 
            { 
                resourceAmountText.text = Mathf.RoundToInt(gameResourceSO.resourceAmount).ToString(); 
            }
            else
            {
                resourceAmountText.text = Mathf.RoundToInt(gameResourceSO.resourceAmount).ToString() + "/" + Mathf.RoundToInt(gameResourceSO.resourceAmountCap).ToString();
            }
        }

        private void GameResourceUpdateStatPopupOnUI(GameResourceSO gameResourceSO)
        {
            if (gameResourceUIStatPopupSpawner == null) return;

            if (this.gameResourceSO == null || gameResourceSO == null) return;

            if (this.gameResourceSO != gameResourceSO) return;

            int roundedUpdatedResourceAmount = Mathf.RoundToInt(gameResourceSO.resourceAmount);

            if (roundedUpdatedResourceAmount == currentResourceAmount) return;

            if(roundedUpdatedResourceAmount > currentResourceAmount)
            {
                gameResourceUIStatPopupSpawner.PopUp(null, "+" + (roundedUpdatedResourceAmount - currentResourceAmount).ToString(), true);
            }

            if(roundedUpdatedResourceAmount < currentResourceAmount)
            {
                gameResourceUIStatPopupSpawner.PopUp(null, "-" + (currentResourceAmount - roundedUpdatedResourceAmount).ToString(), false);
            }

            currentResourceAmount = roundedUpdatedResourceAmount;
        }
    }
}
