using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

/// <summary>
/// Manages the visualization of detected planes in the scene.
/// </summary>
public class MyDetectedPlaneGenerator : MonoBehaviour
{
    
    [SerializeField] Material m_SurfaceMaterial;

    /// <summary>
    /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();

    /// <summary>
    /// The Unity Update method.
    /// </summary>
    public void Update()
    {
        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
        Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
            // coordinates.
            GameObject planeObject = new GameObject("MyDetectedPlaneVisualizer");
            var arSurface = planeObject.AddComponent<MyDetectedPlaneVisualizer>();
            arSurface.Initialize(m_NewPlanes[i], m_SurfaceMaterial);
        }
    }
}