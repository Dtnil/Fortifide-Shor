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
    
    public Camera
}