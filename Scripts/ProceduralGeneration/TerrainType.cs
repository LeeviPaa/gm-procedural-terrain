using UnityEngine;

namespace Procedural.Terrain
{
    [System.Serializable]
    public struct TerrainType
    {
        public string Label => _label;
        public float Height => _height;
        public Color DebugColor => _color;
        public bool Trees => _trees;

        [SerializeField]
        private string _label;
        [SerializeField]
        [Range(-1, 1)]
        private float _height;
        [SerializeField]
        private Color _color;
        [SerializeField]
        private bool _trees;
    }
}