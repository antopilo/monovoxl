using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities
{
    public interface IEntity
    {
        string Name { get; set; }

        Vector3 Position { get; set; }
        Vector3 Rotation { get; set; }

        void Update(float delta);

        void SetPosition(int x, int y, int z);
        void SetPosition(Vector3 positon);

    }
}
