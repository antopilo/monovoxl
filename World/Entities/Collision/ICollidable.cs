using Microsoft.Xna.Framework;

namespace HelloMonoGame.Entities
{
    public interface ICollidable
    {
        BoundingBox BoundingBox { get; set; }

        
    }
}