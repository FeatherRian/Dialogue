using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI nameText;
    
    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;

    [Header("NPC")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcAvatar;

    [Header("Player")]
    [SerializeField] private string playerName;
    [SerializeField] private Sprite playerAvatar;

    private  TextMeshProUGUI[] choicesText;
    private  string text;
    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private bool makeChoice , textIsFinished;
    private static DialogueManager instance;

    public float textSpeed; 

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (InputManager.GetInstance().GetInteractPressed())
        {
            if (makeChoice && textIsFinished)
            {
                makeChoice = false;
                nameText.text = npcName;
                avatar.sprite = npcAvatar;
            }
            if (textIsFinished)
            {
                ContinueStory();
            }
            else
            {
                StopCoroutine("DisPlayDialogue");
                dialogueText.text = text;
                textIsFinished = true;
            }
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, string n_name, Sprite n_avatar)
    {
        currentStory = new Story(inkJSON.text);
        npcName = n_name;
        npcAvatar = n_avatar;

        nameText.text = npcName;
        avatar.sprite = npcAvatar;

        textIsFinished = true;
        makeChoice = false; 
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        dialogueText.text = "";

        if (currentStory.canContinue)
        {
            text = currentStory.Continue();
            //dialogueText.text = currentStory.Continue();
            StartCoroutine("DisPlayDialogue");
            DisPlayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = "";
            text = currentStory.Continue();
            //dialogueText.text = currentStory.Continue();
            StartCoroutine("DisPlayDialogue");
            DisPlayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    public void DisPlayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " 
            + currentChoices.Count);
        }

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        } 

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (textIsFinished)
        {
            StartCoroutine(turnChoice());
            nameText.text = playerName;
            avatar.sprite = playerAvatar;
            currentStory.ChooseChoiceIndex(choiceIndex);
        }
    }

    public IEnumerator turnChoice()
    {
        yield return new WaitForSeconds(0.2f); 
        makeChoice = true;
    }

    public  IEnumerator DisPlayDialogue()
    {
        textIsFinished = false;
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];

            yield return new WaitForSeconds(textSpeed);
        }
        textIsFinished = true;
    }

}
