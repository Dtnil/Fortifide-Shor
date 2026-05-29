using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public class OreDeposit : MapObject
{
    public enum OreType {Copper, Iron}
    
    private readonly OreType _oreType;
    private readonly int _oreYield;
    private bool _depleted;
    
    private readonly Texture2D _oreItemTexture;
    private float _dropDisplayTime;

    public override string ResorurceName => _oreType == OreType.Copper ? "Мідна руда" : "Залізна руда";

    public OreDeposit(Texture2D rockTexture, Texture2D oreItemTexture,
        Vector2 worldPosition, int drawWidth, int drawHeight,
        OreType oreType, int oreYield = 2)
        : base(rockTexture, worldPosition, drawWidth, drawHeight)
    {
        _oreType = oreType;
        _oreYield=oreYield;
        _oreItemTexture = oreItemTexture;
        _depleted = false;
        _dropDisplayTime = 0f;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        Rectangle destRect = GetScreenRect(camera, WorldMap.Tile_Size);

        if (!_depleted)
        {
            Color tint = _oreType == OreType.Copper
                ? new Color(255, 220, 180)
                : new Color(200, 200, 210);
            spriteBatch.Draw(_texture, destRect, tint);
        }
        else if (_dropDisplayTime > 0)
        {
            Rectangle smallRect = new Rectangle(
                destRect.X + destRect.Width / 4, 
                destRect.Y + destRect.Height / 2, 
                destRect.Width / 2, 
                destRect.Height / 2
                );
            float alpha = MathHelper.Clamp(_dropDisplayTime / 2f, 0f, 1f);
            spriteBatch.Draw(_oreItemTexture, smallRect, Color.White * alpha);
            
        }
    }

    public override int Interact()
    {
        if (_depleted) return 0;
        _depleted = true;
        isAlive = false;
        _dropDisplayTime = 2f;
        return _oreYield;
    }

    public void Update(float DeltaTime)
    {
        if (_dropDisplayTime > 0)
            _dropDisplayTime -= DeltaTime;
    }
}