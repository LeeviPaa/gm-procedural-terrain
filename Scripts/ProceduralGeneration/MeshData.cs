using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Procedural.Terrain;
using UnityEngine;
using UnityToolbag;

namespace Procedural
{
    public class MeshData
    {
        public readonly int MeshSize;
        public Vector3[] Vertices;
        private Vector3[] _normals;
	    private int[] _triangles;
	    private Vector2[] _uvs;
	    private int _triangleIndex;
        private bool _useFlatShading;
        private TerrainHeightParameters _terrainHeightParameters;
        private Vector3 _worldPosition;

	    public MeshData(int meshSize, bool useFlatShading) 
        {
            MeshSize = meshSize;
	    	Vertices = new Vector3[meshSize * meshSize];
	    	_normals = new Vector3[meshSize * meshSize];
	    	_uvs = new Vector2[meshSize * meshSize];
	    	_triangles = new int[(meshSize-1)*(meshSize-1)*6];
            _useFlatShading = useFlatShading;
	    }

	    public void AddTriangle(int a, int b, int c) 
        {
	    	_triangles [_triangleIndex] = a;
	    	_triangles [_triangleIndex + 1] = b;
	    	_triangles [_triangleIndex + 2] = c;
	    	_triangleIndex += 3;
	    }

        public void AddVertex(Vector3 vertexPosition, Vector3 vertexNormal, Vector2 Uv, int vertexIndex)
        {
            Vertices[vertexIndex] = vertexPosition;
            _normals[vertexIndex] = vertexNormal;
            _uvs[vertexIndex] = Uv;
        }

	    public Mesh CreateMesh() 
        {
	    	Mesh mesh = new Mesh ();

            if(_useFlatShading)
            {
                FlatShading();

                mesh.vertices = Vertices;
	    	    mesh.triangles = _triangles;
	    	    mesh.uv = _uvs;

	    	    mesh.RecalculateNormals ();
            }
            else
            {
                mesh.vertices = Vertices;
	    	    mesh.triangles = _triangles;
	    	    mesh.uv = _uvs;
                mesh.normals = _normals;

	    	    //mesh.RecalculateNormals ();
            }

	    	

	    	return mesh;
	    }

        private void FlatShading()
        {
            Vector3[] flatShadedVertices = new Vector3[_triangles.Length];
            Vector2[] flatShadedUvs = new Vector2[_triangles.Length];

            for(int i = 0; i < _triangles.Length; i++)
            {
                flatShadedVertices[i] = Vertices[_triangles[i]];
                flatShadedUvs[i] = _uvs[_triangles[i]];
                _triangles[i] = i;
            }

            Vertices = flatShadedVertices;
            _uvs = flatShadedUvs;
        }

        private void ReSampleEdgeNormals()
        {

        }
    }
}