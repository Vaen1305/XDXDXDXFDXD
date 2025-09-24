using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ElectricArcRenderer : MonoBehaviour
{
    [Range(1, 8)] public int iterations = 5;
    public float maxDisplacement = 0.5f;
    public float animationSpeed = 15f;
    public float arcWidth = 0.1f;
    public Gradient arcColor;

    public Transform startPoint;
    public List<Transform> endPoints = new List<Transform>();

    private LineRenderer lineRenderer;
    private List<Vector3> currentPoints = new List<Vector3>();
    private bool isActive = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        DisableArc();
    }

    void Update()
    {
        if (lineRenderer.startWidth != arcWidth)
        {
            lineRenderer.startWidth = arcWidth;
            lineRenderer.endWidth = arcWidth;
        }

        if (isActive && lineRenderer.enabled)
        {
            GenerateLightningPath();
            AnimateLightning();
        }
    }

    public void GenerateLightningPath()
    {
        if (startPoint == null || endPoints.Count == 0 || endPoints[0] == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        currentPoints.Clear();
        currentPoints.Add(startPoint.position);

        for(int i = 0; i < endPoints.Count; i++)
        {
            Transform endPoint = endPoints[i];
            if (endPoint != null)
            {
                List<Vector3> segment = GenerateLightningSegment(
                    currentPoints[currentPoints.Count - 1],
                    endPoint.position,
                    iterations
                );
                
                if (currentPoints.Count > 1 && segment.Count > 0)
                {
                    segment.RemoveAt(0);
                }
                
                currentPoints.AddRange(segment);
            }
        }
        UpdateLineRenderer();
    }

    private List<Vector3> GenerateLightningSegment(Vector3 start, Vector3 end, int iterations)
    {
        List<Vector3> points = new List<Vector3> { start, end };

        for (int i = 0; i < iterations; i++)
        {
            List<Vector3> newPoints = new List<Vector3>();
            for (int j = 0; j < points.Count - 1; j++)
            {
                Vector3 pointA = points[j];
                Vector3 pointB = points[j + 1];

                newPoints.Add(pointA);
                
                Vector3 midpoint = Vector3.Lerp(pointA, pointB, 0.5f);
                Vector3 direction = (pointB - pointA).normalized;
                Vector3 perpendicular = Vector3.Cross(direction, Random.onUnitSphere).normalized;
                
                float displacement = Random.Range(-maxDisplacement, maxDisplacement);
                midpoint += perpendicular * displacement;
                
                newPoints.Add(midpoint);
            }
            newPoints.Add(points[points.Count - 1]);
            points = newPoints;
        }
        return points;
    }

    private void AnimateLightning()
    {
        if (currentPoints.Count < 2) return;

        Vector3[] animatedPoints = new Vector3[currentPoints.Count];
        currentPoints.CopyTo(animatedPoints);

        float timeOffset = Time.time * animationSpeed;
        
        for (int i = 1; i < animatedPoints.Length - 1; i++)
        {
            Vector3 randomOffset = new Vector3(
                (Mathf.PerlinNoise(i * 0.3f, timeOffset) - 0.5f),
                (Mathf.PerlinNoise(i * 0.3f + 5f, timeOffset) - 0.5f),
                (Mathf.PerlinNoise(i * 0.3f + 10f, timeOffset) - 0.5f)
            ) * maxDisplacement * 0.5f;

            animatedPoints[i] += randomOffset;
        }

        lineRenderer.SetPositions(animatedPoints);
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = currentPoints.Count;
        lineRenderer.SetPositions(currentPoints.ToArray());
    }

    public void EnableArc() 
    { 
        isActive = true;
        lineRenderer.enabled = true;
    }
    
    public void DisableArc() 
    { 
        isActive = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }
    }
}