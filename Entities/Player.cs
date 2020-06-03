using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities
{
    class Player : IEntity, ICollidable
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public void SetPosition(int x, int y, int z)
        {
            Position = new Vector3(x, y, z);
        }

        public void SetPosition(Vector3 positon)
        {
            throw new NotImplementedException();
        }

        public void Update(float delta)
        {
            throw new NotImplementedException();
        }
    }
}
