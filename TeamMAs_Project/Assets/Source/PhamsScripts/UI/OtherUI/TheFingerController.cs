// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class TheFingerController : MonoBehaviour, ISaveable
    {
        [SerializeField] private float fingerPointerMoveSpeed = 2.0f;

        private bool alreadyDisplayed = false;

        private bool useCustomControl = false;

        private TDGrid grid;

        private List<Tile> validTilesToPointTo = new List<Tile>();

        private Tile unoccupiedTileToPointTo;

        private Vector3 unoccupiedTileToPointToPositionOffset;

        private Vector3 fingerStartPos;

        private Animator fingerAnimator;

        private bool hasAnimatorDisabled = false;

        private void Awake()
        {
            EnableOnDialogueEndEvent(true);

            DisableOnPlantPlantedOnTileEvent(true);

            TryGetComponent<Animator>(out fingerAnimator);

            fingerStartPos = transform.position;
        }

        private void Start()
        {
            GetUnOccupiedTileToPointTo();

            StartCoroutine(InActiveFingerDelay(0.1f));
        }

        private void OnDestroy()
        {
            EnableOnDialogueEndEvent(false);

            DisableOnPlantPlantedOnTileEvent(false);
        }

        private void Update()
        {
            if(validTilesToPointTo.Count > 0)
            {
                if (unoccupiedTileToPointTo)
                {
                    if (unoccupiedTileToPointTo.isOccupied || unoccupiedTileToPointTo.is_AI_Path) GetUnOccupiedTileToPointTo();
                }
            }

            if (useCustomControl && unoccupiedTileToPointTo)
            {
                if (!hasAnimatorDisabled) DisableFingerAnimIfUsingCustomControl(true);
            }
            else if(!useCustomControl || !unoccupiedTileToPointTo)
            {
                if (hasAnimatorDisabled) DisableFingerAnimIfUsingCustomControl(false);
            }

            if (useCustomControl && unoccupiedTileToPointTo)
            {
                float dist = Vector2.Distance(transform.position, (Vector2)unoccupiedTileToPointToPositionOffset);

                float distSpeedMultiplier = dist;

                distSpeedMultiplier = Mathf.Clamp(distSpeedMultiplier, 0.5f, 4.0f);

                if (dist >= 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, 
                                                             (Vector2)unoccupiedTileToPointToPositionOffset, 
                                                             fingerPointerMoveSpeed * distSpeedMultiplier * Time.deltaTime);
                }
                else
                {
                    transform.position = fingerStartPos;
                }
            }
        }

        public void EnableDragDropFinger(bool enabled)
        {
            if (!alreadyDisplayed && enabled)
            {
                if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

                alreadyDisplayed = true;

                transform.position = fingerStartPos;

                return;
            }

            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);

                transform.position = fingerStartPos;
            }
        }

        public void ResetDragDropFingerDisplay()
        {
            EnableDragDropFinger(true);

            alreadyDisplayed = false;
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

        private IEnumerator InActiveFingerDelay(float delaySec)
        {
            yield return new WaitForSecondsRealtime(delaySec);

            if (gameObject.activeInHierarchy) gameObject.SetActive(false);

            transform.position = fingerStartPos;

            yield break;
        }

        private void GetUnOccupiedTileToPointTo()
        {
            if(!grid) grid = FindObjectOfType<TDGrid>();

            if (!grid) return;

            if(validTilesToPointTo.Count > 0) validTilesToPointTo.Clear();

            Tile[] tilesInGrid = grid.GetGridFlattened2DArray();

            int start = 0;

            int end = tilesInGrid.Length - 1;

            if(tilesInGrid.Length >= 4)
            {
                start = Mathf.RoundToInt(0.2f * tilesInGrid.Length);

                end = Mathf.RoundToInt(0.7f * tilesInGrid.Length);
            }

            for(int i = start; i < end; i++)
            {
                if (!tilesInGrid[i]) continue;

                if (tilesInGrid[i].isOccupied || tilesInGrid[i].is_AI_Path) continue;

                if (tilesInGrid[i].tileNumInRow == 0 || tilesInGrid[i].tileNumInColumn == 0) continue;

                if (tilesInGrid[i].tileNumInRow == grid.gridWidth - 1 || tilesInGrid[i].tileNumInColumn == grid.gridHeight - 1) continue;

                if (!validTilesToPointTo.Contains(tilesInGrid[i])) validTilesToPointTo.Add(tilesInGrid[i]);
            }

            if(validTilesToPointTo.Count > 0)
            {
                unoccupiedTileToPointTo = validTilesToPointTo[Random.Range(0, validTilesToPointTo.Count)];

                unoccupiedTileToPointToPositionOffset = unoccupiedTileToPointTo.transform.position + (Vector3.right * 0.5f) + (Vector3.down * 0.4f);

                useCustomControl = true;
            }
        }

        private void DisableFingerAnimIfUsingCustomControl(bool disabled)
        {
            if (!fingerAnimator)
            {
                TryGetComponent<Animator>(out fingerAnimator);
            }

            if (!fingerAnimator) return;

            if (disabled)
            {
                fingerAnimator.StopPlayback();

                fingerAnimator.enabled = false;

                hasAnimatorDisabled = true;

                transform.position = fingerStartPos;

                return;
            }

            transform.position = fingerStartPos;

            fingerAnimator.enabled = true;

            fingerAnimator.StartPlayback();

            hasAnimatorDisabled = false;
        }

        //ISaveable interface implementations......................................................................................................

        public SaveDataSerializeBase SaveData(string saveName = "")
        {
            SaveDataSerializeBase tutorialFingerSave;

            tutorialFingerSave = new SaveDataSerializeBase(alreadyDisplayed, 
                                                           transform.position, 
                                                           UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            return tutorialFingerSave;
        }

        public void LoadData(SaveDataSerializeBase savedDataToLoad)
        {
            if (savedDataToLoad == null) return;

            alreadyDisplayed = (bool)savedDataToLoad.LoadSavedObject();

            if (alreadyDisplayed)
            {
                EnableOnDialogueEndEvent(false);

                DisableOnPlantPlantedOnTileEvent(false);

                EnableDragDropFinger(false);

                if (gameObject.activeInHierarchy) gameObject.SetActive(false);
            }
        }
    }
}
