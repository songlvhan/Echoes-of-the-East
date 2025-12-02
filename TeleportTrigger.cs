using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform targetLocation;
    public TeleportUIController teleportUI;
    
    [Header("Trigger Settings")]
    public float triggerDistance = 3f;
    public bool useColliderTrigger = false; // 选择使用距离检测还是碰撞体检测
    
    private Transform player;
    private bool isInRange = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        // 如果使用距离检测而不是碰撞体
        if (!useColliderTrigger && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            
            if (distance <= triggerDistance && !isInRange)
            {
                isInRange = true;
                ShowTeleportUI();
            }
            else if (distance > triggerDistance && isInRange)
            {
                isInRange = false;
                HideTeleportUI();
            }
        }
    }
    
    // 碰撞体触发方式
    void OnTriggerEnter(Collider other)
    {
        if (useColliderTrigger && other.CompareTag("Player"))
        {
            isInRange = true;
            ShowTeleportUI();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (useColliderTrigger && other.CompareTag("Player"))
        {
            isInRange = false;
            HideTeleportUI();
        }
    }
    
    void ShowTeleportUI()
    {
        if (teleportUI != null && targetLocation != null)
        {
            teleportUI.ShowUI(targetLocation);
        }
    }
    
    void HideTeleportUI()
    {
        if (teleportUI != null)
        {
            teleportUI.HideUI();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!useColliderTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
        
        // 绘制到目标位置的连线
        if (targetLocation != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetLocation.position);
        }
    }
}