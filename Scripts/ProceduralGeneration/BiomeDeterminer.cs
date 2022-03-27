using System;
using System.Collections.Generic;
using System.Linq;
using Procedural.Terrain;
using UnityEngine;

namespace Procedural.Biomes
{
    public class BiomeDeterminer
    {
        private readonly IReadOnlyCollection<TerrainType> _biomes;
        private readonly TerrainHeightParameters _heightParameters;
        private readonly AnimationCurve _heightCurve;
        private float _waterLevel;

        public BiomeDeterminer(TerrainType[] biomes, TerrainHeightParameters heightParameters)
        {
            _biomes = biomes;
            _heightParameters = heightParameters;
            _heightCurve = heightParameters.HeightCurve;
            _waterLevel = GetScaledWaterLevel();
        }

        public bool AboveWaterLevel(float scaledHeight)
        {
            return scaledHeight > _waterLevel;
        }

        public TerrainType GetTerrainType(float scaledHeight)
        {
            float normalizedHeight = NormalizeHeight(scaledHeight);

            foreach(TerrainType biome in _biomes)
            {
                if(normalizedHeight <= biome.Height)
                {
                    return biome;
                }
            }

            return default;
        }

        public float NormalizeHeight(float scaledHeight) => (scaledHeight / _heightParameters.HeightMultiplier);

        // thread safe
        public bool HasTreesSafe(float scaledHeight)
        {
            return GetTerrainType(scaledHeight).Trees;
        }

        public bool HasTrees(Vector2 worldPosition)
        {
            return GetBiomeAt(worldPosition).Trees;
        }

        public TerrainType GetBiomeAtSafe(Vector2 worldPosition, TerrainHeightParameters parameters, AnimationCurve heightCurve)
        {
            float height = TerrainHeightSampler.SampleHeightAt(worldPosition, parameters, heightCurve).Height;
            return GetTerrainType(height);
        }

        public TerrainType GetBiomeAt(Vector2 worldPosition)
        {
            float height = TerrainHeightSampler.SampleHeightAt(worldPosition, _heightParameters, _heightCurve).Height;
            return GetTerrainType(height);
        }

        public Color[] GetDebugColorMap(NoiseSample[,] noiseMap)
        {
            int mapSize = noiseMap.GetLength(0);
            Color[] colorMap = new Color[mapSize * mapSize];

            for(int y = 0; y < mapSize; y++)
            {
                for(int x = 0; x < mapSize; x++)
                {
                    float currentHeight = noiseMap[x, y].Height;

                    colorMap[y * mapSize + x] = GetTerrainType(currentHeight).DebugColor;
                }
            }
            return colorMap;
        }

        internal float GetScaledWaterLevel()
        {
            return _biomes.First(t => t.Label == "water").Height * _heightParameters.HeightMultiplier;
        }
    }
}