using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactRange = 1.5f;

    [Header("References")]
    [SerializeField] private PlayerHands playerHands;
    [SerializeField] private TextMeshProUGUI promptText;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerHands == null)
            playerHands = GetComponent<PlayerHands>();
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
        if (promptText == null)
            return;

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

            var itemObj = hit.GetComponent<ItemInteractable>();
            if (itemObj != null)
            {
                closestItem = itemObj;
                closestTask = null;
                minDistance = distance;
                continue;
            }

            var taskObj = hit.GetComponent<InteractableObject>();
            if (taskObj != null && taskObj.canInteract)
            {
                closestTask = taskObj;
                closestItem = null;
                minDistance = distance;
            }
        }

        if (closestItem != null)
        {
            promptText.text = $"<color=yellow>[E]</color> {closestItem.GetPrompt(playerHands)}";
        }
        else if (closestTask != null)
        {
            promptText.text = "[E] Interact";
        }
        else
        {
            promptText.text = "";
        }
    }
}
