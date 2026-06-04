using System;
using Fortifid.Systems;
using XnaRect = Microsoft.Xna.Framework.Rectangle;
using Fortifid.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Fortifid.Core;

public class Player : Entity
{
    private float _hunger = 100f;
    private float _thirst = 100f;

    private const float HungerDecay = 1.5f;
    private const float ThirstDecay = 2f;
    private const float StarvationDamagePerSec = 5f;

    private const int DrawSize = 64;
    private const int AnimationRows = 4;
    private const int AnimationColumns = 8;
    private const float AnimationFrameDuration = 0.12f;

    private const float InteractCooldown = 0.5f;
    private const float InteractionRange = 95f;
    private float _interactTimer;
    private float _animationTimer;
    private float _statusTimer;
    private int _currentFrame;
    private int _facingRow;
    private KeyboardState _previousKeyboard;

    public bool IsMoving { get; private set; }

    public float Hunger => _hunger;
    public float Thirst => _thirst;
    public Inventory Inventory { get; } = new();
    public string StatusMessage { get; private set; } = "";

    public Player(Texture2D? texture, Vector2 spawnPosition)
        : base("Гравець", maxHP: 100, speed: 200f)
    {
        _texture  = texture;
        _position = spawnPosition;
    }

    public override void Update(GameTime gameTime, WorldMap world)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var kb = Keyboard.GetState();

        HandleMovement(gameTime, world);
        UpdateAnimation(dt);
        UpdateSurvivalStats(dt);
        UpdateStatusMessage(dt);

        _interactTimer = MathHelper.Max(0f, _interactTimer - dt);

        if (kb.IsKeyDown(Keys.E) && _interactTimer <= 0f)
        {
            _interactTimer = InteractCooldown;
            TryInteract(world);
        }

        if (WasPressed(kb, Keys.R))
            TrySmelt("Мідна руда", "Мідний злиток");

        if (WasPressed(kb, Keys.T))
            TrySmelt("Залізна руда", "Залізний злиток");

        if (WasPressed(kb, Keys.D1))
            TryCraft("Кам'яна сокира",
                ("Деревина", 3),
                ("Камінь", 2));

        if (WasPressed(kb, Keys.D2))
            TryCraft("Мідна кирка",
                ("Мідний злиток", 2),
                ("Деревина", 2),
                ("Камінь", 1));

        if (WasPressed(kb, Keys.D3))
            TryCraft("Залізна кирка",
                ("Залізний злиток", 3),
                ("Деревина", 2));

        if (WasPressed(kb, Keys.D4))
            TryCraft("Залізний меч",
                ("Залізний злиток", 2),
                ("Деревина", 1));

        _previousKeyboard = kb;
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
        UpdateFacingDirection(dir);

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

    private void UpdateAnimation(float dt)
    {
        if (!IsMoving)
        {
            _animationTimer = 0f;
            _currentFrame = 0;
            return;
        }

        _animationTimer += dt;
        if (_animationTimer < AnimationFrameDuration) return;

        _animationTimer = 0f;
        int framesInRow = _facingRow <= 1 ? 4 : AnimationColumns;
        _currentFrame = (_currentFrame + 1) % framesInRow;
    }

    private void UpdateFacingDirection(Vector2 dir)
    {
        if (MathF.Abs(dir.X) > MathF.Abs(dir.Y))
            _facingRow = dir.X < 0 ? 2 : 3;
        else
            _facingRow = dir.Y < 0 ? 1 : 0;
    }

    private void UpdateStatusMessage(float dt)
    {
        if (_statusTimer <= 0f) return;

        _statusTimer -= dt;
        if (_statusTimer <= 0f)
            StatusMessage = "";
    }

    private bool WasPressed(KeyboardState kb, Keys key)
    {
        return kb.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void TrySmelt(string oreName, string ingotName)
    {
        if (!Inventory.Remove(oreName, 1))
        {
            SetStatus($"Потрібно: {oreName}");
            return;
        }

        Inventory.Add(ingotName, 1);
        SetStatus($"+1 {ingotName}");
    }

    private void TryCraft(string toolName, params (string itemName, int amount)[] cost)
    {
        foreach (var item in cost)
        {
            if (!Inventory.Has(item.itemName, item.amount))
            {
                SetStatus($"Не вистачає: {item.itemName}");
                return;
            }
        }

        foreach (var item in cost)
            Inventory.Remove(item.itemName, item.amount);

        Inventory.Add(toolName, 1);
        SetStatus($"Створено: {toolName}");
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        _statusTimer = 2.2f;
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
            int frameWidth = Math.Max(1, _texture.Width / AnimationColumns);
            int frameHeight = Math.Max(1, _texture.Height / AnimationRows);
            int row = Math.Min(_facingRow, AnimationRows - 1);
            int column = Math.Min(_currentFrame, AnimationColumns - 1);
            XnaRect sourceRect = new(
                column * frameWidth,
                row * frameHeight,
                frameWidth,
                frameHeight);
            XnaRect destRect = new(
                (int)screenPos.X, (int)screenPos.Y,
                DrawSize, DrawSize);
            spriteBatch.Draw(_texture, destRect, sourceRect, Color.White);
        }
    }
}
