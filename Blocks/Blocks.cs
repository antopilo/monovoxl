using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum Blocks
{
    Air,
    Stone,
    Grass,
    Dirt
}

public static class BlockManager
{
    public static FastNoise TemperatureMap;

    public static void Initialize()
    {
        TemperatureMap = new FastNoise(1337);
        TemperatureMap.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        TemperatureMap.SetFrequency(0.0025f);
        TemperatureMap.SetFractalType(FastNoise.FractalType.FBM);
        TemperatureMap.SetFractalOctaves(5);
        TemperatureMap.SetFractalLacunarity(2f);
        TemperatureMap.SetFractalGain(2f);
    }

    public static Color GetBlockColor(Blocks type, int gx, int gz) 
    {
        if (type == Blocks.Dirt)
            return Color.SandyBrown;
        if (type == Blocks.Grass)
            return Color.Lerp(Color.GreenYellow, Color.Yellow, TemperatureMap.GetSimplexFractal(gx, gz));
        if (type == Blocks.Stone)
            return Color.SlateGray;

        return Color.SlateGray;
    }
}

