using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Fortifid.Core;
using Fortifid.Systems;
using Fortifid.World;

namespace Fortifid;

public class Game1 : Game
{
    /// <summary>
    /// Константи для вікна
    /// </summary>
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const int Map_Widht = 120;
    private const int Map_Height = 90;
    private const int Map_Seed = 0;
    
    private TextureManager _textures = null;
    private WorldMap _world = null;
    private Camera _camera = null;
    private Player _player = null;
    private readonly List<Enemy> _enemies = new();
    private Texture2D _pixelTexture = null;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null;
    private SpriteFont? _font;

    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.PreferredBackBufferWidth = WindowWidth;
        _graphics.PreferredBackBufferHeight = WindowHeight;
    }

    protected override void Initialize()
    {
        Window.Title = "Fortifid Shor";
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _textures = new TextureManager(Content);
        _world = new WorldMap(WindowWidth, WindowHeight, Map_Seed, _textures);

        _camera = new Camera(
            WindowWidth, WindowHeight,
            _world.Pixel_Width, _world.Pixel_Height
            );
        Vector2 spawnPos = _world.FindSpawnPoint();
        _player = new Player(null, spawnPos);

        SpawnEnemis();
        
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (!_player.IsAlive)
        {
            return;
        }
        
        _player.Update(gameTime, _world);
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _world.Update(dt);

        foreach (var enemy in _enemies)
        {
            if (enemy is Crab crab)
                crab.Update(gameTime, _world, _player);
            else
                enemy.Update(gameTime, _world);
        }
        
        _camera.Follow(_player.Position+ new Vector2(32,32));
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 20, 40));
        
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp
            );
        
        _world.Draw(_spriteBatch, _camera);

        foreach (var enemy in _enemies)
            enemy.Draw(_spriteBatch, _camera);

        DrawPlayerPlaceholder();
        DrawHUD();
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void SpawnEnemis()
    {
        var rng = new System.Random();
        int ts = WorldMap.Tile_Size;
        var crabTex =  _textures.Get("Crab_Anim");
        
        int frameCount = crabTex.Width > crabTex.Height
            ? crabTex.Width / crabTex.Height: 1;
        for (int i = 0; i < 8; i++)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int tx = rng.Next(Map_Widht);
                int ty = rng.Next(Map_Height);
                var t = _world.GetTile(tx, ty);
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
        Vector2 pos = _player.Position - _camera.Position;
        var rect = new Rectangle((int)pos.X, (int)pos.Y, 64, 64);
        _spriteBatch.Draw(_pixelTexture, rect,new Color(60,180,60));
    }

    private void DrawHUD()
    {
        const int BAR_X = 20;
        const int BAR_W = 180;
        const int BAR_H = 18;
        const int GAP   = 8;
        int y = 20;

        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Hp / 100f, new Color(200, 50, 50), "HP");
        y += BAR_H + GAP;
        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Hunger / 100f, new Color(200, 150, 50), "Hunger");
        y += BAR_H + GAP;
        DrawBar(BAR_X, y, BAR_W, BAR_H, _player.Thirst / 100f, new Color(50, 150, 200), "Thirst");
    }

    private void DrawBar(int x, int y, int w, int h, float fill, Color color, string label)
    {
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(x - 2,y - 2,w + 4,h + 4),
            new Color(20,20,20,180));
        
        int fillW = (int)(w * MathHelper.Clamp(fill, 0f,1f));
        if (fillW > 0)
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, fillW, h), color);
        
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, w, 1), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + h, w, 1), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, 1, h), Color.White * 0.5f);
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x + w, y, 1, h + 1), Color.White * 0.5f);
    }
}