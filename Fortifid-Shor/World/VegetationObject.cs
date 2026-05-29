using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public class VegetationObject : MapObject
{
    private readonly int _woodYield;
    private readonly float _opacity;
    private float _fallTimer;

    public override string ResorurceName => "Деревина";

    public VegetationObject(Texture2D texture, Vector2 worldPosition,
        int drawWidth, int drawHeight,
        int woodYield = 3) : base(texture, worldPosition, drawWidth, drawHeight)
    {
        _woodYield = woodYield;
        _opacity = 1.0f;
        _fallTimer = 0.0f;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (!isAlive && _fallTimer <= 0) return;
        
        Rectangle destRect = GetScreenRect(camera, WorldMap.Tile_Size);

        float alpha = isAlive ? 1f : MathHelper.Clamp(_fallTimer / 0.5f, 0f, 1f);
        Color color = Color.White * alpha;
        
        spriteBatch.Draw(_texture, destRect, color);
    }

    public override int Interact()
    {
        if (!isAlive) return 0;
        isAlive = false;
        _fallTimer = 0.5f;
        return _woodYield;
    }

    public void Update(float deltaTime)
    {
        if (_fallTimer > 0)
            _fallTimer -= deltaTime;
    }
}