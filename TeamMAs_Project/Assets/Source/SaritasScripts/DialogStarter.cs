using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogStarter : MonoBehaviour
{
  [SerializeField] string conversation; // the title of the conversation
  
  /* NOTES
  Using Names in Dialog Text
  Player Name: [lua(Actor["Player"].Display_Name)]
  Actor Name: [var=Actor]
  */
  
  void Start()
  {
    StartCoroutine(DialogTest(1f));
  }
  
  IEnumerator DialogTest(float delayTime) {
    yield return new WaitForSeconds(delayTime);
    //DialogueManager.StartConversation(string conversation, Transform actor, Transform conversant); // actor and conversant are optional
    DialogueManager.StartConversation(conversation);
    //GetComponent<DialogueSystemTrigger>().OnUse();  // also works, only if using a DialogueSystemTrigger component set to OnUse
  }
}
