using UnityEngine;
using System.Collections;

public static class MapMeshGenerator
{

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, 
                                               AnimationCurve heightCurve, int LOD)
    {
        int w = heightMap.GetLength(0);
        int h = heightMap.GetLength(1);

        float tlX = (w - 1) / -2f;
        float tlZ = (h - 1) / -2f;

        int simplification = (LOD == 0) ? 1 : LOD * 2;
        int vertsPerLine = (w - 1) / simplification + 1;

        MeshData meshData = new MeshData(vertsPerLine, vertsPerLine);
        int vertIdx = 0;

        for(int y = 0; y < h; y += simplification)
        {
            for(int x = 0; x < w; x += simplification)
            {
                meshData.verts[vertIdx] = new Vector3(tlX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, tlZ - y);
                meshData.uvs[vertIdx] = new Vector2(x / (float)w, y / (float)h);

                if(x < w - 1 && y < h - 1)
                {
                    meshData.AddTriangle(vertIdx, vertIdx + vertsPerLine + 1, vertIdx + vertsPerLine);
                    meshData.AddTriangle(vertIdx + vertsPerLine + 1, vertIdx, vertIdx + 1);
                }

                vertIdx++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] verts;
    public int[] tris;
    public Vector2[] uvs;

    int triangleIdx;

    public MeshData(int w, int h)
    {
        verts = new Vector3[w * h];
        uvs = new Vector2[w * h];
        tris = new int[(w - 1) * (h - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        tris[triangleIdx] = a;
        tris[triangleIdx + 1] = b;
        tris[triangleIdx + 2] = c;
        triangleIdx += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}
