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
    
    public static int Tile[,] Generate(int mapWidth, int mapHeight,
        int seed, TextureManager textures)
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
                    (y + offsetY) * scale
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
        float u = x - ix;
        float v = y - iy;

        float ux = fx * fx * (3f - 2f * fx);
        float uy = fy * fx * (3f - 2f * fy);

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
        
    }
}