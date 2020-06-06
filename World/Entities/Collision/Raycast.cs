using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities.Collision
{
    public struct Raycast
    {
        public Vector3 Start, End;

        public Raycast(Vector3 start, Vector3 end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
