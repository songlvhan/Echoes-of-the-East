using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDirectionDisplay : MonoBehaviour
{
    [Header("玩家对象")]
    public Transform playerTransform;
    
    [Header("UI组件")]
    public TextMeshProUGUI directionText;
    public Image directionIndicator;
    public RectTransform indicatorRect;
    
    [Header("显示设置")]
    public bool useCardinalDirections = true; // 使用基本方向(北/东/南/西)
    public bool showAngle = true; // 显示角度值
    public bool showVector = false; // 显示方向向量
    public float updateInterval = 0.1f;
    
    [Header("指示器设置")]
    public float indicatorSize = 50f;
    public Color indicatorColor = Color.white;
    
    private float timer = 0f;
    private readonly string[] cardinalDirections = { "北", "东北", "东", "东南", "南", "西南", "西", "西北" };
    
    private void Start()
    {
        // 自动查找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        // 获取组件引用
        if (directionIndicator != null)
        {
            indicatorRect = directionIndicator.GetComponent<RectTransform>();
            directionIndicator.color = indicatorColor;
            
            // 设置指示器大小
            if (indicatorRect != null)
            {
                indicatorRect.sizeDelta = new Vector2(indicatorSize, indicatorSize);
            }
        }
        
        if (directionText == null)
            directionText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval && playerTransform != null)
        {
            UpdateDirectionDisplay();
            timer = 0f;
        }
    }
    
    private void UpdateDirectionDisplay()
    {
        // 获取玩家前方方向在XZ平面上的投影
        Vector3 forwardXZ = new Vector3(playerTransform.forward.x, 0, playerTransform.forward.z).normalized;
        
        // 更新方向指示器旋转
        if (indicatorRect != null)
        {
            float angle = Mathf.Atan2(forwardXZ.x, forwardXZ.z) * Mathf.Rad2Deg;
            indicatorRect.rotation = Quaternion.Euler(0, 0, -angle);
        }
        
        // 更新方向文本
        if (directionText != null)
        {
            directionText.text = BuildDirectionText(forwardXZ);
        }
    }
    
    private string BuildDirectionText(Vector3 direction)
    {
        string text = "方向:\n";
        
        if (useCardinalDirections)
        {
            text += GetCardinalDirection(direction) + "\n";
        }
        
        if (showAngle)
        {
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            text += $"角度: {angle:F0}°\n";
        }
        
        if (showVector)
        {
            text += $"向量:\nX: {direction.x:F2}\nZ: {direction.z:F2}";
        }
        
        return text;
    }
    
    private string GetCardinalDirection(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        
        int index = Mathf.RoundToInt(angle / 45f) % 8;
        return cardinalDirections[index];
    }
    
    // 公共方法用于动态更改显示选项
    public void ToggleAngleDisplay(bool show)
    {
        showAngle = show;
    }
    
    public void ToggleVectorDisplay(bool show)
    {
        showVector = show;
    }
    
    public void SetIndicatorColor(Color color)
    {
        indicatorColor = color;
        if (directionIndicator != null)
            directionIndicator.color = color;
    }
}
