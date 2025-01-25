using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.UI;
namespace TeamMAsTD
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] int conversationNum = 1; // the title of the conversation

        [SerializeField] private WaveSpawner waveSpawnerInUse;

        /* NOTES
        Using Names in Dialog Text
        Player Name: [lua(Actor["Player"].Display_Name)]
        Actor Name: [var=Actor]

        Setting Portraits in Dialog Text
        e.g. put
        [pic=2]
        anywhere in the dialog text to use the 2nd sprite

        Set Variables
        DialogueLua.SetVariable("name", value);

        Rich Text
        Italics: [em1]italicized text[/em1]
        Yellow Text: [em2]yellow text[em2]
        * customizable in Database -> Database Properties -> Emphasis Settings

        */

        private void Awake()
        {
            if (!waveSpawnerInUse)
            {
                waveSpawnerInUse = FindObjectOfType<WaveSpawner>();
            }

            if (!waveSpawnerInUse) Debug.LogWarning("Could not find a wave spawner in use for Dialogue process.");
        }

        void OnEnable()
        {
            TeamMAsTD.WaveSpawner.OnAllWaveSpawned += (WaveSpawner ws, bool b) => StartConvoOnWave15Finished(14);
        }

        void OnDisable()
        {
            TeamMAsTD.WaveSpawner.OnAllWaveSpawned -= (WaveSpawner ws, bool b) => StartConvoOnWave15Finished(14);
        }

        void Start()
        {
            //if (waveSpawnerInUse) conversationNum = waveSpawnerInUse.currentWave + 1;

            if (!TeamMAsTD.PersistentSceneLoadUI.persistentSceneLoadUIInstance)
            {
                StartCoroutine(StartConversationDelayCoroutine(0.35f));
            }
            else StartCoroutine(StartConversationAfterSceneLoad());
        }

        void OnConversationStart(Transform actor) // THIS SCRIPT MUST BE ON THE DIALOGUE MANAGER CUSTOM OBJECT TO WORK
        {
            DialogueManager.DisplaySettings.subtitleSettings.continueButton = DisplaySettings.SubtitleSettings.ContinueButtonMode.Never;
            DialogueManager.ConversationView.SetupContinueButton();
            StartCoroutine(DelayContinueButton(0.4f));
            //Time.timeScale = 0;
        }

        void OnConversationEnd(Transform actor) // THIS SCRIPT MUST BE ON THE DIALOGUE MANAGER CUSTOM OBJECT TO WORK
        {
            Time.timeScale = 1;

            if (waveSpawnerInUse) SaveLoadHandler.SaveThisSaveableOnly(waveSpawnerInUse.GetSaveable());
        }

        IEnumerator DelayContinueButton(float delayTime)
        {
            yield return new WaitForSecondsRealtime(delayTime);
            DialogueManager.DisplaySettings.subtitleSettings.continueButton = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;
            DialogueManager.ConversationView.SetupContinueButton();
        }

        IEnumerator StartConversationDelayCoroutine(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            if (waveSpawnerInUse) conversationNum = waveSpawnerInUse.currentWave + 1;

            //DialogueManager.StartConversation(string conversation, Transform actor, Transform conversant); // actor and conversant are optional
            StartConversation(conversationNum);
        }

        private IEnumerator StartConversationAfterSceneLoad()
        {
            if (!PersistentSceneLoadUI.persistentSceneLoadUIInstance) yield break;

            if (PersistentSceneLoadUI.persistentSceneLoadUIInstance.IsPerformingSceneLoad())
            {
                yield return new WaitWhile(() => PersistentSceneLoadUI.persistentSceneLoadUIInstance.IsPerformingSceneLoad());

                yield return StartCoroutine(StartConversationDelayCoroutine(0.4f));

                yield break;
            }
            else yield return StartCoroutine(StartConversationDelayCoroutine(1.0f));
        }

        public void SkipDialog()
        {
            DialogueManager.StopConversation();

            if (waveSpawnerInUse) SaveLoadHandler.SaveThisSaveableOnly(waveSpawnerInUse.GetSaveable());
        }

        public void StartConversation(string conversation)
        {
            DialogueManager.StartConversation(conversation);
        }

        public void StartConversation(int waveNum)
        {
            DialogueManager.StartConversation("Wave/" + (waveNum));
        }

        public void StartConvoOnWave15Finished(int waveNum)
        {
            if (waveNum >= 14)
            {
                StartConversation(16);
            }
        }

        public void PlantBlocked(PlantUnitSO plant, Tile tile)
        {
            if (GameResource.gameResourceInstance != null && GameResource.gameResourceInstance.coinResourceSO != null)
            {
                if (plant != null && plant.plantingCoinCost > GameResource.gameResourceInstance.coinResourceSO.resourceAmount)
                {
                    NoMoney("Plant");
                }
            }
            if (tile.isOccupied)
            {
                DialogueLua.SetVariable("rocksBlocked", true);

            }
            else if (!plant.isPlacableOnPath && tile.is_AI_Path)
            {
                DialogueLua.SetVariable("pathBlocked", true);
            }
        }

        public void NoMoney(string type)
        {
            DialogueLua.SetVariable("noMoney" + type, true);
        }

        public void SetDialogBoolTrue(string name)
        {
            DialogueLua.SetVariable(name, true);
        }
    }

}

