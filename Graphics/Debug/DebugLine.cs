using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Graphics.Debug
{
    public struct DebugLine
    {
        public Vector3 Start;
        public Vector3 End;
        public Color Color;

        public DebugLine(Vector3 start, Vector3 end, Color color)
        {
            this.Start = start;
            this.End = end;
            this.Color = color;
        }
    }
}
