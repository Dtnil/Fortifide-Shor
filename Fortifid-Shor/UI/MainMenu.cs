using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Fortifid.UI;

public enum MenuAction { None, StartGame, OpenSettings, Exit }

public class MainMenu
{
    private readonly Texture2D _bg;
    private readonly Texture2D _btnStart;
    private readonly Texture2D _btnSettings;
    private readonly Texture2D _btnExit;
    
    private int _screenWidth;
    private int _screenHeight;
    
    private const int BtnWidth = 300;
    private const int BtnHeight = 80;
    private const int BtnGap = 20;
    
    private Rectangle _rStart;
    private Rectangle _rExit;
    private Rectangle _rSettings;
    
    private MouseState _prevMouseState;

    private float _scaleStart = 1f;
    private float _scaleExit = 1f;
    private float _scaleSettings = 1f;
    
    private const float HoverScale  = 1.08f;
    private const float ScaleSpeed  = 6f;

    public MainMenu(Texture2D bg,
        Texture2D btnStart,
        Texture2D btnSettings,
        Texture2D btnExit,
        int screenWidth,
        int screenHeight)
    {
        _bg = bg;
        _btnStart = btnStart;
        _btnSettings = btnSettings;
        _btnExit = btnExit;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        RebildRects();
    }

    public void Resize(int screenWidth, int screenHeight)
    {
        if (_screenWidth == screenWidth && _screenHeight == screenHeight)
            return;

        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        RebildRects();
    }

    private void RebildRects()
    {
        int totalHeight = BtnHeight * 3 + BtnGap * 2;
        int startY =(_screenHeight - totalHeight) / 2 + 60;
        int x = (_screenWidth - BtnWidth) / 2;
        
        _rStart = new Rectangle(x, startY, BtnWidth, BtnHeight);
        _rSettings = new Rectangle(x, startY + BtnHeight + BtnGap, BtnWidth, BtnHeight);
        _rExit = new Rectangle(x, startY + (BtnHeight + BtnGap) * 2, BtnWidth, BtnHeight);
    }

    public MenuAction Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var mouse = Mouse.GetState();
        Point mp = mouse.Position;

        bool hStart = _rStart.Contains(mp);
        bool hSettings = _rSettings.Contains(mp);
        bool hExit = _rExit.Contains(mp);
        
        _scaleStart    = Lerp(_scaleStart,    hStart    ? HoverScale : 1f, ScaleSpeed * dt);
        _scaleSettings = Lerp(_scaleSettings, hSettings ? HoverScale : 1f, ScaleSpeed * dt);
        _scaleExit     = Lerp(_scaleExit,     hExit     ? HoverScale : 1f, ScaleSpeed * dt);

        bool clicked = mouse.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed;
        
        _prevMouseState =  mouse;

        if (clicked)
        {
            if (hStart) return MenuAction.StartGame;
            if (hSettings) return MenuAction.OpenSettings;
            if (hExit) return  MenuAction.Exit;
        }
        
        return MenuAction.None;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_bg, new Rectangle(0,0,_screenWidth,_screenHeight), Color.White);
        
        DrawOverlay(spriteBatch);
        
        DrawButton(spriteBatch, _btnStart, _rStart, _scaleStart);
        DrawButton(spriteBatch, _btnSettings, _rSettings, _scaleSettings);
        DrawButton(spriteBatch, _btnExit, _rExit, _scaleExit);
    }

    private void DrawOverlay(SpriteBatch spriteBatch)
    {
        var gd = spriteBatch.GraphicsDevice;
        var pixel = new Texture2D(gd, 1, 1);
        pixel.SetData(new[] { Color.White });
        
        int totalHeight = BtnHeight * 3 + BtnGap * 2 + 80;
        int oy = (_screenHeight - totalHeight) / 2 + 20;
        spriteBatch.Draw(pixel, 
            new Rectangle((_screenWidth - BtnWidth - 80) / 2, oy, BtnWidth + 80, totalHeight)
            , Color.Black * 0.35f);
        pixel.Dispose();
    }

    private static void DrawButton(SpriteBatch spriteBatch, Texture2D tex,
        Rectangle baseRect, float scale)
    {
        int w = (int)(baseRect.Width * scale);
        int h = (int)(baseRect.Height * scale);
        int dx = (baseRect.Width - w) / 2;
        int dy = (baseRect.Height - h) / 2;
        
        var dest = new Rectangle(baseRect.X + dx, baseRect.Y + dy, w, h);
        spriteBatch.Draw(tex, dest, Color.White);
    }

    private static float Lerp(float a, float b, float t)
        => a + (b - a) * MathHelper.Clamp(t, 0f, 1f);
}
