using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMap : MonoBehaviour
{
    public Renderer render;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material material;

    public void DrawMesh(MeshData meshData, Chunk chunk, float size)
    {
        meshFilter.sharedMesh = meshData.GetMesh();
        chunk.ChangeMesh(meshData, material, size);
    }
    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.GetMesh();

    }
}
