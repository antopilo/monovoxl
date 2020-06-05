using HelloMonoGame.Chunk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities.Particles
{
    public class Particle
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Color Color;
        public Vector3 Velocity;
        private Vector3 OldDistance;
        public List<VertexPositionColor> Mesh;

        public Particle(Vector3 position, Color color)
        {
            this.Position = position;
            this.Color = color;
            this.Rotation = new Vector3();
            this.Velocity = new Vector3();
           
            this.CreateMesh();
            OldDistance = position;
        }
         
        private void CreateMesh()
        {
            this.Mesh = ChunkMesher.GetCubeMesh(this.Color, this.Position);
            
        }

        public void Update(float delta)
        {
            if (ChunkManager.IsPointColliding(Position))
            {
                Velocity = new Vector3();
                return;
            }
            if(Vector3.Distance(Position, OldDistance) > 1f)
            {

                this.CreateMesh();
                OldDistance = Position;
            }

            Velocity.Y -= Scene.Gravity * 0.05f;

            Position += Velocity * 0.05f;

        }
    }
}
