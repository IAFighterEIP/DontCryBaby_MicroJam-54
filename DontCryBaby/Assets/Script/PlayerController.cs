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

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("key E");
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

        Debug.Log("try interact");
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue; // éviter de collide avec le babysitter
            Debug.Log("for each");
            Debug.Log(hit);
            InteractableObject interactable = hit.GetComponent<InteractableObject>();

            Debug.Log(interactable);
            Debug.Log(interactable.canInteract);
            if (interactable != null && interactable.canInteract)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = interactable;
                }
            }
        }

        Debug.Log("finish for each");
        if (closest != null)
        {
            Debug.Log("call interact");
            closest.Interact();
        }
    }
}
