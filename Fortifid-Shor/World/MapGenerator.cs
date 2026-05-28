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
    private const float Threshold_Grass = 0.44f;
    
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
        int tileSize = WorldMap.TILE_SIZE;
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
        PlaceObjects(tiles, mapWidth, mapHeight, heightMap, textures);
        return tiles; 
    }
        private static float[,] BuildHeightMap(int w, int h, System.Random rng)
        {
            float[,] Map = new float[w, h];
            
            
        }
        
        private static TileType HightToTileType(float height)
        {
            if (height < Threshold_Water) return  TileType.Water;
            if (height < Threshold_Shore) return  TileType.Grass;
            if (height < Threshold_Grass) return  TileType.Grass;
            return   TileType.Grass;
        }
}