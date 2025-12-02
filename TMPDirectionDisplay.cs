using UnityEngine;
using TMPro;

public class TMPDirectionDisplay : MonoBehaviour
{
    [Header("显示设置")]
    public bool showWorldDirection = true;
    public bool showDirectionVectors = true; // 是否显示方向向量
    
    [Header("UI组件")]
    public TextMeshProUGUI directionText;
    
    private Transform targetTransform;
    
    void Start()
    {
        if (targetTransform == null)
            targetTransform = Camera.main?.transform ?? transform;
            
        if (directionText == null)
            directionText = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        UpdateDirectionDisplay();
    }
    
    void UpdateDirectionDisplay()
    {
        if (targetTransform == null || directionText == null) return;
        
        Vector3 eulerAngles = showWorldDirection ? 
            targetTransform.eulerAngles : 
            targetTransform.localEulerAngles;
            
        // 规范化角度到0-360范围
        float xAngle = NormalizeAngle(eulerAngles.x);
        float zAngle = NormalizeAngle(eulerAngles.z);
        
        string displayText = $"<b>方向显示</b>\n";
        displayText += $"X轴: {xAngle:F1}°\n";
        displayText += $"Z轴: {zAngle:F1}°";
        
        if (showDirectionVectors)
        {
            Vector3 forward = targetTransform.forward;
            Vector3 right = targetTransform.right;
            
            displayText += $"\n\n<b>方向向量</b>\n";
            displayText += $"前向: ({forward.x:F2}, {forward.y:F2}, {forward.z:F2})\n";
            displayText += $"右向: ({right.x:F2}, {right.y:F2}, {right.z:F2})";
        }
        
        directionText.text = displayText;
    }
    
    float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle < 0) angle += 360;
        return angle;
    }
    
    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }
}