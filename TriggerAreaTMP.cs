using UnityEngine;
using TMPro;

public class TriggerAreaTMP : MonoBehaviour
{
    public GameObject uiPanel; // 在Inspector中分配的UI面板
    public TextMeshProUGUI messageText; // 在Inspector中分配的TMP文本组件
    public string message = "Please approach the torii ahead to trigger the teleportation.";
    
    private bool playerInRange = false;
    
    void Start()
    {
        // 开始时隐藏UI
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 检查进入触发器的是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowMessage();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // 检查离开触发器的是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideMessage();
        }
    }
    
    void ShowMessage()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
        
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        Debug.Log("显示提示信息: " + message);
    }
    
    void HideMessage()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        
        Debug.Log("隐藏提示信息");
    }
}