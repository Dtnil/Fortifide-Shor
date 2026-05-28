using System.CodeDom.Compiler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public class MapGenerator
{

    private const float Threshold_Water = 0.30f;
    private const float Threshold_Shore = 0.38f;
    private const float Threshold_Grass = 0.44f;
    
    private const float Tree_Density = 0.22f;
    private const float Rock_Density = 0.08f;
    private const float Ore_Density = 0.04f;
    private const float Sprout_Density = 0.06f;

    private static int _seed;
    
    public static int Tile[,] Generate()
}