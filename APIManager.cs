using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Linq;

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatRequest
{
    public string model = "deepseek-chat";
    public List<Message> messages;
    public int max_tokens = 150;
    public double temperature = 0.7;
}

[System.Serializable]
public class ChatResponse
{
    public List<Choice> choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

public class APIManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiKey = "your-deepseek-api-key";
    private string apiUrl = "https://api.deepseek.com/v1/chat/completions";
    
    [Header("Conversation Settings")]
    public List<Message> conversationHistory = new List<Message>();
    
    [Header("Word Limit Settings")]
    public int maxQuestionWords = 90;
    public int maxOptionWords = 10;
    
    [Header("Scene Management")]
    public List<SceneLocation> sceneLocations = new List<SceneLocation>();
    public int currentSceneIndex = 0;
    public int questionsInCurrentScene = 0;
    
    [Header("Scene Transition Settings")]
    public float transitionDistance = 8f; // 场景转换距离阈值
    
    private static APIManager _instance;
    public static APIManager Instance => _instance;
    
    [System.Serializable]
    public class SceneLocation
    {
        public string sceneName;
        public Vector2 coordinates;
        public string description;
        public int targetQuestionCount;
        public bool completed = false;
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeSceneLocations();
        InitializeSystemPrompt();
    }
    
    private void InitializeSceneLocations()
    {
        // 初始化所有场景位置信息
        sceneLocations = new List<SceneLocation>
        {
            new SceneLocation { 
                sceneName = "Wooden Bridge", 
                coordinates = new Vector2(132, 231),
                description = "Starting point at the wooden bridge",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Stone Pagoda", 
                coordinates = new Vector2(153, 344),
                description = "Buddhist stone pagoda at crossroads",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Song-style Building", 
                coordinates = new Vector2(145, 377),
                description = "Song-style building with stone lions",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Japanese Courtyard", 
                coordinates = new Vector2(143, 410),
                description = "Japanese courtyard with cherry blossoms",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Torii Gate", 
                coordinates = new Vector2(200, 424),
                description = "Japanese torii gate by wooden bridge",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Chinese Round Arch", 
                coordinates = new Vector2(196, 360),
                description = "Chinese round arch with lanterns",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Bamboo Path", 
                coordinates = new Vector2(265, 371),
                description = "Bamboo path with cultural symbolism",
                targetQuestionCount = 3
            },
            new SceneLocation { 
                sceneName = "Mountain Villa", 
                coordinates = new Vector2(341, 434),
                description = "Mountain villa with lake view",
                targetQuestionCount = 3
            }
        };
    }
    
    private void InitializeSystemPrompt()
    {
        string sceneInfo = "Available scenes and coordinates:\n";
        foreach (var scene in sceneLocations)
        {
            sceneInfo += $"- {scene.sceneName}: {scene.description} (Coordinates: {scene.coordinates.x}, {scene.coordinates.y})\n";
        }
        
        string systemPrompt = $@"You are a knowledgeable Chinese scholar specializing in Asian traditional architecture and aesthetic philosophy.

CRITICAL REQUIREMENTS:
1. You MUST ask exactly 3 questions in each scene before moving to the next location
2. Keep track internally: Current scene: {sceneLocations[currentSceneIndex].sceneName}, Questions asked: {questionsInCurrentScene}/3
3. IMPORTANT: After 3 questions, player must also move close to next location (within 8 units) to proceed
4. Do NOT generate questions asking players to choose the next scene
5. Guide players strictly in this fixed order: Wooden Bridge → Stone Pagoda → Song-style Building → Japanese Courtyard → Torii Gate → Chinese Round Arch → Bamboo Path → Mountain Villa
6. NEVER mention question count or progress in your dialogue with the player. Keep all tracking internal.

STRICT WORD LIMITS:
- Keep each question + introduction UNDER {maxQuestionWords} words total
- Keep each option UNDER {maxOptionWords} words
- Be concise and focused on core concepts

ARCHITECTURAL FOCUS:
- Emphasize harmony with nature, material authenticity, spiritual symbolism
- Compare Asian vs Western approaches when relevant
- Focus on aesthetic essence, not technical details

FORMAT REQUIREMENTS:
1. Always provide exactly 4 options labeled A), B), C), D)
2. Options C and D can sense emotional responses
3. After 3 questions AND player is near next location, guide to next scene
4. Use clear coordinates and directions
5. Never mention question counts or progress tracking in dialogue

SCENE GUIDE:
{sceneInfo}

Remember: You must ask 3 questions per scene, then guide player to move to next location. Keep all tracking internal.";

        conversationHistory.Add(new Message { 
            role = "system", 
            content = systemPrompt 
        });
    }
    
    public void SendMessageToNPC(string userMessage, System.Action<string> callback)
    {
        StartCoroutine(SendChatRequest(userMessage, callback));
    }
    
    private IEnumerator SendChatRequest(string userMessage, System.Action<string> callback)
    {
        // 检查词数并截断过长的用户消息
        if (CountWords(userMessage) > maxOptionWords * 2)
        {
            userMessage = TruncateToWordLimit(userMessage, maxOptionWords * 2);
        }
        
        // Add user message to history
        conversationHistory.Add(new Message { 
            role = "user", 
            content = userMessage 
        });
        
        ChatRequest requestData = new ChatRequest
        {
            messages = conversationHistory
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                if (response.choices != null && response.choices.Count > 0)
                {
                    string npcResponse = response.choices[0].message.content;
                    
                    // 检查并修正词数限制
                    npcResponse = EnsureWordLimits(npcResponse);
                    
                    // 检查并移除任何可能的问题计数引用
                    npcResponse = RemoveQuestionCountReferences(npcResponse);
                    
                    // Add NPC response to history
                    conversationHistory.Add(new Message { 
                        role = "assistant", 
                        content = npcResponse 
                    });
                    
                    // Update question count
                    questionsInCurrentScene++;
                    Debug.Log($"Question count updated: {questionsInCurrentScene}/3 in scene {sceneLocations[currentSceneIndex].sceneName}");
                    
                    callback?.Invoke(npcResponse);
                }
            }
            else
            {
                Debug.LogError($"API Request Failed: {request.error}");
                callback?.Invoke("My apologies, I seem to have lost my train of thought. Shall we continue?");
            }
        }
    }
    
    // 移除问题计数引用的方法
    private string RemoveQuestionCountReferences(string text)
    {
        string[] countPhrases = {
            "first question", "second question", "third question",
            "1st question", "2nd question", "3rd question",
            "question 1", "question 2", "question 3",
            "one question", "two questions", "three questions",
            "1/3", "2/3", "3/3",
            "first of three", "second of three", "third of three"
        };
        
        string result = text;
        foreach (string phrase in countPhrases)
        {
            result = result.Replace(phrase, "", System.StringComparison.OrdinalIgnoreCase);
        }
        
        return result;
    }
    
    // 词数统计方法
    private int CountWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Split(new char[] { ' ', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
    }
    
    // 确保响应符合词数限制
    private string EnsureWordLimits(string response)
    {
        string[] parts = response.Split('\n');
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        
        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            
            if (trimmed.StartsWith("A)") || trimmed.StartsWith("B)") || trimmed.StartsWith("C)") || trimmed.StartsWith("D)"))
            {
                // 处理选项词数限制
                if (CountWords(trimmed) > maxOptionWords)
                {
                    trimmed = TruncateToWordLimit(trimmed, maxOptionWords);
                }
            }
            else
            {
                // 处理问题部分词数限制
                if (CountWords(result.ToString() + " " + trimmed) > maxQuestionWords)
                {
                    break; // 达到词数限制，停止添加
                }
            }
            
            if (result.Length > 0) result.Append("\n");
            result.Append(trimmed);
        }
        
        return result.ToString();
    }
    
    // 截断文本到指定词数
    private string TruncateToWordLimit(string text, int wordLimit)
    {
        string[] words = text.Split(new char[] { ' ', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= wordLimit) return text;
        
        return string.Join(" ", words.Take(wordLimit)) + "...";
    }
    
    public void MoveToNextScene()
    {
        if (currentSceneIndex < sceneLocations.Count - 1)
        {
            Debug.Log($"Moving from {sceneLocations[currentSceneIndex].sceneName} to {sceneLocations[currentSceneIndex + 1].sceneName}");
            sceneLocations[currentSceneIndex].completed = true;
            currentSceneIndex++;
            questionsInCurrentScene = 0;
            UpdateSystemPromptForNewScene();
        }
        else
        {
            // All scenes completed
            Debug.Log("All scenes completed!");
            CompleteJourney();
        }
    }
    
    private void UpdateSystemPromptForNewScene()
    {
        // Update system prompt with new scene information
        string sceneInfo = "Available scenes and coordinates:\n";
        foreach (var scene in sceneLocations)
        {
            string status = scene.completed ? "[COMPLETED] " : scene.sceneName == sceneLocations[currentSceneIndex].sceneName ? "[CURRENT] " : "";
            sceneInfo += $"- {status}{scene.sceneName}: {scene.description} (Coordinates: {scene.coordinates.x}, {scene.coordinates.y})\n";
        }
        
        string updatedPrompt = $@"[SCENE UPDATE] Now at: {sceneLocations[currentSceneIndex].sceneName}
Previous scene completed. Questions reset to 0.

CRITICAL REQUIREMENTS:
1. You MUST ask exactly 3 questions in this scene before moving to the next location
2. Current question count: 0/3 (INTERNAL ONLY - DO NOT SHOW)
3. After 3 questions AND player is near next location (within {transitionDistance} units), guide to next scene
4. Follow fixed sequence: Wooden Bridge → Stone Pagoda → Song-style Building → Japanese Courtyard → Torii Gate → Chinese Round Arch → Bamboo Path → Mountain Villa
5. IMPORTANT: NEVER mention question count or progress in dialogue with player

WORD LIMITS:
- Question + intro: {maxQuestionWords} words max
- Each option: {maxOptionWords} words max

{sceneInfo}

Continue with concise architectural guidance. Remember: 3 questions per scene AND player must move close to next location.";

        // Find and update system message
        for (int i = 0; i < conversationHistory.Count; i++)
        {
            if (conversationHistory[i].role == "system")
            {
                conversationHistory[i].content = updatedPrompt;
                break;
            }
        }
        
        Debug.Log($"System prompt updated for {sceneLocations[currentSceneIndex].sceneName}");
    }
    
    private void CompleteJourney()
    {
        string completionMessage = "Journey complete. Provide brief summary and ask about replay.";
        
        conversationHistory.Add(new Message {
            role = "user",
            content = completionMessage
        });
    }
    
    // 新增：检查是否应该移动到下一个场景（双重条件）
    public bool ShouldMoveToNextScene(Vector3 playerPosition)
    {
        // 条件1：问题计数达到3个
        bool questionsCompleted = questionsInCurrentScene >= sceneLocations[currentSceneIndex].targetQuestionCount;
        
        // 条件2：玩家靠近下一个场景
        bool playerNearNextScene = IsPlayerNearNextScene(playerPosition);
        
        bool shouldMove = questionsCompleted && playerNearNextScene;
        
        Debug.Log($"Should move to next scene: {shouldMove} " +
                  $"(Questions: {questionsCompleted}, " +
                  $"(Player near: {playerNearNextScene}, Distance: {GetDistanceToNextScene(playerPosition)})");
        
        return shouldMove;
    }
    
    // 新增：检查玩家是否靠近下一个场景
    public bool IsPlayerNearNextScene(Vector3 playerPosition)
    {
        // 如果是最后一个场景，没有下一个场景
        if (currentSceneIndex >= sceneLocations.Count - 1)
        {
            return false;
        }
        
        // 获取下一个场景的坐标
        SceneLocation nextScene = sceneLocations[currentSceneIndex + 1];
        Vector2 playerPos2D = new Vector2(playerPosition.x, playerPosition.z);
        
        // 计算距离
        float distance = Vector2.Distance(playerPos2D, nextScene.coordinates);
        
        return distance <= transitionDistance;
    }
    
    // 新增：获取到下一个场景的距离
    public float GetDistanceToNextScene(Vector3 playerPosition)
    {
        if (currentSceneIndex >= sceneLocations.Count - 1)
        {
            return float.MaxValue;
        }
        
        SceneLocation nextScene = sceneLocations[currentSceneIndex + 1];
        Vector2 playerPos2D = new Vector2(playerPosition.x, playerPosition.z);
        
        return Vector2.Distance(playerPos2D, nextScene.coordinates);
    }
    
    // 新增：获取当前场景的指导信息
    public string GetCurrentSceneGuide(Vector3 playerPosition = default)
    {
        var current = sceneLocations[currentSceneIndex];
        var next = currentSceneIndex < sceneLocations.Count - 1 ? sceneLocations[currentSceneIndex + 1] : null;
        
        if (next != null)
        {
            // 检查玩家是否靠近下一个场景
            bool isNear = IsPlayerNearNextScene(playerPosition);
            float distance = GetDistanceToNextScene(playerPosition);
            
            if (isNear)
            {
                return $"Excellent! We've completed our exploration here. Now let's proceed to {next.description} at coordinates ({next.coordinates.x}, {next.coordinates.y}).";
            }
            else
            {
                return $"We've finished discussing this location. Please move closer to {next.description} at coordinates ({next.coordinates.x}, {next.coordinates.y}) to continue. " +
                       $"You're currently {distance:F1} units away. (Need to be within {transitionDistance} units)";
            }
        }
        
        return "We've reached our final destination. Thank you for joining me on this architectural journey.";
    }
    
    // 新增：检查问题是否完成（仅问题计数）
    public bool AreQuestionsCompleted()
    {
        return questionsInCurrentScene >= sceneLocations[currentSceneIndex].targetQuestionCount;
    }
    
    public void ResetForNewGame()
    {
        conversationHistory.Clear();
        currentSceneIndex = 0;
        questionsInCurrentScene = 0;
        foreach (var scene in sceneLocations)
        {
            scene.completed = false;
        }
        InitializeSystemPrompt();
    }
    
    // 调试方法
    public void DebugSceneStatus(Vector3 playerPosition = default)
    {
        Debug.Log($"Current Scene: {sceneLocations[currentSceneIndex].sceneName}");
        Debug.Log($"Questions in current scene: {questionsInCurrentScene}/{sceneLocations[currentSceneIndex].targetQuestionCount}");
        Debug.Log($"Player position: {playerPosition}");
        Debug.Log($"Distance to next scene: {GetDistanceToNextScene(playerPosition)}");
        Debug.Log($"Should move to next scene: {ShouldMoveToNextScene(playerPosition)}");
    }
}