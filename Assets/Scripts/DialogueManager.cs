using UnityEngine;
using System.Xml;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{

    public TextAsset xmlDialogFile;                 // Dialogue file
    public AudioClip[] typingClips;                 // Typing audio clips

    private Text text;                              // Text UI
    private Text choiceText1;                       // Button 1 text UI
    private Text choiceText2;                       // Button 2 text UI
    private Canvas buttonsCanvas;                   // Buttons Canvas
    private Button button1;                         // Button 1
    private Button button2;                         // Button 2 
    private Image icon;                             // Icon UI
    private IEnumerator writingCoroutine;           // Writing cooroutine
    private XmlNodeList nodes;                      // XML nodes read from file
    private XmlNode currentNode;                    // Current XML node
    private int nodeIndex = 0;                      // Index of current XML node
    private bool nextNode;                          // Ready to read the next node
    private bool typing;                            // Is message being typed
    private string message;                         // Message
    private const float MIN_TYPING_WAIT = 0.02f;    // Minimum waiting time before next letter
    private const float MAX_TYPING_WAIT = 0.1f;     // Maximum waiting time before next letter

    // Use this for initialization
    public void Start()
    {
        // Get UI components
        text = GameObject.Find("Text").GetComponent<Text>();
        choiceText1 = GameObject.Find("ChoiceText1").GetComponent<Text>();
        choiceText2 = GameObject.Find("ChoiceText2").GetComponent<Text>();
        buttonsCanvas = GameObject.Find("ChoiceButtons").GetComponent<Canvas>();
        button1 = GameObject.Find("ChoiceButton1").GetComponent<Button>();
        button2 = GameObject.Find("ChoiceButton2").GetComponent<Button>();
        icon = GameObject.Find("Icon").GetComponent<Image>();
        // Manage UI components
        button1.onClick.AddListener(OnChoiceClicked);
        button2.onClick.AddListener(OnChoiceClicked);
        DisableButtons();
        // Load Dialog File
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlDialogFile.text);
        // TODO: add xml validation
        // Get all nodes
        nodes = xmlDoc.SelectNodes("dialogue/*");
        // Initialize variables
        nodeIndex = 0;
        nextNode = true;
        typing = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Skip text when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (typing)
            {
                // Stop the writing coroutine
                Stop();
            }else
            {
                // Ready to read next node
                nextNode = true;
            }
        }else if (nextNode && nodeIndex < nodes.Count)
        {
            // Clear the dialogue box
            Clear();
            // Do not read the next node yet
            nextNode = false;
            // Get current node
            currentNode = nodes[nodeIndex];
            // Get icon
            string iconName = currentNode.Attributes["icon"].Value;
            // Set icon
            icon.sprite = Resources.Load<Sprite>(iconName);
            if (currentNode.Name == "message")
            {
                // Get message
                message = currentNode.InnerText;
                // Write message
                Write(message);
            }
            else if(currentNode.Name == "answer")
            {
                // Get choice texts
                string choice1 = currentNode.SelectSingleNode("./choice1").InnerText;
                string choice2 = currentNode.SelectSingleNode("./choice2").InnerText;
                // Set text buttons
                choiceText1.text = choice1;
                choiceText2.text = choice2;
                // Enable button
                EnableButtons();
            }
            // Go to next node
            nodeIndex += 1;
        }
    }

    private void TypeLetter(char letter)
    {
        // Play a typing clip
        PlayTypingSound();
        // Add a letter to the text
        text.text += letter;
    }

    private void Clear()
    {
        // Empty text
        text.text = "";
    }

    public void Write(string text, float min_wait = MIN_TYPING_WAIT, float max_wait = MAX_TYPING_WAIT)
    {
        // Start typing
        typing = true;
        // Create writing coroutine
        writingCoroutine = SlowTypeText(text.ToCharArray(), min_wait, max_wait);
        // Start coroutine
        StartCoroutine(writingCoroutine);
    }

    public void Stop()
    {
        // Stop typing
        typing = false;
        // Stop writing coroutine
        StopCoroutine(writingCoroutine);
        // Set text
        text.text = message;
    }

    private IEnumerator SlowTypeText(char[] text, float min_wait, float max_wait)
    {
        // Wait before starting to type
        yield return new WaitForSeconds(0.4f);
        // Iterate through all letters
        for (int i = 0; i < text.Length; ++i)
        {
            // Type the letter in the text
            TypeLetter(text[i]);
            // Pick a random time
            float waitTime = Random.Range(min_wait, max_wait);
            // Wait given time before typing next letter
            yield return new WaitForSeconds(waitTime);
        }
        typing = false;
    }
    
    // Buttons
    private void OnChoiceClicked()
    {
        // Disable the buttons
        DisableButtons();
        // Go to next node
        nextNode = true;
    }

    private void DisableButtons()
    {
        // Disable button canvas
        buttonsCanvas.enabled = false;
        // Disable buttons
        button1.interactable = false;
        button2.interactable = false;
    }

    private void EnableButtons()
    {
        // Enable buttons
        button1.interactable = true;
        button2.interactable = true;
        // Disable button canvas
        buttonsCanvas.enabled = true;
        // Select first button by default
        button1.Select();
    }

    // Audio
    private void PlayTypingSound()
    {
        // If typing audio clips are set
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