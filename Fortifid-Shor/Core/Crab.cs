using Fortifid.Systems;
using Fortifid.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Fortifid.Core;

public class Crab : Enemy
{
    private readonly int _frameCount;
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private int _currentFrame;
    private float _frameTimer;
    private const  float Frame_Duration = 0.12f;

    private const int Draw_Size = 72;

    public Crab(Texture2D texture, Vector2 spawnPosition, int frameCount = 4)
        : base("Краб", maxHP: 30, speed: 90f, damage: 8, detectionRange: 200f)
    {
        _texture = texture;
        _position = spawnPosition;
        _frameCount = frameCount;
        _frameWidth = texture.Width / frameCount;
        _frameHeight = texture.Height;
        _currentFrame = 0;
        _frameTimer = 0f;
    }

    public override void Update(GameTime gameTime, WorldMap world)
    {
        if (!IsAlive) return;
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        UpdateAnimation(dt);
        
        _attackTimer = MathHelper.Max(0f, _attackTimer - dt);
    }

    public void Update(GameTime gameTime, WorldMap world, Player player)
    {
        if (!IsAlive) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _attackTimer = MathHelper.Max(0f, _attackTimer - dt);

        if (CanSeePlayer(player))
        {
            if (IsInAttackRange(player))
            {
                if (_attackTimer <= 0f)
                    AttackPlayer(player);
            }
            else
            {
                MoveTowards(player.Position, dt);
            }
        }
        
        
        
    }

    public override void AttackPlayer(Player player)
    {
        player.TakeDamage(_damage);
        _attackTimer = _attackCooldown;
        System.Console.WriteLine($"[{_name}] атакує гравця на {_damage} шкоди!");
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if(!IsAlive||_texture == null) return;
        
        Vector2 position = _position - camera.Position;
        
        var sourceRect = new Rectangle(_currentFrame * _frameWidth, 
            0,
            _frameWidth, 
            _frameHeight
            );
        var destRect = new Rectangle(
            (int)position.X,
            (int)position.Y,
            Draw_Size,
            Draw_Size
            );
        spriteBatch.Draw(_texture, destRect, sourceRect, Color.White);
    }

    private void UpdateAnimation(float dt)
    {
        _frameTimer += dt;
        if (_frameTimer >= Frame_Duration)
        {
            _frameTimer = 0f;
            _currentFrame = (_currentFrame + 1) % _frameCount;
        }
    }
}