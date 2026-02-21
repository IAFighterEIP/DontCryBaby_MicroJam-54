using UnityEngine;

public class AngerZone : MonoBehaviour
{
    [SerializeField] private float angerMultiplierPerSecond = 0.6f;

    private void OnTriggerStay2D(Collider2D other)
    {
        var baby = other.GetComponent<BabyController>();
        if (baby == null) return;

        baby.AddFlatAnger(angerMultiplierPerSecond * Time.deltaTime);
        baby.SetShaking(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var baby = other.GetComponent<BabyController>();
        if (baby == null) return;

        baby.SetShaking(false);
    }
}