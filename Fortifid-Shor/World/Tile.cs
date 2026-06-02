using System.Security.AccessControl;
using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Fortifid.World;

public class Tile
{
    private readonly TileType _type;
    private readonly Vector2 _Worldposition;
    private MapObject? _object;
    
    public TileType Type => _type;
    public Vector2 Worldposition => _Worldposition;
    public MapObject? Object => _object;
    
    public bool IsWalkable => _type != TileType.Water && _object == null;

    public Tile(TileType type, Vector2 worldposition)
    {
        _type = type;
        _Worldposition = worldposition;
        _object = null;
    }

    public void PlaceObject(MapObject obj)
    {
        _object = obj;
    }

    public void RemoveObject()
    {
        _object = null;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture, Camera camera, int tileSize)
    {
        Vector2 screenPos = _Worldposition - camera.Position;
        var destRect = new Rectangle(
            (int)screenPos.X, 
            (int)screenPos.Y, 
            tileSize, tileSize
            );
        spriteBatch.Draw(texture, destRect, Color.White);
    }
}