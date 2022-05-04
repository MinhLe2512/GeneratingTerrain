using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator: MonoBehaviour
{
    public int maxChunkSize = 241;
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

    public void GenerateWorld()
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

        WorldVisualize visual = FindObjectOfType<WorldVisualize>();
        if (drawType == DrawType.NoiseMap)
            visual.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawType == DrawType.ColorMap)
            visual.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, maxChunkSize, maxChunkSize));
        else if (drawType == DrawType.DrawMesh)
        {
            visual.DrawMesh(MeshGenerator.GenerateMeshData(noiseMap, heightMultiplier, heightCurve, levelOfDetails),
                TextureGenerator.TextureFromColorMap(colorMap, maxChunkSize, maxChunkSize));
        }
    }

    private void OnValidate()
    {
        if (maxChunkSize <= 1)
            maxChunkSize = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
        if (noiseScale <= 0)
            noiseScale = 0;
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

