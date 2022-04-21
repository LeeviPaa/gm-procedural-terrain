using System.Collections.Generic;
using GPUInstancer;
using Procedural.Terrain;
using UnityEngine;

namespace Procedural.Foliage
{
    /// <summary>
    /// Controls which foliage gets set to around the player according to pre-generated points
    /// </summary>
    public class FoliageSpawnController : MonoBehaviour
    {
        public Vector2 ViewerPositionV2 => new Vector2(_player.position.x, _player.position.z);
        public const float UpdateChunkAfterDistanceTreshold = 25;
        public const float UpdateChunkAfterTime = 1;
        public const float UpdateChunkAfterDistanceTresholdSquared = UpdateChunkAfterDistanceTreshold * UpdateChunkAfterDistanceTreshold;

        [SerializeField]
        private TerrainChunkController _terrainChunkController;
        [SerializeField]
        private TerrainObjectProvider _terrainObjectProvider;
        [SerializeField]
        private Transform _player;
        [SerializeField]
        private GPUInstancerPrefabManager _instancerManager;

        private GpuIstancerPool _treePool;
        private GpuIstancerPool _bushPool;
        private GpuIstancerPool _rockPool;
        private GpuIstancerPool _grassPool;

        private HashSet<FoliageChunk> _distantVisibleChunks = new HashSet<FoliageChunk>();
        private HashSet<FoliageChunk> _distantPreviouslyVisibleChunks = new HashSet<FoliageChunk>();
        private HashSet<FoliageChunk> _nearVisibleChunks = new HashSet<FoliageChunk>();
        private HashSet<FoliageChunk> _nearPreviouslyVisibleChunks = new HashSet<FoliageChunk>();
        private Vector2 _oldViewerPosition;
        private float _nextUpdateTime = 0;

        public void Awake()
        {
            // TODO clean foliage pool generation

            // distant foliage
            GameObject distantParent = new GameObject("Distant objects");
            distantParent.transform.parent = transform;
            
            var treeDefinition = _terrainObjectProvider.GetFoliageDefinition(FoliageGroup.Distant, FoliageType.Tree);
            GameObject treePrefab = treeDefinition.FoliageObjects.GetRandomEntry();
            GameObject treeParent = new GameObject("Trees");
            treeParent.transform.parent = distantParent.transform;
            _treePool = new GpuIstancerPool(treePrefab.GetComponent<GPUInstancerPrefab>(), 1, treeParent.transform, _instancerManager);

            var rockDefinition = _terrainObjectProvider.GetFoliageDefinition(FoliageGroup.Distant, FoliageType.Rock);
            GameObject rockPrefab = rockDefinition.FoliageObjects.GetRandomEntry();
            GameObject rockParent = new GameObject("Rocks");
            rockParent.transform.parent = distantParent.transform;
            _rockPool = new GpuIstancerPool(rockPrefab.GetComponent<GPUInstancerPrefab>(), 1, rockParent.transform, _instancerManager);

            var bushDefinition = _terrainObjectProvider.GetFoliageDefinition(FoliageGroup.Distant, FoliageType.Bush);
            GameObject bushPrefab = bushDefinition.FoliageObjects.GetRandomEntry();
            GameObject bushParent = new GameObject("Bushes");
            bushParent.transform.parent = distantParent.transform;
            _bushPool = new GpuIstancerPool(bushPrefab.GetComponent<GPUInstancerPrefab>(), 1, bushParent.transform, _instancerManager);

            // near foliage
            GameObject nearParent = new GameObject("NearObjects");
            nearParent.transform.parent = transform;
            
            var grassDefinition = _terrainObjectProvider.GetFoliageDefinition(FoliageGroup.Near, FoliageType.Grass);
            GameObject grassPrefab = grassDefinition.FoliageObjects.GetRandomEntry();
            GameObject grassParent = new GameObject("Grass");
            grassParent.transform.parent = nearParent.transform;
            _grassPool = new GpuIstancerPool(grassPrefab.GetComponent<GPUInstancerPrefab>(), 1, grassParent.transform, _instancerManager);

            UpdateFoliageChunks(FoliageGroup.Distant, _terrainObjectProvider.DistantFoliageDistance, _distantVisibleChunks, _distantPreviouslyVisibleChunks);
            UpdateFoliageChunks(FoliageGroup.Near, _terrainObjectProvider.NearFoliageDistance, _nearVisibleChunks, _nearPreviouslyVisibleChunks);
            _nextUpdateTime = Time.time + UpdateChunkAfterTime;
        }

        public void Update()
        {
            if((_oldViewerPosition - ViewerPositionV2).sqrMagnitude >= UpdateChunkAfterDistanceTresholdSquared
                || Time.time > _nextUpdateTime)
            {
                UpdateFoliageChunks(FoliageGroup.Distant, _terrainObjectProvider.DistantFoliageDistance, _distantVisibleChunks, _distantPreviouslyVisibleChunks);
                UpdateFoliageChunks(FoliageGroup.Near, _terrainObjectProvider.NearFoliageDistance, _nearVisibleChunks, _nearPreviouslyVisibleChunks);
                _oldViewerPosition = ViewerPositionV2;
                _nextUpdateTime = Time.time + UpdateChunkAfterTime;
            }
        }

        private void UpdateFoliageChunks(FoliageGroup group, float foliageDistance, ISet<FoliageChunk> visibleChunks, ISet<FoliageChunk> previouslyVisibleChunks)
        {
            // TODO: only check points from a band with radius of delta movement
            // TODO: update only when player has moved x distance

            Vector2 playerPosV2 = new Vector2(_player.position.x, _player.position.z);
            
            foreach(var terrainChunk in _terrainChunkController.TerrainChunks)
            {
                FoliagePointData foliageData = terrainChunk.GetPointData(group);

                if(foliageData != null)
                {
                    foliageData.AppendFoliageChunksAroundTarget(playerPosV2, foliageDistance, visibleChunks);
                }
            }

            // points that are visible but weren't
            foreach(var foliageChunk in visibleChunks)
            {
                if(!previouslyVisibleChunks.Contains(foliageChunk))
                    DisplayFoliage(foliageChunk);
            }

            // points that were visible but aren't
            foreach(var foliageChunk in previouslyVisibleChunks)
            {
                if(!visibleChunks.Contains(foliageChunk))
                    HideFoliage(foliageChunk);
            }

            previouslyVisibleChunks.Clear();
            foreach(var point in visibleChunks)
            {
                previouslyVisibleChunks.Add(point);
            }
            visibleChunks.Clear();
        }

        private void DisplayFoliage(FoliageChunk chunk)
        {
            foreach(var point in chunk.Points)
            {
                GPUInstancerPrefab foliageTransform = GetFoliageForPoint(point);
                chunk.DisplayFoliage(point, foliageTransform);
            }
        }

        private void HideFoliage(FoliageChunk chunk)
        {
            foreach(var point in chunk.Points)
            {
                HideFoliage(point, chunk.GetAndHideFoliage(point));
            }
        }

        private GPUInstancerPrefab GetFoliageForPoint(FoliagePoint foliagePoint)
        {
            GPUInstancerPrefab foliageObject = null;
            switch(foliagePoint.FoliageType)
            {
                case FoliageType.Tree:
                    foliageObject = _treePool.GetItem();
                    break;
                case FoliageType.Rock:
                    foliageObject = _rockPool.GetItem();
                    break;
                case FoliageType.Bush:
                    foliageObject = _bushPool.GetItem();
                    break;
                case FoliageType.Grass:
                    foliageObject = _grassPool.GetItem();
                    break;
                default:
                    break;
            }
        
            foliagePoint.SetTransfromValues(foliageObject.transform);
            _instancerManager.UpdateTransformDataForInstance(foliageObject);
            return foliageObject;
        }

        private void HideFoliage(FoliagePoint foliagePoint, GPUInstancerPrefab foliageObject)
        {
            switch(foliagePoint.FoliageType)
            {
                case FoliageType.Tree:
                    _treePool.ReturnItem(foliageObject);
                    break;
                case FoliageType.Rock:
                    _rockPool.ReturnItem(foliageObject);
                    break;
                case FoliageType.Bush:
                    _bushPool.ReturnItem(foliageObject);
                    break;
                case FoliageType.Grass:
                    _grassPool.ReturnItem(foliageObject);
                    break;
                default:
                    break;
            }
        }

    }
}