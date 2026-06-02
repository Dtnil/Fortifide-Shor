using System;
using System.CodeDom.Compiler;
using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public static class MapGenerator
{

    private const float Threshold_Water = 0.30f;
    private const float Threshold_Shore = 0.38f;
    private const float Threshold_Sand = 0.44f;
    
    private const float Tree_Density = 0.22f;
    private const float Rock_Density = 0.08f;
    private const float Ore_Density = 0.04f;
    private const float Sprout_Density = 0.06f;

    private static int _seed;
    
    public static Tile[,] Generate (
        int mapWidth, int mapHeight,
        int seed, 
        TextureManager textures)
    {
        _seed = seed == 0 ? System.Environment.TickCount : seed;
        var rng = new System.Random(_seed);
        var tiles = new Tile[mapWidth, mapHeight];
        
        float[,] heightMap = BuildHeightMap(mapWidth, mapHeight, rng);
        
        int tileSize = WorldMap.Tile_Size;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float h = heightMap[x, y];
                TileType t = HightToTileType(h);
                var pos = new Vector2(x * tileSize, y * tileSize);
                tiles[x, y] = new Tile(t, pos);
            }
        }
        PlaceObjects(tiles, mapWidth, mapHeight, heightMap, textures, rng);
        
        return tiles;
    }

    private static float[,] BuildHeightMap(int w, int h, System.Random rng)
    {
        float[,] map = new float[w, h];

        float offsetX = (float)(rng.NextDouble() * 1000);
        float offsetY = (float)(rng.NextDouble() * 1000);

        float scale = 0.04f;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float noiseVal = FractionalBrowianMotion(
                    (x + offsetX) * scale,
                    (y + offsetY) * scale,
                    octaves: 4
                    );
                float islandMask = ComputeIslandMask(x, y, w, h);
                
                map[x, y] = MathHelper.Clamp(noiseVal * islandMask, 0f, 1f);
            }
        }
        return map;
    }

    private static float ComputeIslandMask(int x, int y, int w, int h)
    {
        float nx = (x /  (float)w) *2 - 1;
        float ny = (y /  (float)h) *2 - 1;
        
        float dist = MathF.Sqrt(nx * nx + ny * ny);
        
        float mask = 1f - MathHelper.Clamp(dist * 1.4f, 0f, 1f);
        return mask*mask;
    }

    private static float FractionalBrowianMotion(float x, float y, int octaves)
    {
        float value = 0f;
        float amplitude = 0.5f;
        float frequency = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            value += SmoothNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude*=0.5f;
            frequency*=2f;
        }
        
        return value / maxValue;
    }

    private static float SmoothNoise(float x, float y)
    {
        int ix = (int)MathF.Floor(x);
        int iy = (int)MathF.Floor(y);
        float fx = x - ix;
        float fy = y - iy;

        float ux = fx * fx * (3f - 2f * fx);
        float uy = fy * fy * (3f - 2f * fy);

        float n00 = ValueNoise(ix, iy);
        float n10 = ValueNoise(ix + 1, iy);
        float n01 = ValueNoise(ix, iy + 1);
        float n11 = ValueNoise(ix + 1, iy + 1);
        
        return MathHelper.Lerp(
            MathHelper.Lerp(n00, n10, ux), 
            MathHelper.Lerp(n01, n11, ux), 
            uy);
    }

    private static float ValueNoise(int x, int y)
    {
        int n = (x * 1619 + y * 31337 + _seed * 1013) & 0x7FFFFFFF;
        n = (n >> 13) ^ n;
        int m = (n*(n*n*60493 + 19990303) + 1376312589) & 0x7FFFFFFF;
        return m / (float)0x7FFFFFFF;
    }

    private static TileType HightToTileType(float height)
    {
        if (height < Threshold_Water) return  TileType.Water;
        if (height < Threshold_Shore) return  TileType.Shore;
        if (height < Threshold_Sand) return  TileType.Sand;
        return   TileType.Grass;
    }

    private static void PlaceObjects(Tile[,] tiles,
        int w, int h, float[,] heightMap,
        TextureManager textures, System.Random rng)
    {
        int ts = WorldMap.Tile_Size;
        
        var treeTextures = textures.GetTreeTextures();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (tiles[x, y].Type != TileType.Grass) continue;
                
                float roll = (float)rng.NextDouble();
                Vector2 pos = new Vector2(x * ts, y * ts);

                MapObject? obj = null;

                if (roll < Ore_Density)
                {
                    bool isCopper = rng.NextDouble() < 0.5;
                    obj = isCopper
                        ? new OreDeposit(
                            textures.Get("Coper_rock"), textures.Get("Coper_ore"),
                            pos, ts, (int)(ts * 1.1f),
                            OreDeposit.OreType.Copper, oreYield: 2)
                        : new OreDeposit(
                            textures.Get("Iron_rock"), textures.Get("Iron_ore"),
                            pos, ts, (int)(ts * 1.1f),
                            OreDeposit.OreType.Iron, oreYield: 2);
                }
                else if(roll < Ore_Density + Rock_Density)
                {
                    float h2 = heightMap[x, y];
                    string rockKey = h2 > 0.75f ? "Rock_granit"
                        : h2 > 0.65f ? "Rock_big"
                            : h2 > 0.55f ? "Rock_mid"
                            :"Rock_small";
                    int yield = rockKey == "Rock_granit" ? 5 :
                        rockKey == "Rock_big" ? 4 :
                        rockKey == "Rock_mid" ? 2 : 1;
                    obj = new RockObject(
                        textures.Get(rockKey), pos,
                        ts, (int)(ts * 0.9f),
                        stoneYield: yield, maxHits: yield);
                }
                else if (roll < Ore_Density+Rock_Density + Sprout_Density)
                {
                    obj = new VegetationObject(
                        textures.Get("Sprout"), pos,
                        (int)(ts * 0.5f), (int)(ts * 0.6f),
                        woodYield: 1);
                }
                else if (roll < Ore_Density + Rock_Density + Sprout_Density + Tree_Density)
                {
                    bool isPalm = rng.NextDouble() < 0.15;
                    var tex = isPalm
                        ? textures.Get("Palm")
                        : treeTextures[rng.Next(treeTextures.Count)];
                    int w2 = isPalm ? (int)(ts * 1.1f) : (int)(ts * 1.2f);
                    int h3 = isPalm ? (int)(ts * 1.8f) : (int)(ts * 2.0f);
                    obj = new VegetationObject(tex, pos, w2, h3, woodYield: isPalm ? 2 : 3);
                }
                if (obj != null)
                    tiles[x, y].PlaceObject(obj);
            }
        }
        
    }
}