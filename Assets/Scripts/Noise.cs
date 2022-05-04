using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int width, int height, float noiseScale, 
        int octaves, float persistance, float lacunarity, int seed, Vector2 offset) {

        //Pseudo random generated number
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (noiseScale <= 0)
            noiseScale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[width, height];

        float halfWidth = width / 2f, halfHeight = height / 2f;

        for (int x = 0; x < width; x++) { 
            for (int y= 0; y < height; y++) {
                float amplitude = 1, frequency = 1;
                float mapHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / (float)noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / (float)noiseScale * frequency + octaveOffsets[i].y;

                    //Perlin return value between [0, 1] * 2 - 1 so the range is [-1, 1]
                    mapHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (mapHeight < minNoiseHeight)
                    minNoiseHeight = mapHeight;
                else if (mapHeight > maxNoiseHeight)
                    maxNoiseHeight = mapHeight;
                noiseMap[x, y] = mapHeight;
            }
        }

        for (int x = 0; x < width; x++) { 
            for (int y = 0; y < height; y++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
