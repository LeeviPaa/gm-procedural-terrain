using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Procedural.Foliage;

namespace Procedural.Terrain
{
    public class TerrainChunkController : MonoBehaviour
    {
        public const float UpdateChunkAfterDistanceTreshold = 25;
        public const int ChunkMeshDelayMs = 10;
        public const float UpdateChunkAfterDistanceTresholdSquared = UpdateChunkAfterDistanceTreshold * UpdateChunkAfterDistanceTreshold;
        public static float MaxViewDist = 300;
        public static float ReleaseChunkDist => MaxViewDist + 200;

        public IEnumerable<TerrainChunk> TerrainChunks => _terrainChunkDictionary.Values;
        
        public LODInfo[] LODDistanceInfo;
        public Transform Viewer;
        public Vector2 ViewerPositionV2 => new Vector2(Viewer.position.x, Viewer.position.z);
        public MapGenerator MapGenerator;
        
        private int _chunkSize;
        private int _chunksVisibleInViewDst;
        private Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private Vector2 _oldViewerPosition;

        private void Start()
        {
            MaxViewDist = LODDistanceInfo.Last().VisibleDistTreshold;
            _chunkSize = MapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDst = Mathf.RoundToInt(MaxViewDist / _chunkSize);

            UpdateVisibleChunks();
        }

        private void Update()
        {
            if((_oldViewerPosition - ViewerPositionV2).sqrMagnitude >= UpdateChunkAfterDistanceTresholdSquared)
            {
                UpdateVisibleChunks();
                RemoveFarChunks();
                _oldViewerPosition = ViewerPositionV2;
            }
        }

        private async void UpdateVisibleChunks()
        {
            int currentChunkCoordX = Mathf.RoundToInt(ViewerPositionV2.x / _chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(ViewerPositionV2.y / _chunkSize);
            var visibleChunkLodLevels = new Dictionary<Vector2, int>();

            for(int yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
            {
                for(int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
                {
                    if(this == null || gameObject == null)
                        break;
                    
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY - yOffset);
                    Vector2 viewedChunkPosition = viewedChunkCoord * _chunkSize;
                    Bounds chunkBounds = new Bounds(viewedChunkPosition, _chunkSize * Vector2.one);
                    float distance = Mathf.Sqrt (chunkBounds.SqrDistance (ViewerPositionV2));

                    if(distance < MaxViewDist)
                    {
                        int lodLevel = GetLodLevelPerDistance(distance);
                        visibleChunkLodLevels.Add(viewedChunkCoord, lodLevel);
                    }
                }
            }

            visibleChunkLodLevels = visibleChunkLodLevels.OrderBy(kvp => Vector2.Distance(kvp.Key * _chunkSize, ViewerPositionV2)).ToDictionary(t => t.Key, t => t.Value);

            // disable chunks not in view
            foreach(var kvp in _terrainChunkDictionary)
            {
                Vector2 chunkCoord = kvp.Key;
                TerrainChunk chunk = kvp.Value;
                
                if(!visibleChunkLodLevels.ContainsKey(chunkCoord))
                {
                    chunk.SetVisible(false);
                }
            }

            foreach(var kvp in visibleChunkLodLevels)
            {
                Vector2 viewedChunkCoord = kvp.Key;
                int lodLevel = kvp.Value;
                TerrainChunk chunk;
                LodSurroundings surroundingLodMatrix = GetSurroundingChunkCoords(viewedChunkCoord, visibleChunkLodLevels);

                if(_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunk = _terrainChunkDictionary[viewedChunkCoord];

                    chunk.CurrentLodSurroundings = surroundingLodMatrix;
                    chunk.UpdateChunkLodMesh();
                }
                else
                {
                    chunk = new TerrainChunk(viewedChunkCoord, _chunkSize, LODDistanceInfo, surroundingLodMatrix, transform, MapGenerator, this);
                    _terrainChunkDictionary.Add(viewedChunkCoord, chunk);
                }

                _terrainChunkDictionary[viewedChunkCoord].SetVisible(true);

                await Task.Delay(ChunkMeshDelayMs);
            }

            _terrainChunkDictionary = _terrainChunkDictionary.Where(t => t.Value != null).ToDictionary(t => t.Key, t => t.Value);
        }

        private LodSurroundings GetSurroundingChunkCoords(Vector2 chunkCoord, Dictionary<Vector2, int> visibleChunkLods)
        {
            Vector2 leftCoord = chunkCoord + Vector2.left;
            int leftLod = visibleChunkLods.ContainsKey(leftCoord) ? visibleChunkLods[leftCoord] : -1;

            Vector2 rightCoord = chunkCoord + Vector2.right;
            int rightLod = visibleChunkLods.ContainsKey(rightCoord) ? visibleChunkLods[rightCoord] : -1;

            Vector2 upCoord = chunkCoord + Vector2.up;
            int upLod = visibleChunkLods.ContainsKey(upCoord) ? visibleChunkLods[upCoord] : -1;

            Vector2 downCoord = chunkCoord + Vector2.down;
            int downLod = visibleChunkLods.ContainsKey(downCoord) ? visibleChunkLods[downCoord] : -1;

            return new LodSurroundings()
            {
                Up = upLod,
                Down = downLod,
                Left = leftLod,
                Right = rightLod,
                Self = visibleChunkLods[chunkCoord]
            };
        }

        private int GetLodLevelPerDistance(float distanceFromViewer)
        {
            int lodIndex = 0;

            for(int i = 0; i < LODDistanceInfo.Length-1; i++)
            {
                if(distanceFromViewer > LODDistanceInfo[i].VisibleDistTreshold)
                {
                    lodIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            return LODDistanceInfo[lodIndex].LODLevel;
        }

        private void RemoveFarChunks()
        {
            foreach(TerrainChunk chunk in _terrainChunkDictionary.Select(t => t.Value))
            {
                if(Vector2.Distance(ViewerPositionV2, chunk.Position) > ReleaseChunkDist)
                {
                    chunk.ReleaseChunkFromMemory();
                }
            }

            _terrainChunkDictionary = _terrainChunkDictionary.Where(pair => !pair.Value.Destroyed)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value);
        }
    }
}