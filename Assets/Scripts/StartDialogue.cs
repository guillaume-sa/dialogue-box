using UnityEngine;
using UnityEngine.EventSystems;

public class StartDialogue : MonoBehaviour {

    public TextAsset xmlDialogFile;             // XML Dialog file

    public void Dialogue()
    {
        // Disable button
        EventSystem.current.currentSelectedGameObject.SetActive(false);
        // Start dialogue
        DialogueManager.instance.Dialogue(xmlDialogFile);
    }
}
