using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Fortifid.Systems;

namespace Fortifid.World;

public class WorldMap
{
    public const int Tile_Size = 80;
    
    private readonly Tile[,] _tiles;
    private readonly int _mapWidth;
    private readonly int _mapHeight;
    private readonly TextureManager _textures;
    
    public int MapWidth => _mapWidth;
    public int MapHeight => _mapHeight;
    public int Pixel_Width => _mapWidth * Tile_Size;
    public int Pixel_Height => _mapHeight * Tile_Size;

    public WorldMap(int mapWidth, int mapHeight,
        int seed, TextureManager textures)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _textures = textures;
        _tiles = MapGenerator.Ganerate();
    }

    public Tile? GetTile(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= _mapWidth) return null;
        if (tileY < 0 || tileY >= _mapHeight) return null;
        return _tiles[tileX, tileY];
    }

    public (int tileX, int tileY) WorldToTile(Vector2 worldPos)
    {
        return ((int)worldPos.X / Tile_Size, (int)worldPos.Y / Tile_Size);
    }

    public Vector2 FindSpawnPoint()
    {
        int cx = _mapWidth / 2;
        int cy = _mapHeight / 2;

        for (int r = 0; r < 30; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    int tx = cx + dx, ty = cy + dy;
                    var tile = GetTile(tx, ty);
                    if (tile?.Type == TileType.Grass && tile.Object == null)
                        return new Vector2(tx * Tile_Size, ty * Tile_Size);
                    
                }
            }
        }
        return new Vector2(cx * Tile_Size, cy * Tile_Size);
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        int startX = Math.Max(0, (int)(camera.Position.X / Tile_Size) - 1);
        int startY = Math.Max(0, (int)(camera.Position.Y / Tile_Size) - 1);
        int endX = Math.Min(_mapWidth, startX + camera.ViewWidth);
        int endY = Math.Min(_mapHeight, startY + camera.ViewHeight);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                var tile = _tiles[x, y];
                var teture = getGroundTexture(tile.Type);
                tile.Draw(spriteBatch, teture, camera, Tile_Size);
            }
        }

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                _tiles[x,y].Object?.Draw(spriteBatch, camera);
            }
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var tile in _tiles)
        {
            switch (tile.Object)
            {
                case null:
                    
                    break;
                case null:
                    
                    break;
            }
        }
    }


    private Texture2D getGroundTexture(TileType type)
    {
        return type switch
        {
            TileType.Water => _textures.Get("Water"),
            TileType.Shore => _textures.Get("Shore"),
            TileType.Sand => _textures.Get("Sand"),
            TileType.Grass => _textures.Get("Grass"),
            _ => _textures.Get("Grass")
        };
    }
}