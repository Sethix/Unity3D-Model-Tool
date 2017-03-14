using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;

    [Range(0,6)]
    public int editorPreviewLOD;

    public float noiseScale;
    public int octaves;

    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(MapTextureGenerator.TextureFromHeightMap(mData.heightMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(MapTextureGenerator.TextureFromColorMap(mData.colorMap, mapChunkSize, mapChunkSize));
        else if (drawMode == DrawMode.Mesh)
            display.DrawMesh(MapMeshGenerator.GenerateTerrainMesh(mData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), MapTextureGenerator.TextureFromColorMap(mData.colorMap, mapChunkSize, mapChunkSize));
    }

    public void RequestMapData(Vector2 center, Action<MapData> callBack)
    {
        ThreadStart tStart = delegate
        {
            MapDataThread(center, callBack);
        };

        new Thread(tStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callBack)
    {
        MapData mData = GenerateMapData(center);
        lock(mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mData));
        }
    }

    public void RequestMeshData(MapData mData, int lod, Action<MeshData> callBack)
    {
        ThreadStart tStart = delegate
        {
            MeshDataThread(mData, lod, callBack);
        };

        new Thread(tStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callBack)
    {
        MeshData meshData = MapMeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }
    }

    void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; ++i)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; ++i)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for(int y = 0; y < mapChunkSize; ++y)
        {
            for(int x = 0; x < mapChunkSize; ++x)
            {
                float currentH = noiseMap[x, y];
                for(int i = 0; i < regions.Length; ++i)
                {
                    if (currentH >= regions[i].height)
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    else break;
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if (lacunarity < 0) lacunarity = 0;
        if (octaves < 1) octaves = 1;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callBack, T param)
        {
            callback = callBack;
            parameter = param;
        }
    }

}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] height, Color[] color)
    {
        heightMap = height;
        colorMap = color;
    }
}
