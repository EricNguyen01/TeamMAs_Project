using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class GameResourceUI : MonoBehaviour
    {
        [SerializeField] private GameResourceSO gameResourceSO;

        [SerializeField] private TextMeshProUGUI resourceNameText;

        [SerializeField] private TextMeshProUGUI resourceAmountText;

        private void Awake()
        {
            if(gameResourceSO == null)
            {
                Debug.LogError("GameResourceSO reference is missing on GameResourceUI: " + name + ". Disabling script...");

                enabled = false;

                return;
            }

            gameResourceSO.SetGameResourceUIBeingUsedToDisplayResourceData(this);

            DisplayResourceNameText();

            DisplayResourceAmountText();
        }

        public void DisplayResourceNameText()
        {
            if (gameResourceSO == null) return;

            if (resourceNameText == null)
            {
                Debug.LogError("GameResourceUI: " + name + " is missing resource name TextMeshProUGUI component reference!");

                return;
            }

            resourceNameText.text = gameResourceSO.resourceName;
        }

        public void DisplayResourceAmountText()
        {
            if (gameResourceSO == null) return;

            if (resourceAmountText == null)
            {
                Debug.LogError("GameResourceUI: " + name + " is missing resource amount TextMeshProUGUI component reference!");

                return;
            }

            resourceAmountText.text = Mathf.RoundToInt(gameResourceSO.resourceAmount).ToString();
        }
    }
}
