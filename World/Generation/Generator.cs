using HelloMonoGame.Chunk;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Generation
{
    public static class Generator
    {
        private static FastNoise HeightMap;
        private static FastNoise Mountains;
        private static Random Random;
        public static void Initialize(int seed)
        {
            Random = new Random(seed);

            HeightMap = new FastNoise(seed);
            HeightMap.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            HeightMap.SetFrequency(0.000015f);
            HeightMap.SetFractalType(FastNoise.FractalType.RigidMulti);
            HeightMap.SetFractalOctaves(5);
            HeightMap.SetFractalLacunarity(2.0f);
            HeightMap.SetFractalGain(0.5f);

            Mountains = new FastNoise(seed);
            HeightMap.SetNoiseType(FastNoise.NoiseType.Simplex);
   
            HeightMap.SetFrequency(0.0015f);
        }


        public static void Generate(Chunk.Chunk chunk)
        {
            for (int x = 0; x < Chunk.SubChunk.WIDTH; x++)
            {
                for (int z = 0; z < Chunk.SubChunk.DEPTH; z++)
                {
                    float noiseHeight = (HeightMap.GetSimplexFractal(x + chunk.Position.X, z + chunk.Position.Z) + 1.0f);
                    float prev = 0;
                    int height = (int)(noiseHeight * 32f);

                    for (int y = height ; y < Chunk.Chunk.HEIGHT * SubChunk.HEIGHT; y++)
                    {
                        if (y == height - 1 && y < height)
                            chunk.AddBlock(x, y, z, Blocks.Grass);
                        else if (y > height - 5 && y < height)
                            chunk.AddBlock(x, y, z, Blocks.Dirt);

                        float noiseM = Mountains.GetSimplex(x + chunk.Position.X, y, z + chunk.Position.Z);
                        noiseM *= (float)Math.Tanh(Mountains.GetPerlin(x + chunk.Position.X, y, z + chunk.Position.Z));
                        noiseM -= (float)(y - height) / (float)((Chunk.Chunk.HEIGHT * SubChunk.HEIGHT) - height);
                        if ((noiseM + noiseHeight * 0.25f) > 0)
                        {
                            if(y == height || y == height + 1)
                                chunk.AddBlock(x, y, z, Blocks.Grass);
                            if (prev - 0.006f > noiseM)
                                chunk.AddBlock(x, y, z, Blocks.Grass);
                            else if (prev - 0.003f > noiseM)
                                chunk.AddBlock(x, y, z, Blocks.Dirt);
                            else
                            {
                                chunk.AddBlock(x, y, z, Blocks.Stone);
                            }
                        }
                            
                        prev = noiseM;
                    }

                }
            }
        }

    }
}
