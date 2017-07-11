using UnityEngine;
using System.Xml;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{

    public TextAsset xmlDialogFile;                 // Dialogue file
    public AudioClip[] typingClips;                 // Typing audio clips

    private Text text;                              // Text UI
    private Image icon;                             // Icon UI
    private IEnumerator writingCoroutine;           // Writing cooroutine
    private XmlNodeList nodes;                      // XML nodes read from file
    private XmlNode currentNode;                    // Current XML node
    private int nodeIndex = 0;                      // Index of current XML node
    private bool nextNode;                          // Ready to read the next node
    private const float MIN_TYPING_WAIT = 0.02f;    // Minimum waiting time before next letter
    private const float MAX_TYPING_WAIT = 0.1f;     // Maximum waiting time before next letter

    // Use this for initialization
    public void Start()
    {
        // Get UI components
        text = GameObject.Find("Text").GetComponent<Text>();
        icon = GameObject.Find("Icon").GetComponent<Image>();
        // Load Dialog File
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlDialogFile.text);
        // TODO: add xml validation
        // Get all nodes
        nodes = xmlDoc.SelectNodes("dialogue/message");
        // Initialize variables
        nodeIndex = 0;
        nextNode = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Skip text when space is pressed
        if (Input.GetKey(KeyCode.Space))
        {
            // Stop the writing coroutine
            Stop();
            // Ready to read next node
            nextNode = true;
        }else if (nextNode && nodeIndex < nodes.Count)
        {
            // Clear the dialogue box
            Clear();
            // Do not read the next node yet
            nextNode = false;
            // Get current node
            currentNode = nodes[nodeIndex];
            // Get message
            string message = currentNode.InnerText;
            // Get icon
            string iconName = currentNode.Attributes["icon"].Value;
            // Set icon
            icon.sprite = Resources.Load<Sprite>(iconName);
            // Write message
            Write(message);
            // Go to next node
            nodeIndex += 1;
        }
    }

    private void TypeLetter(char letter)
    {
        // Play audio
        PlayTypingSound();
        // Add a letter to the text
        text.text += letter;
    }

    private void Clear()
    {
        text.text = "";
    }

    public void Write(string text, float min_wait = MIN_TYPING_WAIT, float max_wait = MAX_TYPING_WAIT)
    {
        writingCoroutine = SlowTypeText(text.ToCharArray(), min_wait, max_wait);
        StartCoroutine(writingCoroutine);
    }

    public void Stop()
    {
        StopCoroutine(writingCoroutine);
    }

    private IEnumerator SlowTypeText(char[] text, float min_wait, float max_wait)
    {
        yield return new WaitForSeconds(0.4f);
        // Iterate through all letters
        for (int i = 0; i < text.Length; ++i)
        {
            // Type the letter in the logs
            TypeLetter(text[i]);
            // Pick a random time to type it
            float waitTime = Random.Range(min_wait, max_wait);
            // Wait for time
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Audio
    private void PlayTypingSound()
    {
        if (typingClips.Length > 0)
        {
            // Pick a random audio clip
            int randomSound = Random.Range(0, typingClips.Length);
            AudioClip typingClip = typingClips[randomSound];
            // Play audio clip
            SoundManager.instance.PlaySingle(typingClip);
        }
    }
}