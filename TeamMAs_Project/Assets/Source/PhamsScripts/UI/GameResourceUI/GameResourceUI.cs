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

        [SerializeField] protected GameResourceSO gameResourceSO;

        [Header("Game Resource UI Components")]

        [SerializeField] private TextMeshProUGUI resourceNameText;

        [SerializeField] private TextMeshProUGUI resourceAmountText;

        [SerializeField] private bool displayResourceAmountCap = false;

        [Header("Game Resource Stat Popup Components")]

        [SerializeField] protected StatPopupSpawner gameResourceUIStatPopupSpawner;

        protected float currentResourceAmount;

        [Header("Debug Section")]

        [SerializeField] protected bool showDebugAndErrorLog = false;

        protected virtual void Awake()
        {
            if(gameResourceSO == null)
            {
                Debug.LogError("GameResourceSO reference is missing on GameResourceUI: " + name + ". Disabling script...");

                enabled = false;

                return;
            }

            currentResourceAmount = gameResourceSO.resourceAmount;

            DisplayResourceNameText();

            DisplayResourceAmountText(gameResourceSO);
        }

        protected virtual void OnEnable()
        {
            GameResourceSO.OnResourceAmountUpdated += GameResourceUpdateStatPopupOnUI;
            GameResourceSO.OnResourceAmountUpdated += DisplayResourceAmountText;
        }

        protected virtual void OnDisable()
        {
            GameResourceSO.OnResourceAmountUpdated -= GameResourceUpdateStatPopupOnUI;
            GameResourceSO.OnResourceAmountUpdated -= DisplayResourceAmountText;
        }

        public virtual void DisplayResourceNameText()
        {
            if (gameResourceSO == null) return;

            if (resourceNameText == null)
            {
                if(showDebugAndErrorLog) Debug.LogError("GameResourceUI: " + name + " is missing resource name TextMeshProUGUI component reference!");

                return;
            }

            resourceNameText.text = gameResourceSO.resourceName;
        }

        public virtual void DisplayResourceAmountText(GameResourceSO gameResourceSO)
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

        protected virtual void GameResourceUpdateStatPopupOnUI(GameResourceSO gameResourceSO)
        {
            if (gameResourceUIStatPopupSpawner == null) return;

            if (this.gameResourceSO == null || gameResourceSO == null) return;

            if (this.gameResourceSO != gameResourceSO) return;

            float updatedResourceAmount = gameResourceSO.resourceAmount;

            if (updatedResourceAmount == currentResourceAmount || Mathf.Abs(updatedResourceAmount - currentResourceAmount) <= Mathf.Epsilon) return;

            if(updatedResourceAmount > currentResourceAmount)
            {
                gameResourceUIStatPopupSpawner.PopUp(null, "+" + (updatedResourceAmount - currentResourceAmount).ToString(), true);
            }

            if(updatedResourceAmount < currentResourceAmount)
            {
                gameResourceUIStatPopupSpawner.PopUp(null, "-" + (currentResourceAmount - updatedResourceAmount).ToString(), false);
            }

            currentResourceAmount = updatedResourceAmount;
        }
    }
}
