using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Chunk
{
    public class SubChunk : IRenderable
    {
        public const int FULL_COUNT = 16 * 16 * 16;
        public const int EMPTY_COUNT = 0;
        

        // Parent info
        public Chunk Parent { get; set; }
        public int Index { get; set; }

        // Blocks
        private Blocks[] Data { get; set; }
        private int m_Count = 0;

        // Flags
        public bool IsLoaded = false;
        public bool IsSetup = false;
        public bool NeedRebuild = false;

        // Mesh
        public VertexPositionColor[] Mesh { get; set; }

        public SubChunk(Chunk parent, int i)
        {
            Index = i;
            Parent = parent;
            Data = new Blocks[FULL_COUNT];

            // Fill
            for (int x = 0; x < Chunk.WIDTH; x++)
                for (int y = 0; y < Chunk.HEIGHT; y++)
                    for (int z = 0; z < Chunk.DEPTH; z++)
                    {
                        Data[HashCoords(x, y, z)] = Blocks.Air;
                        m_Count = EMPTY_COUNT;
                    }

            IsSetup = true;
            IsLoaded = true;
            
        }

        public int GetCount()
        {
            return m_Count;
        }

        public static int HashCoords(int x, int y, int z)
        {
            return x | (y << 4) | (z << 8);
        }

        private Vector3 DehashCoords(int i)
        {
            return new Vector3((i & 0xFF), (i >> 4) & 0xFF, (i >> 8) & 0xFF);
        }

        public Blocks GetBlock(Vector3 position) 
        { 
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;
            return GetBlock(x, y, z);
        }
        public Blocks GetBlock(int x, int y, int z)
        {
            return Data[x | (y << 4) | (z << 8)];
        }

        public void AddBlock(Vector3 position, Blocks block) 
        {
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;
            AddBlock(x, y, z, block);
        }
        public void AddBlock(int x, int y, int z, Blocks block)
        {
            Data[HashCoords(x, y, z)] = block;

            // TODO: Check previous block before reducing count.
            if (block != Blocks.Air)
                m_Count++;

            NeedRebuild = true;
        }

        public void RemoveBlock(Vector3 position) 
        {
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;

            RemoveBlock(x, y, z);
        }
        public void RemoveBlock(int x, int y, int z)
        {
            // TODO: Check previous block before reducing count.
            Data[HashCoords(x, y, z)] = Blocks.Air;
            m_Count--;

            if (x == 0)
            {
                Parent.Left.Changed = true;
                Parent.Left.subChunks[Index].NeedRebuild = true;
            }
            else if (x == 15)
            {
                Parent.Right.Changed = true;
                Parent.Right.subChunks[Index].NeedRebuild = true;
            }
            if (z == 0)
            {
                Parent.Back.Changed = true;
                Parent.Back.subChunks[Index].NeedRebuild = true;
            }
            else if (z == 15)
            {
                Parent.Front.Changed = true;
                Parent.Front.subChunks[Index].NeedRebuild = true;
            } 
            if (y == 0 && Index > 0)
            {
                GetUnderSubChunk().NeedRebuild = true;
            }
            else if (y == 15 && Index < 15)
            {
                GetAboveSubChunk().NeedRebuild = true;
            }
                

            NeedRebuild = true;
        }

        public SubChunk GetAboveSubChunk()
        {
            if (Index < 16)
                return Parent.GetSubChunk(Index + 1);

            throw new ArgumentOutOfRangeException("No subchunk above.");
        }

        public SubChunk GetUnderSubChunk()
        {
            if (Index > 0)
                return Parent.GetSubChunk(Index - 1);

            throw new ArgumentOutOfRangeException("No subchunk under.");
        }
    }
}
