using UnityEngine;

public class BossHead : MonoBehaviour
{
    private BossAI bossMainScript;

    void Start()
    {
        // Gets the script from the parent object
        bossMainScript = GetComponentInParent<BossAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the thing hitting the head is the Player
        if (collision.CompareTag("Player"))
        {
            bossMainScript.TakeDamage();
            
            // Optional: Bounce the player up after a hit
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);
            }
        }
    }
}