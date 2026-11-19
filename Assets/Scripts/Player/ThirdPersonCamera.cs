using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Orbit & Distance")]
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float minDistance = 0.6f;
    [SerializeField] private float mouseSensitivity = 4f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 70f;

    [Header("3D Wall Collision")]
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private float yaw;
    private float pitch = 15f;
    private bool isDraggingCamera;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
        else
        {
            yaw = transform.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                isDraggingCamera = !overUI;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                isDraggingCamera = false;
            }

            if (isDraggingCamera && mouse.leftButton.isPressed)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue() * 0.1f;
                yaw += mouseDelta.x * mouseSensitivity;
                pitch -= mouseDelta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
        }

        Quaternion camRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + targetOffset;
        Vector3 desiredDirection = -(camRotation * Vector3.forward);

        float finalDistance = distance;
        if (Physics.SphereCast(
                focusPoint,
                collisionRadius,
                desiredDirection,
                out RaycastHit hit,
                distance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                finalDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, distance);
            }
        }

        transform.position = focusPoint + desiredDirection * finalDistance;
        transform.rotation = camRotation;
    }
}
