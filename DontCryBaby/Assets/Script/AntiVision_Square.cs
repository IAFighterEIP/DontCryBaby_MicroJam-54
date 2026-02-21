using UnityEngine;

public class AntiVision_Square : MonoBehaviour
{

    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // GameObject BabySitter = GameObject.Find("BabySitter"); //get the player object reference here
        // if (BabySitter == null) {
        //     Debug.LogError("Player object not found! Please make sure there is a GameObject named 'BabySitter' in the scene.");
        // }

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) {
            Debug.LogError("SpriteRenderer component not found on AntiVision_Square! Please make sure this script is attached to a GameObject with a SpriteRenderer.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if () {
        //     gameObject.SetActive(false);
        // } else {
        //     gameObject.SetActive(true);
        // }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.gameObject.name == "BabySitter") {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f); // Set opacity to 50% when player enters the trigger
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name == "BabySitter") {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f); // Reset opacity when player exits the trigger
        }
    }
}
