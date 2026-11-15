using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueNode
{
    public int id;
    public string speakerName;
    public string text;
    public bool activeSide; //false = left, true = right
}

[System.Serializable]
public class DialogueData
{
    public string sceneId;
    public DialogueNode[] dialogues;
}

public class Dialog : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftAvatar;
    [SerializeField] private Image rightAvatar;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Setting")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private Color avatorActiveColor = Color.white;
    [SerializeField] private Color avatorInActiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private DialogueData currentData;
    private int currentIndex;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public event System.Action OnDialogEnd;

    public static Dialog Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// If the Dialog is currently typing, skip the typing process and display the full sentence, else display next dialogue.
    /// </summary>
    public void TryNextDialogue()
    {
        if (isTyping)
        {
            CompleteCurrentLine();
        }
        else
        {
            DisplayNextLine();
        }
    }

    /// <summary>
    /// The function for starting a Dialog
    /// </summary>
    /// <param name="jsonText">Json Text for dialog</param>
    public void StartDialogue(string jsonText)
    {
        currentData = JsonUtility.FromJson<DialogueData>(jsonText);
        currentIndex = 0;
        dialoguePanel.SetActive(true);

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if(currentIndex >= currentData.dialogues.Length)
        {
            this.EndDialogue();
            return;
        }
        DialogueNode node = currentData.dialogues[currentIndex];
        nameText.text = node.speakerName;
        if(node.activeSide) {
            leftAvatar.color = avatorActiveColor;
            rightAvatar.color = avatorInActiveColor;
        }
        else {
            leftAvatar.color = avatorInActiveColor;
            rightAvatar.color = avatorActiveColor;
        }
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(node.text));

        currentIndex++;
    }

    private IEnumerator TypeSentence(String sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        
        for(int i=0;i<sentence.Length;i++)
        {
            char letter = sentence[i];
            if(letter == '<' && sentence.IndexOf('>', i) != -1)
            {
                dialogueText.text += sentence.Substring(i, sentence.IndexOf('>', i)-i+1);
                i = sentence.IndexOf('>', i);
            }
            else dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        DialogueNode node = currentData.dialogues[currentIndex - 1];
        dialogueText.text = node.text;
        
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        OnDialogEnd?.Invoke();
    }
}
