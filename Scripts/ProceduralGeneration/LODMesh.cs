using System;
using UnityEngine;

namespace Procedural.Terrain
{
    public class LODMesh
    {
        public Mesh Mesh;
        public bool HasRequestedMesh;
        public bool HasMesh => Mesh != null;
        
        private int _lodLevel;
        private Action _updateCallback;
        private MeshData _meshData;

        public LODMesh(int lodLevel, Action updateCallback)
        {
            _lodLevel = lodLevel;
            _updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData, MapGenerator mapGenerator)
        {
            HasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, _lodLevel, OnMeshdataReceived);
        }

        public Mesh GetLodAdjustedMesh(LodSurroundings surroundLodLevels)
        {
            // adjust the mesh per surrounding LOD levels
            Mesh m = Mesh;
            Vector3[] vertices = m.vertices;
            int meshSize = _meshData.MeshSize;

            int myLodLevel = surroundLodLevels.Self;

            // bottom
            int sideLodLevel = surroundLodLevels.Down;
            int lodDelta = sideLodLevel - myLodLevel;
            int meshSimplificationIncrement = lodDelta * 2;
            int startIndex = 0;
            int vertexIncrement = 1;
            int endIndex = meshSize - 1;
            AdjustEdgeVertices(vertices, lodDelta, startIndex, endIndex, vertexIncrement);

            // top
            sideLodLevel = surroundLodLevels.Up;
            lodDelta = sideLodLevel - myLodLevel;
            meshSimplificationIncrement = lodDelta * 2;
            startIndex = (meshSize - 1) * meshSize;
            vertexIncrement = 1;
            endIndex = (meshSize * meshSize) - 1;
            AdjustEdgeVertices(vertices, lodDelta, startIndex, endIndex, vertexIncrement);

            // right
            sideLodLevel = surroundLodLevels.Right;
            lodDelta = sideLodLevel - myLodLevel;
            meshSimplificationIncrement = lodDelta * 2;
            startIndex = meshSize - 1;
            vertexIncrement = meshSize;
            endIndex = (meshSize * (meshSize - 1));
            AdjustEdgeVertices(vertices, lodDelta, startIndex, endIndex, vertexIncrement);

            // left
            sideLodLevel = surroundLodLevels.Left;
            lodDelta = sideLodLevel - myLodLevel;
            meshSimplificationIncrement = lodDelta * 2;
            startIndex = 0;
            vertexIncrement = meshSize;
            endIndex = ((meshSize - 1) * meshSize);
            AdjustEdgeVertices(vertices, lodDelta, startIndex, endIndex, vertexIncrement);

            // 0 1 2 X 
            // 1 - -
            // 2 - -
            // Y

            m.vertices = vertices;
            return m;
        }

        private void AdjustEdgeVertices(Vector3[] vertices, int lodDelta, int startIndex, int endIndex, int vertexIncrement)
        {
            int meshSize = _meshData.MeshSize;

            // higher detail mesh has to change (lower lod level)
            // otherwise use default positions
            if(lodDelta <= 0)
            {
                // use default positions
                for(int i = startIndex; i < endIndex; i += vertexIncrement)
                {
                    vertices[i] = _meshData.Vertices[i];
                }
            }
            else
            {
                int meshSimplificationIncrement = lodDelta * 2;
                for(int i = startIndex; i < endIndex; i += vertexIncrement * meshSimplificationIncrement)
                {
                    if(i + vertexIncrement * meshSimplificationIncrement >= vertices.Length)
                    {
                        Debug.Log("ASD");
                    }
                    Vector3 a = vertices[i];
                    Vector3 b = vertices[i + vertexIncrement * meshSimplificationIncrement];
                    float alpha = 1f / ((float)meshSimplificationIncrement);

                    // all the vertices in between the two lod vertices
                    for(int j = 1; j < meshSimplificationIncrement; j++)
                    {
                        Vector3 betweenVertex = Vector3.Lerp(a, b, alpha * j);
                        vertices[i + j * vertexIncrement] = betweenVertex;
                    }
                }
            }
        }

        private void OnMeshdataReceived(MeshData meshData)
        {
            _meshData = meshData;
            Mesh = meshData.CreateMesh();
            _updateCallback?.Invoke();
        }
    }
}