using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Biomes
{
    [CreateAssetMenu(fileName = "BiomeDefinitions", menuName = "Terrain/BiomeDefinitionProvider", order = 2)]
    public class BiomeDefinitionProvider : ScriptableObject
    {
        public IReadOnlyList<TerrainType> Biomes => _terrainTypes;

        [SerializeField]
        private List<TerrainType> _terrainTypes;
    }
}