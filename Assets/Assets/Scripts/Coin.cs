using UnityEngine;

public class Coin : MonoBehaviour
{
    [HideInInspector] public PlatformGenerator generator;

    private void Awake()
    {
        // ensure coin has a trigger collider (optional if prefab already has one)
        Collider2D col = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Do NOT modify scale here — scale is handled in PlatformGenerator to preserve correct world size.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Notify generator (if assigned)
        if (generator != null)
        {
            generator.RegisterCoinCollected();
        }

        // Optionally play sound/animation here

        // Destroy the coin
        Destroy(gameObject);
    }
}
