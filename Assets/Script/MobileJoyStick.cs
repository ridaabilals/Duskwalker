using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoyStick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public static MobileJoyStick Instance { get; private set; }

    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [Range(0.01f, 0.5f)]
    [SerializeField] private float deadZone = 0.15f;

    private Vector2 input;
    private int? activePointer;

    public float Horizontal => Mathf.Abs(input.x) <= deadZone ? 0f : input.x;
    public float Vertical => Mathf.Abs(input.y) <= deadZone ? 0f : input.y;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (joystickBackground == null)
            joystickBackground = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointer.HasValue) return;
        activePointer = eventData.pointerId;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activePointer != eventData.pointerId || joystickBackground == null) return;
        Vector2 localPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out localPosition))
        {
            Vector2 radius = joystickBackground.rect.size / 2f;
            if (radius.x <= 0f || radius.y <= 0f) return;
            localPosition -= joystickBackground.rect.center;

            input = new Vector2(
                localPosition.x / radius.x,
                localPosition.y / radius.y
            );

            input = Vector2.ClampMagnitude(input, 1f);

            if (Mathf.Abs(input.x) < deadZone)
                input.x = 0f;
            if (Mathf.Abs(input.y) < deadZone)
                input.y = 0f;

            if (joystickHandle != null) joystickHandle.anchoredPosition = new Vector2(
                input.x * radius.x,
                input.y * radius.y
            );
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointer != eventData.pointerId) return;
        ResetInput();
    }

    private void OnDisable() => ResetInput();

    private void ResetInput()
    {
        activePointer = null;
        input = Vector2.zero;
        if (joystickHandle != null) joystickHandle.anchoredPosition = Vector2.zero;
    }
}
