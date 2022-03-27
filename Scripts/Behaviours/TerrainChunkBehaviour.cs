using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Procedural.Biomes;
using Procedural.Foliage;
using UnityEngine;
using UnityToolbag;

namespace Procedural.Terrain
{
    public class TerrainChunkBehaviour : MonoBehaviour
    {
        public TerrainChunk TerrainChunk 
        { 
            get
            {
                return _terrainChunk;
            }
        }

        [SerializeField]
        private TerrainChunk _terrainChunk;
        [SerializeField]
        private LodSurroundings _lodSurroundings;
        private MapGenerator _mapGenerator;
        private BiomeDeterminer _biomeDeterminer;
        private bool _treesSpawned = false;
        private HashSet<GameObject> _trees = new HashSet<GameObject>();

        private void Update()
        {
            _lodSurroundings = _terrainChunk.CurrentLodSurroundings;
        }

        public void Initialize(MapGenerator mapGenerator, TerrainChunk terrainChunk)
        {
            _treesSpawned = false;
            _terrainChunk = terrainChunk;
            _mapGenerator = mapGenerator;
            _biomeDeterminer = _mapGenerator.BiomeDeterminer;

            GenerateWater();
        }

        private void GenerateWater()
        {
            float waterHeight = _biomeDeterminer.GetScaledWaterLevel();
            GameObject water = Instantiate(_mapGenerator.TerrainObjectProvider.WaterPrefab, transform);
            water.transform.localPosition = Vector3.up * waterHeight;
            water.transform.localScale = Vector3.one * (MapGenerator.MapChunkSize - 1) * 0.1f;
        }

        public async void UpdateLodLevel(int lodLevel)
        {

        }
    }
}