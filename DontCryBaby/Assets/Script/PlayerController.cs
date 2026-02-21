using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float interactRange = 1.5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        InteractableObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue; // éviter de collide avec le babysitter

            var component = hit.GetComponent<MonoBehaviour>();
            if (component is InteractableObject interactable && interactable.canInteract)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = interactable;
                }
            }
        }

        if (closest != null)
        {
            closest.Interact();
        }
    }
}
