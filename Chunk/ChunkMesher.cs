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

        private static List<TimeSpan> Average = new List<TimeSpan>();
        public static VertexPositionColor[] Mesh(SubChunk chunk)
        {
            


            if (chunk.GetCount() == 0)
                return new VertexPositionColor[] { };

            List<VertexPositionColor> CurrentArray = new List<VertexPositionColor>();
            for (int x = 0; x < SubChunk.WIDTH; x++)
            {
                for (int y = 0; y < SubChunk.HEIGHT; y++)
                {
                    for (int z = 0; z < SubChunk.DEPTH; z++)
                    {
                        Blocks type = chunk.GetBlock(x, y, z);
                        if (type == Blocks.Air)
                            continue;
            
                        CreateBlock(x, y, z, chunk, type, CurrentArray);
                    }
                }
            }
            //for (int i = 0; i < SubChunk.FULL_COUNT; i++)
            //{
            //    Blocks type = chunk.GetBlock(i);
            //    if (type == Blocks.Air)
            //        continue;
            //
            //
            //    int z = (i >> 8) & 0xF;
            //    int y = (i >> 4) & 0xF;
            //    int x = i & 0xF;
            //    CreateBlock(x, y, z, chunk, type, CurrentArray);
            //}

            
            return CurrentArray.ToArray();
        }

        private static void CreateBlock(int x, int y, int z, SubChunk chunk, Blocks type, List<VertexPositionColor> CurrentArray)
        {
            int gx = (int)chunk.Parent.Position.X + x;
            int gy = SubChunk.HEIGHT * chunk.Index + y;
            int gz = (int)chunk.Parent.Position.Z + z;

            bool top, bottom, left, right, front, back;
            top    = y != SubChunk.HEIGHT - 1 ? chunk.GetBlock(x, y + 1, z) == Blocks.Air : true;
            bottom = y != 0  ? chunk.GetBlock(x, y - 1, z) == Blocks.Air : true;
            left   = x != 0  ? chunk.GetBlock(x - 1, y, z) == Blocks.Air : true;
            right  = x != SubChunk.WIDTH - 1 ? chunk.GetBlock(x + 1, y, z) == Blocks.Air : true;
            front  = z != SubChunk.DEPTH - 1 ? chunk.GetBlock(x, y, z + 1) == Blocks.Air : true;
            back   = z != 0  ? chunk.GetBlock(x, y, z - 1) == Blocks.Air : true;

            // Block is surrounded
            if (top && bottom && left && right && front && back)
                return;

            bool topChunk = true;
            bool bottomChunk = true;
            bool leftChunk = true;
            bool rightChunk = true;
            bool backChunk = true;
            bool frontChunk = true;

            if (x == 0)
                leftChunk = chunk.Parent.Left.GetSubChunk(chunk.Index).GetBlock(SubChunk.DEPTH - 1, y, z) == Blocks.Air;
            if (x == SubChunk.WIDTH - 1)
                rightChunk = chunk.Parent.Right.GetSubChunk(chunk.Index).GetBlock(0, y, z) == Blocks.Air;
            if (z == 0)
                backChunk = chunk.Parent.Back.GetSubChunk(chunk.Index).GetBlock(x, y, SubChunk.DEPTH - 1) == Blocks.Air;
            if (z == SubChunk.DEPTH - 1)
                frontChunk = chunk.Parent.Front.GetSubChunk(chunk.Index).GetBlock(x, y, 0) == Blocks.Air;

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
            bool rightBorder  = x == SubChunk.WIDTH - 1 ? rightChunk  : right;
            bool frontBorder  = z == SubChunk.DEPTH - 1 ? frontChunk  : front;
            bool backBorder   = z == 0  ? backChunk   : back;

            Color color = BlockManager.GetBlockColor(type, gx, gz);
            if (topBorder)
            {
                PushQuadAO(CUBE_FACES.Top, gx, gy, gz, 4, 5, 6, 7,  color, CurrentArray);
            }                                                    
            if (bottomBorder)                                          
            {
                PushQuadAO(CUBE_FACES.Bottom, gx, gy, gz, 3, 2, 1, 0,  color, CurrentArray);
            }                                            
            if (leftBorder)                                    
            {
                PushQuadAO(CUBE_FACES.Left, gx, gy, gz, 0, 4, 7, 3,  Color.Lerp(color, Color.Black, 0.1f), CurrentArray);
            }                                               
            if (rightBorder)                                    
            {
                PushQuadAO(CUBE_FACES.Right,gx, gy, gz, 1, 2, 6, 5, Color.Lerp(color, Color.Black, 0.15f), CurrentArray);
            }                                                  
            if (frontBorder)                                         
            {
                PushQuadAO(CUBE_FACES.Front, gx, gy, gz, 2, 3, 7, 6, Color.Lerp(color, Color.Black, 0.1f), CurrentArray);
            }                                              
            if (backBorder)                                      
            {
                PushQuadAO(CUBE_FACES.Back, gx, gy, gz, 5, 4, 0, 1, Color.Lerp(color, Color.Black, 0.15f), CurrentArray);
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
            if(face != CUBE_FACES.Top && face != CUBE_FACES.Bottom)
            {
                if (vertex == 0)
                {
                    corner = ChunkManager.GetBlock(x - 1, y  , z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y , z - 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;

                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 1) 
                {
                    corner = ChunkManager.GetBlock(x + 1, y , z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x + 1, y , z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 2) 
                {
                    corner = ChunkManager.GetBlock(x + 1, y , z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y , z + 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x + 1, y , z) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 3)
                {
                    corner = ChunkManager.GetBlock(x - 1, y , z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x - 1, y , z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y , z + 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 4)
                {
                    corner = ChunkManager.GetBlock(x - 1, y , z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y , z - 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x - 1, y , z) == Blocks.Air ? 0 : 1;

                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 5)
                {
                    corner = ChunkManager.GetBlock(x + 1, y, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 6) 
                {
                    corner = ChunkManager.GetBlock(x + 1, y, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y , z + 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x + 1, y , z) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 7)
                {
                    corner = ChunkManager.GetBlock(x - 1, y , z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x - 1, y , z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }

            }
            else
            {
                if (vertex == 4)
                {

                    corner = ChunkManager.GetBlock(x - 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;

                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 5)
                {
                    corner = ChunkManager.GetBlock(x + 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 6)
                {
                    corner = ChunkManager.GetBlock(x + 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 7)
                {
                    corner = ChunkManager.GetBlock(x - 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 0)
                {
                    corner = ChunkManager.GetBlock(x - 1, y - 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y - 1, z - 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x - 1, y - 1, z) == Blocks.Air ? 0 : 1;

                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 1)
                {
                    corner = ChunkManager.GetBlock(x + 1, y - 1, z - 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x + 1, y - 1, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y - 1, z - 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 2)
                {
                    corner = ChunkManager.GetBlock(x + 1, y - 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x, y - 1, z + 1) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x + 1, y - 1, z) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
                if (vertex == 3)
                {
                    corner = ChunkManager.GetBlock(x - 1, y - 1, z + 1) == Blocks.Air ? 0 : 1;
                    side1 = ChunkManager.GetBlock(x - 1, y - 1, z) == Blocks.Air ? 0 : 1;
                    side2 = ChunkManager.GetBlock(x, y - 1, z + 1) == Blocks.Air ? 0 : 1;
                    return VertexAO(side1, side2, corner);
                }
            }
            return 0;
        }

        private static void PushTriangle(int px, int py, int pz, int v1, int v2, int v3, Color color, List<VertexPositionColor> CurrentArray)
        {
            Vector3 position = new Vector3(px, py, pz);
            var entry = new VertexPositionColor(position + CUBE_VERTICES[v1], color);
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v2], color);
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v3], color);
            CurrentArray.Add(entry);
        }

        public static List<VertexPositionColor> GetCubeMesh(Color color, Vector3 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            int z = (int)position.Z;

            var CurrentArray = new List<VertexPositionColor>();
            PushQuad( x, y, z, 4, 5, 6, 7, color, CurrentArray);
            PushQuad( x, y, z, 3, 2, 1, 0, color, CurrentArray);
            PushQuad( x, y, z, 0, 4, 7, 3, Color.Lerp(color, Color.Black, 0.1f), CurrentArray);
            PushQuad( x, y, z, 1, 2, 6, 5, Color.Lerp(color, Color.Black, 0.15f), CurrentArray);
            PushQuad( x, y, z, 2, 3, 7, 6, Color.Lerp(color, Color.Black, 0.1f), CurrentArray);
            PushQuad( x, y, z, 5, 4, 0, 1, Color.Lerp(color, Color.Black, 0.15f), CurrentArray);
            return CurrentArray;
        }

        private static void PushTriangleAO(int px, int py, int pz, int v1, int v2, int v3, Color color, float a1, float a2, float a3, List<VertexPositionColor> CurrentArray)
        {
            Vector3 position = new Vector3(px, py, pz);
            var entry = new VertexPositionColor(position + CUBE_VERTICES[v1], Color.Lerp(color, Color.Black, a1 * .05f));
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v2], Color.Lerp(color, Color.Black, a2 * .05f));
            CurrentArray.Add(entry);
            entry = new VertexPositionColor(position + CUBE_VERTICES[v3], Color.Lerp(color, Color.Black, a3 * .05f));
            CurrentArray.Add(entry);
        }

        private static void PushQuad(int x, int y, int z, int c1, int c2, int c3, int c4, Color color, List<VertexPositionColor> CurrentArray)
        {
            PushTriangle(x, y, z, c1, c2, c3, color, CurrentArray);
            PushTriangle(x, y, z, c1, c3, c4, color, CurrentArray);
        }

        private static void PushQuadAO(CUBE_FACES face, int x, int y, int z, int c1, int c2, int c3, int c4, Color color, List<VertexPositionColor> CurrentArray)
        {
            // Calculate AO in all corners and decide if inverted or not.

            float a00 = AOLookUp(face, x, y, z, c1);
            float a10 = AOLookUp(face, x, y, z, c2);
            float a11 = AOLookUp(face, x, y, z, c3);
            float a01 = AOLookUp(face, x, y, z, c4);

            if (a00 + a11 < a01 + a10)
            {
                PushTriangleAO(x, y, z, c1, c2, c3, color, a00, a10, a11, CurrentArray);
                PushTriangleAO(x, y, z, c1, c3, c4, color, a00, a11, a01, CurrentArray);
            }
            else
            {
                PushTriangleAO(x, y, z, c1, c2, c4, color, a00, a10, a01, CurrentArray);
                PushTriangleAO(x, y, z, c2, c3, c4, color, a10, a11, a01, CurrentArray);
            }
           
        }
    }
}
