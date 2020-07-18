
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MeshGen
{
	
	//test, fills 1 by 1 cube
	public static MeshData GenerateTerrainMesh(turnOnMarch.Sphere[] sphere)
	{
		MeshData meshData = new MeshData();
		int cubeIndex = 0;
		if (sphere[0].onSphere)
		{
			cubeIndex |= 1;
		}
		if (sphere[1].onSphere)
		{
			cubeIndex |= 2;
		}
		if (sphere[2].onSphere)
		{
			cubeIndex |= 4;
		}
		if (sphere[3].onSphere)
		{
			cubeIndex |= 8;
		}
		if (sphere[4].onSphere)
		{
			cubeIndex |= 16;
		}
		if (sphere[5].onSphere)
		{
			cubeIndex |= 32;
		}
		if (sphere[6].onSphere)
		{
			cubeIndex |= 64;
		}
		if (sphere[7].onSphere)
		{
			cubeIndex |= 128;
		}
		List<Vector3> vertices = new List<Vector3>();
		int [] triangles = MarchTables.triangulation[cubeIndex];
		int index = 0;
		foreach (int edgeIndex in triangles)
		{
			if (edgeIndex is -1)
			{
				break;
			}
			int indexA = MarchTables.cornerIndexAFromEdge[edgeIndex];
			int indexB = MarchTables.cornerIndexBFromEdge[edgeIndex];

			Vector3 vertexPos = (sphere[indexA].position + sphere[indexB].position) / 2;
			vertices.Add(vertexPos);
			index += 1;
		}
		meshData.AddVertices(vertices);
		return meshData;
	}

	//triangle struct that gets data from gpu
	public struct Triangle
	{
		public Vector3 vertexA;
		public Vector3 vertexB;
		public Vector3 vertexC;

		public Vector3 this [int i]
		{
			get
			{
				switch(i)
				{
					case 0: return vertexA;
					case 1: return vertexB;
					default: return vertexC;

				}
			}
		}
	};

	//for fillspace gpu
	public static MeshData GenerateTerrainMesh(float[] noiseMap, float genocide, int size, 
		ComputeShader shader, float[] edge, bool applySandpaper)
	{
		int maxCountTriangles = (size - 1) * (size - 1)* (size - 1)* 5;
		MeshData meshData = new MeshData();
		ComputeBuffer triangleBuffer = new ComputeBuffer(maxCountTriangles, sizeof(float)*3*3, ComputeBufferType.Append);
		ComputeBuffer noiseMapBuffer = new ComputeBuffer(noiseMap.Length, sizeof(float));
		ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		triangleBuffer.SetCounterValue(0);
		noiseMapBuffer.SetData(noiseMap);

		int kernel = shader.FindKernel("normal");
		shader.SetBuffer(kernel, "triangles", triangleBuffer);
		shader.SetBuffer(kernel, "noiseMap", noiseMapBuffer);
		shader.SetFloat("genocide", genocide);
		shader.SetInt("size", size);
		shader.SetFloats("edge", edge); 
		shader.SetBool("applySandpaper", applySandpaper);

		int sizeThread = Mathf.CeilToInt(size / 8f);

		shader.Dispatch(kernel, sizeThread, sizeThread, sizeThread);

		ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
		int [] numTriangles = { 0 };
		triangleCountBuffer.GetData(numTriangles);
		int numTris = numTriangles[0];

		Triangle[] triangles = new Triangle[numTris];
		triangleBuffer.GetData(triangles, 0, 0, numTris);
		meshData.AddVertices(triangles, numTris);
		triangleBuffer.Release();
		noiseMapBuffer.Release();
		triangleCountBuffer.Release();
		return meshData;
	}

	//for sphere gpu
	public static MeshData GenerateTerrainMesh(float[] noiseMap, float genocide, int size, 
		float radius, ComputeShader shader, float[] center, bool applySandpaper)
	{
		int maxCountTriangles = (size - 1) * (size - 1) * (size - 1) * 5;
		MeshData meshData = new MeshData();
		ComputeBuffer triangleBuffer = new ComputeBuffer(maxCountTriangles, sizeof(float) * 3 * 3, ComputeBufferType.Append);
		ComputeBuffer noiseMapBuffer = new ComputeBuffer(noiseMap.Length, sizeof(float));
		ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		triangleBuffer.SetCounterValue(0);
		noiseMapBuffer.SetData(noiseMap);

		int kernel = shader.FindKernel("sphere");
		shader.SetBuffer(kernel, "triangles", triangleBuffer);
		shader.SetBuffer(kernel, "noiseMap", noiseMapBuffer);
		shader.SetFloat("genocide", genocide);
		shader.SetFloat("radius", radius);
		shader.SetInt("size", size);
		shader.SetFloats("center", center);
		shader.SetBool("applySandpaper", applySandpaper);

		int sizeThread = Mathf.CeilToInt(size-1 / 8f);

		shader.Dispatch(kernel, sizeThread, sizeThread, sizeThread);

		ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
		int[] numTriangles = { 0 };
		triangleCountBuffer.GetData(numTriangles);
		int numTris = numTriangles[0];

		Triangle[] triangles = new Triangle[numTris];
		triangleBuffer.GetData(triangles, 0, 0, numTris);
		meshData.AddVertices(triangles, numTris);
		triangleBuffer.Release();
		noiseMapBuffer.Release();
		triangleCountBuffer.Release();
		return meshData;
	}

	//fillspace nongpu
	public static MeshData GenerateTerrainMesh(float [,,] noiseMap, float genocide, int size)
	{
		
		MeshData meshData = new MeshData();
		
		List<Vector3> fullVertices = new List<Vector3>();
		List<Vector3> tempVertices = new List<Vector3>();
		for (int z=0; z< size - 1; z+=1)
		{
			for (int y=0; y< size - 1; y+=1)
			{
				for (int x=0; x< size - 1; x+=1)
				{
					Cube cube = new Cube(new Vector3Int(x,y,z), noiseMap, genocide);
					if (cube.hasVertices == true)
					{
						fullVertices = new List<Vector3>(tempVertices.Count + cube.vertices.Count);
						fullVertices.AddRange(tempVertices);
						fullVertices.AddRange(cube.vertices);
						tempVertices = fullVertices;
					}
					
				}
			}
		}

		meshData.AddVertices(fullVertices);
		return meshData;
	}
}
public class Cube
{
	int cubeIndex;
	Vector3Int[] cubeCorners;
	Vector3Int position;
	float genocide;
	public bool hasVertices = false;
	public List <Vector3> vertices = new List<Vector3>();
	

	
	public Cube(Vector3Int xyz, float [,,] noiseMap, float genocide)
	{
		this.genocide = genocide;
		this.position = xyz;

		fillCubeCorners();
		generateCubeIndex(noiseMap);
		int[] triangles = MarchTables.triangulation[cubeIndex];
		getVertices(triangles);
	}

	void fillCubeCorners()
	{
		cubeCorners = new Vector3Int[8];
		cubeCorners[0] = new Vector3Int(position.x, position.y, position.z);
		cubeCorners[1] = new Vector3Int(position.x + 1, position.y, position.z);
		cubeCorners[2] = new Vector3Int(position.x + 1, position.y, position.z + 1);
		cubeCorners[3] = new Vector3Int(position.x, position.y, position.z + 1);
		cubeCorners[4] = new Vector3Int(position.x, position.y + 1, position.z);
		cubeCorners[5] = new Vector3Int(position.x + 1, position.y + 1, position.z);
		cubeCorners[6] = new Vector3Int(position.x + 1, position.y + 1, position.z + 1);
		cubeCorners[7] = new Vector3Int(position.x, position.y + 1, position.z + 1);
	}
	void generateCubeIndex(float [,,] noiseMap)
	{
		int cubeIndexValue = 1;
		Vector3 maxCoord = new Vector3(noiseMap.GetLength(0) - 1, noiseMap.GetLength(1) - 1, noiseMap.GetLength(2) - 1);
		Vector3 minCoord = Vector3.zero;
		for (int i=0; i<8; i++)
		{

			cubeIndex |= getCubeIndex(noiseMap, cubeCorners[i], cubeIndexValue, maxCoord, minCoord);
			cubeIndexValue *= 2;
		}

	}

	//responsible for filling cube index values
	int getCubeIndex(float[,,] noiseMap, Vector3Int position, int valueOfPos, Vector3 maxCoord, Vector3 minCoord)
	{
		if (position.x == minCoord.x || position.x == maxCoord.x || position.y == minCoord.y ||
			position.y == maxCoord.y|| position.z == minCoord.z || position.z == maxCoord.z)
		{
			//return 0;
		}
		float noiseValue = noiseMap[(int)position.x, (int)position.y, (int)position.z];
		if (noiseValue< genocide)
		{
			return valueOfPos;
		}
		return 0;

	}
	void getVertices(int[] triangles)
	{
		int index = 0;

		foreach(int edgeIndex in triangles)
		{
			if (edgeIndex is -1)
			{
				break;
			}
			hasVertices = true;
			int indexA = MarchTables.cornerIndexAFromEdge[edgeIndex];
			int indexB = MarchTables.cornerIndexBFromEdge[edgeIndex];
			
			Vector3 vertexPos = (cubeCorners[indexA]+cubeCorners[indexB])/2;
			vertexPos *= Random.Range(1, 10);
			vertices.Add(vertexPos);
			index += 1;
		}
	}
}
public class MeshData
{
	Vector3[] vertices;
	int[] triangles;
	
	
	public void AddVertices(List<Vector3> allVerts)
	{
		int length = allVerts.Count;
		vertices = new Vector3[length];
		triangles = new int[length];
		for (int i = 0; i < length; i++)
		{
			vertices[i] = allVerts[i];
			triangles[i] = i;
		}
	}
	public void AddVertices(MeshGen.Triangle[] triangle, int numTriangles)
	{
		vertices = new Vector3[triangle.Length * 3];
		triangles = new int[triangle.Length * 3];
		if (numTriangles*3>64000)
		{
			Debug.Log("too many");
		}
		for (int i = 0; i< numTriangles; i++)
		{
			for(int j=0; j<3; j++)
			{
				vertices[i * 3 + j] = triangle[i][j];
				triangles[i * 3 + j] = i * 3 + j;
			}
		}
	}
	
	public Mesh GetMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		return mesh;
	}
	
}