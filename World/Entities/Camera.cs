using HelloMonoGame.Chunk;
using HelloMonoGame.Entities.Collision;
using HelloMonoGame.Entities.Particles;
using HelloMonoGame.Graphics.Debug;
using HelloMonoGame.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace HelloMonoGame.Entities
{
    public class Camera : IEntity
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Vector3 Target { get; set; }
        public Vector3 Direction { get; set; }
        private Vector3 TargetDirection { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Right { get; set; }
        public Matrix View;

        #region Mouse
        public float Sensitivity { get; set; } = 0.25f;
        public bool First = true;
        private float LastX { get; set; }
        private float LastY { get; set; }
        private float Pitch { get; set; }
        private float Yaw { get; set; }
        private float Smoothing = 0.1f;

        public bool pressed = false;
        public Vector3 SelectVoxel { get; set; } = new Vector3();
        #endregion

        #region Movement
        public const float MAX_SPEED = 25;
        public const float GRAVITY = 16f;
        public const float ACCELERATION = 0.3f;
        public const float DECELERATION = 0.4f;
        public const float HEIGHT = 1.75f;
        private Vector2 Input = new Vector2();

        public bool IsFlying = true;
        public bool IsGrounded = false;
        public Raycast GroundRay;
        public Vector3 Velocity = new Vector3();
        #endregion

        public Camera(Vector3 Position, Vector3 Target, Vector3 Up)
        {
            this.Position = Position;
            this.Target = Target;
            this.Direction = Vector3.Normalize(this.Position - this.Target);

            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.Up = Vector3.Normalize(Vector3.Cross(this.Direction, this.Right));

            this.View = Matrix.CreateLookAt(Position, Target, Up);
            GroundRay = new Raycast(Position, Position + Vector3.Down * HEIGHT);
        }

        public void Update(GameTime gameTime)
        {
            // If window is focused
            if (Game1.Instance.IsActive)
            {
                GetInput();
                MouseControl();
               
            }
            else
            {
                First = true;
            }
            
            if (InputManager.IsKeyDown(Keys.F1))
                IsFlying = true;
            if (InputManager.IsKeyDown(Keys.F2))
                IsFlying = false;

            KeyboardControl();

            if (!IsFlying)
            {
                GroundRay.Start = Position;
                GroundRay.End = Position - new Vector3(0, HEIGHT, 0);
                CollisionResult? result = CollisionHelper.IsCollidingWithWorld(GroundRay);
                IsGrounded = result != null;

                if (IsGrounded)
                {
                    Position = Vector3.Lerp(Position, new Vector3(Position.X, result.Value.VoxelPosition.Y + HEIGHT, Position.Z), 0.2f);
                    Velocity.Y = 0;
                }
                    
                else
                    Velocity -= GRAVITY * Up * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            Direction = Vector3.Lerp(Direction, TargetDirection, Smoothing);

            DestroyRaycast(25f);

            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            this.View = Matrix.CreateLookAt(Position, Position + Direction, Up);
        }

        #region Controls

        public void GetInput()
        {
            if (InputManager.IsKeyDown(Keys.W))
                Input.Y = 1;
            else if (InputManager.IsKeyDown(Keys.S))
                Input.Y = -1;
            else
                Input.Y = 0;
            if (InputManager.IsKeyDown(Keys.A))
                Input.X = 1;
            else if (InputManager.IsKeyDown(Keys.D))
                Input.X = -1;
            else
                Input.X = 0;
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

            TargetDirection = new Vector3(
                (float)(Math.Cos(radYaw) * Math.Cos(radPitch)),
                (float)Math.Sin(radPitch),
                (float)(Math.Sin(radYaw) * Math.Cos(radPitch)));


            Game1.ResetMousePosition();
            mouse = Mouse.GetState();
            LastX = mouse.X;
            LastY = mouse.Y;
        }

        private void KeyboardControl()
        {
            if (Input.Y == 1)
                Velocity += ACCELERATION * Direction;
            else if (Input.Y == -1)
                Velocity -= ACCELERATION * Direction;

            if (Input.X == 1)
                Velocity += ACCELERATION * Right;
            else if (Input.X == -1)
                Velocity -= ACCELERATION * Right;

            else if (InputManager.IsKeyDown(Keys.Space))
                Velocity += Up * ACCELERATION;
            else if (InputManager.IsKeyDown(Keys.LeftShift))
                Velocity -= Up * ACCELERATION;

            

            if (Input.Y == 0)
            {
                var dir = Direction;
                dir.Y = 0;
                var dot = Vector3.Dot(Velocity, Vector3.Normalize(Direction));
                Velocity -= Math.Sign(dot) * Direction * DECELERATION;
            }

            if (Input.X == 0)
            {
                var dir = Direction;
                dir.Y = 0;
                var vel = Velocity;
                vel.Y = 0;
                var dot = Vector3.Dot(Velocity, Vector3.Normalize(Right));
                Velocity -= Math.Sign(dot) * Right * DECELERATION;

            }

            if (Velocity.Length() < 1f && Input.X == 0 && Input.Y == 0)
                Velocity = new Vector3(0);

            // MaxSpeed
            if (Velocity.Length() > MAX_SPEED)
            {
                Velocity = Vector3.Normalize(Velocity) * MAX_SPEED;
            }
        }

        #endregion

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

                    pressed = true;
                    return;
                }
                else if (m.LeftButton == ButtonState.Released && pressed == true)
                {
                    pressed = false;
                }
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                float fieldOfView = 0.90f;
                float nearClipPlane = 0.1f;
                float farClipPlane = 20000;
                float aspectRatio = Renderer.GraphicsDevice.Viewport.Width / (float)Renderer.GraphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }
    }
}
