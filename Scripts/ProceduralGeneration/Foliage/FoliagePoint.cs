using UnityEngine;

namespace Procedural.Foliage
{
    public class FoliagePoint : IPositionV2
    {
        public Vector2 PositionV2 => new Vector2(Position.x, Position.z);

        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        public readonly FoliageType FoliageType;

        public FoliagePoint(Vector3 position, Quaternion rotation, Vector3 scale, FoliageType foliageType)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            FoliageType = foliageType;
        }

        public void SetTransfromValues(Transform transform)
        {
            transform.position = Position;
            transform.rotation = Rotation;
            transform.localScale = Scale;
        }
    }
}