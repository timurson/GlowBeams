// Sunbeams charge-up effect.
// Roman Timurson, 2016
// https://github.com/timurson/GlowBeams

using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Class responsible for creating a charge-up beams effect. 
/// </summary>
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class Sunbeams : MonoBehaviour
{
    #region Public variables

    /// <summary>
    /// Number of beams that will be spawned. 
    /// </summary>
    public int beamCount = 100;

    /// <summary>
    /// Beam effect radius (separation between beams). 
    /// </summary>
    [Range(1.0f, 6.0f)] 
    public float beamRadius = 1.0f;

    /// <summary>
    /// The thickness of each individual beam created. 
    /// </summary>
    [Range(0.01f, 0.2f)]
    public float beamWidth = 0.1f;

    /// <summary>
    /// Time scale to speed up or slowdown beam velocity. 
    /// </summary>
    [Range(0.1f, 10.0f)]
    public float timeScale = 0.4f;
    #endregion

    #region Beam list

    /// <summary>
    /// Helper class to hold beam properties. 
    /// </summary>
    public class Beam
    {
        /// <summary>
        /// Accumulated speed of each speed. 
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Current randomly selected acceleration of each beam. 
        /// </summary>
        public float Acceleration { get; set; }

        /// <summary>
        /// Beam's location from the spawn center. 
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// Beam's travel direction of a unit sphere. 
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Extent used in determining each beam's width. 
        /// </summary>
        public Vector3 Extent { get; set; }
    }
 
    #endregion

    /// <summary>
    /// List of beams. 
    /// </summary>
    private List<Beam> beams;

    #region Mesh data

    /// <summary>
    /// Procedurally modified mesh. 
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// Array of vertices for the mesh. 
    /// </summary>
    Vector3[] vertices;

    #endregion

    #region Private functions

    /// <summary>
    /// Called to recreate beams. 
    /// </summary>
    void ResetBeams ()
    {
        // Allocate arrays.
        beams = new List<Beam>();
        vertices = new Vector3[beamCount * 3];
        var normals = new Vector3[beamCount * 3];

        // Initialize the beam vectors.
        var normalIndex = 0;
        var vertexIndex = 0;
        for (var i = 0; i < beamCount; i++) {
            // Make a beam in a completely random way.
            var dir = Random.onUnitSphere;
            var ext = Random.onUnitSphere;
            beams.Add(new Beam());

            beams[i].Direction = dir;
            beams[i].Extent = ext;
            beams[i].Origin = dir * beamRadius;
            beams[i].Speed = 0.0f;
            beams[i].Acceleration = Random.Range(0.05f, 0.3f);

            // calculate a triangle normals
            var normal = Vector3.Cross (dir, ext).normalized;
            normals [normalIndex++] = Vector3.Lerp (dir, normal, 0.5f).normalized;
            normals [normalIndex++] = normal;
            normals [normalIndex++] = normal;

            // initialize beam vertices
            var scale = Random.Range(0.1f, 0.5f);
            vertices[vertexIndex++] = beams[i].Origin;
            // Update the 2nd and 3rd vertices.
            var tip = beams[i].Direction * scale;
            var extent = beams[i].Extent * beamWidth * scale;
            vertices[vertexIndex++] = beams[i].Origin + tip - extent;
            vertices[vertexIndex++] = beams[i].Origin + tip + extent;
        }
        
        // Initialize the triangle set.
        var indices = new int[beamCount * 3];
        for (var i = 0; i < indices.Length; i++) {
            indices [i] = i;
        }
        
        // Initialize the mesh.
        mesh.Clear ();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = indices;
    }

    /// <summary>
    /// Called to update each beam's positions based on direction, speed, time, etc. 
    /// </summary>
    void UpdateVertices ()
    {
        var vertexIndex = 0;
        for (var i = 0; i < beamCount; i++)
        {
            Vector3 beamDir = beams[i].Direction;
            beams[i].Speed += beams[i].Acceleration * Time.deltaTime * timeScale;
            Vector3 delta = -beamDir * beams[i].Speed;

            int pointsInCenter = 0;
            for (var vert = vertexIndex; vert < vertexIndex + 3; ++vert)
            {
                Vector3 vertex = vertices[vert];
                if (vertex.sqrMagnitude >= 0.1f)
                {
                    vertices[vert] += delta;
                }
                else
                {
                    pointsInCenter++;
                }
            }

            // reset beam position when it has reached center
            if (pointsInCenter == 3)
            {
                var scale = Random.Range(0.1f, 0.5f);
                vertices[vertexIndex] = beams[i].Origin;
                // Update the 2nd and 3rd vertices.
                var tip = beamDir * scale;
                var extent = beams[i].Extent * beamWidth * scale;
                vertices[vertexIndex + 1] = beams[i].Origin + tip - extent;
                vertices[vertexIndex + 2] = beams[i].Origin + tip + extent;
                beams[i].Speed = 0.0f;
                beams[i].Acceleration = Random.Range(0.05f, 0.2f);
            }
            vertexIndex += 3;
        }
    }
    #endregion

    #region Monobehaviour functions
    void Awake ()
    {
        // Initialize the mesh instance.
        mesh = new Mesh ();
        mesh.MarkDynamic ();
        GetComponent<MeshFilter> ().sharedMesh = mesh;

        // Initialize the beam array.
        ResetBeams ();
    }

    void Update ()
    {
        // Reset the beam array.
        if (beamCount != beams.Count) {
            ResetBeams ();
        }

        // Do animation.
        UpdateVertices ();

        // Update the vertex array.
        mesh.vertices = vertices;

    }
    #endregion
}