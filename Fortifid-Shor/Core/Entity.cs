using System;
using Fortifid.Systems;
using Fortifid.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Fortifid.Core;

public abstract class Entity
{
    protected string _name;
    protected int _hp;
    protected int _maxHP;
    protected Vector2 _position;
    protected float _speed;
    protected Texture2D? _texture;
    
    public int Hp => _hp;
    public bool IsAlive => _hp > 0;
    public Vector2 Position => _position;
    
    public Vector2 Ceneter => _position + new Vector2(32f, 32f);

    protected Entity(string name, int maxHP, float speed)
    {
        _name = name;
        _maxHP = maxHP;
        _hp = maxHP;
        _speed = speed;
        _position = Vector2.Zero;
    }

    public abstract void Update(GameTime gameTime, WorldMap world);
    
    public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_texture == null) return;
        Vector2 screenPos = _position - camera.Position;
        spriteBatch.Draw(_texture, screenPos, Color.White);
    }

    public  void TakeDamage(int amount)
    {
        _hp = Math.Max(0, _hp - amount);
    }

    protected void Heal(int amount)
    {
        _hp = Math.Min(_maxHP, _hp + amount);
    }
}
