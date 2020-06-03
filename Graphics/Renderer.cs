using HelloMonoGame.Chunk;
using HelloMonoGame.Entities;
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

    public struct DebugHit
    {
        public Vector3 Point;
        public Color Color;

        public DebugHit(Vector3 point, Color color) 
        {
            this.Point = point;
            this.Color = color;
        }
    }


    public static class Renderer
    {
        public static GraphicsDeviceManager Graphics;
        public static GraphicsDevice GraphicsDevice;
        private static BasicEffect DefaultEffect;

        // List of renderable object
        public static List<VertexBuffer> RenderList;
        public static List<DebugLine> DebugList;
        public static List<DebugHit> DebugHit;

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
            
            // Render list
            RenderList = new List<VertexBuffer>();
            DebugList = new List<DebugLine>();
            DebugHit = new List<DebugHit>();
        }
        

        /// <summary>
        /// Add the renderable entity to the render list.
        /// </summary>
        public static void AddToRenderList(IRenderable renderable)
        {
            if (renderable.Mesh.Length <= 0)
                return;

            var vb = new VertexBuffer(Graphics.GraphicsDevice, typeof(VertexPositionColor), renderable.Mesh.Count(), BufferUsage.WriteOnly);
            
            vb.SetData<VertexPositionColor>(renderable.Mesh);

            RenderList.Add(vb);
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

            for (int i = 0; i < RenderList.Count; i++)
            {
                if (RenderList[i].VertexCount < 1)
                    continue;
                
                Graphics.GraphicsDevice.SetVertexBuffer(RenderList[i]);
                Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, RenderList[i].VertexCount / 3);
                
            }
        }

        public static void AddDebugLine(DebugLine line)
        {
            DebugList.Add(line);
        }

        public static void AddDebugHit(DebugHit hit)
        {
            DebugHit.Add(hit);
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

            DrawDebug();
        }
    }
}
