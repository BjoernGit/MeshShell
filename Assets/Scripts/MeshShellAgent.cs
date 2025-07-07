using UnityEngine;

[System.Serializable]
public class MeshShellAgent
{
    // Agent Properties
    public Vector3 Position { get; private set; }
    public Vector3 Normal { get; private set; }
    public GameObject TargetObject { get; private set; }
    
    // Settings reference
    private MeshShellAgentSettings settings;
    
    // Optional visual representation
    private GameObject visualObject;
    
    // Constructor with 4 parameters
    public MeshShellAgent(Vector3 position, Vector3 normal, GameObject targetObject, MeshShellAgentSettings agentSettings)
    {
        Position = position;
        Normal = normal.normalized;
        TargetObject = targetObject;
        settings = agentSettings;
    }
    
    public void SetVisualObject(GameObject visual)
    {
        visualObject = visual;
    }
    
    public void DrawDebugArcs()
    {
        if (settings == null)
        {
            Debug.LogError("MeshShellAgent has no settings assigned!");
            return;
        }
        
        // Create local coordinate system based on normal
        Vector3 right = Vector3.Cross(Normal, Vector3.up);
        if (right.magnitude < 0.001f) // If Normal ~ Vector3.up
            right = Vector3.Cross(Normal, Vector3.forward);
        right.Normalize();
        
        Vector3 forward = Vector3.Cross(right, Normal).normalized;
        
        // Draw debug arcs based on number of rays setting
        float angleStep = 360f / settings.numberOfRays;
        for (int i = 0; i < settings.numberOfRays; i++)
        {
            float baseAngle = i * angleStep;
            DrawArc(baseAngle, right, forward);
        }
    }
    
    private void DrawArc(float baseAngle, Vector3 right, Vector3 forward)
    {
        // Rotate local coordinate system around normal
        Quaternion baseRotation = Quaternion.AngleAxis(baseAngle, Normal);
        Vector3 arcRight = baseRotation * right;
        Vector3 arcForward = baseRotation * forward;
        
        // Circle center must be positioned so agent point lies on the circle
        Vector3 circleCenter = Position - arcForward * settings.arcRadius;
        
        Vector3 lastPoint = Position;
        bool surfaceHit = false;
        float currentAngle = 0f;
        
        // Search along arc until surface is hit
        while (currentAngle < settings.maxArcAngle && !surfaceHit)
        {
            currentAngle += settings.arcStepAngle;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            
            // Calculate next position on circle
            Vector3 pointOnCircle = circleCenter 
                + arcForward * settings.arcRadius * Mathf.Cos(angleRad)
                + Normal * settings.arcRadius * Mathf.Sin(angleRad);
            
            // Raycast from last point to next
            Vector3 direction = pointOnCircle - lastPoint;
            float distance = direction.magnitude;
            
            if (distance > 0.001f) // Avoid division by zero
            {
                RaycastHit hit;
                if (Physics.Raycast(lastPoint, direction.normalized, out hit, distance))
                {
                    // Surface hit!
                    Debug.DrawLine(lastPoint, hit.point, settings.hitColor);
                    Debug.DrawRay(hit.point, hit.normal * 0.1f, settings.normalColor); // Show normal
                    
                    // Mark hit with small cross
                    Debug.DrawLine(hit.point - Vector3.right * 0.02f, hit.point + Vector3.right * 0.02f, Color.red);
                    Debug.DrawLine(hit.point - Vector3.up * 0.02f, hit.point + Vector3.up * 0.02f, Color.red);
                    Debug.DrawLine(hit.point - Vector3.forward * 0.02f, hit.point + Vector3.forward * 0.02f, Color.red);
                    
                    surfaceHit = true;
                }
                else
                {
                    // No hit, draw line - color changes based on progress
                    float progress = currentAngle / settings.maxArcAngle;
                    Color lineColor = settings.arcColorGradient.Evaluate(progress);
                    Debug.DrawLine(lastPoint, pointOnCircle, lineColor);
                }
            }
            
            lastPoint = pointOnCircle;
        }
        
        // If no surface hit after almost 360°
        if (!surfaceHit && currentAngle >= settings.maxArcAngle)
        {
            // Probably a hole in the mesh!
            Debug.DrawRay(lastPoint, (lastPoint - circleCenter).normalized * 0.2f, settings.holeWarningColor);
            
            // Draw warning - almost closed circle without hit
            Vector3 warnPos = circleCenter + Normal * settings.arcRadius * 0.5f;
            Debug.DrawLine(warnPos - arcRight * 0.1f, warnPos + arcRight * 0.1f, settings.holeWarningColor);
            Debug.DrawLine(warnPos - arcForward * 0.1f, warnPos + arcForward * 0.1f, settings.holeWarningColor);
        }
        
        // Optional: Draw helper line to circle center
        if (settings.showCircleCenter)
        {
            Color centerColor = new Color(1f, 0f, 0f, settings.circleCenterAlpha);
            Debug.DrawLine(Position, circleCenter, centerColor);
        }
    }
    
    public void Destroy()
    {
        if (visualObject != null)
            GameObject.Destroy(visualObject);
    }
    
    public void UpdatePosition(Vector3 newPosition, Vector3 newNormal)
    {
        Position = newPosition;
        Normal = newNormal.normalized;
        
        if (visualObject != null)
        {
            visualObject.transform.position = Position;
            // Rotate so Y-axis points in direction of normal
            visualObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, Normal);
        }
    }
    
    // Future methods for actual functionality
    public void SearchForNeighbors()
    {
        // TODO: Implement raycast-based neighbor search
    }
}