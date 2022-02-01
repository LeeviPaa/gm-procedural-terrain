using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Foliage
{
    public class FoliagePointData
    {
        private const int ChunkDivisions = 1;
        private static Vector3 Vec3OneXZ => new Vector3(1, 0, 1);
        public FoliageGroup FoliageGroup => _group;

        private HashSet<FoliageChunk> _chunks;
        private bool _hasData;
        private Bounds _bounds;
        private FoliageGroup _group;

        public void SetData(FoliageGroup group, HashSet<FoliagePoint> points, Bounds bounds)
        {
            _group = group;
            _bounds = bounds;
            _chunks = new HashSet<FoliageChunk>();
        
            int divisionCount = (int)Mathf.Pow(2, ChunkDivisions);
            float childSize = bounds.size.x / divisionCount;
            Vector3 childCenterOrigin = bounds.center - (Vec3OneXZ * childSize * 0.5f);

            for(int x = 0; x < divisionCount; x++)
            {
                for(int z = 0; z < divisionCount; z++)
                {
                    Vector3 childPos = childCenterOrigin + new Vector3(x, 0, z) * childSize;
                    Bounds childBounds = new Bounds(childPos, Vec3OneXZ * childSize);

                    HashSet<FoliagePoint> childPoints = new HashSet<FoliagePoint>(points.Where(t => childBounds.ContainsXZ(t.PositionV2)));

                    _chunks.Add(new FoliageChunk(childPoints, childBounds));
                }
            }

            _hasData = true;
        }

        public void AppendFoliageChunksAroundTarget(Vector2 position, float distance, ISet<FoliageChunk> chunks)
        {
            if(!_hasData || !_bounds.IsWithinRangeV2(position, distance))
                return;

            foreach(var chunk in _chunks)
            {
                if(chunk.Bounds.IsWithinRangeV2(position, distance))
                {
                    chunks.Add(chunk);
                }
            }
        }
    }
}