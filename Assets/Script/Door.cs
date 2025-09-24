using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public float openAngle = 90f;
    public float animationSpeed = 2f;
    
    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Renderer doorRenderer;
    private bool isAnimating = false;
    private Color originalColor;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        doorRenderer = GetComponent<Renderer>();
        
        if (doorRenderer != null)
            originalColor = doorRenderer.material.color;
    }
    
    void Update()
    {
        if (isAnimating)
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * animationSpeed);
            
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isAnimating = false;
            }
        }
    }
    
    public void Interact()
    {
        if (!isAnimating)
        {
            isOpen = !isOpen;
            isAnimating = true;
        }
    }
    
    public void OnSelect()
    {
        if (doorRenderer != null)
            doorRenderer.material.color = highlightColor;
    }
    
    public void OnDeselect()
    {
        if (doorRenderer != null)
            doorRenderer.material.color = originalColor;
    }
}