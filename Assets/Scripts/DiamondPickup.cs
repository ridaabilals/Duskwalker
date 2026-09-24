using System.Collections;
using UnityEngine;

/// <summary>
/// Controls diamond movement, collection, score display,
/// sound and collection animation for the current level only.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class DiamondPickup : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("Diamond Reward")]
    [Min(1)]
    [SerializeField] private int scoreValue = 1;

    [SerializeField] private AudioClip collectSound;

    [Range(0f, 1f)]
    [SerializeField] private float collectSoundVolume = 0.7f;

    [Header("Floating Animation")]
    [Min(0f)]
    [SerializeField] private float bobHeight = 0.15f;

    [Min(0f)]
    [SerializeField] private float bobSpeed = 2f;

    [Header("Collection Animation")]
    [Range(0.3f, 0.5f)]
    [SerializeField] private float collectDuration = 0.4f;

    [Min(1f)]
    [SerializeField] private float collectScaleUp = 1.3f;

    [Min(0f)]
    [SerializeField] private float collectRiseDistance = 0.65f;

    [SerializeField] private float collectSpinDegrees = 450f;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] collectibleColliders;

    private Vector3 originalPosition;
    private bool isCollected;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collectibleColliders = GetComponents<Collider2D>();
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected)
            return;

        ApplyFloatingAnimation();
    }

    private void ApplyFloatingAnimation()
    {
        float verticalOffset =
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position =
            originalPosition + Vector3.up * verticalOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
            return;

        if (other == null)
            return;

        if (!other.CompareTag(PlayerTag))
            return;

        CollectDiamond();
    }

    private void CollectDiamond()
    {
        // Prevent this diamond from being collected twice.
        isCollected = true;

        DisableDiamondColliders();
        AddDiamonds(scoreValue);
        PlayCollectionSound();

        StartCoroutine(CollectRoutine());
    }

    /// <summary>
    /// Adds diamonds to the level score only.
    /// </summary>
    private void AddDiamonds(int amount)
    {
        if (amount <= 0)
            return;

        if (DiamondScoreManager.Instance != null)
            DiamondScoreManager.Instance.AddDiamond(amount);
    }

    private void DisableDiamondColliders()
    {
        foreach (Collider2D diamondCollider in collectibleColliders)
        {
            if (diamondCollider != null)
                diamondCollider.enabled = false;
        }
    }

    private void PlayCollectionSound()
    {
        if (collectSound == null)
            return;

        AudioSource.PlayClipAtPoint(
            collectSound,
            transform.position,
            collectSoundVolume
        );
    }

    private IEnumerator CollectRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.rotation;
        Color startColor = spriteRenderer.color;

        float duration = Mathf.Max(0.01f, collectDuration);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(elapsedTime / duration);

            float easedTime =
                1f - Mathf.Pow(1f - normalizedTime, 3f);

            float scaleMultiplier;

            if (normalizedTime < 0.25f)
            {
                float scaleUpTime = Mathf.SmoothStep(
                    0f,
                    1f,
                    normalizedTime / 0.25f
                );

                scaleMultiplier = Mathf.Lerp(
                    1f,
                    collectScaleUp,
                    scaleUpTime
                );
            }
            else
            {
                float scaleDownTime = Mathf.SmoothStep(
                    0f,
                    1f,
                    (normalizedTime - 0.25f) / 0.75f
                );

                scaleMultiplier = Mathf.Lerp(
                    collectScaleUp,
                    0f,
                    scaleDownTime
                );
            }

            transform.position =
                startPosition +
                Vector3.up * (collectRiseDistance * easedTime);

            transform.rotation =
                startRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    collectSpinDegrees * easedTime
                );

            transform.localScale =
                startScale * scaleMultiplier;

            Color fadedColor = startColor;

            fadedColor.a =
                startColor.a *
                (1f - Mathf.SmoothStep(
                    0.15f,
                    1f,
                    normalizedTime
                ));

            spriteRenderer.color = fadedColor;

            yield return null;
        }

        Destroy(gameObject);
    }
}