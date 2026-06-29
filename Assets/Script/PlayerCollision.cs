using System.Collections;
using SupanthaPaul;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    [Header("UI References")]
    public MenuManager menuManager;
    [Tooltip("Drag your 4 Heart Images from the Hierarchy into this list")]
    public Image[] heartImages; 

    [Header("Health Settings")]
    public int health = 4;
    public float fallThreshold = -10f;
    public float invincibilityDuration = 1.5f;

    [Header("Death Settings")]
    [SerializeField] private float gameOverDelay = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip killEnemySound;

    private PlayerController m_playerController;
    private Rigidbody2D m_rb;
    private SpriteRenderer m_spriteRenderer;
    private Transform m_spriteTransform;
    
    private bool m_isDead = false;
    private bool m_isInvincible = false;

    private void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_rb = GetComponent<Rigidbody2D>();

        m_spriteTransform = transform.Find("_Sprite");
        if (m_spriteTransform != null)
            m_spriteRenderer = m_spriteTransform.GetComponent<SpriteRenderer>();
            
        UpdateHeartUI();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!m_isDead && transform.position.y < fallThreshold)
        {
            health = 0;
            UpdateHeartUI();
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_isDead || m_isInvincible) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // We use a small check here: if the player is falling, we treat it as a stomp.
            // However, since your HeadStomp script is NOW handling the stomp, 
            // you can actually REMOVE this 'if' block to rely 100% on the HeadCheck trigger.
            if (collision.relativeVelocity.y > 0.1f && transform.position.y > collision.transform.position.y)
            {
                // The HeadStomp script will likely trigger first, so we do nothing here
                // to avoid playing the sound twice.
            }
            else
            {
                TakeDamage(1);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
    // Don't do anything if dead or currently in the flickering "invincibility" state
    if (m_isDead || m_isInvincible) return;

    if (collision.gameObject.CompareTag("Enemy"))
    {
        // Check if we are touching the side (not jumping on top)
        if (!(collision.relativeVelocity.y > 0.1f && transform.position.y > collision.transform.position.y))
        {
            TakeDamage(1);
        }
    }
    }


    // CHANGE: Made this PUBLIC so HeadStomp.cs can call it
    public void KillEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        // Play Kill Sound
        if (audioSource && killEnemySound)
            audioSource.PlayOneShot(killEnemySound);

        // Small bounce when killing enemy
        if (m_rb) m_rb.velocity = new Vector2(m_rb.velocity.x, 10f);

        // If the enemy script has its own death logic, call it, otherwise destroy
        Destroy(enemy); 
    }

    public void TakeDamage(int amount)
    {
        if (m_isInvincible || m_isDead) return;

        health -= amount;
        UpdateHeartUI();

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlickerEffect());
        }
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].enabled = (i < health);
        }
    }

    private IEnumerator FlickerEffect()
    {
        m_isInvincible = true;
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            if (m_spriteRenderer) m_spriteRenderer.enabled = !m_spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
        if (m_spriteRenderer) m_spriteRenderer.enabled = true;
        m_isInvincible = false;
    }

    private void Die()
    {
        if (m_isDead) return;
        m_isDead = true;

        if (audioSource && gameOverSound)
            audioSource.PlayOneShot(gameOverSound);

        if (m_playerController != null)
            m_playerController.canMove = false;

        GetComponent<Collider2D>().enabled = false;

        if (m_rb != null)
        {
            m_rb.simulated = true; 
            m_rb.velocity = Vector2.zero; 
            m_rb.AddForce(new Vector2(0, 10f), ForceMode2D.Impulse); 
        }

        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        if (m_spriteTransform != null)
            m_spriteTransform.localRotation = Quaternion.Euler(0, 0, 90f);

        yield return new WaitForSeconds(gameOverDelay);

        if (menuManager != null)
            menuManager.GameOver();
    }
}