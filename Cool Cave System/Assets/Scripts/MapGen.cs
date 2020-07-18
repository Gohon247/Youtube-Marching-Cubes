using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
//Control script that generates everything
public class MapGen : MonoBehaviour
{
	public enum Mode {test, marchingMesh, compute, radius};
	public Mode mode;

	public bool autoUpdate;
	public bool applySandpaper;

	[Header("size")]
	public int size;
	public float radius;
	public float scale;

	[Header("Default Settings")]
	public Vector3 offset;
	public float genocideValue;
	public int octaves;
	[Range (0,1)]
	public float persistence; //.5 by default
	public float lacunarity; //2 is default
	public int seed;



	[Header ("Shaders")]
	public ComputeShader noiseGen;
	public ComputeShader marchCubes;


	public Color color;


	private void OnValidate() {
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
		if (radius < 1)
		{
			radius = 1;
		}
	}
	
	public void Run()
	{
		float[] edge = new float[3];
		float[] centerSph = new float[3];
		GetChunks();
		for (int i=0; i< chunkHolder.Length; i++)
		{
			chunkHolder[i].chunk.SetPosition(size);
			SetBounds(i, ref edge, ref centerSph);
			MapData mapData = GenerateMapData(chunkHolder[i].position);
			DisplayMap display = FindObjectOfType<DisplayMap>();
			if (mode is Mode.marchingMesh)
			{
				//only for first chunk
				display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMap, genocideValue, size));
				break;
			}
			else if (mode is Mode.compute)
			{
				display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMapCompute, genocideValue, size, marchCubes, edge, applySandpaper),
					chunkHolder[i].chunk, size);
			}
			else if (mode is Mode.radius)
			{
				display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMapCompute, genocideValue, size, radius, marchCubes, centerSph, applySandpaper),
					chunkHolder[i].chunk, size);
			}
		}

	}
	
	MapData GenerateMapData (Vector3 center) //correlates noiseMap with a color
	{
		if (mode is Mode.compute || mode is Mode.radius)
		{
			float[] noiseMap = NoiseGen.GenerateNoise(
			   size, seed, scale, octaves, persistence, lacunarity,
			   center + offset, noiseGen);
			return new MapData(noiseMap, color);
		}
		else
		{
			float[,,] noiseMap = NoiseGen.GenerateNoise(
			   size, seed, scale, octaves, persistence, lacunarity,
			   center + offset);
			return new MapData(noiseMap, color);
		}
	}
	 
	void SetBounds(int i, ref float[] edge, ref float[] center)
	{
		Chunk chunk = chunkHolder[i].chunk;
		center = new float[3] { size, size, size };
		edge = new float[3] {0,0,0};
		if (chunk.coord.x is 1)
		{
			center[0] =0;
			edge[0] = size - 1;
		}
		if (chunk.coord.y is 1)
		{
			center[1] =0;
			edge[1] = size - 1;
		}
		if (chunk.coord.z is 1)
		{
			center[2] =0;
			edge[2] = size - 1;
		}
		return;

	}

	ChunkHolder[] chunkHolder;
	public void GetChunks()
	{
		Chunk[] chunks = FindObjectsOfType<Chunk>();
		chunkHolder = new ChunkHolder[chunks.Length];
		int counter = 0;
		foreach (Chunk chunk in chunks)
		{
			chunkHolder[counter] = new ChunkHolder(chunk, chunk.coord, size);
			counter++;
		}

	}

	struct ChunkHolder
	{
		public readonly Chunk chunk;
		public readonly Vector3 position;
		public ChunkHolder(Chunk chunk, Vector3 position, float size)
		{
			this.chunk = chunk;
			this.position = (Vector3)(chunk.coord)*(size-1);
		}
	}
}

public struct MapData
{
	public readonly float[,,] heightMap;
	public readonly float[] heightMapCompute;
	public readonly Color color;
	
	public MapData(float[] hMap, Color color)
	{
		heightMapCompute = hMap;
		heightMap = null;
		this.color = color;
	}
	public MapData(float[,,] hMap, Color color)
	{
		heightMap = hMap;
		heightMapCompute = null;
		this.color = color;
	}
}
	
