using UnityEngine;

[CreateAssetMenu(fileName = "MeshShellAgentSettings", menuName = "MeshShell/Agent Settings")]
public class MeshShellAgentSettings : ScriptableObject
{
    [Header("Arc Configuration")]
    [Tooltip("Radius of the search arcs")]
    [Range(0.1f, 5f)]
    public float arcRadius = 0.5f;
    
    [Tooltip("Degrees per step along the arc")]
    [Range(1f, 30f)]
    public float arcStepAngle = 5f;
    
    [Tooltip("Maximum angle an arc can travel (359 = almost full circle)")]
    [Range(45f, 359f)]
    public float maxArcAngle = 359f;
    
    [Tooltip("Number of rays/arcs to cast in different directions")]
    [Range(1, 16)]
    public int numberOfRays = 4;
    
    [Header("Visual Settings")]
    [Tooltip("Color for arc lines when no hit is detected")]
    public Gradient arcColorGradient;
    
    [Tooltip("Color for arc lines when surface is hit")]
    public Color hitColor = Color.green;
    
    [Tooltip("Color for normal visualization at hit points")]
    public Color normalColor = Color.cyan;
    
    [Tooltip("Color for hole detection warning")]
    public Color holeWarningColor = Color.magenta;
    
    [Header("Debug Settings")]
    [Tooltip("Show helper line to circle center")]
    public bool showCircleCenter = true;
    
    [Tooltip("Alpha value for circle center line")]
    [Range(0f, 1f)]
    public float circleCenterAlpha = 0.1f;
    
    // Initialize default gradient in OnEnable
    void OnEnable()
    {
        if (arcColorGradient == null || arcColorGradient.colorKeys.Length == 0)
        {
            arcColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
            colorKeys[1] = new GradientColorKey(Color.magenta, 1f);
            arcColorGradient.colorKeys = colorKeys;
        }
    }
}