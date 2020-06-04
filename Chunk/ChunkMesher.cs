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
                // 00 10 01 11
                //PushQuad(gp, new Vector4(4, 5, 7, 6), color);
                PushQuad(gx, gy, gz, 4, 5, 6, 7, false, color);
                //PushTriangle(gx, gy, gz, 4, 5, 7, color, CUBE_FACES.Top);
                //PushTriangle(gx, gy, gz, 5, 6, 7, color, CUBE_FACES.Top);
            }                                                          
            if (bottomBorder)                                                
            {
                //PushQuad(gp, new Vector4(1, 2, 0), color, array);
                PushQuad(gx, gy, gz, 3, 2, 1, 0, false, color);
                //PushTriangle(gx, gy, gz, 1, 3, 2, color, CUBE_FACES.Bottom);
                //PushTriangle(gx, gy, gz, 1, 0, 3, color, CUBE_FACES.Bottom);
            }                                                  
            if (leftBorder)                                          
            {
                PushQuad(gx, gy, gz, 0, 4, 7, 3, false, color);
                //PushTriangle(gx, gy, gz, 0, 7, 3, Color.Lerp(color, Color.Black, 0.1f), CUBE_FACES.Left);
                //PushTriangle(gx, gy, gz, 0, 4, 7, Color.Lerp(color, Color.Black, 0.1f), CUBE_FACES.Left);
            }                                               
            if (rightBorder)                                    
            {
                PushQuad(gx, gy, gz, 1, 2, 6, 5, false, color);
                //PushTriangle(gx, gy, gz, 2, 5, 1, Color.Lerp(color, Color.Black, 0.15f), CUBE_FACES.Right);
                //PushTriangle(gx, gy, gz, 2, 6, 5, Color.Lerp(color, Color.Black, 0.15f), CUBE_FACES.Right);
            }                                                  
            if (frontBorder)                                         
            {
                PushQuad(gx, gy, gz, 2, 3, 7, 6, false, color);
                //PushTriangle(gx, gy, gz, 3, 6, 2, Color.Lerp(color, Color.Black, 0.20f), CUBE_FACES.Front);
                //PushTriangle(gx, gy, gz, 3, 7, 6, Color.Lerp(color, Color.Black, 0.20f), CUBE_FACES.Front);
            }                                                     
            if (backBorder)                                             
            {
                PushQuad(gx, gy, gz, 5, 4, 0, 1, false, color);
                //PushTriangle(gx, gy, gz, 0, 1, 5, Color.Lerp(color, Color.Black, 0.2f), CUBE_FACES.Back);
                //PushTriangle(gx, gy, gz, 5, 4, 0, Color.Lerp(color, Color.Black, 0.2f), CUBE_FACES.Back);
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

        private static Color AOLookUp(int x, int y, int z, CUBE_FACES face, int vertex, Color color)
        {   
            switch (face)
            {
                case CUBE_FACES.Top:
                    if (vertex == 4) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 5) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 6) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 7) 
                    {

                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, 1, y + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    } 
                    break;
                case CUBE_FACES.Bottom:
                    if (vertex == 0) 
                    {
                       int corner = ChunkManager.GetBlock(x - 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left   = ChunkManager.GetBlock(x - 1, y, z    ) == Blocks.Air ? 0 : 1;
                        int right  = ChunkManager.GetBlock(x,     y, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 1) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 3) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 2) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    break;
                case CUBE_FACES.Left:
                    if(vertex == 0) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 3) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 4) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 7) 
                    {

                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, 1, y + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    //color = Color.Lerp(color, Color.Black, 0.1f);
                    break;
                case CUBE_FACES.Right:
                    if(vertex == 1) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 2) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 5) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 6) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    //color = Color.Lerp(color, Color.Black, 0.15f);
                    break;
                case CUBE_FACES.Front:
                    if (vertex == 2) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 3) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 6) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 7) 
                    {

                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z + 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, 1, y + 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    //color = Color.Lerp(color, Color.Black, 0.20f);
                    break;
                case CUBE_FACES.Back:
                    if (vertex == 0) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x - 1, y, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 1) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.1f);
                    }
                    if (vertex == 4) 
                    {
                        int corner = ChunkManager.GetBlock(x - 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x - 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    if (vertex == 5) 
                    {
                        int corner = ChunkManager.GetBlock(x + 1, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int left = ChunkManager.GetBlock(x, y + 1, z - 1) == Blocks.Air ? 0 : 1;
                        int right = ChunkManager.GetBlock(x + 1, y + 1, z) == Blocks.Air ? 0 : 1;
                        color = Color.Lerp(color, Color.Black, VertexAO(left, right, corner) * 0.25f);
                    }
                    //color = Color.Lerp(color, Color.Black, 0.25f);
                    break;
                default:
                    break;  
            } 

            return color;
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

        private static void PushQuad(int x, int y, int z, int c1, int c2, int c3, int c4,bool inverted, Color color)
        {
            if (inverted)
            {
                PushTriangle(x, y, z, c1, c2, c3, color);
                PushTriangle(x, y, z, c1, c3, c4, color);
            }
            else
            {
                PushTriangle(x, y, z, c1, c2, c4, color);
                PushTriangle(x, y, z, c2, c3, c4, color);
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
