using UnityEngine;

/// <summary>
/// Adds a subtle floating bob to a diamond collectible and awards score when the player collects it.
/// </summary>
public class DiamondPickup : MonoBehaviour
{
    private const string PlayerTag = "Player";

    public int scoreValue = 1;
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;
    public AudioClip collectSound;

    private float previousBobOffset;

    private void LateUpdate()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        float deltaOffset = bobOffset - previousBobOffset;
        transform.position += new Vector3(0f, deltaOffset, 0f);
        previousBobOffset = bobOffset;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null && col.CompareTag(PlayerTag))
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 0.7f);
            }

            Destroy(gameObject);
        }
    }
}
