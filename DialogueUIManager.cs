using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image npcPortrait;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    
    [Header("Option Buttons")]
    public GameObject[] optionButtons;
    public TextMeshProUGUI[] optionTexts;
    
    [Header("Animation")]
    public Animator dialogueAnimator;
    
    [Header("Feedback Colors")]
    public Color positiveColor = Color.green;
    public Color neutralColor = Color.white;
    public Color negativeColor = Color.red;
    
    private static DialogueUIManager _instance;
    public static DialogueUIManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Hide dialogue panel at start
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowDialogue(string npcName, string dialogue, Sprite portrait = null)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        npcNameText.text = npcName;
        dialogueText.text = dialogue;
        dialogueText.color = neutralColor;
        
        if (portrait != null && npcPortrait != null)
        {
            npcPortrait.sprite = portrait;
            npcPortrait.gameObject.SetActive(true);
        }
        else if (npcPortrait != null)
        {
            npcPortrait.gameObject.SetActive(false);
        }
            
        // Trigger show animation
        if (dialogueAnimator != null)
            dialogueAnimator.SetTrigger("Show");
    }
    
    public void SetupOptions(string[] options)
    {
        // 始终显示所有四个选项按钮
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length && !string.IsNullOrEmpty(options[i]))
            {
                optionButtons[i].SetActive(true);
                optionTexts[i].text = options[i];
            }
            else
            {
                // 如果选项为空，隐藏该按钮
                optionButtons[i].SetActive(false);
            }
        }
    }
    
    public void HideOptions()
    {
        foreach (var button in optionButtons)
        {
            if (button != null) button.SetActive(false);
        }
    }
    
    public void HideDialogue()
    {
        if (dialogueAnimator != null)
        {
            dialogueAnimator.SetTrigger("Hide");
            Invoke("DeactivatePanel", 0.5f);
        }
        else
        {
            DeactivatePanel();
        }
    }
    
    private void DeactivatePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowFeedback(FeedbackType feedback)
    {
        switch (feedback)
        {
            case FeedbackType.Positive:
                dialogueText.color = positiveColor;
                break;
            case FeedbackType.Neutral:
                dialogueText.color = neutralColor;
                break;
            case FeedbackType.Negative:
                dialogueText.color = negativeColor;
                break;
        }
        
        Invoke("ResetTextColor", 1.5f);
    }
    
    private void ResetTextColor()
    {
        dialogueText.color = neutralColor;
    }
    
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }
}

public enum FeedbackType
{
    Positive,
    Neutral,
    Negative
}