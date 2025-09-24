using UnityEngine;

public class Palanc : MonoBehaviour, IInteractable
{
    [Header("Lever Settings")]
    public bool isOpen = false;
    public float openAngle = 90f;
    public float animationSpeed = 2f;
    
    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Renderer leverRenderer;
    private bool isAnimating = false;
    private Color originalColor;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0, 0);
        leverRenderer = GetComponent<Renderer>();
        
        if (leverRenderer != null)
            originalColor = leverRenderer.material.color;
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
        if (leverRenderer != null)
            leverRenderer.material.color = highlightColor;
    }
    
    public void OnDeselect()
    {
        if (leverRenderer != null)
            leverRenderer.material.color = originalColor;
    }
}