using HelloMonoGame.Graphics.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Chunk
{
    public enum CUBE_FACES
    {
        Top, Bottom,
        Left, Right,
        Front, Back
    }

    public static class ChunkMesher
    {
        public const float CUBE_SIZE = 1f;
        public static Vector3[] CUBE_VERTICES =
        {
            new Vector3(0, 0, 0), // 0
            new Vector3(1, 0, 0), // 1
            new Vector3(1, 0, 1), // 2
            new Vector3(0, 0, 1), // 3
            new Vector3(0, 1, 0), // 4
            new Vector3(1, 1, 0), // 5
            new Vector3(1, 1, 1), // 6
            new Vector3(0, 1, 1)  // 7
        };
        public static Vector3[] CUBE_NORMALS =
        {
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1), new Vector3(0, 0, -1)
        };

        private static Random rng = new Random();
        
        private static List<VertexPositionColor> CurrentArray;
        public static VertexPositionColor[] Mesh(SubChunk chunk)
        {
            if (chunk.GetCount() == 0)
                return new VertexPositionColor[] { };

            CurrentArray = new List<VertexPositionColor>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        Blocks type = chunk.GetBlock(x, y, z);
                        if (type == Blocks.Air)
                            continue;

                        CreateBlock(x, y, z, chunk, type);
                    }
                }
            }
            return CurrentArray.ToArray();
        }

        private static void CreateBlock(int x, int y, int z, SubChunk chunk, Blocks type)
        {
            var gp = new Vector3(chunk.Parent.Position.X + x, 16 * chunk.Index + y, chunk.Parent.Position.Z + z);
            int gx = (int)gp.X, gy = (int)gp.Y, gz = (int)gp.Z;

            bool top, bottom, left, right, front, back;
            top    = y != 15 ? chunk.GetBlock(x, y + 1, z) == Blocks.Air : true;
            bottom = y != 0  ? chunk.GetBlock(x, y - 1, z) == Blocks.Air : true;
            left   = x != 0  ? chunk.GetBlock(x - 1, y, z) == Blocks.Air : true;
            right  = x != 15 ? chunk.GetBlock(x + 1, y, z) == Blocks.Air : true;
            front  = z != 15 ? chunk.GetBlock(x, y, z + 1) == Blocks.Air : true;
            back   = z != 0  ? chunk.GetBlock(x, y, z - 1) == Blocks.Air : true;

            // Block is surrounded
            if (top && bottom && left && right && front && back)
                return;

            bool leftChunk = true;
            bool rightChunk = true;
            bool backChunk = true;
            bool frontChunk = true;
            if (x == 0)
                leftChunk = chunk.Parent.Left.GetSubChunk(chunk.Index).GetBlock(15, y, z) == Blocks.Air;
            if (x == 15)
                rightChunk = chunk.Parent.Right.GetSubChunk(chunk.Index).GetBlock(0, y, z) == Blocks.Air;
            if (z == 0)
                backChunk = chunk.Parent.Back.GetSubChunk(chunk.Index).GetBlock(x, y, 15) == Blocks.Air;
            if (z == 15)
                frontChunk = chunk.Parent.Front.GetSubChunk(chunk.Index).GetBlock(x, y, 0) == Blocks.Air;

            // Check if there is a block in the above subchunk.
            // Dont check if the subchunk is the bottom or top one.
            bool topChunk = true;
            bool bottomChunk = true;
            if (chunk.Index != 15)
            {
                SubChunk topSubChunk = chunk.GetAboveSubChunk();
                if (topSubChunk.GetCount() == SubChunk.FULL_COUNT)
                    topChunk = false;
                else
                    topChunk = topSubChunk.GetBlock(x, 0, z) == Blocks.Air;
            }
            if (chunk.Index != 0)
            {
                SubChunk botSubChunk = chunk.GetUnderSubChunk();
                if (botSubChunk.GetCount() == SubChunk.FULL_COUNT)
                    bottomChunk = false;
                else
                    bottomChunk = botSubChunk.GetBlock(x, 15, z) == Blocks.Air;
            }

            if (!leftChunk && !rightChunk && !backChunk && !frontChunk && !topChunk && !bottomChunk)
                return;

            // False if should not render chunk border faces.
            bool topBorder    = y == 15 ? topChunk    : top;
            bool bottomBorder = y == 0  ? bottomChunk : bottom;
            bool leftBorder   = x == 0  ? leftChunk   : left;
            bool rightBorder  = x == 15 ? rightChunk  : right;
            bool frontBorder  = z == 15 ? frontChunk  : front;
            bool backBorder   = z == 0  ? backChunk   : back;

            Color color = BlockManager.GetBlockColor(type, gx, gz);
            if (topBorder)
            {
                
                PushQuad(CUBE_FACES.Top, gx, gy, gz, 4, 5, 6, 7, false, color);
            }                                                          
            if (bottomBorder)                                                
            {
                PushQuad(CUBE_FACES.Bottom, gx, gy, gz, 3, 2, 1, 0, false, color);
            }                                                  
            if (leftBorder)                                          
            {
                PushQuad(CUBE_FACES.Left, gx, gy, gz, 0, 4, 7, 3, false, Color.Lerp(color, Color.Black, 0.1f));
            }                                               
            if (rightBorder)                                    
            {
                PushQuad(CUBE_FACES.Right, gx, gy, gz, 1, 2, 6, 5, false, Color.Lerp(color, Color.Black, 0.15f));
            }                                                  
            if (frontBorder)                                         
            {
                PushQuad(CUBE_FACES.Front, gx, gy, gz, 2, 3, 7, 6, false, Color.Lerp(color, Color.Black, 0.1f));
            }                                                     
            if (backBorder)                                             
            {
                PushQuad(CUBE_FACES.Back, gx, gy, gz, 5, 4, 0, 1, false, Color.Lerp(color, Color.Black, 0.15f));
            }

        }

        private static int VertexAO(int side1, int side2, int corner)
        {
            if (side1 == 1 && side2 == 1)
            {
                return 3;
            }
            return (side1 + side2 + corner);
        }

        private static float AOLookUp(CUBE_FACES face, int x, int y, int z, int vertex)
        {
            int corner, side1, side2;
            if(face == CUBE_FACES.Top)
            {
                if(vertex == 4)
                {
                    corner = ChunkManager.GetBlock(x - 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x , y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 5)
                {
                    corner = ChunkManager.GetBlock(x + 1 , y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 =  ChunkManager.GetBlock(x + 1, y + 1, z ) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x , y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 6)
                {
                    corner = ChunkManager.GetBlock(x + 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    //if (corner == 1 )
                    //{
                    //    Renderer.AddDebugBox(new DebugBox(new Vector3(x + 0.5f + 1, y + 1 + 0.5f, z + 1 + 0.5f), Color.Red));
                    //    Renderer.AddDebugBox(new DebugBox(new Vector3(x + 0.5f + 1, y + 1 + 0.5f, z + 0.5f), Color.Purple));
                    //    Renderer.AddDebugBox(new DebugBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Color.Blue));
                    //}
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 7)
                {
                    corner = ChunkManager.GetBlock(x - 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
            }
            return 0;
        }

        private static void PushTriangle(int px, int py, int pz, int v1, int v2, int v3, Color color)
        {
            Vector3 position = new Vector3(px, py, pz);
            var entry = new VertexPositionColor(position + CUBE_VERTICES[v1], color);
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v2], color);
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v3], color);
            CurrentArray.Add(entry);
        }

        private static void PushTriangleAO(int px, int py, int pz, int v1, int v2, int v3, Color color, float a1, float a2, float a3)
        {
            Vector3 position = new Vector3(px, py, pz);
            var entry = new VertexPositionColor(position + CUBE_VERTICES[v1], Color.Lerp(color, Color.Black, a1 * .1f));
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v2], Color.Lerp(color, Color.Black, a2 * .1f));
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v3], Color.Lerp(color, Color.Black, a3 * .1f));
            CurrentArray.Add(entry);
        }

        private static void PushQuad(CUBE_FACES face, int x, int y, int z, int c1, int c2, int c3, int c4, bool inverted, Color color)
        {
            // Calculate AO in all corners and decide if inverted or not.
            float a00 = AOLookUp(face, x, y, z, c1);
            float a10 = AOLookUp(face, x, y, z, c2);
            float a11 = AOLookUp(face, x, y, z, c3);
            float a01 = AOLookUp(face, x, y, z, c4);

            if (a00 + a11 < a01 + a10)
            {
                PushTriangleAO(x, y, z, c1, c2, c3, color, a00, a10, a11);
                PushTriangleAO(x, y, z, c1, c3, c4, color, a00, a11, a01);
            }
            else
            {
                PushTriangleAO(x, y, z, c1, c2, c4, color, a00, a10, a01);
                PushTriangleAO(x, y, z, c2, c3, c4, color, a10, a11, a01);
            }
           
        }

        private static void PushVertex(SubChunk chunk, Vector3 position, int vertex, CUBE_FACES face, Color color)
        {
            //if (face != CUBE_FACES.Top)
            //{
            //    if(face == CUBE_FACES.Right)
            //        color = Color.Lerp(color, Color.Black, 0.25f);
            //    else if(face == CUBE_FACES.Left)
            //        color = Color.Lerp(color, Color.Black, 0.33f);
            //    else if(face == CUBE_FACES.Front)
            //        color = Color.Lerp(color, Color.Black, 0.15f);
            //    else if (face == CUBE_FACES.Back)
            //        color = Color.Lerp(color, Color.Black, 0.10f);
            //}
           
            switch (vertex)
            {
                case 0: // 0 0
                    int corner = ChunkManager.GetBlock(position + new Vector3(1, 0, 1)) != Blocks.Air ? 1 : 0;
                    int left   = ChunkManager.GetBlock(position - new Vector3(1, 0, 0)) != Blocks.Air ? 1 : 0;
                    int right  = ChunkManager.GetBlock(position - new Vector3(0, 0, 1)) != Blocks.Air ? 1 : 0;
                    color = Color.Lerp(color, Color.Red, VertexAO(left, right, corner) * 0.25f);
                    break;
                case 1: // 1 0
                    corner = ChunkManager.GetBlock(position - new Vector3(1, 0, 1)) != Blocks.Air ? 1 : 0;
                    left = ChunkManager.GetBlock(position - new Vector3(1, 0, 0)) != Blocks.Air ? 1 : 0;
                    right = ChunkManager.GetBlock(position - new Vector3(0, 0, 1)) != Blocks.Air ? 1 : 0;
                    color = Color.Lerp(color, Color.Red, VertexAO(left, right, corner) * 0.25f);
                    break;
                case 2: // 1 1
                    //corner = ChunkManager.GetBlock(position) != Blocks.Air ? 1 : 0;
                    //left   = ChunkManager.GetBlock(position) != Blocks.Air ? 1 : 0;
                    //right  = ChunkManager.GetBlock(position) != Blocks.Air ? 1 : 0;
                    //color = Color.Lerp(color, Color.Red, VertexAO(left, right, corner) * 0.25f);
                    break;
                case 3: // 0 1
                    //corner = ChunkManager.GetBlock(position - new Vector3(1, 0, 1)) != Blocks.Air ? 1 : 0;
                    //left = ChunkManager.GetBlock(position - new Vector3(1, 0, 0)) != Blocks.Air ? 1 : 0;
                    //right = ChunkManager.GetBlock(position - new Vector3(0, 0, 1)) != Blocks.Air ? 1 : 0;
                    //color = Color.Lerp(color, Color.Red, VertexAO(left, right, corner) * 0.25f);
                    break;
            }
            
            
            VertexPositionColor entry = new VertexPositionColor(position + CUBE_VERTICES[vertex], color);
            CurrentArray.Add(entry);
        }
    }
}
