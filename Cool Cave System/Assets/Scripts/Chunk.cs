using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector3Int coord;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public void ChangeMesh(MeshData meshData, Material material, float size)
    {
        Mesh mesh = meshData.GetMesh();
        meshRenderer.material = material;
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
    public void SetPosition(float size)
    {
        gameObject.transform.position = coord * (int)(size-1);
    }
}
