using UnityEngine;
using System.Collections.Generic;
using ClipperLib;
using System.Collections;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class LineTool : MonoBehaviour
{
    public GameObject meshObject;

    public Material meshMaterial;

    public int extrusionCount;

    public float extrusionDistance;

    public float offset;

    bool CCW = false;

    Camera cam;

    Mesh mesh;

    CustomMesh newMesh;

    //LineRenderer lineRenderer;

    List<Vector2> polygonPoints;

	// Use this for initialization
	void Start ()
    {
        polygonPoints = new List<Vector2>();
        mesh = new Mesh();

        cam = FindObjectOfType<Camera>();
        //lineRenderer = GetComponent<LineRenderer>();

        //meshObject.AddComponent<MeshFilter>().mesh = mesh;
        //meshObject.AddComponent<MeshRenderer>().material = meshMaterial;
	}

    void OnDrawGizmos()
    {

        if (mesh != null)
        {

            Gizmos.DrawMesh(mesh);

            foreach (Vector3 p in mesh.vertices)
            {
                Gizmos.DrawWireSphere(p, .05f);
            }

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Gizmos.DrawLine(mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]]);
                Gizmos.DrawLine(mesh.vertices[mesh.triangles[i + 2]], mesh.vertices[mesh.triangles[i + 1]]);
                Gizmos.DrawLine(mesh.vertices[mesh.triangles[i + 2]], mesh.vertices[mesh.triangles[i]]);
            }

        }

        if(polygonPoints != null && polygonPoints.Count == 1)
        {
            Gizmos.DrawWireSphere(polygonPoints[0], 0.1f);
        }
        else if (polygonPoints != null && polygonPoints.Count > 1)
        {
            for(int i = 0; i < polygonPoints.Count; ++i)
            {
                Gizmos.DrawWireSphere(polygonPoints[i], 0.1f);
                Gizmos.DrawLine(polygonPoints[i], polygonPoints[(i + 1) % polygonPoints.Count]);
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //lineRenderer.SetVertexCount(polygonPoints.Count);
        //lineRenderer.SetPositions(polygonPoints.ToArray());      

        if (Input.GetButtonDown("Fire1"))
        {
            polygonPoints.Add(new Vector2((Input.mousePosition.x / cam.pixelWidth) * 2 - 1, (Input.mousePosition.y / cam.pixelHeight) * 2 - 1));

            Debug.Log(new Vector2((Input.mousePosition.x / cam.pixelWidth), 
                                  (Input.mousePosition.y / cam.pixelHeight)).ToString() + " : MP");
        }

        if(Input.GetButtonDown("Fire3"))
        {
            newMesh = new CustomMesh(polygonPoints, meshMaterial, false, extrusionCount, extrusionDistance, offset);

            polygonPoints.Clear();

            meshObject.AddComponent<MeshFilter>().mesh = newMesh.mesh;
            meshObject.AddComponent<MeshRenderer>().material = newMesh.material;
        }
	}
}
