using UnityEngine;
using TMPro;

public class PlayerPositionDisplayTMP : MonoBehaviour
{
    [Header("玩家对象")]
    public Transform playerTransform;
    
    [Header("TMP文本组件")]
    public TextMeshProUGUI positionText;
    
    [Header("显示设置")]
    public bool showWorldPosition = true;
    public int decimalPlaces = 2;
    public string displayFormat = "Position:\nX: {0}\nZ: {1}";
    
    [Header("刷新设置")]
    public float updateInterval = 0.1f; // 更新间隔（秒）
    private float timer = 0f;

    private void Start()
    {
        // 自动查找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("未找到玩家对象，请手动赋值或确保玩家有'Player'标签");
        }
        
        // 自动获取TMP组件
        if (positionText == null)
            positionText = GetComponent<TextMeshProUGUI>();
        
        if (positionText == null)
            Debug.LogWarning("未找到TextMeshProUGUI组件");
    }

    private void Update()
    {
        // 控制更新频率
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            if (playerTransform != null && positionText != null)
            {
                UpdatePositionDisplay();
            }
            timer = 0f;
        }
    }

    private void UpdatePositionDisplay()
    {
        Vector3 position = showWorldPosition ? playerTransform.position : playerTransform.localPosition;
        
        // 格式化坐标
        string xValue = position.x.ToString($"F{decimalPlaces}");
        string zValue = position.z.ToString($"F{decimalPlaces}");
        
        // 更新TMP文本
        positionText.text = string.Format(displayFormat, xValue, zValue);
    }
    
    // 公共方法，可供其他脚本调用
    public void SetDisplayFormat(string newFormat)
    {
        displayFormat = newFormat;
    }
    
    public void SetDecimalPlaces(int places)
    {
        decimalPlaces = Mathf.Clamp(places, 0, 6);
    }
}