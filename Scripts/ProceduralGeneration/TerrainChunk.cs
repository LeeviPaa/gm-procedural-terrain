using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Procedural.Foliage;
using UnityEngine;

namespace Procedural.Terrain
{
    [System.Serializable]
    public class TerrainChunk
    {
        public TerrainChunkBehaviour ChunkGameObject { get; private set; }

        public readonly Bounds Bounds;
        public readonly Vector2 Position;

        public LodSurroundings CurrentLodSurroundings;
        public bool Destroyed = false;

        private GameObject _meshObject;
        private MapGenerator _mapGenerator;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;
        private LODInfo[] _detailLevels;
        private Dictionary<int, LODMesh> _lodMeshes;
        private MapData _mapData;
        private bool _mapDataReceived;
        private Texture2D _texture;
        private TerrainChunkController _endlessTerrain;
        private FoliagePointData _distantFoliageData;
        private FoliagePointData _nearFoliageData;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, LodSurroundings chunkLodMatrix, Transform parent, MapGenerator mapGenerator, TerrainChunkController endlessTerrain)
        {
            _endlessTerrain = endlessTerrain;
            CurrentLodSurroundings = chunkLodMatrix;
            Position = coord * size;
            Vector3 PositionV3 = new Vector3(Position.x, 0, Position.y);
            Bounds = new Bounds(PositionV3, new Vector3(1, 0, 1)* size);
            _mapGenerator = mapGenerator;

            // TODO make this a prefab
            _meshObject = new GameObject("Terrain Chunk");
            ChunkGameObject = _meshObject.AddComponent<TerrainChunkBehaviour>();
            ChunkGameObject.Initialize(mapGenerator, this);
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshCollider = _meshObject.AddComponent<MeshCollider>();
            _meshRenderer.material = mapGenerator.ChunkMaterial;

            _meshObject.transform.position = PositionV3;
            _meshObject.transform.parent = parent;

            SetVisible(false);

            _detailLevels = detailLevels;
            _lodMeshes = new Dictionary<int, LODMesh>();

            for(int i = 0; i < _detailLevels.Length; i++)
            {
                var detail = _detailLevels[i];
                _lodMeshes.Add(detail.LODLevel, new LODMesh(detail.LODLevel, () => UpdateChunkLodMesh()));
            }
            
            //TODO: It bugs me that the following threaded tasks don't work similarly
            mapGenerator.RequestMapData(Position, OnMapDataReceived);

            //TODO: Get spawn radius from somewhere
            _distantFoliageData = FoliagePointGenerator.GenerateFoliageData(PositionV3, Bounds.size, 
                _mapGenerator.HeightCurve, 
                _mapGenerator.TerrainParameters, 
                _mapGenerator.BiomeDeterminer,
                _mapGenerator.TerrainObjectProvider,
                FoliageGroup.Distant, foliageSpawnRadius: 5);

            _nearFoliageData = FoliagePointGenerator.GenerateFoliageData(PositionV3, Bounds.size, 
                _mapGenerator.HeightCurve, 
                _mapGenerator.TerrainParameters, 
                _mapGenerator.BiomeDeterminer,
                _mapGenerator.TerrainObjectProvider,
                FoliageGroup.Near, foliageSpawnRadius: 1f);
        }

        public FoliagePointData GetPointData(FoliageGroup group)
        {
            switch(group)
            {
                case FoliageGroup.Distant:
                    return _distantFoliageData;
                case FoliageGroup.Near:
                    return _nearFoliageData;
                default:
                    return null;
            }

        }

        public void ReleaseChunkFromMemory()
        {
            GameObject.Destroy(_meshObject);
            foreach(LODMesh lodMesh in _lodMeshes.Values)
            {
                if(lodMesh.HasMesh)
                {
                    GameObject.Destroy(lodMesh.Mesh);
                    GameObject.Destroy(_meshRenderer.material);
                }
            }
            GameObject.Destroy(ChunkGameObject.gameObject);
            GameObject.Destroy(_texture);
            Destroyed = true;
            Debug.Log("RELEASE CHUNK");
        }
        
        public void UpdateChunkLodMesh()
        {
            if(!_mapDataReceived)
                return;

            int lodLevel = CurrentLodSurroundings.Self;
            ChunkGameObject?.UpdateLodLevel(lodLevel);

            LODMesh lodMesh = _lodMeshes[lodLevel];
            if(lodMesh.HasMesh)
            {
                _meshFilter.mesh = lodMesh.GetLodAdjustedMesh(CurrentLodSurroundings);

                //VisualizeMeshNormals(lodMesh.Mesh);

                // only use the most detailed mesh for collision
                _meshCollider.sharedMesh = _lodMeshes[0].Mesh;
            }
            else if (!lodMesh.HasRequestedMesh)
            {
                lodMesh.RequestMesh(_mapData, _mapGenerator);
            }
        }

        public void SetVisible(bool visible)
        {
            _meshObject?.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }

        private void OnMapDataReceived(MapData mapData)
        {
            if(_meshRenderer == null) 
                return;
            
            _mapData = mapData;
            _mapDataReceived = true;

            UpdateChunkLodMesh();
        }

        private void VisualizeMeshNormals(Mesh mesh)
        {
            var deltaPos = new Vector3(Position.x, 0, Position.y);
            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                var vertex = mesh.vertices[i];
                var normal = mesh.normals[i];
                Debug.DrawRay(deltaPos + vertex, normal, Color.red, 10);
            }

            // sets the default normas as comparison
            mesh.RecalculateNormals();

            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                var vertex = mesh.vertices[i];
                var normal = mesh.normals[i];
                Debug.DrawRay(deltaPos + vertex, normal, Color.green, 30);
            }

            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                var vertex = mesh.vertices[i];
                var normal = mesh.normals[i];
                Debug.DrawRay(deltaPos + vertex, Vector3.up, Color.yellow, 30);
            }
        }
    }
}