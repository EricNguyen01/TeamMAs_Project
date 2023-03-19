using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class TheFingerController : MonoBehaviour
    {
        private bool alreadyDisplayed = false;

        private void Awake()
        {
            EnableOnDialogueEndEvent(true);

            DisableOnPlantPlantedOnTileEvent(true);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EnableOnDialogueEndEvent(false);

            DisableOnPlantPlantedOnTileEvent(false);
        }

        public void EnableDragDropFinger(bool enabled)
        {
            if (!alreadyDisplayed && enabled)
            {
                if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                alreadyDisplayed = true;

                return;
            }

            if (gameObject.activeInHierarchy) gameObject.SetActive(false);
        }

        private void DisableOnPlantPlantedOnTileEvent(bool subToEvent)
        {
            Tile[] tiles = FindObjectsOfType<Tile>();

            if (tiles == null || tiles.Length == 0) return;

            if (subToEvent)
            {
                for(int i = 0; i < tiles.Length; i++)
                {
                    tiles[i].OnPlantUnitPlantedOnTile.AddListener((PlantUnit pU, Tile t) => EnableDragDropFinger(false));
                }

                return;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].OnPlantUnitPlantedOnTile.RemoveListener((PlantUnit pU, Tile t) => EnableDragDropFinger(false));
            }
        }

        private void EnableOnDialogueEndEvent(bool subToEvent)
        {
            if (!DialogueManager.hasInstance) return;

            DialogueSystemEvents dialogueSystemEvents = DialogueManager.instance.GetComponent<DialogueSystemEvents>();

            if (dialogueSystemEvents == null) return;

            if(subToEvent)
            {
                dialogueSystemEvents.conversationEvents.onConversationEnd.AddListener((transform) => EnableDragDropFinger(true));

                return;
            }

            dialogueSystemEvents.conversationEvents.onConversationEnd.RemoveListener((transform) => EnableDragDropFinger(true));
        }
    }
}
