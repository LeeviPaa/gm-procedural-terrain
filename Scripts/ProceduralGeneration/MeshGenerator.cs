using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public static class MeshGenerator
    {

        public static MeshData GenerateTerrainMesh(NoiseSample[,] heightMap, float heightMultiplier, float resolution, int lodLevel, bool useFlatShading = false)
        {
		    int meshSize = heightMap.GetLength (0);
		    float topLeftX = (meshSize - 1) / -2f;
		    float topLeftZ = (meshSize - 1) / 2f;

		    int meshSimplificationIncrement = (int)System.Math.Pow(2, lodLevel);
		    int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

		    MeshData meshData = new MeshData (verticesPerLine, useFlatShading);
		    int vertexIndex = 0;

		    for (int y = 0; y < meshSize; y += meshSimplificationIncrement) 
            {
		    	for (int x = 0; x < meshSize; x += meshSimplificationIncrement) 
                {
					NoiseSample sample = heightMap[x, y];

					float normalHeightScale = resolution;
                    float height = sample.Height;
		    		Vector3 vertexPosition = new Vector3 (topLeftX + x, height, topLeftZ + y);
		    		Vector2 percent = new Vector2 (x / (float)meshSize, y / (float)meshSize);

					Vector3 normalFromDerivative = sample.GetNormal(normalHeightScale);

                    meshData.AddVertex(vertexPosition, normalFromDerivative, percent, vertexIndex);

		    		if (x < meshSize - 1 && y < meshSize - 1) {
		    			meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1);
		    			meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex + 1, vertexIndex);
		    		}

		    		vertexIndex++;
		    	}
		    }

		    return meshData;
        }
    }
}