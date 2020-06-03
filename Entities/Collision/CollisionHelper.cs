using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities.Collision
{
    public static class CollisionHelper
    {
        public static bool BoxIntersect(BoundingBox a, BoundingBox b)
        {
            return (a.Min.X <= b.Max.X && a.Max.X >= b.Min.X) &&
                   (a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y) &&
                   (a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z);
        }

        public static bool IsPointInside(Vector3 point, BoundingBox box)
        {
            return (point.X >= box.Min.X && point.X <= box.Max.X) &&
                   (point.Y >= box.Min.Y && point.Y <= box.Max.Y) &&
                   (point.Z >= box.Min.Z && point.Z <= box.Max.Z);
        }
    }
}
