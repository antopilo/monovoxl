using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using HelloMonoGame.Chunk;

namespace HelloMonoGame
{
    public class Game1 : Game
    {
        public static Scene CurrentScene;
        public static string WindowTitle;
        public static Vector2 Resolution = new Vector2(1280, 720);
        public static bool Debug = true;

        private GraphicsDeviceManager graphics;
        public static Game1 Instance;

        public static Thread ChunkThread;
        public Game1()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = (int)Resolution.X;
            graphics.PreferredBackBufferHeight = (int)Resolution.Y;

            CurrentScene = new Scene("Scene 1");

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / 144);


        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;
            Renderer.Initialize(graphics);
            BlockManager.Initialize();
            CurrentScene.Initialize();

            Mouse.SetPosition((int)Resolution.X / 2, (int)Resolution.Y / 2);

            base.Initialize();
            this.Window.Title = "Voxel engine";

            ChunkThread = new Thread(ChunkManager.AsyncUpdate);
            ChunkThread.Start();
        }

        protected override void LoadContent() { }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            CurrentScene.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

           
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {

            Renderer.Draw(gameTime);

            this.Window.Title = "Voxel engine - " + (1 / gameTime.ElapsedGameTime.TotalSeconds);
            base.Draw(gameTime);
        }

        public static void ResetMousePosition()
        {
            
            Mouse.SetPosition((int)Resolution.X / 2, (int)Resolution.Y / 2);
        }
    }
}
