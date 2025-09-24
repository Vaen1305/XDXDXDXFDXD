using UnityEngine;
using System.Collections.Generic;

public class ControlerArc : MonoBehaviour
{
    public ElectricArcRenderer electricArcRenderer;
    public Transform startPoint;
    public Transform defaultTarget;

    [SerializeField] private SphereCollider triggerCollider;
    [SerializeField] private LayerMask targetLayers = -1;

    [SerializeField] private bool interruptorEnabled = false;
    [SerializeField] private bool parte2Required = true;
    
    private List<Transform> targetsInRange = new List<Transform>();
    private List<Transform> parte2ObjectsInRange = new List<Transform>();
    private Transform currentTarget;
    private bool isArcActive = false;

    void OnEnable()
    {
        SlidingDoor.OnInterruptorStateChanged += OnInterruptorStateChanged;
    }

    void OnDisable()
    {
        SlidingDoor.OnInterruptorStateChanged -= OnInterruptorStateChanged;
    }

    public void OnInterruptorStateChanged(bool shouldActivateArcs)
    {
        interruptorEnabled = shouldActivateArcs;
        UpdateArcState();
    }

    private void UpdateArcState()
    {
        bool parte2Present = false;
        for (int i = 0; i < parte2ObjectsInRange.Count; i++)
        {
            if (parte2ObjectsInRange[i] != null)
            {
                parte2Present = true;
                break;
            }
        }

        bool isBlocked = parte2Required && parte2Present;

        if (!interruptorEnabled || isBlocked)
        {
            DisableArc();
            return;
        }

        Transform bestTarget = GetClosestNonParte2Target();

        if (bestTarget != null)
        {
            SetAndEnableArc(bestTarget);
        }
        else if (defaultTarget != null)
        {
            SetAndEnableArc(defaultTarget);
        }
        else
        {
            DisableArc();
        }
    }

    private void SetAndEnableArc(Transform newTarget)
    {
        if (newTarget == null)
        {
            DisableArc();
            return;
        }

        if (currentTarget == newTarget && isArcActive) return;

        currentTarget = newTarget;
        isArcActive = true;

        if (electricArcRenderer != null)
        {
            electricArcRenderer.endPoints.Clear();
            electricArcRenderer.endPoints.Add(currentTarget);
            electricArcRenderer.EnableArc();
        }
    }

    public void DisableArc()
    {
        if (!isArcActive) return;
        
        isArcActive = false;
        currentTarget = null;

        if (electricArcRenderer != null)
        {
            electricArcRenderer.DisableArc();
        }
    }

    Transform GetClosestNonParte2Target()
    {
        Transform closestTarget = null;
        float minDistanceSqr = float.MaxValue;

        for (int i = 0; i < targetsInRange.Count; i++)
        {
            Transform target = targetsInRange[i];
            if (target != null && target.gameObject.tag != "parte2")
            {
                float distanceSqr = (startPoint.position - target.position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closestTarget = target;
                }
            }
        }
        return closestTarget;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == triggerCollider || ((1 << other.gameObject.layer) & targetLayers) == 0) return;

        if (other.gameObject.tag == "parte2")
        {
            if (!parte2ObjectsInRange.Contains(other.transform))
                parte2ObjectsInRange.Add(other.transform);
        }
        else
        {
            if (!targetsInRange.Contains(other.transform))
                targetsInRange.Add(other.transform);
        }
        UpdateArcState();
    }

    void OnTriggerExit(Collider other)
    {
        if (other == triggerCollider) return;

        if (other.gameObject.tag == "parte2")
        {
            parte2ObjectsInRange.Remove(other.transform);
        }
        else
        {
            targetsInRange.Remove(other.transform);
        }
        UpdateArcState();
    }
}