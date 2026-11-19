using System.Collections;
using UnityEngine;

public class PlatformButton : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3.2f;

    [Header("Retractable Platforms")]
    [SerializeField] private Transform[] platforms;
    [SerializeField] private Vector3 retractWorldOffset = new Vector3(0f, 0f, -1.8f);
    [SerializeField] private float expandSpeed = 5f;
    [SerializeField] private float delayBetweenPlatforms = 0.15f;

    [Header("Button Press Feedback")]
    [SerializeField] private Vector3 buttonPressOffset = new Vector3(0f, 0f, -0.08f);

    private bool isActivated;
    private Renderer meshRenderer;
    private Vector3[] expandedPositions;

    public bool CanInteract => !isActivated;
    public float InteractRange => interactRange;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<Renderer>();

        // Lưu vị trí mở sẵn của các bậc platform rồi thu gọn chúng vào trong tường lúc bắt đầu game
        expandedPositions = new Vector3[platforms.Length];
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null) continue;
            expandedPositions[i] = platforms[i].position;
            platforms[i].position = expandedPositions[i] + retractWorldOffset;
        }
    }

    public Vector3 GetInteractPoint()
    {
        return meshRenderer != null ? meshRenderer.bounds.center : transform.position;
    }

    public void Interact()
    {
        if (isActivated) return;
        isActivated = true;
        transform.position += buttonPressOffset;
        StartCoroutine(ExpandPlatformsRoutine());
    }

    private IEnumerator ExpandPlatformsRoutine()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null)
            {
                StartCoroutine(MovePlatform(platforms[i], expandedPositions[i]));
                yield return new WaitForSeconds(delayBetweenPlatforms);
            }
        }
    }

    private IEnumerator MovePlatform(Transform platform, Vector3 targetPos)
    {
        while (Vector3.Distance(platform.position, targetPos) > 0.01f)
        {
            platform.position = Vector3.MoveTowards(platform.position, targetPos, expandSpeed * Time.deltaTime);
            yield return null;
        }
        platform.position = targetPos;
    }

    private void OnDrawGizmosSelected()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 center = r != null ? r.bounds.center : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, interactRange);
    }
}
