using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Procedural.Biomes;
using Procedural.Terrain;
using UnityEngine;
using UnityToolbag;
using Random = System.Random;

namespace Procedural.Foliage
{
    public class FoliagePointGenerator
    {
        public static FoliagePointData GenerateFoliageData(Vector3 position, 
            Vector3 boundingSize, 
            AnimationCurve heightCurve, 
            TerrainHeightParameters parameters, 
            BiomeDeterminer biomeDeterminer, 
            TerrainObjectProvider terrainObjectProvider,
            FoliageGroup group,
            float foliageSpawnRadius)
        {
            FoliagePointData pointData = new FoliagePointData();

            AnimationCurve curve = new AnimationCurve(heightCurve.keys);
            Vector3 worldPosition = position;
            Vector2 worldPositionV2 = new Vector2(position.x, position.z);

            int seed = (int)(System.DateTime.Now.Ticks/2);

            ThreadStart threadStart = () => 
            {
                var size = Vector2.one * (MapGenerator.MapChunkSize - foliageSpawnRadius);
                List<Vector2> points = PoissonDiscSample.GeneratePoints(foliageSpawnRadius, size, seed, 5);
                HashSet<TerrainPoint> adjustedPoints = new HashSet<TerrainPoint>();
                var foliagePoints = new HashSet<FoliagePoint>();

                foreach(Vector2 point in points)
                {           
                    Vector2 adjustedPoint = point - (size * 0.5f);

                    Vector3 worldPoint = worldPosition + new Vector3(adjustedPoint.x, 1, adjustedPoint.y);

                    NoiseSample noiseSample = TerrainHeightSampler.SampleHeightAt(adjustedPoint + worldPositionV2, parameters, curve);
                    worldPoint.y = noiseSample.Height;

                    Vector3 normal = noiseSample.GetNormal(parameters.Resolution);

                    adjustedPoints.Add(new TerrainPoint(worldPoint, normal));
                }

                foliagePoints = GenerateTerrainPoints(seed, adjustedPoints, terrainObjectProvider, group, biomeDeterminer);
                
                Dispatcher.Invoke(() => 
                {
                    var bounds = new Bounds(position, boundingSize);
                    
                    pointData.SetData(group, foliagePoints, bounds);
                });
            };

            new Thread(threadStart).Start();

            return pointData;
        }

        private static HashSet<FoliagePoint> GenerateTerrainPoints(int seed, HashSet<TerrainPoint> points, TerrainObjectProvider terrainObjectProvider, FoliageGroup group, BiomeDeterminer biomeDeterminer)
        {
            var foliagePoints = new HashSet<FoliagePoint>();
            var rand = new Random(seed);
            
            foreach(TerrainPoint point in points)
            {
                FoliageType foliageType = terrainObjectProvider.GetRandomWeightedFoliage(group, rand);

                if(biomeDeterminer.HasTreesSafe(point.Position.y))
                {
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = Vector3.one;
                    Vector2 rotXZ = default;
                    int yAngle = 0;


                    //TODO: get foliage type transform values form somewhere else
                    
                    switch(foliageType)
                    {
                        case FoliageType.Tree:
                            yAngle = rand.Next(0, 360);
                            rotXZ = rand.InsideUnitCircle() * 5;
                            scale = Vector3.one * rand.NextFloat(1, 2);
                            rotation = Quaternion.Euler(rotXZ.x, yAngle, rotXZ.y);
                            break;
                        case FoliageType.Rock:
                            Vector3 euler = rand.InsideUnitSphere() * 360;
                            scale = 2 * Vector3.one + rand.InsideUnitSphere().normalized;
                            rotation = Quaternion.Euler(euler);
                            break;
                        case FoliageType.Bush:
                            yAngle = rand.Next(0, 360);
                            rotXZ = rand.InsideUnitCircle() * 5;
                            scale = Vector3.one * rand.NextFloat(1, 2);
                            rotation = Quaternion.Euler(rotXZ.x, yAngle, rotXZ.y);
                            break;
                        case FoliageType.Grass:
                            yAngle = rand.Next(0, 360);
                            rotation = Quaternion.LookRotation(point.Normal) * Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, yAngle, 0);
                            break;
                        default: 
                            break;
                    }

                    foliagePoints.Add(new FoliagePoint(point.Position, rotation, scale, foliageType));
                }
            }

            return foliagePoints;
        }

        private class TerrainPoint
        {
            public Vector3 Position;
            public Vector3 Normal;

            public TerrainPoint(Vector3 point, Vector3 normal)
            {
                Position = point;
                Normal = normal;
            }
        }
    }
}