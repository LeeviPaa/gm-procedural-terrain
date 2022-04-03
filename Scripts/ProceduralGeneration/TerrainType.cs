using UnityEngine;

namespace Procedural.Biomes
{
    [System.Serializable]
    public struct TerrainType
    {
        public string Label => _label;
        public float Height => _height;
        public Color DebugColor => _color;
        public bool Trees => _trees;
        public AudioClip AmbientTrack => _ambientTrack;

        [SerializeField]
        private string _label;
        [SerializeField]
        [Range(-1, 1)]
        private float _height;
        [SerializeField]
        private Color _color;
        [SerializeField]
        private bool _trees;
        [SerializeField]
        private AudioClip _ambientTrack;

        public override bool Equals(object obj)
        {
            return obj is TerrainType terrainType && Equals(terrainType);
        }

        public bool Equals(TerrainType terrainType)
        {
            return _label == terrainType.Label;
        }

        public static bool operator ==(TerrainType A, TerrainType B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(TerrainType A, TerrainType B)
        {
            return !A.Equals(B);
        }
    }
}