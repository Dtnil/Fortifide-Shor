using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fortifid.World;

public class RockObject : MapObject
{
    private readonly int _stoneYield;
    private int _maxHits;
    private int _hitsLeft;

    public override string ResorurceName => "Камінь";

    public RockObject(
        Texture2D texture, Vector2 worldPosition,
        int drawWidth, int drawHeight,
        int stoneYield = 2, int maxHits = 3)
        : base(texture, worldPosition, drawWidth, drawHeight)
    {
        _stoneYield = stoneYield;
        _maxHits = maxHits;
        _hitsLeft = maxHits;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (!IsAlive) return;

        Rectangle destRect = GetScreenRect(camera, WorldMap.Tile_Size);
        
        float damage = 1f - (float)_hitsLeft / _maxHits;
        Color tint = new Color(
            1f - damage * 0.3f, 
            1f - damage * 0.3f, 
            1f - damage * 0.3f
            );
        spriteBatch.Draw(_texture, destRect, tint);
    }

    public override int Interact()
    {
        if(!isAlive) return 0;

        _hitsLeft--;
        if (_hitsLeft <= 0)
        {
            isAlive = false;
            return _stoneYield;
        }
        return 0;
    }
}