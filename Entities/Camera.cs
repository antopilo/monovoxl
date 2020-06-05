using HelloMonoGame.Chunk;
using HelloMonoGame.Entities.Collision;
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
        public float Gravity { get; set; } = 0.02f;

        public bool pressed = false;
        public Vector3 SelectVoxel { get; set; } = new Vector3();

        public bool IsFlying = true;
        public float Height = 6f;
        public Raycast GroundRay;

        public Camera(Vector3 Position, Vector3 Target, Vector3 Up)
        {
            this.Position = Position;
            this.Target = Target;
            this.Direction = Vector3.Normalize(this.Position - this.Target);

            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.Up = Vector3.Normalize(Vector3.Cross(this.Direction, this.Right));

            this.View = Matrix.CreateLookAt(Position, Target, Up);
            GroundRay = new Raycast(Position, Position + Vector3.Down * Height);

        }



        public void Update(float delta)
        {
            if (Game1.Instance.IsActive)
            {
                
                MouseControl();
            }
            else
            {
                First = true;
            }

            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.C))
                IsFlying = !IsFlying;

            if (!IsFlying)
            {
                GroundRay.Start = Position;
                GroundRay.End = Position - new Vector3(0, Height, 0);
                CollisionResult? result = CollisionHelper.IsCollidingWithWorld(GroundRay);
                if(result != null)
                {
                    if(Position.Y - Height <= result.Value.VoxelPosition.Y)
                        Position = new Vector3(Position.X, result.Value.GlobalPosition.Y + Height - 0.01f, Position.Z);
                }
                else
                {
                    Position = Position - new Vector3(0, Gravity * delta, 0);
                }
                KeyboardControlGround();
            }
            else
            {
                KeyboardControl();
            }

           
            DestroyRaycast(25f);
            

            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.View = Matrix.CreateLookAt(Position, Position + Direction, Up);
        }

        
        private void DestroyRaycast(float length)
        {
            var m = Mouse.GetState();
            Vector3 startPoint = Position;
            Vector3 endPoint = Position + (Direction * length);

            for (int i = 0; i < length * 4; i++)
            {
                Vector3 current = Vector3.Lerp(startPoint, endPoint, (i / 4f) / (length));

                // Check collision
                if (ChunkManager.CheckCollision(current) && m.LeftButton == ButtonState.Pressed && pressed == false)
                {
                    Renderer.AddDebugLine(new DebugLine(startPoint, current, Color.Red));
                    ChunkManager.RemoveBlock(current);

                    
                    ChunkManager.RemoveBlock(current);
                            
                    pressed = true;
                    return;
                }
                else if(m.LeftButton == ButtonState.Released && pressed == true)
                {
                    pressed = false;
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

        private void KeyboardControlGround()
        {
            var speed = 0.1f;

            var nd = new Vector3(Direction.X, 0, Direction.Z);
            // movement
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.W))
                Position += speed * nd;
            if (keyboard.IsKeyDown(Keys.S))
                Position -= speed * nd;
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

            if (Pitch - yOffset < -89)
            {
                Pitch = -89;
                yOffset = 0;
            }
                
            if (Pitch - yOffset > 89)
            {
                Pitch = 89;
                yOffset = 0;
            }

            Yaw += xOffset;
            Pitch -= yOffset;

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
