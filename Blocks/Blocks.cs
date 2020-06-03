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
        int cx = gx / 16;
        int cz = gz / 16;
        int x = gx - (16 * cx);
        int z = gz - (16 * cz);

        var xColor = Color.Lerp(Color.Red, Color.Blue, x / 16f);
        return Color.Lerp(xColor, Color.Green, z / 16f);

        //if (type == Blocks.Dirt)
        //    return Color.SandyBrown;
        //if (type == Blocks.Grass)
        //    return Color.Lerp(Color.GreenYellow, Color.Yellow, TemperatureMap.GetSimplexFractal(gx, gz));
        //if (type == Blocks.Stone)
        //    return Color.SlateGray;

        return Color.SlateGray;
    }
}

