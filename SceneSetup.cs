// SceneSetup.cs
using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [Header("Player Setup")]
    public GameObject player;
    public string playerTag = "Player";
    
    [Header("Teleport Locations")]
    public Transform teleportTarget;
    public GameObject triggerZone;
    
    void Start()
    {
        SetupTeleportSystem();
    }
    
    void SetupTeleportSystem()
    {
        // Ensure player has tag
        if (player != null)
        {
            player.tag = playerTag;
        }
        
        // Setup trigger zone
        if (triggerZone != null)
        {
            var collider = triggerZone.GetComponent<Collider>();
            if (collider == null)
            {
                collider = triggerZone.AddComponent<BoxCollider>();
            }
            collider.isTrigger = true;
        }
    }
}