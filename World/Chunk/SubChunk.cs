using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Chunk
{
    public class SubChunk : IRenderable
    {
        public const int WIDTH = 64;
        public const int HEIGHT = 64;
        public const int DEPTH = 64;

        public const int FULL_COUNT = WIDTH * HEIGHT * DEPTH;
        public const int EMPTY_COUNT = 0;
        

        // Parent info
        public Chunk Parent { get; set; }
        public int Index { get; set; }

        // Blocks
        private byte[,,] Data { get; set; }
        private int m_Count = 0;

        // Flags
        public bool IsLoaded = false;
        public bool IsSetup = false;
        public bool NeedRebuild = false;

        // Mesh
        public VertexPositionColor[] Mesh { get; set; }

        public SubChunk(Chunk parent)
        {
           
            Parent = parent;
        }

        public void Initialize(int i)
        {
            Index = i;
            //Blocks[,,]
            Data = new byte[WIDTH, HEIGHT, DEPTH];
            // Fill
            //for (int z = 0; z < FULL_COUNT; z++)
            //{
            //    
            //}
            m_Count = EMPTY_COUNT;
            for (int z = 0; z < WIDTH; z++)
                for (int y = 0; y < HEIGHT; y++)
                    for (int x = 0; x < DEPTH; x++)
                    {
                        Data[x, y, z] = (byte)Blocks.Air;

                    }

            IsSetup = true;
            IsLoaded = true;
            //Data = dataTemp;
        }

        public int GetCount()
        {
            return m_Count;
        }

        public static int HashCoords(int x, int y, int z)
        {
            return x | (y << 8) | (z << 16);
        }

        private Vector3 DehashCoords(int i)
        {
            return new Vector3((i & 0xFF), (i >> 8) & 0xFF, (i >> 16) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Blocks GetBlock(Vector3 position) 
        { 
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;
            return GetBlock(x, y, z);
        }

        public Blocks GetBlock(int i)
        {
            return Blocks.Air;//Data[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Blocks GetBlock(int x, int y, int z)
        {
            return (Blocks)Data[x, y, z];
            //return Data[x | (y << 4) | (z << 8)];
        }

        public void AddBlock(Vector3 position, Blocks block) 
        {
            int x = (int)position.X, y = (int)position.Y, z = (int)position.Z;
            AddBlock(x, y, z, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBlock(int x, int y, int z, Blocks block)
        {
            Data[x, y, z] = (byte)block;

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
            Data[x, y, z] = (byte)Blocks.Air;
            m_Count--;

            if (x == 0)
            {
                Parent.Left.Changed = true;
                Parent.Left.subChunks[Index].NeedRebuild = true;
            }
            else if (x == WIDTH)
            {
                Parent.Right.Changed = true;
                Parent.Right.subChunks[Index].NeedRebuild = true;
            }
            if (z == 0)
            {
                Parent.Back.Changed = true;
                Parent.Back.subChunks[Index].NeedRebuild = true;
            }
            else if (z == DEPTH)
            {
                Parent.Front.Changed = true;
                Parent.Front.subChunks[Index].NeedRebuild = true;
            } 
            if (y == 0 && Index > 0)
            {
                GetUnderSubChunk().NeedRebuild = true;
            }
            else if (y == HEIGHT && Index < Chunk.HEIGHT - 1)
            {
                GetAboveSubChunk().NeedRebuild = true;
            }
                

            NeedRebuild = true;
        }

        public SubChunk GetAboveSubChunk()
        {
            if (Index < Chunk.HEIGHT)
                return Parent.GetSubChunk(Index + 1);

            throw new ArgumentOutOfRangeException("No subchunk above.");
        }

        public SubChunk GetUnderSubChunk()
        {
            if (Index > 0)
                return Parent.GetSubChunk(Index - 1);

            throw new ArgumentOutOfRangeException("No subchunk under.");
        }

        public SubChunk Clone()
        {
            return (SubChunk)this.MemberwiseClone();
        }
    }
}
