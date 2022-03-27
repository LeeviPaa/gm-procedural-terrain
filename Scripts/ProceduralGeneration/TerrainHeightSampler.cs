using System;
using UnityEngine;

namespace Procedural.Terrain
{
    public static class TerrainHeightSampler
    {
        public static NoiseSample SampleHeightAt(Vector2 coordinates, TerrainHeightParameters parameters, AnimationCurve heightCurve)
        {
            float amplitude = 1;
            float frequency = 1;
            Vector2 point = coordinates + parameters.Offset;

            NoiseSample sampleSum = Noise.PerlinSample(point, parameters.Resolution, frequency);
            NoiseSample macroHeight = Noise.PerlinSample(point, parameters.Resolution * parameters.MacroHeightScale, frequency) * parameters.MacroHeightAmplitude;
            sampleSum += macroHeight;

            for(int octave = 1; octave < parameters.Octaves; octave++)
            {
                amplitude *= parameters.Peristance;
                frequency *= parameters.Lacunarity;

                point += parameters.OctaveOffsets[octave];

                sampleSum += Noise.PerlinSample(point, parameters.Resolution, frequency) * amplitude;
            }

            Vector3 derivative = sampleSum.Derivative;
            sampleSum.Derivative = new Vector3(derivative.x, derivative.y, derivative.z);

            // map height between -1 and 1
            NoiseSample height = ((sampleSum) * (1 / parameters.MaxNoiseHeight));

            
            // the curve multiplier throws the derivate normal scale off a bit
			// not a big problem but with the curve multiplier, the normals are constantly a bit off in the Y axis
			// without this the normals sway a bit from the calculated ones but not consistenly to any direction
			// problem could be the curve turning -1 values to 0 ???

			// remapping the curve multiplier value may have solved the problem (value + 1) / 2
            float curveMultiplier = Mathf.Abs(heightCurve.Evaluate (height.Height));

            return height * curveMultiplier * parameters.HeightMultiplier;
        }

        public static NoiseSample[,] GenerateHeightMap(int mapEdgeSize, Vector2 center, TerrainHeightParameters parameters, AnimationCurve heightCurve, float resolution = 1)
        {
            int noiseMapEdgeSize = Mathf.CeilToInt(mapEdgeSize * resolution);
            NoiseSample[,] noiseMap = new NoiseSample[noiseMapEdgeSize, noiseMapEdgeSize];
            AnimationCurve heightCurveCopy = new AnimationCurve (heightCurve.keys);


            float halfWidth = mapEdgeSize / 2;
            float halfHeight = mapEdgeSize / 2;         

            for(int y = 0; y < noiseMapEdgeSize; y++)
            {
                for(int x = 0; x < noiseMapEdgeSize; x++)
                {
                    float xScaled = ((float)x) / resolution;
                    float yScaled = ((float)y) / resolution;
                    noiseMap[x, y] = TerrainHeightSampler.SampleHeightAt(new Vector2(xScaled - halfWidth, yScaled - halfHeight) + center, parameters, heightCurveCopy);
                }
            }
            
            return noiseMap;
        }
    }
}