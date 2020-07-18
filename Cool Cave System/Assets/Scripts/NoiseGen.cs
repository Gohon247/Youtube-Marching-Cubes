using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Generates the a 3 dimensional noise map
public static class NoiseGen
{
	public static float[] GenerateNoise(int size, int seed,
		float scale, int numOctaves, float persistence, float lacunarity,
		Vector3 offset, ComputeShader shader)
	{
		float amplitude = 1;

		if (scale <= 0)
		{
			scale = .0001f;
		}

		var random = new System.Random(seed);
		Vector3[] seedOctaveOffset = new Vector3[numOctaves];
		for (int i = 0; i < numOctaves; i++)
		{
			float sOffsetX = random.Next(-100000, 100000) + offset.x;
			float sOffsetY = random.Next(-100000, 100000) + offset.y;
			float sOffsetZ = random.Next(-100000, 100000) + offset.z;
			seedOctaveOffset[i] = new Vector3(sOffsetX, sOffsetY, sOffsetZ);
			amplitude *= persistence;
		}

		float[] noiseMapCompute = new float[size * size * size];

		ComputeBuffer rwBuffer = new ComputeBuffer(noiseMapCompute.Length, sizeof(float));
		ComputeBuffer octBuffer = new ComputeBuffer(seedOctaveOffset.Length, 4*3);//might need to increase size
		//rwBuffer.SetData(noiseMapCompute);
		octBuffer.SetData(seedOctaveOffset);
		shader.SetBuffer(0, "noiseBuffer", rwBuffer);
		shader.SetBuffer(0, "seedOctaveOffset", octBuffer);

		shader.SetInt("size", size);
		shader.SetInt("numOctaves", numOctaves);

		shader.SetFloat("scale", scale);
		shader.SetFloat("persistence", persistence);
		shader.SetFloat("lacunarity", lacunarity);

		int sizeThreads = Mathf.CeilToInt(size / 8f);
		shader.Dispatch(0, sizeThreads, sizeThreads, sizeThreads);
		rwBuffer.GetData(noiseMapCompute);

		rwBuffer.Release();
		octBuffer.Release();
		return noiseMapCompute;

	}
	public static float[,,] GenerateNoise(int size, int seed,
		float scale, int numOctaves, float persistence, float lacunarity, 
		Vector3 offset)
    {
      float [,,] noiseMap = new float[size, size, size];
	  float amplitude = 1;
	  
	  if (scale <=0)
	  {
		  scale = .0001f;
	  }
	  
	  System.Random random = new System.Random(seed);
	  Vector3[] seedOctaveOffset = new Vector3[numOctaves];
	  for (int i=0; i<numOctaves; i++)
	  {
		  float sOffsetX = random.Next(-100000, 100000) + offset.x;
		  float sOffsetY = random.Next(-100000, 100000) + offset.y;
		  float sOffsetZ = random.Next(-100000, 100000) + offset.z;
		  seedOctaveOffset[i] = new Vector3(sOffsetX, sOffsetY, sOffsetZ);
		  amplitude *= persistence;
	  }
	  
	  
	  float halfSize = size / 2;

	  for (int z = 0; z < size; z++)
	  {
		  for (int y = 0; y< size; y++)
		  {
			  for (int x = 0; x< size; x++)
			  {
				  float frequency=1;
				  amplitude=1;
				  float noiseHeight = 0;
				  float perlinValue;
				  for (int o = 0; o < numOctaves; o++)
				  {
					  perlinValue = DootDoot(x- halfSize, y- halfSize,
					     z- halfSize, scale, frequency, seedOctaveOffset[o]);
                      noiseHeight += perlinValue*amplitude;
					  amplitude *= persistence;
					  frequency *= lacunarity;
				  }
				  noiseMap[x, y, z] = noiseHeight;
			  }
		  }

	  }
		return noiseMap;
   }
   
   static float DootDoot(float x, float y, float z, float scale, float frequency, Vector3 sOffset)
   {
		float tempZ = (z + sOffset.z) / scale;// * frequency;
	   float tempX = (x + sOffset.x) / scale;// * frequency;
		float tempY = (y + sOffset.y) / scale;// * frequency;
		float perlinValue = Perlin.Noise(tempX, tempY, tempZ); //keijiro's 3d perlin github
	   return perlinValue;
   }
   
}
