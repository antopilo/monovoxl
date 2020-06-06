using HelloMonoGame.Entities;
using HelloMonoGame.Entities.Collision;
using HelloMonoGame.Entities.Particles;
using HelloMonoGame.Generation;
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
        public static int MIN_RENDER_DISTANCE = 4;
        public static int MAX_RENDER_DISTANCE = 8;
        public static int RENDER_DISTANCE = 6;
        private const int MAX_CHUNKS_PER_FRAME = 4;

        // Camera used
        private static Camera Camera { get; set; }
        private static int CamX = 0, CamZ = 0;

        // Lists
        private static Dictionary<Vector2, Chunk> LoadedChunks { get; set; }
        private static HashSet<Vector2> CreationQueue { get; set; }

        public static int ClosestUnloadedChunk = 16;

        public static void Initialize(Camera camera)
        {
            // Set current camera
            Camera = camera;

            LoadedChunks = new Dictionary<Vector2, Chunk>();
            CreationQueue = new HashSet<Vector2>();
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
                CheckForUnload();
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

        #region States
        /// <summary>
        /// Prepare unloaded chunks
        /// </summary>
        private static void UpdateCreationQueue()
        {
            // Max chunk per frame counter
            int counter = 0;
            int cacheDistance = ClosestUnloadedChunk;
            // Chunk position of the camera.
            CamX = (int)Camera.Position.X / SubChunk.WIDTH;
            CamZ = (int)Camera.Position.Z / SubChunk.DEPTH;

            for (int x = CamX - RENDER_DISTANCE; x < CamX + RENDER_DISTANCE; x++)
            {
                for (int z = CamZ - RENDER_DISTANCE; z < CamZ + RENDER_DISTANCE; z++)
                {
                    if (counter > MAX_CHUNKS_PER_FRAME)
                        return;

                    Vector2 cam = new Vector2(CamX, CamZ);
                    Vector2 chunk = new Vector2(x, z);

                    float distance = Math.Abs(Vector2.Distance(cam, chunk));
                    // Check if we are in the render distance.
                    if (distance < RENDER_DISTANCE)
                    {

                        if (CreationQueue.Contains(chunk))
                            continue;

                        if (distance < ClosestUnloadedChunk && distance > 2)
                        {
                            if (RENDER_DISTANCE > 4)
                                RENDER_DISTANCE -= 1;

                            ClosestUnloadedChunk = (int)distance;
                            Renderer.UpdateFog((ClosestUnloadedChunk - 1) * SubChunk.WIDTH, ClosestUnloadedChunk * SubChunk.WIDTH);
                        }

                        CreationQueue.Add(chunk);
                        counter++;
                    }
                }
            }

            if (cacheDistance == ClosestUnloadedChunk && ClosestUnloadedChunk < RENDER_DISTANCE)
            {
                if (RENDER_DISTANCE < 32)
                {
                    RENDER_DISTANCE += 1;
                }
                ClosestUnloadedChunk += 1;
                Renderer.UpdateFog((ClosestUnloadedChunk - 2) * SubChunk.WIDTH, (ClosestUnloadedChunk - 1) * SubChunk.WIDTH);
            }
        }

        private static void UpdateToMesh()
        {
            if (CreationQueue.Count < 1)
                return;

            // Max chunk per frame counter
            int counter = 0;
            
            foreach (Vector2 chunkPos in CreationQueue.ToList())
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                if (IsChunkLoaded(chunkPos))
                    continue;

                Chunk newChunk = new Chunk((int)chunkPos.X, (int)chunkPos.Y);

                LoadedChunks.Add(chunkPos, newChunk);
                CreationQueue.Remove(chunkPos);
                counter++;
            }
        }

        private static void Generate()
        {
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.IsGenerated == false).OrderBy(c => Vector3.Distance(c.Position, Camera.Position)))
            {
                if (counter > MAX_CHUNKS_PER_FRAME)
                    return;

                Generator.Generate(chunk);
                
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

        private static void Mesh()
        {
            int counter = 0;

            foreach (Chunk chunk in LoadedChunks.Values.Where(c => c.IsSurrounded == true && c.IsMeshed == false).OrderBy( c => Vector3.Distance(c.Position, Camera.Position)))
            {
                //if (counter > MAX_CHUNKS_PER_FRAME)
                //    return;

                
                chunk.Mesh();
                
                counter++;
            }
        }

        private static void CheckForUnload() 
        {
            int counter = 0;
            foreach (var item in LoadedChunks.Values.OrderByDescending(c => Vector2.Distance(new Vector2(CamX, CamZ), c.ChunkPosition)))
            {
                if(Vector2.Distance(new Vector2(CamX, CamZ), item.ChunkPosition) > MAX_RENDER_DISTANCE)
                {
                    //Console.WriteLine("ChunkDistance: " + Vector2.Distance(new Vector2(CamX, CamZ), item.ChunkPosition));

                    if (counter > MAX_CHUNKS_PER_FRAME)
                        return;

                    Chunk chunk = item;
                    chunk.Unload();

                    LoadedChunks.Remove(chunk.ChunkPosition);
                    
                    counter++;
                }
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

        #endregion

        public static Chunk GetChunk(Vector2 position)
        {
            if (!IsChunkLoaded(position))
                return null;

            return LoadedChunks[position];
        }

        public static Blocks GetBlock(Vector3 pos)
        {
            return GetBlock((int)pos.X, (int)pos.Y, (int)pos.Z);
        }

        

        public static Blocks GetBlock(int gx, int gy, int gz)
        {
            // Chunk position
            int cx = gx / SubChunk.WIDTH;
            int cz = gz / SubChunk.WIDTH;

            // Local position
            int lx = gx - (cx * SubChunk.WIDTH);
            int ly = gy;
            int lz = gz - (cz * SubChunk.WIDTH);

            if (ly > Chunk.MAX_BLOCK_HEIGHT - 1 || ly < 0)
                return Blocks.Air;

            if (gx < 0)
            {
                if (gx % SubChunk.WIDTH != 0)
                    cx = gx / SubChunk.WIDTH - 1;

                lx = gx - (SubChunk.WIDTH * cx);
            }
            if (gz < 0)
            {
                if (gz % SubChunk.WIDTH != 0)
                    cz = gz / SubChunk.WIDTH - 1;

                lz= gz - (SubChunk.WIDTH * cz);
            }

            //Renderer.AddDebugBox(new DebugBox(new Vector3(gx + 0.5f, gy + 0.5f, gz + 0.5f), Color.Green));
            Vector2 cv = new Vector2(cx, cz);
            try
            {
                return LoadedChunks[cv].GetBlock(lx, ly, lz);
            }catch(Exception e)
            {
                return Blocks.Air;
            }
            
            
        }

        public static void RemoveBlock(Vector3 point)
        {
            Vector2 chunkPosition = new Vector2((int)point.X / SubChunk.WIDTH, (int)point.Z / SubChunk.DEPTH);
            Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * SubChunk.WIDTH),
                                                (int)point.Y,
                                                (int)point.Z - (chunkPosition.Y * SubChunk.DEPTH));

            if (localPosition.Y > Chunk.MAX_BLOCK_HEIGHT || localPosition.Y < 0)
                return;

            if (point.X < 0)
            {
                if (point.X % SubChunk.WIDTH != 0)
                    chunkPosition.X = (int)point.X / SubChunk.WIDTH - 1;

                localPosition.X = point.X - (SubChunk.WIDTH * chunkPosition.X);
            }
            if (point.Z < 0)
            {
                if (point.Z % SubChunk.DEPTH != 0)
                    chunkPosition.Y = (int)point.Z / SubChunk.DEPTH - 1;

                localPosition.Z = point.Z - (SubChunk.DEPTH * chunkPosition.Y);
            }

            if (IsChunkLoaded(chunkPosition))
            {
                Chunk chunk = LoadedChunks[chunkPosition];

                //ParticleManager.SpawnCubeParticle(point, Color.Red);

                chunk.RemoveBlock(localPosition);
            }
        }

        public static bool IsChunkLoaded(Vector2 position)
        {
            return LoadedChunks.ContainsKey(position);
        }

        #region Collision
        public static CollisionResult? RaycastCollision(Raycast ray)
        {
            Vector3 startPoint = ray.Start;
            Vector3 endPoint = ray.End;
            float length = Vector3.Distance(startPoint, endPoint);

            for (int i = 0; i < length * 8; i++)
            {
                Vector3 point = Vector3.Lerp(startPoint, endPoint, (i / 8f) / (length));
                Vector2 chunkPosition = new Vector2((int)point.X / SubChunk.WIDTH, (int)point.Z / SubChunk.DEPTH);
                Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * SubChunk.DEPTH),
                                                    (int)point.Y,
                                                    (int)point.Z - (chunkPosition.Y * SubChunk.DEPTH));

                // Offset
                localPosition += new Vector3(0.5f, 0.5f, 0.5f);

                Vector3 chunkPos3 = new Vector3(chunkPosition.X, 0, chunkPosition.Y);

                if (localPosition.Y > Chunk.HEIGHT * SubChunk.HEIGHT - 1 || localPosition.Y < 0)
                    return null;

                if (point.X < 0)
                {
                    if (point.X % SubChunk.WIDTH != 0)
                        chunkPosition.X = (int)point.X / SubChunk.WIDTH - 1;

                    localPosition.X = point.X - (SubChunk.WIDTH * chunkPosition.X);
                }
                if (point.Z < 0)
                {
                    if (point.Z % SubChunk.DEPTH != 0)
                        chunkPosition.Y = (int)point.Z / SubChunk.DEPTH - 1;

                    localPosition.Z = point.Z - (SubChunk.DEPTH * chunkPosition.Y);
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
                            VoxelPosition = localPosition + (chunkPos3 * SubChunk.WIDTH),
                            BlockType = blockType
                        };
                        return result;
                    }
                }
            }
            return null;
        }

        public static bool IsPointColliding(Vector3 point)
        {
            Vector2 chunkPosition = new Vector2((int)point.X / SubChunk.WIDTH, (int)point.Z / SubChunk.WIDTH);
            Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * SubChunk.WIDTH),
                                                (int)point.Y,
                                                (int)point.Z - (chunkPosition.Y * SubChunk.DEPTH));
            // Offset
            localPosition += new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 chunkPos3 = new Vector3(chunkPosition.X, 0, chunkPosition.Y);

            if (localPosition.Y > 255 || localPosition.Y < 0)
                return false;

            if (point.X < 0)
            {
                if (point.X % SubChunk.WIDTH != 0)
                    chunkPosition.X = (int)point.X / SubChunk.WIDTH - 1;

                localPosition.X = point.X - (SubChunk.WIDTH * chunkPosition.X);
            }
            if (point.Z < 0)
            {
                if (point.Z % SubChunk.DEPTH != 0)
                    chunkPosition.Y = (int)point.Z / SubChunk.DEPTH - 1;

                localPosition.Z = point.Z - (SubChunk.DEPTH * chunkPosition.Y);
            }

            if (IsChunkLoaded(chunkPosition))
            {
                Chunk chunk = LoadedChunks[chunkPosition];

                // Collision!
                return chunk.GetBlock(localPosition) != Blocks.Air;
            }
            return false;
        }
        
        public static bool CheckCollision(Vector3 point)
        {
            
            Vector2 chunkPosition = new Vector2((int)point.X / SubChunk.WIDTH, (int)point.Z / SubChunk.DEPTH);
            Vector3 localPosition = new Vector3((int)point.X - (chunkPosition.X * SubChunk.WIDTH),
                                                (int)point.Y,
                                                (int)point.Z - (chunkPosition.Y * SubChunk.DEPTH));

            // Offset
            localPosition += new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 chunkPos3 = new Vector3(chunkPosition.X, 0, chunkPosition.Y);


            var m = Mouse.GetState();


            if (localPosition.Y > Chunk.MAX_BLOCK_HEIGHT - 1 || localPosition.Y < 0)
                return false;

            if (point.X < 0)
            {
                if (point.X % 16 != 0)
                    chunkPosition.X = (int)point.X / SubChunk.WIDTH - 1;

                localPosition.X = point.X - (SubChunk.WIDTH * chunkPosition.X);
            }
            if (point.Z < 0)
            {
                if (point.Z % SubChunk.WIDTH != 0)
                    chunkPosition.Y = (int)point.Z / SubChunk.WIDTH - 1;

                localPosition.Z = point.Z - (SubChunk.WIDTH * chunkPosition.Y);
            }

            if (IsChunkLoaded(chunkPosition))
            {
                Chunk chunk = LoadedChunks[chunkPosition];
                return chunk.GetBlock(localPosition) != Blocks.Air;
            }

            return false;
        }

#endregion
    }
}
