using UnityEngine;

public class CalmZone : MonoBehaviour
{
    [SerializeField] private float calmMultiplierPerSecond = 2.4f;
    [SerializeField] private float calmAngerPerSecond = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        var baby = other.GetComponent<BabyController>();
        if (baby == null) return;

        baby.CalmBaby(calmAngerPerSecond * Time.deltaTime);
        baby.DecreaseAngerMultiplier(calmMultiplierPerSecond * Time.deltaTime);

    }
}