using UnityEngine;
using System.Collections;

public class Enemy2AI : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Invincibility Cooldown")]
    public float invincibilityDuration = 0.2f; // Minimum time between hits
    private bool isInvincible = false;

    [Header("Movement")]
    public float moveSpeed = 2f;
    private bool movingRight = true;

    [Header("Detection")]
    public Transform edgeCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead = false;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        originalColor = sr.color;
    }

    // Called by the HeadCheck child object script or player attack
    public void TakeDamage()
    {
        // Ignore hits if dead or currently in cooldown
        if (isDead || isInvincible) return;

        currentHealth--;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashRedAndCooldown());
        }
    }

    private void Die()
    {
        isDead = true;

        // Physics reaction: Disable main collider and fall through floor
        GetComponent<BoxCollider2D>().enabled = false;
        rb.velocity = new Vector2(0, 5f); // Small pop up before falling

        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FlashRedAndCooldown()
    {
        isInvincible = true;
        sr.color = Color.red;

        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;

        // Wait out the rest of the invincibility window before allowing hits again
        yield return new WaitForSeconds(invincibilityDuration - 0.1f);
        isInvincible = false;
    }

    IEnumerator FadeAndDestroy()
    {
        float duration = 1.5f;
        float currentTime = 0;
        Color startColor = sr.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    void FixedUpdate() 
    {
        if (isDead) return;

        rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

        // 1. CLIFF DETECTION
        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, 1f, groundLayer);

        // 2. WALL DETECTION
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(edgeCheck.position, rayDirection, wallCheckDistance, groundLayer);

        if (groundInfo.collider == null || wallInfo.collider != null)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}