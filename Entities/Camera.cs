using HelloMonoGame.Chunk;
using HelloMonoGame.Graphics.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Entities
{
    public class Camera : IEntity
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Vector3 Target { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Right { get; set; }

        private float LastX { get; set; }
        private float LastY { get; set; }
        private float Pitch { get; set; }
        private float Yaw { get; set; }

        public Matrix View;
        public bool First = true;

        public float Sensitivity { get; set; } = 0.25f;
        public float MoveSpeed { get; set; } = 0.3f;

        public bool pressed = false;
        public Vector3 SelectVoxel { get; set; } = new Vector3();

        public Camera(Vector3 Position, Vector3 Target, Vector3 Up)
        {
            this.Position = Position;
            this.Target = Target;
            this.Direction = Vector3.Normalize(this.Position - this.Target);

            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.Up = Vector3.Normalize(Vector3.Cross(this.Direction, this.Right));

            this.View = Matrix.CreateLookAt(Position, Target, Up);

        }



        public void Update(float delta)
        {
            if (Game1.Instance.IsActive)
            {
                KeyboardControl();
                MouseControl();
            }
            else
            {
                First = true;
            }

            var m = Mouse.GetState();

            if (m.LeftButton == ButtonState.Pressed && pressed == false)
            {
                Raycast(25f);
                pressed = true;
            }
            else if(m.LeftButton == ButtonState.Released && pressed == true)
            {
                pressed = false;
            }
                

            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.View = Matrix.CreateLookAt(Position, Position + Direction, Up);
        }

        private void Raycast(float length)
        {
            Vector3 startPoint = Position;
            Vector3 endPoint = Position + (Direction * length);

            for (int i = 0; i < length; i++)
            {
                Vector3 current = Vector3.Lerp(startPoint, endPoint, i / (length));

                // Check collision
                if (ChunkManager.CheckCollision(current))
                {
                    Renderer.AddDebugLine(new DebugLine(startPoint, current, Color.Red));
                    ChunkManager.RemoveBlock(current);
                    Renderer.AddDebugHit(new DebugHit(current, Color.Blue));
                    Console.WriteLine("Collision made at: " + current);
                    return;
                }
            }
        }


        private void KeyboardControl()
        {
            var speed = 0.25f;

            // movement
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.W))
                Position += speed * Direction;
            if (keyboard.IsKeyDown(Keys.S))
                Position -= speed * Direction;
            if (keyboard.IsKeyDown(Keys.A))
                Position += speed * Right;
            if (keyboard.IsKeyDown(Keys.D))
                Position -= speed * Right;
            if (keyboard.IsKeyDown(Keys.Space))
                Position += Up * speed;
            if (keyboard.IsKeyDown(Keys.LeftShift))
                Position -= Up * speed;
        }


        private void MouseControl()
        {
            var mouse = Mouse.GetState();

            if (First)
            {
                LastX = mouse.X;
                LastY = mouse.Y;
                First = false;
            }
            float xOffset = mouse.X - LastX;
            float yOffset = mouse.Y - LastY;

            xOffset *= Sensitivity;
            yOffset *= Sensitivity;

            Yaw += xOffset;
            Pitch += yOffset;

            float radYaw = MathHelper.ToRadians(Yaw);
            float radPitch = MathHelper.ToRadians(Pitch);

            Direction = new Vector3(
                (float)(Math.Cos(radYaw) * Math.Cos(radPitch)),
                (float)Math.Sin(radPitch),
                (float)(Math.Sin(radYaw) * Math.Cos(radPitch)));


            Game1.ResetMousePosition();
            mouse = Mouse.GetState();
            LastX = mouse.X;
            LastY = mouse.Y;
        }


        public void SetPosition(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void SetPosition(Vector3 positon)
        {
            throw new NotImplementedException();
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 20000;
                float aspectRatio = Renderer.GraphicsDevice.Viewport.Width / (float)Renderer.GraphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }
    }
}
