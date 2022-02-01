using UnityEngine;

namespace Procedural.Terrain
{
    public struct MapData
    {
        public readonly NoiseSample[,] HeightMap;
        public readonly Color[] ColorMap;

        public MapData(NoiseSample[,] heightMap, Color[] colorMap)
        {
            HeightMap = heightMap;
            ColorMap = colorMap;
        }
    }
}