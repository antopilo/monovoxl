using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Graphics.Debug
{
    public struct DebugBox
    {
        public Vector3 Point;
        public float Size;
        public Color Color;


        public DebugBox(Vector3 Point, float Size, Color color)
        {
            this.Point = Point;
            this.Size = Size;
            this.Color = color;
        }

        public DebugBox(Vector3 Point, Color color)
        {
            this.Point = Point;
            this.Size = 1f;
            this.Color = color;
        }
    }
}
