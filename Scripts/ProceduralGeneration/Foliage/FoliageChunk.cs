using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer;

namespace Procedural.Foliage
{
    public class FoliageChunk
    {
        public IEnumerable<FoliagePoint> Points => _foliagePoints;
        public readonly Bounds Bounds;

        private readonly IDictionary<FoliagePoint, GPUInstancerPrefab> _foliageObjectDictionary;
        private readonly HashSet<FoliagePoint> _foliagePoints;

        public FoliageChunk(HashSet<FoliagePoint> points, Bounds bounds)
        {
            _foliagePoints = points;
            _foliageObjectDictionary = new Dictionary<FoliagePoint, GPUInstancerPrefab>();
            Bounds = bounds;

            foreach(var point in points)
            {
                _foliageObjectDictionary.Add(point, null);
            }
        }

        public void DisplayFoliage(FoliagePoint point, GPUInstancerPrefab foliageObject)
        {
            _foliageObjectDictionary[point] = foliageObject;
        }

        public GPUInstancerPrefab GetAndHideFoliage(FoliagePoint point)
        {
            GPUInstancerPrefab foliageObject = _foliageObjectDictionary[point];
            _foliageObjectDictionary[point] = null;

            return foliageObject;
        }
    }
}