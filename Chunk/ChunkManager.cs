using HelloMonoGame.Entities;
using HelloMonoGame.Entities.Collision;
using HelloMonoGame.Graphics.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Chunk
{
    public static class ChunkManager
    {
        // Settings
        public const int RENDER_DISTANCE = 8;
        private const int MAX_CHUNKS_PER_FRAME = 8;

        // Camera used
        private static Camera Camera { get; set; }
        private static int CamX = 0, CamZ = 0;

        // Lists
        private static Dictionary<Vector2, Chunk> LoadedChunks { get; set; }
        private static List<Vector2> CreationQueue { get; set; }

        private static FastNoise Noise { get; set; }
        private static int Seed = 1337;
         
        public static void Initialize(Camera camera)
        {
            // Set current camera
            Camera = camera;

            LoadedChunks = new Dictionary<Vector2, Chunk>();
            CreationQueue = new List<Vector2>();

            Noise = new FastNoise(Seed);
            Noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            Noise.SetFrequency(0.00015f);
            Noise.SetFractalType(FastNoise.FractalType.RigidMulti);
            Noise.SetFractalOctaves(5);
            Noise.SetFractalLacunarity(2.0f);
            Noise.SetFractalGain(0.5f);
            
        }


        public static CollisionResult? RaycastCollision( Raycast ray) 
        {
            Vector3 startPoint = ray.Start;
            Vector3 endPoint = ray.End;
            float length = Vector3.Distance(startPoint, endPoint);

            for (int i = 0; i < length * 8; i++)
            {
                Vector3 point = Vector3.Lerp(startPoint, endPoint, (i / 8f) / (length));
                Vector2 chunkPosition = new Vector2((int)point.X / 16, (int)point.Z / 16);
                Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * 16),
                                                    (int)point.Y,
                                                    (int)point.Z - (chunkPosition.Y * 16));

                // Offset
                localPosition += new Vector3(0.5f, 0.5f, 0.5f);

                Vector3 chunkPos3 = new Vector3(chunkPosition.X, 0, chunkPosition.Y);

                if (localPosition.Y > 255 || localPosition.Y < 0)
                    return null;

                if (point.X < 0)
                {
                    if (point.X % 16 != 0)
                        chunkPosition.X = (int)point.X / 16 - 1;

                    localPosition.X = point.X - (16 * chunkPosition.X);
                }
                if (point.Z < 0)
                {
                    if (point.Z % 16 != 0)
                        chunkPosition.Y = (int)point.Z / 16 - 1;

                    localPosition.Z = point.Z - (16 * chunkPosition.Y);
                }

                if (IsChunkLoaded(chunkPosition))
                {
                    Chunk chunk = LoadedChunks[chunkPosition];

                    // Collision!
                    Blocks blockType = chunk.GetBlock(localPosition);
                    if (blockType != Blocks.Air) 
                    { 
                        CollisionResult result = new CollisionResult()
                        {
                            GlobalPosition = point,
                            VoxelPosition = localPosition + (chunkPos3 * 16),
                            BlockType = blockType
                        };
                        return result;
                    }
                }
            }
            return null;
        } 

        public static bool CheckCollision(Vector3 point)
        {
            Vector2 chunkPosition = new Vector2((int)point.X / 16, (int)point.Z / 16);
            Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * 16), 
                                                (int)point.Y, 
                                                (int)point.Z - (chunkPosition.Y * 16));

            // Offset
            localPosition += new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 chunkPos3 = new Vector3(chunkPosition.X, 0, chunkPosition.Y);


            var m = Mouse.GetState();
           

            if (localPosition.Y > 255 || localPosition.Y < 0)
                return false;

            if (point.X < 0)
            {
                if (point.X % 16 != 0)
                    chunkPosition.X = (int)point.X / 16 - 1;

                localPosition.X = point.X - (16 * chunkPosition.X);
            }
            if (point.Z < 0)
            {
                if (point.Z % 16 != 0)
                    chunkPosition.Y = (int)point.Z / 16 - 1;

                localPosition.Z = point.Z - (16 * chunkPosition.Y);
            }

            if (IsChunkLoaded(chunkPosition))
            {
                Chunk chunk = LoadedChunks[chunkPosition];
                return chunk.GetBlock(localPosition) != Blocks.Air;
            }

            return false;
        }

        public static void RemoveBlock(Vector3 point) 
        {
            Vector2 chunkPosition = new Vector2((int)point.X / 16, (int)point.Z / 16);
            Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * 16),
                                                (int)point.Y,
                                                (int)point.Z - (chunkPosition.Y * 16));

            if (localPosition.Y > 255 || localPosition.Y < 0)
                return;

            if (point.X < 0)
            {
                if (point.X % 16 != 0)
                    chunkPosition.X = (int)point.X / 16 - 1;

                localPosition.X = point.X - (16 * chunkPosition.X);
            }
            if (point.Z < 0)
            {
                if (point.Z % 16 != 0)
                    chunkPosition.Y = (int)point.Z / 16 - 1;

                localPosition.Z = point.Z - (16 * chunkPosition.Y);
            }

            if (IsChunkLoaded(chunkPosition))
            {
                Chunk chunk = LoadedChunks[chunkPosition];
                chunk.RemoveBlock(localPosition);
            }
        }

        public static void AsyncUpdate()
        {
            while (true)
            {
                UpdateCreationQueue();
                UpdateToMesh();
                Generate();
                UpdateFlags();
                Mesh();
                CheckForRebuild();
            }
        }


        public static void Update()
        {
            UpdateCreationQueue();
            UpdateToMesh();
            Generate();
            UpdateFlags();
            Mesh();
            CheckForRebuild();
        }

        /// <summary>
        /// Prepare unloaded chunks
        /// </summary>
        private static void UpdateCreationQueue()
        {
            // Max chunk per frame counter
            int counter = 0;

            // Chunk position of the camera.
            CamX = (int)Camera.Position.X / 16;
            CamZ = (int)Camera.Position.Z / 16;

            for (int x = CamX - RENDER_DISTANCE; x < CamX + RENDER_DISTANCE; x++)
            {
                for (int z = CamZ - RENDER_DISTANCE; z < CamZ + RENDER_DISTANCE; z++)
                {
                    if (counter > MAX_CHUNKS_PER_FRAME)
                        return;

                    Vector2 cam = new Vector2(CamX, CamZ);
                    Vector2 chunk = new Vector2(x, z);

                    // Check if we are in the render distance.
                    if (Math.Abs(Vector2.Distance(cam, chunk)) < RENDER_DISTANCE)
                    {
                        if (CreationQueue.Contains(chunk))
                            continue;

                        CreationQueue.Add(chunk);
                        counter++;
                    }
                }
            }
        }

        private static void Generate()
        {
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.IsGenerated == false).OrderBy(c => Vector3.Distance(c.Position, Camera.Position)))
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        float noise = Noise.GetSimplexFractal(x + chunk.Position.X, z + chunk.Position.Z) + 1.0f;
                        int height = (int)(noise * 128f);
                        for (int i = height - 6; i < height; i++)
                        {
                            if (i == height - 1)
                                chunk.AddBlock(x, i, z, Blocks.Grass);
                            else if (i > height - 5)
                                chunk.AddBlock(x, i, z, Blocks.Dirt);
                            else
                                chunk.AddBlock(x, i, z, Blocks.Stone);
                        }
                    }
                }
                chunk.IsGenerated = true;
                counter++;
            }
        }

        private static void UpdateFlags()
        {
            // Max chunk per frame counter
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.IsSurrounded == false && c.IsGenerated == true).OrderBy(c => Vector3.Distance(c.Position, Camera.Position)))
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                chunk.UpdateFlags();
            }
        }

        private static void UpdateToMesh()
        {
            // Max chunk per frame counter
            int counter = 0;

            foreach (Vector2 chunkPos in CreationQueue)
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                if (IsChunkLoaded(chunkPos))
                    continue;

                Chunk newChunk = new Chunk((int)chunkPos.X, (int)chunkPos.Y);

                LoadedChunks.Add(chunkPos, newChunk);

                counter++;
            }
        }

        private static void Mesh()
        {
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.IsSurrounded == true && c.IsMeshed == false).OrderBy( c => Vector3.Distance(c.Position, Camera.Position)))
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                chunk.Mesh();
                chunk.QueueToRender();
                counter++;
            }
        }

        private static void CheckForRebuild()
        {
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.Changed == true ).OrderBy(c => Vector3.Distance(c.Position, Camera.Position)))
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;
                chunk.Rebuild();
            }
        }

        public static Chunk GetChunk(Vector2 position)
        {
            return LoadedChunks[position];
        }

        public static bool IsChunkLoaded(Vector2 position)
        {
            return LoadedChunks.ContainsKey(position);
        }
    }
}
