using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Button interactIconButton;
    [SerializeField] private Vector3 iconWorldOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Interaction Timing")]
    [SerializeField] private float interactDelay = 1.5f;

    private readonly List<IInteractable> interactables = new List<IInteractable>();
    private IInteractable currentTarget;
    private Camera mainCam;
    private bool isInteracting;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    private void Start()
    {
        mainCam = Camera.main;

        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb is IInteractable interactable)
            {
                interactables.Add(interactable);
            }
        }

        if (interactIconButton != null)
        {
            interactIconButton.onClick.AddListener(OnInteractButtonClicked);
            interactIconButton.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        HandleInteractButton();
    }

    private void HandleInteractButton() 
    {
        if (interactIconButton == null || mainCam == null) return;

        if (isInteracting || (playerController != null && playerController.IsDead))
        {
            currentTarget = null;
            interactIconButton.gameObject.SetActive(false);
            return;
        }

        currentTarget = FindClosestInteractable();

        if (currentTarget != null)
        {
            Vector3 worldPos = currentTarget.GetInteractPoint() + iconWorldOffset;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

            bool isInFrontOfCamera = screenPos.z > 0f;
            interactIconButton.gameObject.SetActive(isInFrontOfCamera);

            if (isInFrontOfCamera)
            {
                interactIconButton.transform.position = screenPos;
            }
        }
        else
        {
            interactIconButton.gameObject.SetActive(false);
        }
    }

    private IInteractable FindClosestInteractable()
    {
        Vector3 playerCenter = transform.position + Vector3.up * 1f;
        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (IInteractable item in interactables)
        {
            if (!item.CanInteract) continue;

            float dist = Vector3.Distance(playerCenter, item.GetInteractPoint());
            if (dist <= item.InteractRange && dist < closestDist)
            {
                closestDist = dist;
                closest = item;
            }
        }

        return closest;
    }

    private void OnInteractButtonClicked()
    {
        if (isInteracting || currentTarget == null || !currentTarget.CanInteract) return;

        IInteractable target = currentTarget;
        currentTarget = null;
        interactIconButton.gameObject.SetActive(false);

        StartCoroutine(InteractWithDelayRoutine(target));
    }

    private IEnumerator InteractWithDelayRoutine(IInteractable target)
    {
        isInteracting = true;

        if (playerController != null)
        {
            playerController.PlayInteractAnimation();
        }

        if (interactDelay > 0f)
        {
            yield return new WaitForSeconds(interactDelay);
        }

        if ((playerController == null || !playerController.IsDead) && target != null && target.CanInteract)
        {
            target.Interact();
        }

        isInteracting = false;
    }
}
