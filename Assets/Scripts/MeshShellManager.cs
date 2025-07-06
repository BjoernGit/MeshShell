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
    private float arcStepAngle = 5f; // Grad pro Schritt
    private float maxArcAngle = 359f; // Fast ein ganzer Kreis
    
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
        Vector3 circleCenter = Position - arcForward * arcRadius;
        
        Vector3 lastPoint = Position;
        bool surfaceHit = false;
        float currentAngle = 0f;
        
        // Suche entlang des Arcs bis eine Oberfläche getroffen wird
        while (currentAngle < maxArcAngle && !surfaceHit)
        {
            currentAngle += arcStepAngle;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            
            // Berechne nächste Position auf dem Kreis
            Vector3 pointOnCircle = circleCenter 
                + arcForward * arcRadius * Mathf.Cos(angleRad)
                + Normal * arcRadius * Mathf.Sin(angleRad);
            
            // Raycast von letztem Punkt zum nächsten
            Vector3 direction = pointOnCircle - lastPoint;
            float distance = direction.magnitude;
            
            if (distance > 0.001f) // Vermeidet Division durch 0
            {
                RaycastHit hit;
                if (Physics.Raycast(lastPoint, direction.normalized, out hit, distance))
                {
                    // Oberfläche getroffen!
                    Debug.DrawLine(lastPoint, hit.point, Color.green);
                    Debug.DrawRay(hit.point, hit.normal * 0.1f, Color.cyan); // Zeige Normale
                    
                    // Markiere Treffer mit kleiner Kugel
                    Debug.DrawLine(hit.point - Vector3.right * 0.02f, hit.point + Vector3.right * 0.02f, Color.red);
                    Debug.DrawLine(hit.point - Vector3.up * 0.02f, hit.point + Vector3.up * 0.02f, Color.red);
                    Debug.DrawLine(hit.point - Vector3.forward * 0.02f, hit.point + Vector3.forward * 0.02f, Color.red);
                    
                    surfaceHit = true;
                }
                else
                {
                    // Kein Treffer, zeichne Linie - Farbe ändert sich je nach Fortschritt
                    float progress = currentAngle / maxArcAngle;
                    Color lineColor = Color.Lerp(Color.yellow, Color.magenta, progress);
                    Debug.DrawLine(lastPoint, pointOnCircle, lineColor);
                }
            }
            
            lastPoint = pointOnCircle;
        }
        
        // Falls keine Oberfläche getroffen wurde nach fast 360°
        if (!surfaceHit && currentAngle >= maxArcAngle)
        {
            // Wahrscheinlich ein Loch im Mesh!
            Debug.DrawRay(lastPoint, (lastPoint - circleCenter).normalized * 0.2f, Color.magenta);
            
            // Zeichne Warnung - fast geschlossener Kreis ohne Treffer
            Vector3 warnPos = circleCenter + Normal * arcRadius * 0.5f;
            Debug.DrawLine(warnPos - arcRight * 0.1f, warnPos + arcRight * 0.1f, Color.magenta);
            Debug.DrawLine(warnPos - arcForward * 0.1f, warnPos + arcForward * 0.1f, Color.magenta);
        }
        
        // Optional: Zeichne Hilfslinie zum Kreismittelpunkt
        Debug.DrawLine(Position, circleCenter, new Color(1f, 0f, 0f, 0.1f));
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