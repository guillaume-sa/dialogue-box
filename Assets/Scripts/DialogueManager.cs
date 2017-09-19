using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class DialogueManager : MonoBehaviour
{
    private static GameObject dialogueManagerGO;    // Dialogue Manager Game Object
    public static DialogueManager instance = null;  // Dialogue Manager singleton instance
    
    // Parameters
    private const float MIN_TYPING_WAIT = 0.02f;    // Minimum waiting time before next letter
    private const float MAX_TYPING_WAIT = 0.1f;     // Maximum waiting time before next letter

    // UI 
    private Text text;                              // Text UI
    private Text choiceText1;                       // Button 1 text UI
    private Text choiceText2;                       // Button 2 text UI
    private Canvas buttonsCanvas;                   // Buttons Canvas
    private Button button1;                         // Button 1
    private Button button2;                         // Button 2 
    private Image icon;                             // Icon UI

    // Audio
    public AudioClip[] typingClips;                 // Typing audio clips

    // XML parsing
    private IEnumerator writingCoroutine;           // Writing cooroutine
    private string message;                         // Message read from XML file
    private XmlNodeList nodes;                      // XML nodes read from file
    private XmlNode currentNode;                    // Current XML node
    private int nodeIndex = 0;                      // Index of current XML node
    private bool nextNode;                          // Ready to read the next node
    private bool typing;                            // Is message being typed
    private bool answering;                         // Is user answering

    private List<string> answers;                   // List of answers selected by the user
    public List<string> Answers                     // Answers property
    {
        get { return answers; }
    }

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a DialogueManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    public void Start()
    {
        // Get Dialogue Manager
        dialogueManagerGO = GameObject.Find("Dialogue Manager");
        // Get UI components
        text = GameObject.Find("Dialogue Manager/Text").GetComponent<Text>();
        icon = GameObject.Find("Dialogue Manager/Icon").GetComponent<Image>();
        buttonsCanvas = GameObject.Find("Dialogue Manager/ChoiceButtons").GetComponent<Canvas>();
        Button[] buttons = buttonsCanvas.GetComponentsInChildren<Button>();
        button1 = buttons[0];
        button2 = buttons[1];
        choiceText1 = button1.GetComponentInChildren<Text>();
        choiceText2 = button2.GetComponentInChildren<Text>();
        // Manage UI components
        button1.onClick.AddListener(OnChoiceClicked);
        button2.onClick.AddListener(OnChoiceClicked);
        
        dialogueManagerGO.SetActive(false);
    }

    public void Dialogue(TextAsset xmlDialogFile)
    {
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
        answering = false;
        answers = new List<string>();
        dialogueManagerGO.SetActive(true);
        DisableButtons();
    }

    // Update is called once per frame
    void Update()
    {
        // User is answering
        if (answering)
        {
            return;
        }

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
                // waiting for player to answer
                answering = true;
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
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        if (buttonName.Equals("ChoiceButton1"))
        {
            answers.Add(currentNode.SelectSingleNode("./choice1").Attributes["value"].Value);
        }else if (buttonName.Equals("ChoiceButton2"))
        {
            answers.Add(currentNode.SelectSingleNode("./choice2").Attributes["value"].Value);
        }
        answering = false;
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