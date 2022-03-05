using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer TextureRenderer;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        public void DrawTexture(Texture2D texture)
        {
            TextureRenderer.sharedMaterial.mainTexture = texture;
            TextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

        public void DrawMesh(MeshData meshData, Texture2D height)
        {
            MeshFilter.sharedMesh = meshData.CreateMesh();
            MeshRenderer.sharedMaterial.mainTexture = height;
        }

        public void DrawMap(MeshData meshData, Texture2D height, Texture2D color)
        {
            //MeshFilter.sharedMesh = meshData.CreateMesh();

            MeshRenderer.sharedMaterial.SetTexture("_Color", color);
            MeshRenderer.sharedMaterial.SetTexture("_Height", height);
        }
    }
}