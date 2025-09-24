using UnityEngine;
using System;

public class SlidingDoor : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    public float openAngle = 90f;
    public float animationSpeed = 2f;
    public Color highlightColor = Color.yellow;
    
    public static event Action<bool> OnInterruptorStateChanged;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Renderer doorRenderer;
    private Color originalColor;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0, 0);
        doorRenderer = GetComponent<Renderer>();
        
        if (doorRenderer != null)
        {
            originalColor = doorRenderer.material.color;
        }
            
        Invoke(nameof(NotifyInitialState), 0.1f);
    }
    
    void NotifyInitialState()
    {
        OnInterruptorStateChanged?.Invoke(!isOpen);
    }
    
    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * animationSpeed);
    }
    
    public void Interact()
    {
        isOpen = !isOpen;
        OnInterruptorStateChanged?.Invoke(!isOpen);
    }
    
    public void OnSelect()
    {
        if (doorRenderer != null)
        {
            doorRenderer.material.color = highlightColor;
        }
    }
    
    public void OnDeselect()
    {
        if (doorRenderer != null)
        {
            doorRenderer.material.color = originalColor;
        }
    }
}