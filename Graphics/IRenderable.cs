using Microsoft.Xna.Framework.Graphics;

namespace HelloMonoGame
{
    public interface IRenderable
    {
        VertexPositionColor[] Mesh { get; set; }    
    }
}