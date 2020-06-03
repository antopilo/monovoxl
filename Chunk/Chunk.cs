using HelloMonoGame.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Chunk
{
    public class Chunk : IEntity
    {
        public const int WIDTH = 16;
        public const int DEPTH = 16;
        public const int HEIGHT = 16;

        public bool IsSurrounded = false;
        public bool IsGenerated = false;
        public bool IsMeshed = false;
        public bool Changed = false;

        public string Name { get; set; }
        public Vector2 ChunkPosition { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public SubChunk[] subChunks = new SubChunk[16];

        public Chunk(int x, int z)
        {
            ChunkPosition = new Vector2(x, z);
            Position = new Vector3(16 * ChunkPosition.X, 0, 16 * ChunkPosition.Y);

            // Fill with subchunks
            for (int i = 0; i < HEIGHT; i++)
            {
                subChunks[i] = new SubChunk(this, i);
            }


        }

        public void AddBlock(int x, int y, int z, Blocks type)
        {
            int subChunkIndex = GetSubChunkIdFromHeight(y);
            int subChunkHeight = y - ((int)16 * (subChunkIndex));
            var localPosition = new Vector3(x, subChunkHeight, z);

            subChunks[subChunkIndex].AddBlock(localPosition, type);
        }


        public void RemoveBlock(Vector3 position)
        {
            RemoveBlock((int)position.X, (int)position.Y, (int)position.Z);
        }
        public void RemoveBlock(int x, int y, int z)
        {
            int subChunkIndex = GetSubChunkIdFromHeight(y);
            int subChunkHeight = y - ((int)16 * (subChunkIndex));
            var localPosition = new Vector3(x, subChunkHeight, z);

            subChunks[subChunkIndex].RemoveBlock(localPosition);
            Changed = true;
        }

        public Blocks GetBlock(Vector3 pos)
        {
            return GetBlock((int)pos.X, (int)pos.Y, (int)pos.Z);
        }

        public Blocks GetBlock(int x, int y, int z)
        {
            int subChunkIndex = GetSubChunkIdFromHeight(y);
            int subChunkHeight = y - ((int)16 * (subChunkIndex));
            var localPosition = new Vector3(x, subChunkHeight, z);

            return subChunks[subChunkIndex].GetBlock(localPosition);
        }

        private int GetSubChunkIdFromHeight(int i)
        {
            return (i / 16);
        }

        public void QueueToRender()
        {
            for (int i = 0; i < HEIGHT; i++)
                Renderer.AddToRenderList(subChunks[i]);
        }

        public void Mesh()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                SubChunk sc = subChunks[i];
                sc.Mesh = ChunkMesher.Mesh(sc);
            }

            IsMeshed = true;
            Changed = false;
        }

        public void Rebuild()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                SubChunk sc = subChunks[i];
                if (sc.NeedRebuild)
                {
                    sc.Mesh = ChunkMesher.Mesh(sc);
                    sc.NeedRebuild = false;
                    Renderer.UpdateVertexBuffer(sc);
                }
            }
            Changed = false;
        }

        public void SetPosition(int x, int y, int z)
        {
            this.Position = new Vector3(x, y, z);
        }

        public void SetPosition(Vector3 positon)
        {
            this.Position = Position;
        }

        public SubChunk GetSubChunk(int pSubChunkIndex)
        {
            if (pSubChunkIndex < 16 && pSubChunkIndex >= 0)
                return subChunks[pSubChunkIndex];

            throw new ArgumentOutOfRangeException("Index out of range");
        }

        public void Update(float delta)
        {
            
        }

        public void UpdateFlags()
        {
            Vector2 left  = ChunkPosition - new Vector2(1, 0),
                    right = ChunkPosition + new Vector2(1, 0),
                    front = ChunkPosition + new Vector2(0, 1),
                    back  = ChunkPosition - new Vector2(0, 1);

            if (ChunkManager.IsChunkLoaded(left) && 
                ChunkManager.IsChunkLoaded(right) &&
                ChunkManager.IsChunkLoaded(front) && 
                ChunkManager.IsChunkLoaded(back))
                IsSurrounded = true;
        }

        #region Other chunks
        public Chunk Left
        {
            get { return ChunkManager.GetChunk(ChunkPosition - new Vector2(1, 0)); }
        }

        public Chunk Right
        {
            get { return ChunkManager.GetChunk(ChunkPosition + new Vector2(1, 0)); }
        }

        public Chunk Front
        {
            get { return ChunkManager.GetChunk(ChunkPosition + new Vector2(0, 1)); }
        }

        public Chunk Back
        {
            get { return ChunkManager.GetChunk(ChunkPosition - new Vector2(0, 1)); }
        }
        #endregion
    }
}
