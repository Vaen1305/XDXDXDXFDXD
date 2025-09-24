using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 3f;
    public LayerMask interactableLayerMask = -1;
    
    [Header("Camera")]
    public Camera playerCamera;
    
    [Header("UI")]
    public GameObject interactionPrompt;
    
    [Header("Debug")]
    public bool showRaycastDebug = true;
    
    private RaycastHit hit;
    private IInteractable currentInteractable;
    
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        DetectInteractableObjects();
        HandleCursorToggle();
    }
    
    void DetectInteractableObjects()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayerMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null)
                        currentInteractable.OnDeselect();
                    
                    currentInteractable = interactable;
                    currentInteractable.OnSelect();
                    
                    if (interactionPrompt != null)
                        interactionPrompt.SetActive(true);
                }
                
                // Verde cuando detecta objeto interactuable
                if (showRaycastDebug)
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            }
            else
            {
                ClearCurrentInteractable();
                
                // Amarillo cuando golpea algo no interactuable
                if (showRaycastDebug)
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
            }
        }
        else
        {
            ClearCurrentInteractable();
            
            // Rojo cuando no golpea nada
            if (showRaycastDebug)
                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);
        }
    }
    
    public void TriggerInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
    
    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnDeselect();
            currentInteractable = null;
            
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}