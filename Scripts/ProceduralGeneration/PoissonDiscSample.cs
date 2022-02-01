using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Procedural
{
    public class PoissonDiscSample
    {
        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int seed = 123, int samplesBeforeRejection = 30)
        {
            var rand = new Random(seed);
            float cellSize = radius / Mathf.Sqrt(2);

            int sizeX = Mathf.CeilToInt(sampleRegionSize.x / cellSize);
            int sizeY = Mathf.CeilToInt(sampleRegionSize.y / cellSize);

            int[,] grid = new int[sizeX, sizeY];

            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            spawnPoints.Add(sampleRegionSize / 2);

            while(spawnPoints.Count > 0)
            {
                int spawnIndex = rand.Next(0, spawnPoints.Count);
                Vector2 spawnCentre = spawnPoints[spawnIndex];
                bool candidateAccepted = false;

                for(int i = 0; i < samplesBeforeRejection; i++)
                {
                    float angle = rand.NextFloat(0, 1) * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCentre + dir * rand.NextFloat(radius, 2f * radius);

                    if(IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }

                if(!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }

            return points;
        }

        private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
        {
            return IsInSampleRegion(candidate, sampleRegionSize) && DoesNotOverlapExistingPoint(candidate, cellSize, grid, points, radius);
        }

        private static bool IsInSampleRegion(Vector2 candidate, Vector2 sampleRegionSize)
        {
            return candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y;
        }

        private static bool DoesNotOverlapExistingPoint(Vector2 candidate, float cellSize, int[,] grid, List<Vector2> points, float radius)
        {
            int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x,y]-1;
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
						if (sqrDst < radius*radius) {
							return false;
						}
					}
				}
			}
			return true;
        }

    }
}