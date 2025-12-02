using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Configuration")]
    public float typingSpeed = 0.03f;
    
    [Header("Fixed Dialogue System")]
    public bool useFixedFirstResponse = true;
    public FixedDialogueData fixedFirstDialogue;
    
    [System.Serializable]
    public class FixedDialogueData
    {
        public string fixedQuestion;
        public FixedResponse[] fixedResponses;
    }
    
    [System.Serializable]
    public class FixedResponse
    {
        public string optionText;
        public string npcResponse;
        public FeedbackType feedbackType;
    }
    
    private NPCController currentNPC;
    private bool isWaitingForResponse = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool dialogueActive = false;
    private bool isFirstInteraction = true;
    private string currentFullText = "";
    private GameObject player; // 引用玩家对象
    
    private static DialogueManager _instance;
    public static DialogueManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 获取玩家对象
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (DialogueUIManager.Instance != null && DialogueUIManager.Instance.dialoguePanel.activeInHierarchy)
        {
            DialogueUIManager.Instance.HideDialogue();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentNPC != null && !dialogueActive)
        {
            StartDialogue(currentNPC);
        }
        
        if (dialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            HideDialogueUI();
        }
        
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            SkipTyping();
        }
        
        if (dialogueActive && !isWaitingForResponse && !isTyping)
        {
            if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && IsOptionActive(0))
                SelectOption(0);
            else if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && IsOptionActive(1))
                SelectOption(1);
            else if ((Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) && IsOptionActive(2))
                SelectOption(2);
            else if ((Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) && IsOptionActive(3))
                SelectOption(3);
        }
        
        // 调试快捷键
        if (Input.GetKeyDown(KeyCode.F5) && player != null)
        {
            APIManager.Instance.DebugSceneStatus(player.transform.position);
        }
    }
    
    private bool IsOptionActive(int index)
    {
        return index >= 0 && index < DialogueUIManager.Instance.optionButtons.Length && 
               DialogueUIManager.Instance.optionButtons[index].activeInHierarchy;
    }
    
    public void StartDialogue(NPCController npc)
    {
        if (isWaitingForResponse) return;
        
        currentNPC = npc;
        dialogueActive = true;
        
        DialogueUIManager.Instance.ShowDialogue(npc.npcName, "", npc.GetComponentInChildren<SpriteRenderer>()?.sprite);
        
        if (isFirstInteraction && useFixedFirstResponse && fixedFirstDialogue != null)
        {
            ShowFixedFirstDialogue();
        }
        else
        {
            // 检查是否需要场景转换（双重条件）
            if (player != null && APIManager.Instance.ShouldMoveToNextScene(player.transform.position))
            {
                Debug.Log("Starting scene transition - both conditions met");
                ShowSceneTransition();
            }
            else
            {
                // 检查是否是场景的第一个问题
                if (APIManager.Instance.questionsInCurrentScene == 0)
                {
                    ShowSceneIntroduction();
                }
                else
                {
                    // 检查是否完成问题但玩家不在下一个场景附近
                    if (APIManager.Instance.AreQuestionsCompleted() && player != null)
                    {
                        // 问题已完成，但玩家不在下一个场景附近
                        ShowLocationGuidance();
                    }
                    else
                    {
                        // 继续当前对话
                        ContinueDialogue();
                    }
                }
            }
        }
    }
    
    private void ShowFixedFirstDialogue()
    {
        string fixedQuestion = fixedFirstDialogue.fixedQuestion;
        currentFullText = fixedQuestion;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fixedQuestion));
        
        string[] options = new string[4];
        for (int i = 0; i < fixedFirstDialogue.fixedResponses.Length && i < 4; i++)
        {
            options[i] = fixedFirstDialogue.fixedResponses[i].optionText;
        }
        
        DialogueUIManager.Instance.SetupOptions(options);
    }
    
    public void SelectOption(int optionIndex)
    {
        if (isWaitingForResponse || isTyping) return;
        
        Debug.Log($"Option {optionIndex} selected");
        
        if (isFirstInteraction && useFixedFirstResponse && fixedFirstDialogue != null)
        {
            HandleFixedResponse(optionIndex);
            return;
        }
        
        string selectedOption = GetOptionText(optionIndex);
        if (!string.IsNullOrEmpty(selectedOption))
        {
            string playerChoiceText = $"You: {selectedOption}";
            DialogueUIManager.Instance.SetDialogueText(playerChoiceText);
            
            isWaitingForResponse = true;
            DialogueUIManager.Instance.HideOptions();
            
            APIManager.Instance.SendMessageToNPC(selectedOption, OnNPCResponse);
        }
        else
        {
            Debug.LogWarning($"No option text found for index {optionIndex}");
        }
    }
    
    private void HandleFixedResponse(int optionIndex)
    {
        if (optionIndex >= 0 && optionIndex < fixedFirstDialogue.fixedResponses.Length)
        {
            FixedResponse response = fixedFirstDialogue.fixedResponses[optionIndex];
            currentFullText = response.npcResponse;
            
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(response.npcResponse));
            
            DialogueUIManager.Instance.ShowFeedback(response.feedbackType);
            AddFixedDialogueToHistory(optionIndex);
            DialogueUIManager.Instance.HideOptions();
            isFirstInteraction = false;
            
            StartCoroutine(ContinueAfterFixedResponse(2f));
        }
    }
    
    private void AddFixedDialogueToHistory(int selectedOptionIndex)
    {
        string userMessage = fixedFirstDialogue.fixedResponses[selectedOptionIndex].optionText;
        string assistantMessage = fixedFirstDialogue.fixedResponses[selectedOptionIndex].npcResponse;
        
        APIManager.Instance.conversationHistory.Add(new Message {
            role = "user",
            content = userMessage
        });
        
        APIManager.Instance.conversationHistory.Add(new Message {
            role = "assistant", 
            content = assistantMessage
        });
        
        APIManager.Instance.questionsInCurrentScene++;
    }
    
    private IEnumerator ContinueAfterFixedResponse(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 检查是否需要场景转换（双重条件）
        if (player != null && APIManager.Instance.ShouldMoveToNextScene(player.transform.position))
        {
            ShowSceneTransition();
        }
        else
        {
            // 检查是否完成问题但玩家不在下一个场景附近
            if (APIManager.Instance.AreQuestionsCompleted() && player != null)
            {
                ShowLocationGuidance();
            }
            else
            {
                // 使用AI生成下一个问题
                string continuePrompt = "Please ask the next question about this location's architectural significance.";
                isWaitingForResponse = true;
                APIManager.Instance.SendMessageToNPC(continuePrompt, OnNPCResponse);
            }
        }
    }
    
    private void ShowSceneIntroduction()
    {
        string sceneIntro = GetSceneIntroduction();
        currentFullText = sceneIntro;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(sceneIntro));
        
        string[] options = ExtractOptionsFromText(sceneIntro);
        DialogueUIManager.Instance.SetupOptions(options);
        
        Debug.Log($"Started new scene: {APIManager.Instance.currentSceneIndex}");
    }
    
    private void ShowSceneTransition()
    {
        string transitionText = player != null 
            ? APIManager.Instance.GetCurrentSceneGuide(player.transform.position) 
            : APIManager.Instance.GetCurrentSceneGuide();
        
        currentFullText = transitionText;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(transitionText));
        
        // 延迟后移动到下一个场景
        StartCoroutine(MoveToNextSceneAfterDelay(2f));
    }
    
    private IEnumerator MoveToNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 移动到下一个场景
        APIManager.Instance.MoveToNextScene();
        
        // 显示新场景的介绍
        ShowSceneIntroduction();
    }
    
    // 新增：显示位置引导（当完成问题但玩家不在下一个场景附近时）
    private void ShowLocationGuidance()
    {
        string guidanceText = player != null 
            ? APIManager.Instance.GetCurrentSceneGuide(player.transform.position) 
            : "Please move closer to the next location to continue our journey.";
        
        currentFullText = guidanceText;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(guidanceText));
        
        // 隐藏选项按钮，因为这只是引导信息
        DialogueUIManager.Instance.HideOptions();
        
        Debug.Log("Showing location guidance - questions completed but player not near next scene");
    }
    
    private void ContinueDialogue()
    {
        // 显示当前对话的最后一个消息
        if (APIManager.Instance.conversationHistory.Count > 0)
        {
            var lastMessage = APIManager.Instance.conversationHistory[APIManager.Instance.conversationHistory.Count - 1];
            if (lastMessage.role == "assistant")
            {
                currentFullText = lastMessage.content;
                DialogueUIManager.Instance.SetDialogueText(ExtractMainText(lastMessage.content));
                string[] options = ExtractOptionsFromText(lastMessage.content);
                DialogueUIManager.Instance.SetupOptions(options);
            }
        }
    }
    
    private string GetSceneIntroduction()
    {
        int sceneIndex = APIManager.Instance.currentSceneIndex;
        
        switch (sceneIndex)
        {
            case 0: // Wooden Bridge
                return "Welcome. I'm Scholar Wei. This wooden bridge shows Asian harmony with nature. Curves work with water flow, not against it. What interests you most?\n\nA) Bridge construction methods\nB) Asian vs Western design\nC) Feeling peaceful here\nD) Natural materials beauty";
            
            case 1: // Stone Pagoda
                return "This stone pagoda embodies spiritual geometry. Each tier represents enlightenment stages. Unlike Western towers reaching skyward, pagodas accumulate energy gently. Notice the peaceful presence?\n\nA) Tier symbolism meaning\nB) Material spiritual values\nC) Peaceful feeling here\nD) Craftsmanship details";
                
            case 2: // Song-style Building
                return "Song-style architecture shows refined proportions. Elegant roof curves and stone lions guard spiritually. Integration with landscape is key. What catches your attention?\n\nA) Song aesthetic principles\nB) Lion symbolism meaning\nC) Balanced space feeling\nD) Nature integration";
                
            case 3: // Japanese Courtyard
                return "Japanese courtyard demonstrates wabi-sabi beauty. Cherry blossoms show life's transience. Architecture respects material authenticity. Intentional asymmetry creates harmony. Your thoughts?\n\nA) Wabi-sabi expression\nB) Tang-Japan connections\nC) Emotional cherry response\nD) Intimate space feeling";
                
            case 4: // Torii Gate
                return "Torii gates mark sacred transition. Simple structure, deep meaning. Passage represents spiritual journey. Framing landscape intentionally. Your experience?\n\nA) Spiritual significance\nB) Ceremonial design\nC) Otherworldly feeling\nD) Simple aesthetics";
                
            case 5: // Chinese Round Arch
                return "Round arch symbolizes harmony. Circular form suggests continuity. Lanterns light physical and spiritual paths. Every element meaningful here. What interests you?\n\nA) Circular form meaning\nB) Lantern evolution\nC) Framed view beauty\nD) Function symbolism blend";
                
            case 6: // Bamboo Path
                return "Bamboo represents Asian values. Flexibility with strength. Hollow centers mean humility. Teaches resilience and grace. Walking here, what do you feel?\n\nA) Construction uses\nB) Philosophical lessons\nC) Soothing sounds\nD) Cultural reverence";
                
            case 7: // Mountain Villa
                return "Mountain retreats offer perspective. Scholars sought wisdom, not conquest. This view represents life's journey from wisdom's height. Asian philosophy values nature contemplation. Your reflection?\n\nA) Scholar retreat reasons\nB) Landscape philosophy\nC) Perspective feeling\nD) Peaceful experience";
                
            default:
                return "Our architectural journey continues. Brief insights about current surroundings. Cultural significance revealed.";
        }
    }
    
    private void OnNPCResponse(string response)
    {
        isWaitingForResponse = false;
        currentFullText = response;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(response));
        
        string[] options = ExtractOptionsFromText(response);
        DialogueUIManager.Instance.SetupOptions(options);
        
        AnalyzeAndShowFeedback(response);
        
        Debug.Log($"Question count: {APIManager.Instance.questionsInCurrentScene}/3");
    }
    
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        string mainText = ExtractMainText(text);
        DialogueUIManager.Instance.SetDialogueText("");
        
        foreach (char character in mainText.ToCharArray())
        {
            DialogueUIManager.Instance.SetDialogueText(DialogueUIManager.Instance.dialogueText.text + character);
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    private string ExtractMainText(string text)
    {
        string[] lines = text.Split('\n');
        System.Text.StringBuilder mainText = new System.Text.StringBuilder();
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("A)") && !trimmedLine.StartsWith("B)") && 
                !trimmedLine.StartsWith("C)") && !trimmedLine.StartsWith("D)"))
            {
                mainText.AppendLine(trimmedLine);
            }
        }
        
        return mainText.ToString().Trim();
    }
    
    private string[] ExtractOptionsFromText(string text)
    {
        string[] options = new string[4];
        string[] lines = text.Split('\n');
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("A)")) options[0] = trimmedLine;
            else if (trimmedLine.StartsWith("B)")) options[1] = trimmedLine;
            else if (trimmedLine.StartsWith("C)")) options[2] = trimmedLine;
            else if (trimmedLine.StartsWith("D)")) options[3] = trimmedLine;
        }
        
        return options;
    }
    
    private string GetOptionText(int optionIndex)
    {
        if (optionIndex >= 0 && optionIndex < DialogueUIManager.Instance.optionButtons.Length && 
            DialogueUIManager.Instance.optionButtons[optionIndex].activeInHierarchy)
        {
            return DialogueUIManager.Instance.optionTexts[optionIndex].text;
        }
        return null;
    }
    
    private void AnalyzeAndShowFeedback(string response)
    {
        response = response.ToLower();
        
        if (response.Contains("excellent") || response.Contains("insightful") || 
            response.Contains("well chosen") || response.Contains("perceptive"))
        {
            DialogueUIManager.Instance.ShowFeedback(FeedbackType.Positive);
        }
        else if (response.Contains("interesting") || response.Contains("thoughtful"))
        {
            DialogueUIManager.Instance.ShowFeedback(FeedbackType.Neutral);
        }
        else
        {
            DialogueUIManager.Instance.ShowFeedback(FeedbackType.Neutral);
        }
    }
    
    public void HideDialogueUI()
    {
        dialogueActive = false;
        DialogueUIManager.Instance.HideDialogue();
    }
    
    public void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
            
        dialogueActive = false;
        DialogueUIManager.Instance.HideDialogue();
        isWaitingForResponse = false;
        isTyping = false;
    }
    
    private void SkipTyping()
    {
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
            DialogueUIManager.Instance.SetDialogueText(ExtractMainText(currentFullText));
        }
    }
    
    public bool IsInDialogue()
    {
        return dialogueActive;
    }
    
    public void SetCurrentNPC(NPCController npc)
    {
        currentNPC = npc;
    }
    
    public void RestartJourney()
    {
        APIManager.Instance.ResetForNewGame();
        isFirstInteraction = true;
        if (currentNPC != null)
        {
            StartDialogue(currentNPC);
        }
    }
    
    public void SetFirstInteraction(bool isFirst)
    {
        isFirstInteraction = isFirst;
    }
}