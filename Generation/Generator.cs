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

        public static void Initialize(int seed)
        {
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
                    float noise = HeightMap.GetSimplexFractal(x + chunk.Position.X, z + chunk.Position.Z) + 1.0f;
                    int height = (int)(noise * 32f);
                    for (int i = height - 1; i < height; i++)
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
        }

    }
}
