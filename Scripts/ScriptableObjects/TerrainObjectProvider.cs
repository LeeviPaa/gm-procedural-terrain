using System.Collections.Generic;
using System.Linq;
using Procedural.Foliage;
using UnityEngine;
using UnityEngine.Serialization;

namespace Procedural.Terrain
{
    [CreateAssetMenu(fileName = "TerrainObjectProvider", menuName = "Terrain/ObjectProvider", order = 1)]
    public class TerrainObjectProvider : ScriptableObject
    {
        public GameObject WaterPrefab => _waterPrefab;
        public float DistantFoliageDistance => _farFoliageDistance;
        public float NearFoliageDistance => _nearFoliageDistance;

        [SerializeField]
        private GameObject _waterPrefab;
        [SerializeField]
        [FormerlySerializedAs("_foliageDistance")]
        private float _farFoliageDistance;
        [SerializeField]
        private float _nearFoliageDistance;

        [SerializeField]
        [FormerlySerializedAs("_foliageDefinitions")]
        private WeightedFoliageType[] _farFoliageDefinitions;
        [SerializeField]
        private WeightedFoliageType[] _nearFoliageDefinitions;

        public WeightedFoliageType GetFoliageDefinition(FoliageGroup group, FoliageType type)
        {
            return DefinitionsPerGroup(group).First(t => t.Type == type);
        }

        // thread safe
        public FoliageType GetRandomWeightedFoliage(FoliageGroup group, System.Random rand)
        {
            WeightedFoliageType[] foliageDefinitions = DefinitionsPerGroup(group);

            float[] weights = new float[foliageDefinitions.Length];

            for(int i = 0; i < weights.Length; i++)
            {
                weights[i] = foliageDefinitions[i].Weight;
            }

            int definitionIndex = rand.Sample(weights);
            return foliageDefinitions[definitionIndex].Type;
        }

        private WeightedFoliageType[] DefinitionsPerGroup(FoliageGroup group)
        {
            switch(group)
            {
                case FoliageGroup.Distant:
                    return _farFoliageDefinitions;
                case FoliageGroup.Near:
                    return _nearFoliageDefinitions;
                default:
                    return null;
            }
        }
    }

    [System.Serializable]
    public class WeightedFoliageType
    {
        public FoliageType Type => _type;
        public float Weight => _weight;
        public IReadOnlyList<GameObject> FoliageObjects => _foliageObjects;

        [SerializeField]
        private FoliageType _type;
        [SerializeField]
        private float _weight;
        [SerializeField]
        private GameObject[] _foliageObjects;
    }
}