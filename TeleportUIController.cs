using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TeleportUIController : MonoBehaviour
{
    [Header("UI References")]
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI messageText;
    
    [Header("Teleport Target")]
    public Transform teleportTargetLocation;
    
    [Header("Keyboard Controls")]
    public KeyCode confirmKey = KeyCode.Y;
    public KeyCode cancelKey = KeyCode.N;
    
    [Header("UI Settings")]
    public string teleportMessage = "Teleport to the stone tower?";
    
    private GameObject player;
    private bool isUIVisible = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // 设置消息文本（包含按键提示）
        if (messageText != null)
            UpdateMessageText();
        
        // 绑定按钮事件
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmTeleport);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelTeleport);
        
        // 初始隐藏UI
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        // 只有UI可见时才检测键盘输入
        if (isUIVisible)
        {
            if (Input.GetKeyDown(confirmKey))
            {
                OnConfirmTeleport();
            }
            else if (Input.GetKeyDown(cancelKey))
            {
                OnCancelTeleport();
            }
        }
    }
    
    void UpdateMessageText()
    {
        if (messageText != null)
        {
            messageText.text = $"{teleportMessage}\nPress [{confirmKey}] to confirm or [{cancelKey}] to cancel";
        }
    }
    
    public void ShowUI(Transform targetLocation)
    {
        teleportTargetLocation = targetLocation;
        
        // 更新消息文本，包含目标位置名称
        if (messageText != null && targetLocation != null)
        {
            messageText.text = $"Teleport to {targetLocation.name}?\nPress [{confirmKey}] to confirm or [{cancelKey}] to cancel";
        }
        
        gameObject.SetActive(true);
        isUIVisible = true;
        
        // 可选：暂停游戏或限制玩家输入
        // Time.timeScale = 0f;
    }
    
    public void HideUI()
    {
        gameObject.SetActive(false);
        isUIVisible = false;
        Time.timeScale = 1f; // 恢复游戏时间
    }
    
    void OnConfirmTeleport()
    {
        if (player != null && teleportTargetLocation != null)
        {
            TeleportPlayer();
        }
        
        HideUI();
    }
    
    void OnCancelTeleport()
    {
        HideUI();
    }
    
    void TeleportPlayer()
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = teleportTargetLocation.position;
            player.transform.rotation = teleportTargetLocation.rotation;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = teleportTargetLocation.position;
            player.transform.rotation = teleportTargetLocation.rotation;
        }
        
        Debug.Log($"Player teleported to: {teleportTargetLocation.name} at position {teleportTargetLocation.position}");
    }
    
    // 公开方法用于自定义消息
    public void SetCustomMessage(string message)
    {
        teleportMessage = message;
        UpdateMessageText();
    }
    
    // 公开方法用于自定义按键
    public void SetCustomKeys(KeyCode confirm, KeyCode cancel)
    {
        confirmKey = confirm;
        cancelKey = cancel;
        UpdateMessageText();
    }
}