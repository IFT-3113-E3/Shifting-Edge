using System;
using UnityEngine;

public class GetBounds : MonoBehaviour
{
    public MeshRenderer mesh_renderer;
    public bool show_bounds = true;

    public MeshFilter meshFilter;
    public Mesh mesh;

    private void Start()
    {
        if (!mesh_renderer) mesh_renderer = GetComponent<MeshRenderer>();
        if (!mesh_renderer) return;
        if (!meshFilter) meshFilter = mesh_renderer.GetComponent<MeshFilter>();
        if (!meshFilter) return;
        if (!mesh) mesh = meshFilter.mesh;
        if (!mesh) return;
    }

    private void OnDrawGizmos()
    {
        if (!mesh_renderer) return;
        if (!show_bounds) return;

        if (!meshFilter) meshFilter = mesh_renderer.GetComponent<MeshFilter>();
        if (!meshFilter) return;

        if (!mesh) mesh = meshFilter.mesh;
        if (!mesh) return;

        var vertices = mesh.vertices;
        if (vertices.Length <= 0) return;

        // TransformPoint converts the local mesh vertice dependent on the transform
        // position, scale and orientation into a global position
        var min = transform.TransformPoint(vertices[0]);
        var max = min;
    
        // Iterate through all vertices
        // except first one
        for (var i = 1; i < vertices.Length; i++)
        {
            var V = transform.TransformPoint(vertices[i]);

            // Go through X,Y and Z of the Vector3
            for (var n = 0; n < 3; n++)
            {
                max = Vector3.Max(V, max);
                min = Vector3.Min(V, min);
            }
        }

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);

        // ust to compare it to the original bounds
        Gizmos.DrawWireCube(mesh_renderer.bounds.center, mesh_renderer.bounds.size);
        Gizmos.DrawWireSphere(mesh_renderer.bounds.center, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawWireSphere(bounds.center, 0.3f);
    }
}