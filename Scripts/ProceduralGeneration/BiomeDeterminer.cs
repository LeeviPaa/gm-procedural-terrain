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

        public BiomeDeterminer(TerrainType[] biomes, TerrainHeightParameters heightParameters)
        {
            _biomes = biomes;
            _heightParameters = heightParameters;
            _heightCurve = heightParameters.HeightCurve;
        }

        public bool AboveWaterLevel(float scaledHeight)
        {
            throw new NotImplementedException();
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
            float height = TerrainHeightSampler.SampleHeightAt(Vector2.zero, worldPosition, parameters, heightCurve).Height;
            return GetTerrainType(height);
        }

        public TerrainType GetBiomeAt(Vector2 worldPosition)
        {
            float height = TerrainHeightSampler.SampleHeightAt(Vector2.zero, worldPosition, _heightParameters, _heightCurve).Height;
            return GetTerrainType(height);
        }

        public Color[] GetDebugColorMap(int mapChunkSize, NoiseSample[,] noiseMap)
        {
            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

            for(int y = 0; y < mapChunkSize; y++)
            {
                for(int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y].Height;

                    colorMap[y * mapChunkSize + x] = GetTerrainType(currentHeight).DebugColor;
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