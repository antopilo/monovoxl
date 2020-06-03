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
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1)
        };
        public static Vector3[] CUBE_NORMALS =
        {
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1), new Vector3(0, 0, -1)
        };

        private static Random rng = new Random();
        
        public static VertexPositionColor[] Mesh(SubChunk chunk)
        {
            var array = new List<VertexPositionColor>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (chunk.GetBlock(x, y, z) == Blocks.Air)
                            continue;

                        CreateBlock(x, y, z, chunk, array);
                    }
                }
            }
            return array.ToArray();
        }

        private static void CreateBlock(int x, int y, int z, SubChunk chunk, List<VertexPositionColor> array)
        {

            var gp = new Vector3(chunk.Parent.Position.X + x, 16 * chunk.Index + y, chunk.Parent.Position.Z + z);
            float gx = gp.X, gy = gp.Y, gz = gp.Z;

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

            // TODO: Check neighbors chunks
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

            // False if should not render chunk border faces.
            bool topBorder    = y == 15 ? topChunk    : top;
            bool bottomBorder = y == 0  ? bottomChunk : bottom;
            bool leftBorder   = x == 0  ? leftChunk   : left;
            bool rightBorder  = x == 15 ? rightChunk  : right;
            bool frontBorder  = z == 15 ? frontChunk  : front;
            bool backBorder   = z == 0  ? backChunk   : back;

            Color color = BlockManager.GetBlockColor(chunk.GetBlock(x, y, z), (int)gx, (int)gz);
            if (topBorder)
            {
                PushVertex(new Vector3(gx, gy, gz), 4, CUBE_FACES.Top, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Top, color, array);
                PushVertex(new Vector3(gx, gy, gz), 7, CUBE_FACES.Top, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Top, color, array);
                PushVertex(new Vector3(gx, gy, gz), 6, CUBE_FACES.Top, color, array);
                PushVertex(new Vector3(gx, gy, gz), 7, CUBE_FACES.Top, color, array);
            }                                                          
            if (bottomBorder)                                                
            {                                                          
                PushVertex(new Vector3(gx, gy, gz), 1, CUBE_FACES.Bottom, color, array);
                PushVertex(new Vector3(gx, gy, gz), 3, CUBE_FACES.Bottom, color, array);
                PushVertex(new Vector3(gx, gy, gz), 2, CUBE_FACES.Bottom, color, array);
                PushVertex(new Vector3(gx, gy, gz), 1, CUBE_FACES.Bottom, color, array);
                PushVertex(new Vector3(gx, gy, gz), 0, CUBE_FACES.Bottom, color, array);
                PushVertex(new Vector3(gx, gy, gz), 3, CUBE_FACES.Bottom, color, array);
            }                                                         
            if (leftBorder)                                                 
            {                                                         
                PushVertex(new Vector3(gx, gy, gz), 0, CUBE_FACES.Left, color, array);
                PushVertex(new Vector3(gx, gy, gz), 7, CUBE_FACES.Left, color, array);
                PushVertex(new Vector3(gx, gy, gz), 3, CUBE_FACES.Left, color, array);
                PushVertex(new Vector3(gx, gy, gz), 0, CUBE_FACES.Left, color, array);
                PushVertex(new Vector3(gx, gy, gz), 4, CUBE_FACES.Left, color, array);
                PushVertex(new Vector3(gx, gy, gz), 7, CUBE_FACES.Left, color, array);
            }                                                    
            if (rightBorder)                                           
            {                                                    
                PushVertex(new Vector3(gx, gy, gz), 2, CUBE_FACES.Right, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Right, color, array);
                PushVertex(new Vector3(gx, gy, gz), 1, CUBE_FACES.Right, color, array);
                PushVertex(new Vector3(gx, gy, gz), 2, CUBE_FACES.Right, color, array);
                PushVertex(new Vector3(gx, gy, gz), 6, CUBE_FACES.Right, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Right, color, array);
            }                                                         
            if (frontBorder)                                                
            {                                                         
                PushVertex(new Vector3(gx, gy, gz), 3, CUBE_FACES.Front, color, array);
                PushVertex(new Vector3(gx, gy, gz), 6, CUBE_FACES.Front, color, array);
                PushVertex(new Vector3(gx, gy, gz), 2, CUBE_FACES.Front, color, array);
                PushVertex(new Vector3(gx, gy, gz), 3, CUBE_FACES.Front, color, array);
                PushVertex(new Vector3(gx, gy, gz), 7, CUBE_FACES.Front, color, array);
                PushVertex(new Vector3(gx, gy, gz), 6, CUBE_FACES.Back, color, array);
            }                                                            
            if (backBorder)                                                    
            {                                                            
                PushVertex(new Vector3(gx, gy, gz), 0, CUBE_FACES.Back, color, array);
                PushVertex(new Vector3(gx, gy, gz), 1, CUBE_FACES.Back, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Back, color, array);
                PushVertex(new Vector3(gx, gy, gz), 5, CUBE_FACES.Back, color, array);
                PushVertex(new Vector3(gx, gy, gz), 4, CUBE_FACES.Back, color, array);
                PushVertex(new Vector3(gx, gy, gz), 0, CUBE_FACES.Back, color, array);
            }

        }

        private static void PushVertex(Vector3 position, int vertex, CUBE_FACES face, Color color, List<VertexPositionColor> array)
        {
            if (face != CUBE_FACES.Top)
            {
                if(face == CUBE_FACES.Right)
                    color = Color.Lerp(color, Color.Black, 0.25f);
                else if(face == CUBE_FACES.Left)
                    color = Color.Lerp(color, Color.Black, 0.33f);
                else if(face == CUBE_FACES.Front)
                    color = Color.Lerp(color, Color.Black, 0.15f);
                else if (face == CUBE_FACES.Back)
                    color = Color.Lerp(color, Color.Black, 0.10f);
            }
            
            VertexPositionColor entry = new VertexPositionColor(position + CUBE_VERTICES[vertex], color);
            array.Add(entry);
        }
    }
}
