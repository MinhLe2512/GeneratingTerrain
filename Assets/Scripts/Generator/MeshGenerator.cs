using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
	public static MeshData GenerateMeshData(float[,] heightMap, float heightMultiplier,
		AnimationCurve _heightCurve, int levelOfDetails)
	{
		AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int increaseLevel;
		if (levelOfDetails == 0)
			increaseLevel = 1;
		else
			increaseLevel = levelOfDetails * 2;

		MeshData meshData = new MeshData(width, height);
		int vertexIndex = 0;

		int verticesPerLine = (width - 1) / increaseLevel + 1;

		for (int y = 0; y < height; y += increaseLevel)
		{
			for (int x = 0; x < width; x += increaseLevel)
			{

				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x,
					heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}

}