using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public abstract class MapObject
{
    protected Texture2D _texture;
    protected Vector2 _worldPosition;
    protected int _drawWidth;
    protected int _drawHeight;

    public bool isAlive { get; protected set; } = true;

    public abstract string ResorurceName { get; }

    protected MapObject(Texture2D texture, Vector2 worldPosition, int drawWidth, int drawHeight)
    {
        _texture = texture;
        _worldPosition = worldPosition;
        _drawWidth = drawWidth;
        _drawHeight = drawHeight;
    }
    public abstract void Draw(SpriteBatch spriteBatch, Camera camera);

    public abstract int Interact();

    protected Rectangle GetScreenRect(Camera camera, int tileSize)
    {
        Vector2 screen = _worldPosition - camera.Position;
        return new Rectangle(
            (int)screen.X + tileSize / 2 - _drawWidth / 2,
            (int)screen.Y + tileSize - _drawHeight,
            _drawWidth,
            _drawHeight
        );
    }
}