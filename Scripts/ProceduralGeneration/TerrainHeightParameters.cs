using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Terrain
{
    public struct TerrainHeightParameters
    {
        public readonly float Resolution;
        public readonly Vector2 Offset;
        public readonly float MacroHeightScale;
        public readonly float MacroHeightAmplitude;
        public readonly int Octaves;
        public readonly float Peristance;
        public readonly float Lacunarity;
        public readonly float MaxPossibleHeight;
        public readonly float Amplitude;
        public readonly float Frequency;
        public readonly float HeightMultiplier;
        public readonly IReadOnlyList<Vector2> OctaveOffsets => _octaveOffsets;

        private Vector2[] _octaveOffsets;
        private int _seed;
        private System.Random _prng;

        public TerrainHeightParameters(float resolution, Vector2 offset, float macroHeightScale, float macroHeightAmplitude, int octaves, float peristance, float lacunarity, int seed, float heightMultiplier)
        {
            Offset = offset;
            HeightMultiplier = heightMultiplier;
            MacroHeightScale = macroHeightScale;
            MacroHeightAmplitude = macroHeightAmplitude;
            Resolution = resolution;
            Octaves = octaves;
            Peristance = peristance;
            Lacunarity = lacunarity;
            _seed = seed;
            Amplitude = 1;
            Frequency = 1;
            MaxPossibleHeight = 1 + macroHeightAmplitude;

            _prng = new System.Random(_seed);
            _octaveOffsets = new Vector2[octaves];

            float localAmplitude = 1;
            for(int i = 1; i < octaves; i++)
            {
                float offsetX = _prng.Next(-100000, 100000);
                float offsetY = _prng.Next(-100000, 100000);
                _octaveOffsets[i] = new Vector2(offsetX, offsetY);

                localAmplitude *= peristance;
                MaxPossibleHeight += localAmplitude;
            }

            if(resolution <= 0)
                resolution = 0.0001f;
        }
    }
}