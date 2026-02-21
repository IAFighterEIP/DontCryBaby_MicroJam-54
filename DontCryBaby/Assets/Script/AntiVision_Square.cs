using UnityEngine;

public class AntiVision_Square : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get the player object reference here
    }

    // Update is called once per frame
    void Update()
    {
        if (1 == 0) {
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
        }
    }
}
