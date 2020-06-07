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

        //Vector3 Position { get; set; }
        Vector3 Rotation { get; set; }

        void Update(GameTime gameTime);
    }
}
