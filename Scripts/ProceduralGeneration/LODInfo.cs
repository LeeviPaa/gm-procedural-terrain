using UnityEngine;

namespace Procedural.Terrain
{
    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, 5)]
        public int LODLevel;
        public float VisibleDistTreshold;
    }
}