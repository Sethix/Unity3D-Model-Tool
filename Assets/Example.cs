using UnityEngine;
using System.Collections;

public class Example : MonoBehaviour
{

    public Vector3[] verts;
    public Vector3[] normals;
    public Vector2[] uv;
    public int[] tris;

    Mesh msh;

	// Use this for initialization
	void Start ()
    {
        msh = new Mesh();
        GetComponent<MeshFilter>().mesh = msh;
        msh.vertices = verts;
        msh.uv = uv;
        msh.triangles = tris;
        msh.normals = normals;
	}
	
	// Update is called once per frame
	void Update ()
    {
        msh.Clear();
        msh.vertices = verts;
        msh.uv = uv;
        msh.triangles = tris;
        msh.normals = normals;
	}
}
