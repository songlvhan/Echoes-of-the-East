using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Scholar Wei";
    public float interactionDistance = 3f;
    
    private Transform player;
    private bool hasInteractedOnce = false;
    private bool isPlayerInRange = false;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= interactionDistance;
            
            // First interaction requires proximity
            if (!hasInteractedOnce && isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                StartInteraction();
            }
            // Subsequent interactions can be done from anywhere
            else if (hasInteractedOnce && Input.GetKeyDown(KeyCode.E) && !DialogueManager.Instance.IsInDialogue())
            {
                StartInteraction();
            }
        }
    }
    
    private void StartInteraction()
    {
        DialogueManager.Instance.SetCurrentNPC(this);
        DialogueManager.Instance.StartDialogue(this);
        hasInteractedOnce = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}