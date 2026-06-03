using Fortifid.Systems;
using XnaRect = Microsoft.Xna.Framework.Rectangle;
using Fortifid.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Fortifid.Core;

public class Player : Entity
{
    private float _hunger = 100f;
    private float _thirst = 100f;

    private const float Hunger_Decay = 1.5f;
    private const float Thirst_Decay = 2f;
    private const float Starvation_Damage_Per_Sec = 5f;

    private const int Draw_Size = 64;

    private const float Interact_Cooldown = 0.5f;
    private float _interactTimer;

    public bool IsMoving { get; private set; }

    public float Hunger => _hunger;
    public float Thirst => _thirst;

    public Player(Texture2D? texture, Vector2 spawnPosition)
        : base("Гравець", maxHP: 100, speed: 200f)
    {
        _texture  = texture;
        _position = spawnPosition;
    }

    public override void Update(GameTime gameTime, WorldMap world)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        HandleMovement(gameTime, world);
        UpdateSurvivalStats(dt);

        _interactTimer = MathHelper.Max(0f, _interactTimer - dt);

        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.E) && _interactTimer <= 0f)
        {
            _interactTimer = Interact_Cooldown;
            TryInteract(world);
        }
    }

    public void Eat(int hungerRestore)
    {
        _hunger = MathHelper.Clamp(_hunger + hungerRestore, 0f, 100f);
    }

    public void Drink(int thirstRestore)
    {
        _thirst = MathHelper.Clamp(_thirst + thirstRestore, 0f, 100f);
    }

    private void HandleMovement(GameTime gameTime, WorldMap world)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var   kb  = Keyboard.GetState();
        var   dir = Vector2.Zero;

        if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))    dir.Y -= 1;
        if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))  dir.Y += 1;
        if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))  dir.X -= 1;
        if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) dir.X += 1;

        IsMoving = dir != Vector2.Zero;
        if (!IsMoving) return;

        if (dir.LengthSquared() > 1) dir.Normalize();

        Vector2 newPos = _position + dir * _speed * dt;

        var (tx, ty) = world.WorldToTile(newPos + new Vector2(Draw_Size / 2f));
        var tile = world.GetTile(tx, ty);

        if (tile?.IsWalkable == true)
        {
            _position = newPos;
        }
        else
        {
            var (txX, tyX) = world.WorldToTile(
                _position + new Vector2(dir.X * _speed * dt, 0)
                + new Vector2(Draw_Size / 2f));
            if (world.GetTile(txX, tyX)?.IsWalkable == true)
                _position.X += dir.X * _speed * dt;

            var (txY, tyY) = world.WorldToTile(
                _position + new Vector2(0, dir.Y * _speed * dt)
                + new Vector2(Draw_Size / 2f));
            if (world.GetTile(txY, tyY)?.IsWalkable == true)
                _position.Y += dir.Y * _speed * dt;
        }
    }

    private void UpdateSurvivalStats(float dt)
    {
        _hunger = MathHelper.Clamp(_hunger - Hunger_Decay * dt, 0f, 100f);
        _thirst = MathHelper.Clamp(_thirst - Thirst_Decay * dt, 0f, 100f);

        if (_hunger <= 0f || _thirst <= 0f)
            TakeDamage((int)(Starvation_Damage_Per_Sec * dt));
    }

    private void TryInteract(WorldMap world)
    {
        var (tx, ty) = world.WorldToTile(_position + new Vector2(Draw_Size / 2f));
        var tile = world.GetTile(tx, ty);
        if (tile?.Object == null) return;

        int gained = tile.Object.Interact();
        if (gained > 0)
            System.Console.WriteLine(
                $"[Гравець] Отримано: {gained}x {tile.Object.ResorurceName}");

        if (!tile.Object.isAlive)
            tile.RemoveObject();
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        Vector2 screenPos = _position - camera.Position;

        if (_texture != null)
        {
            XnaRect destRect = new(
                (int)screenPos.X, (int)screenPos.Y,
                Draw_Size, Draw_Size);
            spriteBatch.Draw(_texture, destRect, Color.White);
        }
    }
}