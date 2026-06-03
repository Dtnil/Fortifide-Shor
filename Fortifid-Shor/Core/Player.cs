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

    private const float HungerDecay = 1.5f;
    private const float ThirstDecay = 2f;
    private const float StarvationDamagePerSec = 5f;

    private const int DrawSize = 64;

    private const float InteractCooldown = 0.5f;
    private const float InteractionRange = 95f;
    private float _interactTimer;

    public bool IsMoving { get; private set; }

    public float Hunger => _hunger;
    public float Thirst => _thirst;
    public Inventory Inventory { get; } = new();

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
            _interactTimer = InteractCooldown;
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

        var (tx, ty) = world.WorldToTile(newPos + new Vector2(DrawSize / 2f));
        var tile = world.GetTile(tx, ty);

        if (tile?.IsWalkable == true)
        {
            _position = newPos;
        }
        else
        {
            var (txX, tyX) = world.WorldToTile(
                _position + new Vector2(dir.X * _speed * dt, 0)
                + new Vector2(DrawSize / 2f));
            if (world.GetTile(txX, tyX)?.IsWalkable == true)
                _position.X += dir.X * _speed * dt;

            var (txY, tyY) = world.WorldToTile(
                _position + new Vector2(0, dir.Y * _speed * dt)
                + new Vector2(DrawSize / 2f));
            if (world.GetTile(txY, tyY)?.IsWalkable == true)
                _position.Y += dir.Y * _speed * dt;
        }
    }

    private void UpdateSurvivalStats(float dt)
    {
        _hunger = MathHelper.Clamp(_hunger - HungerDecay * dt, 0f, 100f);
        _thirst = MathHelper.Clamp(_thirst - ThirstDecay * dt, 0f, 100f);

        if (_hunger <= 0f || _thirst <= 0f)
            TakeDamage((int)(StarvationDamagePerSec * dt));
    }

    private void TryInteract(WorldMap world)
    {
        if (!TryFindInteractTarget(world, out var tile)) return;

        int gained = tile.Object.Interact();
        string resourceName = tile.Object.ResorurceName;

        if (gained > 0)
            Inventory.Add(resourceName, gained);

        if (tile.Object is { isAlive: false })
            tile.RemoveObject();
    }

    private bool TryFindInteractTarget(WorldMap world, out Tile tile)
    {
        Vector2 playerCenter = _position + new Vector2(DrawSize / 2f);
        var (centerX, centerY) = world.WorldToTile(playerCenter);

        Tile? bestTile = null;
        float bestDistance = float.MaxValue;

        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                Tile? candidate = world.GetTile(x, y);
                if (candidate?.Object == null) continue;

                Vector2 objectCenter = candidate.Worldposition
                    + new Vector2(WorldMap.Tile_Size / 2f);
                float distance = Vector2.Distance(playerCenter, objectCenter);

                if (distance <= InteractionRange && distance < bestDistance)
                {
                    bestTile = candidate;
                    bestDistance = distance;
                }
            }
        }

        tile = bestTile!;
        return bestTile != null;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        Vector2 screenPos = _position - camera.Position;

        if (_texture != null)
        {
            XnaRect destRect = new(
                (int)screenPos.X, (int)screenPos.Y,
                DrawSize, DrawSize);
            spriteBatch.Draw(_texture, destRect, Color.White);
        }
    }
}
