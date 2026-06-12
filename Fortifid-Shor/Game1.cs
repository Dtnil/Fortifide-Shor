using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Fortifid.Core;
using Fortifid.Core.Items;
using Fortifid.Systems;
using Fortifid.World;
using Fortifid.UI;

namespace Fortifid;

public class Game1 : Game
{
    /// <summary>
    /// Константи для вікна
    /// </summary>
    private const int WindowWidth  = 1280;
    private const int WindowHeight = 720;
    private const int Map_Width    = 1280;
    private const int Map_Height   = 720;
    private const int Map_Seed     = 0;
    private const float DayCycleDuration = 180f;
    private const float MaxNightDarkness = 0.58f;

    private enum GameState { MainMenu, Settings, Playing }
    private GameState _state = GameState.MainMenu;

    private MainMenu _mainMenu = null;
    private SettingsMenu _settingsMenu = null;

    private TextureManager _textures     = null;
    private WorldMap       _world        = null;
    private Camera         _camera       = null;
    private Player         _player       = null;
    private SettingsManager _settings    = null;
    private SoundManager    _sound       = null;
    private readonly List<Enemy> _enemies = new();
    private Texture2D _pixelTexture      = null;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null;
    private SpriteFont? _font;
    private KeyboardState _previousPlayingKeyboard;
    private float _dayTimer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _settings = SettingsManager.Load();
        _graphics.PreferredBackBufferWidth  = _settings.ResolutionWidth;
        _graphics.PreferredBackBufferHeight = _settings.ResolutionHeight;
        _graphics.IsFullScreen = _settings.IsFullscreen;
    }

    protected override void Initialize()
    {
        Window.Title = "Fortifid Shor";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _textures    = new TextureManager(Content);
        _sound       = new SoundManager();

        var bgTex       = Content.Load<Texture2D>("Textures/UI/BG");
        var startTex    = Content.Load<Texture2D>("Textures/UI/Start");
        var settingsTex = Content.Load<Texture2D>("Textures/UI/Settings");
        var exitTex     = Content.Load<Texture2D>("Textures/UI/Exit");
        _font           = Content.Load<SpriteFont>("Fonts/UIFont");

        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        _mainMenu = new MainMenu(bgTex, startTex, settingsTex, exitTex,
                                 screenW, screenH);
        _sound.LoadContent(Content);
        _sound.MasterVolume = _settings.MasterVolume;
        _sound.MusicVolume  = _settings.MusicVolume;
        _sound.SfxVolume    = _settings.SfxVolume;

        _settingsMenu = new SettingsMenu(GraphicsDevice, _graphics,
            _settings, _sound, _font, screenW, screenH);

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    private void StartGame()
    {
        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        _world  = new WorldMap(Map_Width, Map_Height, Map_Seed, _textures);
        _camera = new Camera(screenW, screenH,
                             _world.Pixel_Width, _world.Pixel_Height);

        Vector2 spawnPos = _world.FindSpawnPoint();
        _player = new Player(_textures.Get("Player"), spawnPos);
        _dayTimer = DayCycleDuration * 0.15f;

        _enemies.Clear();
        SpawnEnemies();

        _state = GameState.Playing;
    }

    protected override void Update(GameTime gameTime)
    {
        ResizeMenusToViewport();

        switch (_state)
        {
            case GameState.MainMenu:
                var action = _mainMenu.Update(gameTime);
                if (action == MenuAction.StartGame)   StartGame();
                else if (action == MenuAction.OpenSettings) _state = GameState.Settings;
                else if (action == MenuAction.Exit)   Exit();
                break;

            case GameState.Settings:
                if (_settingsMenu.Update(gameTime) == SettingsAction.Back)
                    _state = GameState.MainMenu;
                break;

            case GameState.Playing:
                UpdatePlaying(gameTime);
                break;
        }

        base.Update(gameTime);
    }

    private void ResizeMenusToViewport()
    {
        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        _mainMenu?.Resize(screenW, screenH);
        _settingsMenu?.Resize(screenW, screenH);
    }

    private void UpdatePlaying(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Escape))
        {
            _sound.StopMusic();
            _state = GameState.MainMenu;
            _previousPlayingKeyboard = kb;
            return;
        }

        if (!_player.IsAlive) return;

        _player.Update(gameTime, _world);
        if (WasPressed(kb, Keys.F))
            TrySwordAttack();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateDayNightCycle(dt);
        _world.Update(dt);
        UpdateSound(dt);

        foreach (var enemy in _enemies)
        {
            if (enemy is Crab crab)
                crab.Update(gameTime, _world, _player);
            else
                enemy.Update(gameTime, _world);
        }

        RemoveDefeatedEnemies();
        _camera.Follow(_player.Ceneter);
        _previousPlayingKeyboard = kb;
    }

    private void UpdateDayNightCycle(float dt)
    {
        _dayTimer = (_dayTimer + dt) % DayCycleDuration;
    }

    private bool WasPressed(KeyboardState kb, Keys key)
    {
        return kb.IsKeyDown(key) && !_previousPlayingKeyboard.IsKeyDown(key);
    }

    private void TrySwordAttack()
    {
        Crab? nearestCrab = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in _enemies)
        {
            if (enemy is not Crab crab || !crab.IsAlive) continue;

            float distance = Vector2.Distance(_player.Ceneter, crab.Ceneter);
            if (distance < nearestDistance)
            {
                nearestCrab = crab;
                nearestDistance = distance;
            }
        }

        if (nearestCrab == null)
        {
            _player.SetStatus("Поруч немає крабів");
            return;
        }

        _player.TryAttackCrab(nearestCrab);
    }

    private void RemoveDefeatedEnemies()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            if (!_enemies[i].IsAlive)
                _enemies.RemoveAt(i);
        }
    }

    private void UpdateSound(float dt)
    {
        var (tx, ty) = _world.WorldToTile(_player.Position + new Vector2(32, 32));
        TileType tileType = _world.GetTile(tx, ty)?.Type ?? TileType.Grass;
        _sound.Update(tileType, _player.IsMoving, dt);
    }

    protected override void Draw(GameTime gameTime)
    {
        switch (_state)
        {
            case GameState.MainMenu:
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _mainMenu.Draw(_spriteBatch);
                _spriteBatch.End();
                break;

            case GameState.Settings:
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _mainMenu.Draw(_spriteBatch);
                _settingsMenu.Draw(_spriteBatch);
                _spriteBatch.End();
                break;

            case GameState.Playing:
                GraphicsDevice.Clear(new Color(10, 20, 40));
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _world.Draw(_spriteBatch, _camera);
                foreach (var enemy in _enemies)
                    enemy.Draw(_spriteBatch, _camera);
                _player.Draw(_spriteBatch, _camera);
                DrawDayNightOverlay();
                DrawHUD();
                _spriteBatch.End();
                break;
        }

        base.Draw(gameTime);
    }

    private void SpawnEnemies()
    {
        var rng    = new System.Random();
        int ts     = WorldMap.Tile_Size;
        var crabTex = _textures.Get("Crab_Anim");

        int frameCount = crabTex.Width > crabTex.Height
            ? crabTex.Width / crabTex.Height : 1;

        var farSpawnTiles = new List<Point>();
        var fallbackSpawnTiles = new List<Point>();
        var playerTile = _world.WorldToTile(_player.Position + new Vector2(32, 32));
        const int enemyCount = 12;
        const int minTilesFromPlayer = 4;

        for (int tx = 0; tx < Map_Width; tx++)
        {
            for (int ty = 0; ty < Map_Height; ty++)
            {
                var tile = _world.GetTile(tx, ty);
                if (tile == null || !CanSpawnEnemyOnTile(tile)) continue;
                if (tx == playerTile.tileX && ty == playerTile.tileY) continue;

                int dx = tx - playerTile.tileX;
                int dy = ty - playerTile.tileY;
                var spawnTile = new Point(tx, ty);

                if (dx * dx + dy * dy >= minTilesFromPlayer * minTilesFromPlayer)
                    farSpawnTiles.Add(spawnTile);
                else
                    fallbackSpawnTiles.Add(spawnTile);
            }
        }

        Shuffle(farSpawnTiles, rng);
        Shuffle(fallbackSpawnTiles, rng);
        farSpawnTiles.AddRange(fallbackSpawnTiles);

        int spawnsToCreate = System.Math.Min(enemyCount, farSpawnTiles.Count);
        for (int i = 0; i < spawnsToCreate; i++)
        {
            Point tile = farSpawnTiles[i];
            var pos = new Vector2(tile.X * ts, tile.Y * ts);
            _enemies.Add(new Crab(crabTex, pos, frameCount));
        }
    }

    private static bool CanSpawnEnemyOnTile(Tile tile)
    {
        return tile.Type == TileType.Sand && tile.Object == null;
    }

    private static void Shuffle<T>(IList<T> items, System.Random rng)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    private void DrawPlayerPlaceholder()
    {
        Vector2 pos  = _player.Position - _camera.Position;
        var     rect = new Rectangle((int)pos.X, (int)pos.Y, 64, 64);
        _spriteBatch.Draw(_pixelTexture, rect, new Color(60, 180, 60));
    }

    private void DrawHUD()
    {
        const int BAR_X = 20;
        const int BAR_W = 180;
        const int BAR_H = 18;
        const int GAP   = 8;
        int y = 20;

        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Hp     / 100f, new Color(200,  50,  50), "HP");
        y += BAR_H + GAP;
        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Hunger / 100f, new Color(200, 150,  50), "Hunger");
        y += BAR_H + GAP;
        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Thirst / 100f, new Color( 50, 150, 200), "Thirst");
        DrawInventoryHUD();
    }

    private void DrawDayNightOverlay()
    {
        float cycle = _dayTimer / DayCycleDuration;
        float nightAmount = (1f - System.MathF.Cos(cycle * MathHelper.TwoPi)) * 0.5f;
        float darkness = nightAmount * MaxNightDarkness;

        if (darkness <= 0.01f)
            return;

        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;
        var overlayColor = new Color(8, 18, 45) * darkness;

        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(0, 0, screenW, screenH),
            overlayColor);
    }

    private void DrawBar(int x, int y, int w, int h, float fill, Color color, string label)
    {
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(x - 2, y - 2, w + 4, h + 4),
            new Color(20, 20, 20, 180));

        int fillW = (int)(w * MathHelper.Clamp(fill, 0f, 1f));
        if (fillW > 0)
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, fillW, h), color);

        _spriteBatch.Draw(_pixelTexture, new Rectangle(x,     y,     w, 1), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x,     y + h, w, 1), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x,     y,     1, h), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x + w, y,     1, h + 1), Color.White * 0.5f);
    }

    private void DrawInventoryHUD()
    {
        const int panelX = 20;
        const int panelY = 104;
        const int panelW = 300;
        const int rowH = 28;
        const int padding = 10;

        IItem[] resources =
        {
            GameItems.Wood,
            GameItems.Stone,
            GameItems.CopperOre,
            GameItems.IronOre,
            GameItems.CopperIngot,
            GameItems.IronIngot,
            GameItems.StoneAxe,
            GameItems.CopperPickaxe,
            GameItems.IronPickaxe,
            GameItems.IronSword
        };

        int panelH = padding * 2 + resources.Length * rowH;
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, panelW, panelH),
            new Color(20, 24, 30, 190));

        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, panelW, 1),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY + panelH, panelW, 1),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, 1, panelH),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX + panelW, panelY, 1, panelH + 1),
            Color.White * 0.45f);

        for (int i = 0; i < resources.Length; i++)
        {
            IItem resource = resources[i];
            int amount = _player.Inventory.Count(resource);
            int y = panelY + padding + i * rowH;

            _spriteBatch.DrawString(_font, resource.Name,
                new Vector2(panelX + padding, y),
                Color.White);
            _spriteBatch.DrawString(_font, amount.ToString(),
                new Vector2(panelX + panelW - padding - 38, y),
                new Color(230, 220, 160));
        }

        DrawCraftingHUD(panelX, panelY + panelH + 12);
    }

    private void DrawCraftingHUD(int panelX, int panelY)
    {
        const int panelW = 330;
        const int rowH = 24;
        const int padding = 10;

        string[] lines =
        {
            "E: взаємодія / пити воду",
            "F: удар мечем",
            "R: плавити мідь",
            "T: плавити залізо",
            "1: кам'яна сокира",
            "2: мідна кирка",
            "3: залізна кирка",
            "4: залізний меч",
            "Інструменти дають бонус добування"
        };

        int statusRows = string.IsNullOrWhiteSpace(_player.StatusMessage) ? 0 : 1;
        int panelH = padding * 2 + (lines.Length + statusRows) * rowH;

        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, panelW, panelH),
            new Color(18, 22, 28, 190));

        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, panelW, 1),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY + panelH, panelW, 1),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX, panelY, 1, panelH),
            Color.White * 0.45f);
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(panelX + panelW, panelY, 1, panelH + 1),
            Color.White * 0.45f);

        for (int i = 0; i < lines.Length; i++)
        {
            _spriteBatch.DrawString(_font, lines[i],
                new Vector2(panelX + padding, panelY + padding + i * rowH),
                Color.White);
        }

        if (!string.IsNullOrWhiteSpace(_player.StatusMessage))
        {
            _spriteBatch.DrawString(_font, _player.StatusMessage,
                new Vector2(panelX + padding, panelY + padding + lines.Length * rowH),
                new Color(240, 210, 120));
        }
    }
}
