using System.Collections.Generic;
using UnityEngine;

public class MeshShellManager : MonoBehaviour
{
    [Header("Agent Settings")]
    [SerializeField] private MeshShellAgentSettings agentSettings;
    [SerializeField] private GameObject agentVisualPrefab; // Optional: For visual representation
    [SerializeField] private float agentScale = 0.1f;
    [SerializeField] private Color agentColor = Color.cyan;
    
    [Header("Raycast Settings")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask targetLayers = -1; // All layers
    
    private List<MeshShellAgent> agents = new List<MeshShellAgent>();
    
    void Start()
    {
        // If no camera assigned, use main camera
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }
    
    void Update()
    {
        // Left click to create new agent
        if (Input.GetMouseButtonDown(0))
        {
            CreateAgentAtMousePosition();
        }
        
        // Press 'C' to clear all agents (for testing)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllAgents();
        }
        
        // Update all agents
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
            // Check if settings are assigned
            if (agentSettings == null)
            {
                Debug.LogError("MeshShellAgentSettings not assigned to MeshShellManager!");
                return;
            }
            
            // Create new agent with settings
            MeshShellAgent newAgent = new MeshShellAgent(
                hit.point,
                hit.normal,
                hit.collider.gameObject,
                agentSettings
            );
            
            agents.Add(newAgent);
            
            // Optional: Create visual GameObject for the agent
            if (agentVisualPrefab != null)
            {
                GameObject visual = Instantiate(agentVisualPrefab, hit.point, Quaternion.identity);
                // Rotate so Y-axis points in direction of normal
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
        
        // Draw all agent positions
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