using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D t)
    {
        textureRender.sharedMaterial.mainTexture = t;
        textureRender.transform.localScale = new Vector3(t.width, 1, t.height);
    }

    public void DrawMesh(MeshData mData, Texture2D t)
    {
        meshFilter.sharedMesh = mData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = t;
    }
}
