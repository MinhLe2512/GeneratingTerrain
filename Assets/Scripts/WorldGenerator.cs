using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator: MonoBehaviour
{
    public int width;
    public int height;
    public float noiseScale;

    public int octaves;
    public float lacunarity;
    [Range (0, 1)]
    public float persistance;
    public int seed;
    public Vector2 offset;

    public TerrainType[] terrainTypes;

    public bool autoUpdate;
    public enum DrawType { NoiseMap, ColorMap};
    public DrawType drawType;

    public void GenerateWorld() {
        float[,] noiseMap = Noise.GenerateNoiseMap(width, height, noiseScale,
            octaves, persistance, lacunarity, seed, offset);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) { 
            for (int x = 0; x < height; x++) { 
                for (int i = 0; i < terrainTypes.Length; i++) { 
                    if (noiseMap[x, y] <= terrainTypes[i].height) {
                        colorMap[y * width + x] = terrainTypes[i].color; 
                        break;
                    }
                }
            }
        }

        WorldVisualize visual = FindObjectOfType<WorldVisualize>();
        if (drawType == DrawType.NoiseMap)
            visual.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawType == DrawType.ColorMap)
            visual.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, width, height));
    }

    private void OnValidate()
    {
        if (width <= 1)
            width = 1;
        if (height <= 1)
            height = 1;
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

