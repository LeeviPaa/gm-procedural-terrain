using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityToolbag;
using Procedural.Biomes;
using UnityEngine.Serialization;

namespace Procedural.Terrain
{
    public enum DrawMode { Single, Color, Mesh }
    
    public class MapGenerator : MonoBehaviour
    {
        public TerrainHeightParameters TerrainParameters => _terrainParameters;
        public BiomeDeterminer BiomeDeterminer => _biomeDeterminer;
        public TerrainObjectProvider TerrainObjectProvider => _terrainObjectProvider;

        public const int MapChunkSize = 97; // 240 & 120 & 96 is nicely divisible by 2, 4, 6, 8, 12
        [FormerlySerializedAs("NoiseScale")]
        public float NoiseResolution;
        public float MacroHeightScale = 5;
        public float MacroHeightAmplitude = 1;
        public int Octaves;
        [Range(0, 1)]
        public float Persistance;
        public float Lacunarity;
        public int Seed;
        public Vector2 Offset;
        [FormerlySerializedAs("MaxHeight")]
        public float HeightScale = 100;

        [Header("Regions")]
        public AnimationCurve HeightCurve = new AnimationCurve();
        public BiomeDefinitionProvider BiomeProvider;
        [Header("References")]
        [SerializeField]
        public MapDisplay _mapDisplay;
        [SerializeField]
        private TerrainObjectProvider _terrainObjectProvider;
        public bool AutoUpdate = false;
        public Material ChunkMaterial;
        [Range(0, 4)]
        public int EditorPreviewLODValue;
        private TerrainHeightParameters _terrainParameters;
        private BiomeDeterminer _biomeDeterminer;
        private bool _initialized = false;

        public void Start()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if(_initialized) return;
            
            _terrainParameters = new TerrainHeightParameters(NoiseResolution, Offset, MacroHeightScale, MacroHeightAmplitude, Octaves, Persistance, Lacunarity, Seed, HeightScale, HeightCurve);
            _biomeDeterminer = new BiomeDeterminer(BiomeProvider.Biomes, _terrainParameters);
            _initialized = true;
        }

        public void RequestMeshData(MapData mapData, int lodLevel, Action<MeshData> callback)
        {
            ThreadStart threadStart = () => {

                MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, HeightScale, NoiseResolution, lodLevel, false);

                // run this on the main thread
                Dispatcher.Invoke(() => 
                {
                    callback(meshData);
                });
            };

            new Thread(threadStart).Start();
        }

        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            ThreadStart threadStart = () => {
                MapData mapData = GenerateMapData(center);

                // run this on the main thread
                Dispatcher.Invoke(() => 
                {
                    callback(mapData);
                });
            };

            new Thread(threadStart).Start();
        }

        public MapData GenerateMapData(Vector2 center, int scale = 1, float resolution = 1)
        {
            if(Seed == 0)
                Seed = System.DateTime.UtcNow.Ticks.GetHashCode();

            EnsureInitialized();
            
            NoiseSample[,] noiseMap = TerrainHeightSampler.GenerateHeightMap(MapChunkSize * scale, center, TerrainParameters, HeightCurve, resolution);
            var colorMap = BiomeDeterminer.GetDebugColorMap(noiseMap);

            return new MapData(noiseMap, colorMap);
        }

        public void DrawMapInEditor()
        {
            _initialized = false;
            EnsureInitialized();
            
            MapData mapData = GenerateMapData(Vector2.zero, 1);
            MapData heightMap = GenerateMapData(Vector2.zero, 5);

            float height = TerrainParameters.MaxNoiseHeight * TerrainParameters.HeightMultiplier;
            int mapSize = heightMap.HeightMap.GetLength(0);
            Debug.Log(height);

            //_mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, HeightScale, NoiseResolution, EditorPreviewLODValue), TextureGenerator.TextureFromColorMap(mapData.ColorMap, MapChunkSize, MapChunkSize));
            _mapDisplay.DrawMap(MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, HeightScale, NoiseResolution, EditorPreviewLODValue), 
                TextureGenerator.TextureFromHeightMap(heightMap.HeightMap, height),
                TextureGenerator.TextureFromColorMap(heightMap.ColorMap, mapSize, mapSize));
        }

        private void OnValidate()
        {
            if(Lacunarity < 1)
                Lacunarity = 1;
            if(Octaves < 0)
                Octaves = 0;
        }
    }
}