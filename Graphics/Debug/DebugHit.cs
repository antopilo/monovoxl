using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Graphics.Debug
{
    public struct DebugHit
    {
        public Vector3 Point;
        public Color Color;

        public DebugHit(Vector3 point, Color color)
        {
            this.Point = point;
            this.Color = color;
        }
    }
}
