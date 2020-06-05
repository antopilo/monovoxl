using HelloMonoGame.Chunk;
using HelloMonoGame.Entities;
using HelloMonoGame.Entities.Particles;
using HelloMonoGame.Graphics.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame
{
    public static class Renderer
    {
        public static GraphicsDeviceManager Graphics;
        public static GraphicsDevice GraphicsDevice;
        private static BasicEffect DefaultEffect;

        // List of renderable object
        public static List<VertexBuffer> RenderList;
        public static List<Particle> ParticleList;
        public static List<DebugLine> DebugList;
        public static List<DebugHit> DebugHit;
        public static List<DebugBox> DebugBoxes;

        /// <summary>
        /// Initialize
        /// </summary>
        public static void Initialize(GraphicsDeviceManager graphics)
        {
            Graphics = graphics;
            GraphicsDevice = graphics.GraphicsDevice;
            
            // Default shader
            DefaultEffect = new BasicEffect(graphics.GraphicsDevice);
            DefaultEffect.VertexColorEnabled = true;
            DefaultEffect.FogEnabled = true;
            DefaultEffect.FogStart = 16 * ChunkManager.RENDER_DISTANCE - 16;
            DefaultEffect.FogEnd = 16 * ChunkManager.RENDER_DISTANCE - 8;
            DefaultEffect.FogColor = Color.CornflowerBlue.ToVector3();
            
            // Render list
            RenderList = new List<VertexBuffer>();
            ParticleList = new List<Particle>();

            // DEBUG
            DebugList = new List<DebugLine>();
            DebugHit = new List<DebugHit>();
            DebugBoxes = new List<DebugBox>();
        }
        
        public static void UpdateFog(float start, float end)
        {
            DefaultEffect.FogStart = start;
            DefaultEffect.FogEnd = end;
        }


        /// <summary>
        /// Add the renderable entity to the render list.
        /// </summary>
        public static void AddToRenderList(IRenderable renderable)
        {
            if (renderable.Mesh.Length <= 0)
                return;

            var vb = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPositionColor), renderable.Mesh.Count(), BufferUsage.WriteOnly);

            // Set Tag for updating.
            if(renderable is SubChunk)
            {
                Chunk.Chunk c = ((SubChunk)renderable).Parent;
                vb.Tag = new Vector3(c.ChunkPosition.X, ((SubChunk)renderable).Index, c.ChunkPosition.Y);
            }
            
            vb.SetData<VertexPositionColor>(renderable.Mesh);

            RenderList.Add(vb);
        }

        public static void RemoveVertexBuffer(IRenderable renderable)
        {
            // Set Tag for updating.
            if (!(renderable is SubChunk))
                return;

            // Create new tag 
            Chunk.Chunk c = ((SubChunk)renderable).Parent;
            Vector3 tag = new Vector3(c.ChunkPosition.X, ((SubChunk)renderable).Index, c.ChunkPosition.Y);

            // Find buffer to udpate
            VertexBuffer vb = RenderList.SingleOrDefault(b => b.Tag != null && (Vector3)b.Tag == tag);

            // Check if not null
            if (vb is null)
                return;

            vb.Dispose();
            RenderList.Remove(vb);

        }

        public static void UpdateVertexBuffer(IRenderable renderable)
        {
            // Set Tag for updating.
            if (!(renderable is SubChunk))
                return;

            // Create new tag 
            Chunk.Chunk c = ((SubChunk)renderable).Parent;
            Vector3 tag = new Vector3(c.ChunkPosition.X, ((SubChunk)renderable).Index, c.ChunkPosition.Y);

            // Find buffer to udpate
            VertexBuffer vb = RenderList.SingleOrDefault(b => b.Tag != null && (Vector3)b.Tag == tag);

            // Check if not null
            if (vb is null)
                return;
            
            // add the updated one.
            var nvb = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPositionColor), renderable.Mesh.Count(), BufferUsage.WriteOnly);
            nvb.Tag = tag;
            nvb.SetData(renderable.Mesh);

            RenderList[RenderList.IndexOf(vb)] = nvb;
        }


        /// <summary>
        /// Draw a mesh on the screen with the correct view and projection.
        /// </summary>
        public static void DrawMesh()
        {
            // Update view and projection with updated camera coordinates.
            DefaultEffect.View = Game1.CurrentScene.CurrentCamera.View;
            DefaultEffect.Projection = Game1.CurrentScene.CurrentCamera.ProjectionMatrix;

            DefaultEffect.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < RenderList.Count(); i++)
            {
                if (RenderList[i].VertexCount < 1)
                    continue;
                
                Graphics.GraphicsDevice.SetVertexBuffer(RenderList[i]);
                Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, RenderList[i].VertexCount / 3);
                
            }
        }

        public static void DrawParticles()
        {
            for (int i = 0; i < ParticleList.Count(); i++)
            {
                if (ParticleList[i].Mesh.Count < 1)
                    continue;
                VertexBuffer vb = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPositionColor), ParticleList[i].Mesh.Count(), BufferUsage.WriteOnly);
                vb.SetData<VertexPositionColor>(ParticleList[i].Mesh.ToArray());
                Graphics.GraphicsDevice.SetVertexBuffer(vb);
                Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vb.VertexCount / 3);
            }
        }

        public static void AddParticle(Particle p)
        {

            ParticleList.Add(p);
        }

        public static void AddDebugLine(DebugLine line)
        {
            DebugList.Add(line);
        }

        public static void AddDebugHit(DebugHit hit)
        {
            DebugHit.Add(hit);
        }

        public static void AddDebugBox(DebugBox box)
        {
            DebugBoxes.Add(box);
        }

        public static void DrawDebug()
        {
            // Update view and projection with updated camera coordinates.
            DefaultEffect.View = Game1.CurrentScene.CurrentCamera.View;
            DefaultEffect.Projection = Game1.CurrentScene.CurrentCamera.ProjectionMatrix;

            DefaultEffect.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < DebugList.Count; i++)
            {
                DebugLine dl = DebugList[i];

                VertexPositionColor[] buffer = { new VertexPositionColor(dl.Start, dl.Color), new VertexPositionColor(dl.End, dl.Color) };

                Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, buffer, 0, 1);
            }

            for (int i = 0; i < DebugHit.Count; i++)
            {
                DebugHit dh = DebugHit[i];
                VertexPositionColor[] buffer = { 
                    new VertexPositionColor(dh.Point + new Vector3(0, 0, 1), dh.Color),  new VertexPositionColor(dh.Point - new Vector3(0, 0, 1), dh.Color),
                    new VertexPositionColor(dh.Point + new Vector3(1, 0, 0), dh.Color),  new VertexPositionColor(dh.Point - new Vector3(1, 0, 0), dh.Color),
                };
                Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, buffer, 0, 2);
            }

            for (int i = 0; i < DebugBoxes.Count; i++)
            {
                DebugBox db = DebugBoxes[i];
                VertexPositionColor[] buffer = {
                    // TOP
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, -0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, -0.5f), db.Color),

                    // middle
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f,  0.5f, -0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(-0.5f, -0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f,  0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f, -0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, -0.5f), db.Color),

                    // bottom
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, 0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, 0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(-0.5f, 0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, 0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, 0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, -0.5f), db.Color),

                    new VertexPositionColor(db.Point + new Vector3(0.5f,  0.5f, -0.5f), db.Color),
                    new VertexPositionColor(db.Point + new Vector3(-0.5f, 0.5f, -0.5f), db.Color),
                };
                Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, buffer, 0, buffer.Count() / 2);
            }
        }


        /// <summary>
        /// Draw the screen every frame.
        /// </summary>
        public static void Draw(GameTime gameTime)
        {
            // Clear the screen.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw everything in the render list.
            DrawMesh();

            DrawParticles();

            DrawDebug();
        }
    }
}
