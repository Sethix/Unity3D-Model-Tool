using UnityEngine;

public static class MapTextureGenerator
{

	public static Texture2D TextureFromColorMap(Color[] colorMap, int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(colorMap);
        tex.Apply();

        return tex;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int w = heightMap.GetLength(0);
        int h = heightMap.GetLength(1);

        Color[] colorMap = new Color[w * h];

        for(int y = 0; y < h; ++y)
            for(int x = 0; x < w; ++x)
                colorMap[y * w + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

        return TextureFromColorMap(colorMap, w, h);

    }
}
