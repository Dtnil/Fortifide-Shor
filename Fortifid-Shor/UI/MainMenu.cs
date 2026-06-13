using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fortifid.UI;

public enum MenuAction { None, StartGame, OpenSettings, Exit }

public class MainMenu
{
    private const float ReferenceWidth = 560f;
    private const float ReferenceHeight = 315f;
    private const float HoverScale = 1.06f;
    private const float ScaleSpeed = 8f;

    private readonly Texture2D _bg;
    private readonly Texture2D _btnStart;
    private readonly Texture2D _btnSettings;
    private readonly Texture2D _btnExit;
    private readonly Texture2D _pixel;
    private readonly SpriteFont _titleFont;

    private int _screenWidth;
    private int _screenHeight;
    private Rectangle _panelRect;
    private Rectangle _rStart;
    private Rectangle _rSettings;
    private Rectangle _rExit;
    private Vector2 _titlePosition;
    private float _uiScale;
    private float _scaleStart = 1f;
    private float _scaleSettings = 1f;
    private float _scaleExit = 1f;
    private MouseState _prevMouseState;

    public MainMenu(
        Texture2D bg,
        Texture2D btnStart,
        Texture2D btnSettings,
        Texture2D btnExit,
        Texture2D pixel,
        SpriteFont titleFont,
        int screenWidth,
        int screenHeight)
    {
        _bg = bg;
        _btnStart = btnStart;
        _btnSettings = btnSettings;
        _btnExit = btnExit;
        _pixel = pixel;
        _titleFont = titleFont;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        RebuildLayout();
    }

    public void Resize(int screenWidth, int screenHeight)
    {
        if (_screenWidth == screenWidth && _screenHeight == screenHeight)
            return;

        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        RebuildLayout();
    }

    public MenuAction Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        MouseState mouse = Mouse.GetState();
        Point mousePosition = mouse.Position;

        bool hoverStart = _rStart.Contains(mousePosition);
        bool hoverSettings = _rSettings.Contains(mousePosition);
        bool hoverExit = _rExit.Contains(mousePosition);

        _scaleStart = Lerp(_scaleStart, hoverStart ? HoverScale : 1f, ScaleSpeed * dt);
        _scaleSettings = Lerp(_scaleSettings, hoverSettings ? HoverScale : 1f, ScaleSpeed * dt);
        _scaleExit = Lerp(_scaleExit, hoverExit ? HoverScale : 1f, ScaleSpeed * dt);

        bool clicked = mouse.LeftButton == ButtonState.Released
                       && _prevMouseState.LeftButton == ButtonState.Pressed;
        _prevMouseState = mouse;

        if (!clicked)
            return MenuAction.None;

        if (hoverStart) return MenuAction.StartGame;
        if (hoverSettings) return MenuAction.OpenSettings;
        if (hoverExit) return MenuAction.Exit;
        return MenuAction.None;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_bg, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

        DrawPanel(spriteBatch);
        DrawTitle(spriteBatch);
        DrawButton(spriteBatch, _btnStart, _rStart, _scaleStart);
        DrawButton(spriteBatch, _btnSettings, _rSettings, _scaleSettings);
        DrawButton(spriteBatch, _btnExit, _rExit, _scaleExit);
    }

    private void RebuildLayout()
    {
        _uiScale = MathHelper.Min(
            _screenWidth / ReferenceWidth,
            _screenHeight / ReferenceHeight);

        float offsetX = (_screenWidth - ReferenceWidth * _uiScale) / 2f;
        float offsetY = (_screenHeight - ReferenceHeight * _uiScale) / 2f;

        _panelRect = ScaleRect(196, 105, 168, 158, offsetX, offsetY);
        _rStart = BuildButtonRect(_btnStart, 88, 124, offsetX, offsetY);
        _rSettings = BuildButtonRect(_btnSettings, 100, 166, offsetX, offsetY);
        _rExit = BuildButtonRect(_btnExit, 88, 211, offsetX, offsetY);

        Vector2 titleSize = _titleFont.MeasureString("Fortifid Shor") * _uiScale;
        _titlePosition = new Vector2(
            (_screenWidth - titleSize.X) / 2f,
            offsetY + 25f * _uiScale);
    }

    private Rectangle BuildButtonRect(
        Texture2D texture,
        float width,
        float y,
        float offsetX,
        float offsetY)
    {
        float height = width * texture.Height / texture.Width;
        float x = (ReferenceWidth - width) / 2f;
        return ScaleRect(x, y, width, height, offsetX, offsetY);
    }

    private Rectangle ScaleRect(
        float x,
        float y,
        float width,
        float height,
        float offsetX,
        float offsetY)
    {
        return new Rectangle(
            (int)System.MathF.Round(offsetX + x * _uiScale),
            (int)System.MathF.Round(offsetY + y * _uiScale),
            (int)System.MathF.Round(width * _uiScale),
            (int)System.MathF.Round(height * _uiScale));
    }

    private void DrawPanel(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, _panelRect, new Color(0, 0, 0, 242));

        int border = System.Math.Max(1, (int)System.MathF.Round(_uiScale));
        spriteBatch.Draw(_pixel,
            new Rectangle(_panelRect.X, _panelRect.Y, _panelRect.Width, border),
            Color.White);
        spriteBatch.Draw(_pixel,
            new Rectangle(_panelRect.X, _panelRect.Bottom - border, _panelRect.Width, border),
            Color.White);
        spriteBatch.Draw(_pixel,
            new Rectangle(_panelRect.X, _panelRect.Y, border, _panelRect.Height),
            Color.White);
        spriteBatch.Draw(_pixel,
            new Rectangle(_panelRect.Right - border, _panelRect.Y, border, _panelRect.Height),
            Color.White);
    }

    private void DrawTitle(SpriteBatch spriteBatch)
    {
        Vector2 shadowOffset = new(
            System.Math.Max(1f, _uiScale),
            System.Math.Max(1f, _uiScale));

        spriteBatch.DrawString(
            _titleFont,
            "Fortifid Shor",
            _titlePosition + shadowOffset,
            new Color(52, 39, 64),
            0f,
            Vector2.Zero,
            _uiScale,
            SpriteEffects.None,
            0f);
        spriteBatch.DrawString(
            _titleFont,
            "Fortifid Shor",
            _titlePosition,
            new Color(255, 242, 185),
            0f,
            Vector2.Zero,
            _uiScale,
            SpriteEffects.None,
            0f);
    }

    private static void DrawButton(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle baseRect,
        float scale)
    {
        int width = (int)System.MathF.Round(baseRect.Width * scale);
        int height = (int)System.MathF.Round(baseRect.Height * scale);
        var destination = new Rectangle(
            baseRect.Center.X - width / 2,
            baseRect.Center.Y - height / 2,
            width,
            height);

        spriteBatch.Draw(texture, destination, Color.White);
    }

    private static float Lerp(float from, float to, float amount)
        => from + (to - from) * MathHelper.Clamp(amount, 0f, 1f);
}
