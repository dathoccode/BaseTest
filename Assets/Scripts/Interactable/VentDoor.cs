using System.Collections;
using UnityEngine;

public class VentDoor : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3.2f;

    [Header("Open Motion (World Space)")]
    [SerializeField] private Vector3 openWorldOffset = new Vector3(0f, 2.8f, 0f);
    [SerializeField] private float openSpeed = 4f;

    private bool isOpened;
    private Renderer meshRenderer;
    private Collider doorCollider;

    public bool CanInteract => !isOpened;
    public float InteractRange => interactRange;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<Renderer>();
        doorCollider = GetComponent<Collider>();
    }

    public Vector3 GetInteractPoint()
    {
        return meshRenderer != null ? meshRenderer.bounds.center : transform.position;
    }

    public void Interact()
    {
        if (isOpened) return;
        isOpened = true;
        StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        Vector3 targetPos = transform.position + openWorldOffset;
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, openSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }

    private void OnDrawGizmosSelected()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 center = r != null ? r.bounds.center : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, interactRange);
    }
}
