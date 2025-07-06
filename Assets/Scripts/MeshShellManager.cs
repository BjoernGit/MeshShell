using System.Collections.Generic;
using UnityEngine;

public class MeshShellManager : MonoBehaviour
{
    [Header("Agent Settings")]
    [SerializeField] private GameObject agentVisualPrefab; // Optional: Für visuelle Darstellung
    [SerializeField] private float agentScale = 0.1f;
    [SerializeField] private Color agentColor = Color.cyan;
    
    [Header("Raycast Settings")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask targetLayers = -1; // Alle Layer
    
    private List<MeshShellAgent> agents = new List<MeshShellAgent>();
    
    void Start()
    {
        // Falls keine Kamera zugewiesen, nimm die Hauptkamera
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }
    
    void Update()
    {
        // Linksklick für neuen Agent
        if (Input.GetMouseButtonDown(0))
        {
            CreateAgentAtMousePosition();
        }
        
        // Rechtsklick zum Löschen aller Agents (zum Testen)
        if (Input.GetMouseButtonDown(1))
        {
            ClearAllAgents();
        }
        
        // Update alle Agents
        foreach (var agent in agents)
        {
            agent.DrawDebugArcs();
        }
    }
    
    void CreateAgentAtMousePosition()
    {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayers))
        {
            // Erstelle neuen Agent
            MeshShellAgent newAgent = new MeshShellAgent(
                hit.point,
                hit.normal,
                hit.collider.gameObject
            );
            
            agents.Add(newAgent);
            
            // Optional: Erstelle visuelles GameObject für den Agent
            if (agentVisualPrefab != null)
            {
                GameObject visual = Instantiate(agentVisualPrefab, hit.point, Quaternion.identity);
                // Rotiere so, dass Y-Achse in Richtung der Normale zeigt
                visual.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                visual.transform.localScale = Vector3.one * agentScale;
                newAgent.SetVisualObject(visual);
            }
            
            Debug.Log($"Created Agent #{agents.Count} at {hit.point} on {hit.collider.name}");
        }
    }
    
    void ClearAllAgents()
    {
        foreach (var agent in agents)
        {
            agent.Destroy();
        }
        agents.Clear();
        Debug.Log("Cleared all agents");
    }
    
    void OnDrawGizmos()
    {
        if (agents == null) return;
        
        // Zeichne alle Agent-Positionen
        Gizmos.color = agentColor;
        foreach (var agent in agents)
        {
            if (agent != null)
            {
                Gizmos.DrawWireSphere(agent.Position, agentScale * 0.5f);
                Gizmos.DrawRay(agent.Position, agent.Normal * agentScale * 2f);
            }
        }
    }
}

// Separate Datei: MeshShellAgent.cs
[System.Serializable]
public class MeshShellAgent
{
    // Agent Properties
    public Vector3 Position { get; private set; }
    public Vector3 Normal { get; private set; }
    public GameObject TargetObject { get; private set; }
    
    // Debug Settings
    private float arcRadius = 0.5f;
    private int arcSegments = 8;
    private float arcAngle = 45f; // Grad
    
    // Optional visual representation
    private GameObject visualObject;
    
    // Constructor
    public MeshShellAgent(Vector3 position, Vector3 normal, GameObject targetObject)
    {
        Position = position;
        Normal = normal.normalized;
        TargetObject = targetObject;
    }
    
    public void SetVisualObject(GameObject visual)
    {
        visualObject = visual;
    }
    
    public void DrawDebugArcs()
    {
        // Erstelle ein lokales Koordinatensystem basierend auf der Normale
        Vector3 right = Vector3.Cross(Normal, Vector3.up);
        if (right.magnitude < 0.001f) // Falls Normal ~ Vector3.up
            right = Vector3.Cross(Normal, Vector3.forward);
        right.Normalize();
        
        Vector3 forward = Vector3.Cross(right, Normal).normalized;
        
        // Zeichne Debug-Arcs in verschiedene Richtungen
        for (int i = 0; i < 4; i++)
        {
            float baseAngle = i * 90f; // 0°, 90°, 180°, 270°
            DrawArc(baseAngle, right, forward);
        }
    }
    
    private void DrawArc(float baseAngle, Vector3 right, Vector3 forward)
    {
        // Rotiere das lokale Koordinatensystem um die Normale
        Quaternion baseRotation = Quaternion.AngleAxis(baseAngle, Normal);
        Vector3 arcRight = baseRotation * right;
        Vector3 arcForward = baseRotation * forward;
        
        // Der Mittelpunkt des Kreises muss so liegen, dass der Agent-Punkt auf dem Kreis liegt
        // Wenn der Arc nach "vorne" gehen soll, liegt der Mittelpunkt "hinten"
        Vector3 circleCenter = Position - arcForward * arcRadius;
        
        Vector3 lastPoint = Position;
        
        for (int j = 1; j <= arcSegments; j++)
        {
            float t = (float)j / arcSegments;
            float angle = t * arcAngle * Mathf.Deg2Rad;
            
            // Berechne Position auf dem Kreis
            // Start ist bei angle=0 am Agent-Punkt
            Vector3 pointOnCircle = circleCenter 
                + arcForward * arcRadius * Mathf.Cos(angle)
                + Normal * arcRadius * Mathf.Sin(angle);
            
            // Zeichne Linie
            Debug.DrawLine(lastPoint, pointOnCircle, Color.yellow);
            lastPoint = pointOnCircle;
        }
        
        // Optional: Zeichne Hilfslinie zum Kreismittelpunkt
        Debug.DrawLine(Position, circleCenter, Color.red * 0.3f);
    }
    
    public void Destroy()
    {
        if (visualObject != null)
            GameObject.Destroy(visualObject);
    }
    
    // Zukünftige Methoden für die eigentliche Funktionalität
    public void SearchForNeighbors()
    {
        // TODO: Implementiere Raycast-basierte Nachbarsuche
    }
    
    public void UpdatePosition(Vector3 newPosition, Vector3 newNormal)
    {
        Position = newPosition;
        Normal = newNormal.normalized;
        
        if (visualObject != null)
        {
            visualObject.transform.position = Position;
            // Rotiere so, dass Y-Achse in Richtung der Normale zeigt
            visualObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, Normal);
        }
    }
}