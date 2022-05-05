using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class WorldGenerator: MonoBehaviour
{
    public const int maxChunkSize = 241;
    [Range (0, 6)]
    public int levelOfDetails;

    public float noiseScale;

    public int octaves;
    public float lacunarity;
    [Range (0, 1)]
    public float persistance;
    public int seed;
    public Vector2 offset;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public TerrainType[] terrainTypes;

    public bool autoUpdate;
    public enum DrawType { NoiseMap, ColorMap, DrawMesh };
    public DrawType drawType;


    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue;
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue;

    private void Awake()
    {
        mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    }
    public void DrawInEditor() {
        MapData mapData = GenerateWorld();

        WorldVisualize visual = FindObjectOfType<WorldVisualize>();
        if (drawType == DrawType.NoiseMap)
            visual.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        else if (drawType == DrawType.ColorMap)
            visual.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, maxChunkSize, maxChunkSize));
        else if (drawType == DrawType.DrawMesh)
        {
            visual.DrawMesh(MeshGenerator.GenerateMeshData(mapData.heightMap, heightMultiplier, heightCurve, levelOfDetails),
                TextureGenerator.TextureFromColorMap(mapData.colorMap, maxChunkSize, maxChunkSize));
        }
    }
    MapData GenerateWorld()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(maxChunkSize, maxChunkSize, noiseScale, octaves, persistance, lacunarity, seed, offset);

        Color[] colorMap = new Color[maxChunkSize * maxChunkSize];
        for (int y = 0; y < maxChunkSize; y++)
        {
            for (int x = 0; x < maxChunkSize; x++)
            {
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (noiseMap[x, y] <= terrainTypes[i].height)
                    {
                        colorMap[y * maxChunkSize + x] = terrainTypes[i].color;
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
        if (noiseScale <= 0)
            noiseScale = 0;
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0) { 
            for (int i =0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0) { 
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


    //Day 6: Threading
    //RequestMapData
    public void RequestMapData(Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    //MapDataThread
    void MapDataThread(Action<MapData> callback) {
        MapData mapData = GenerateWorld();
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateMeshData(mapData.heightMap, heightMultiplier, heightCurve, levelOfDetails);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }
    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}


