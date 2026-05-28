using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.Systems;

public class Camera
{
    private Vector2 _position;
    
    private readonly int _viewWidth;
    
    private readonly int _viewHeight;
    
    private readonly int _worldPixelWidth;
    
    private readonly int _worldPixelHeight;
    
    public Vector2 Position =>_position;
    
    public int ViewWidth => _viewWidth;
    
    public int ViewHeight => _viewHeight;

    public Camera(int viewWidth, int viewHeight, int worldPixelWidth, int worldPixelHeight)
    {
        _viewWidth = viewWidth;
        _viewHeight = viewHeight;
        _worldPixelWidth = worldPixelWidth;
        _worldPixelHeight = worldPixelHeight;
        _position = Vector2.Zero;
    }
    public void Follow(Vector2 position)
    {
        float desiredX = targetCenter.X - _viewWidth / 2;
        float desiredY = targetCenter.Y - _viewHeight / 2;
        
        _position.X = MathHelper.Clamp(desiredX, 0, _worldPixelWidth);
        _position.Y = MathHelper.Clamp(desiredY, 0, _worldPixelHeight);
    }
}