using UnityEngine;

public class SceneGuideTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName;
    public Vector2 coordinates;
    
    [Header("Trigger Settings")]
    public float triggerDistance = 5f;
    
    private Transform player;
    private bool hasTriggered = false;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (player != null && !hasTriggered)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= triggerDistance)
            {
                TriggerSceneGuide();
                hasTriggered = true;
            }
        }
    }
    
    private void TriggerSceneGuide()
    {
        // This can be used to automatically advance dialogue when player reaches location
        Debug.Log($"Player reached {sceneName} at coordinates {coordinates}");
        
        // Optional: Auto-advance dialogue if NPC conversation is active
        if (DialogueManager.Instance.IsInDialogue())
        {
            // Could trigger specific scene-related dialogue
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}