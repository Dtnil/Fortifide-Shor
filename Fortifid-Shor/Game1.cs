using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Fortifid.Core;
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
    private const int Map_Widht    = 120;
    private const int Map_Height   = 90;
    private const int Map_Seed     = 0;

    // ── стан гри ──────────────────────────────────────────────────
    private enum GameState { MainMenu, Settings, Playing }
    private GameState _state = GameState.MainMenu;

    // ── головне меню ──────────────────────────────────────────────
    private MainMenu _mainMenu = null;
    private SettingsMenu _settingsMenu = null;

    // ── ігрові об'єкти ────────────────────────────────────────────
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

        // ── завантаження текстур меню ──────────────────────────────
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

        // ── однопіксельна текстура для HUD ────────────────────────
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    // ── ініціалізація ігрового світу (викликається при старті) ────
    private void StartGame()
    {
        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        _world  = new WorldMap(Map_Widht, Map_Height, Map_Seed, _textures);
        _camera = new Camera(screenW, screenH,
                             _world.Pixel_Width, _world.Pixel_Height);

        Vector2 spawnPos = _world.FindSpawnPoint();
        _player = new Player(null, spawnPos);

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
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            // повернення в головне меню
            _sound.StopMusic();
            _state = GameState.MainMenu;
            return;
        }

        if (!_player.IsAlive) return;

        _player.Update(gameTime, _world);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _world.Update(dt);
        UpdateSound(dt);

        foreach (var enemy in _enemies)
        {
            if (enemy is Crab crab)
                crab.Update(gameTime, _world, _player);
            else
                enemy.Update(gameTime, _world);
        }

        _camera.Follow(_player.Position + new Vector2(32, 32));
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
                DrawPlayerPlaceholder();
                DrawHUD();
                _spriteBatch.End();
                break;
        }

        base.Draw(gameTime);
    }

    // ──────────────────────────────────────────────────────────────
    private void SpawnEnemies()
    {
        var rng    = new System.Random();
        int ts     = WorldMap.Tile_Size;
        var crabTex = _textures.Get("Crab_Anim");

        int frameCount = crabTex.Width > crabTex.Height
            ? crabTex.Width / crabTex.Height : 1;

        for (int i = 0; i < 8; i++)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int tx = rng.Next(Map_Widht);
                int ty = rng.Next(Map_Height);
                var t  = _world.GetTile(tx, ty);
                if (t?.Type is TileType.Sand or TileType.Grass && t.Object == null)
                {
                    var pos = new Vector2(tx * ts, ty * ts);
                    _enemies.Add(new Crab(crabTex, pos, frameCount));
                    break;
                }
            }
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
        const int panelW = 230;
        const int rowH = 28;
        const int padding = 10;

        string[] resources =
        {
            "Деревина",
            "Камінь",
            "Мідна руда",
            "Залізна руда"
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
            string resource = resources[i];
            int amount = _player.Inventory.Count(resource);
            int y = panelY + padding + i * rowH;

            _spriteBatch.DrawString(_font, resource,
                new Vector2(panelX + padding, y),
                Color.White);
            _spriteBatch.DrawString(_font, amount.ToString(),
                new Vector2(panelX + panelW - padding - 38, y),
                new Color(230, 220, 160));
        }
    }
}
