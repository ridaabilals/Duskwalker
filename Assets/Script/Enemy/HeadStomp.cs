using UnityEngine;

public class HeadStomp : MonoBehaviour
{
    // Adding health variables
    public int maxHealth = 1; 
    private int currentHealth;

    private void Start()
    {
        // Set health when the game starts
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCollision playerCollision = collision.gameObject.GetComponent<PlayerCollision>();

            // Every time the player hits the head, reduce health
            currentHealth--;

            if (currentHealth <= 0)
            {
                // Only kill the enemy if health is 0 or less
                if (playerCollision != null)
                {
                    playerCollision.KillEnemy(transform.parent.gameObject);
                }
                else
                {
                    // Fallback logic
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);
                    }
                    Destroy(transform.parent.gameObject);
                }
            }
            else
            {
                // If the enemy is NOT dead yet, just make the player bounce
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);
                }
                
                // Optional: Play a "hit" sound or animation here
                Debug.Log("Enemy hit! Health remaining: " + currentHealth);
            }
        }
    }
}