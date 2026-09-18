using UnityEngine;

public class Background2DSCroll : MonoBehaviour
{
    public Transform mainCamera;

    [Range(0f, 1f)]
    public float parallaxSpeed = 0.5f;

    private float textureWidth;
    private float startPosX;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (mainCamera == null)
        {
            Camera camera = Camera.main;
            if (camera != null)
                mainCamera = camera.transform;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Background2DSCroll: No SpriteRenderer found on this object. Disabling script.", this);
            enabled = false;
            return;
        }

        startPosX = transform.position.x;
        textureWidth = spriteRenderer.bounds.size.x;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            mainCamera = camera.transform;
        }

        float distance = mainCamera.position.x * parallaxSpeed;
        float cameraRelativePos = mainCamera.position.x * (1 - parallaxSpeed);

        transform.position = new Vector3(startPosX + distance, transform.position.y, transform.position.z);

        if (cameraRelativePos > startPosX + textureWidth)
        {
            startPosX += textureWidth * 2;
        }
        else if (cameraRelativePos < startPosX - textureWidth)
        {
            startPosX -= textureWidth * 2;
        }
    }
}