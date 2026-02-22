using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactRange = 1.5f;

    [Header("References")]
    [SerializeField] private PlayerHands playerHands;
    [SerializeField] private PlayerHUD hud;
    [SerializeField] private LayerMask interactMask = ~0;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerHands == null)
            playerHands = GetComponent<PlayerHands>();

        // Subscribe to hands changes -> update HUD
        if (playerHands != null && hud != null)
        {
            hud.SetHeldItem(playerHands.HeldItem);
            playerHands.OnHeldItemChanged += hud.SetHeldItem;
        }
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal") * 2;
        movement.y = Input.GetAxisRaw("Vertical") * 2;

        movement = movement.normalized;

        if (movement.sqrMagnitude > 0.01f) // pas de rotation si immobile
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
        
        UpdatePrompt();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        ItemInteractable closestItem = null;
        InteractableObject closestTask = null;

        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance > minDistance)
                continue;

            // FIRST: Check for item-based interaction
            var itemObj = hit.GetComponent<ItemInteractable>();
            if (itemObj != null)
            {
                closestItem = itemObj;
                closestTask = null;
                minDistance = distance;
                continue;
            }

            // SECOND: Check for task-based interaction
            var taskObj = hit.GetComponent<InteractableObject>();
            if (taskObj != null && taskObj.canInteract)
            {
                closestTask = taskObj;
                closestItem = null;
                minDistance = distance;
            }
        }

        // Execute interaction
        if (closestItem != null)
        {
            closestItem.Interact(playerHands);
            return;
        }

        if (closestTask != null)
        {
            closestTask.Interact();
            return;
        }
    }
    
    void UpdatePrompt()
    {
        if (hud == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactMask);

        ItemInteractable closestItem = null;
        InteractableObject closestTask = null;
        float bestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector2 p = hit.ClosestPoint(transform.position);
            float d = Vector2.Distance(transform.position, p);
            if (d > bestDist) continue;

            var itemObj = hit.GetComponentInParent<ItemInteractable>();
            if (itemObj != null)
            {
                closestItem = itemObj;
                closestTask = null;
                bestDist = d;
                continue;
            }

            var taskObj = hit.GetComponentInParent<InteractableObject>();
            if (taskObj != null && taskObj.canInteract)
            {
                closestTask = taskObj;
                closestItem = null;
                bestDist = d;
            }
        }

        if (closestItem != null)
        {
            hud.SetPrompt(closestItem.GetPrompt(playerHands));
        }
        else if (closestTask != null)
        {
            hud.SetPrompt("Interact");
        }
        else
        {
            hud.SetPrompt("");
        }
    }
}
