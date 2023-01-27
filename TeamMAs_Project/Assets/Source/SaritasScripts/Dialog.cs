using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class Dialog : MonoBehaviour
{
    //[SerializeField] string conversation; // the title of the conversation

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

    Italics
    [em1]italicized text[/em1]
    * customizable in Database -> Database Properties -> Emphasis Settings
    */

    void Start()
    {
        StartCoroutine(DialogTest(1f));
    }

    void OnConversationStart(Transform actor) // THIS SCRIPT MUST BE ON DIALOGUE MANAGER CUSTOM TO WORK
    {
        Time.timeScale = 0;
    }

    void OnConversationEnd(Transform actor) // THIS SCRIPT MUST BE ON DIALOGUE MANAGER CUSTOM TO WORK
    {
        Time.timeScale = 1;
    }

    IEnumerator DialogTest(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        //DialogueManager.StartConversation(string conversation, Transform actor, Transform conversant); // actor and conversant are optional
        StartConversation(1);
    }

    public void SkipDialog()
    {
        DialogueManager.StopConversation();
    }

    public void StartConversation(string conversation)
    {
        DialogueManager.StartConversation(conversation);
    }

    public void StartConversation(int waveNum)
    {
        DialogueManager.StartConversation("Wave/" + (waveNum));
    }
}
