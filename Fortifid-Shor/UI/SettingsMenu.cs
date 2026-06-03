using Fortifid.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Fortifid.UI;

public enum SettingsAction { None, Back }

public class SettingsMenu
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly SettingsManager _settings;
    private readonly SoundManager _sound;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    private int _sw;
    private int _sh;
    private MouseState _prevMouse;
    private KeyboardState _prevKb;

    private const int PanelW = 520;
    private const int PanelH = 500;

    private Rectangle _panelRect;
    private SliderState _sliderMaster;
    private SliderState _sliderMusic;
    private SliderState _sliderSfx;
    private Rectangle _btnResolution;
    private int _resIndex;
    private Rectangle _checkFullscreen;
    private Rectangle _btnBack;
    private float _scaleBack = 1f;

    private static readonly Color ColPanel = new(20, 25, 35, 220);
    private static readonly Color ColSlider = new(60, 120, 200);
    private static readonly Color ColBg = new(40, 40, 50);
    private static readonly Color ColBtn = new(45, 90, 160);
    private static readonly Color ColBtnHover = new(70, 130, 210);
    private static readonly Color ColText = Color.White;
    private static readonly Color ColSubtext = new(180, 180, 200);

    public SettingsMenu(GraphicsDevice gd, GraphicsDeviceManager graphics,
        SettingsManager settings, SoundManager sound, SpriteFont font,
        int screenW, int screenH)
    {
        _graphics = graphics;
        _settings = settings;
        _sound = sound;
        _font = font;
        _sw = screenW;
        _sh = screenH;

        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _resIndex = 0;
        for (int i = 0; i < SettingsManager.Resolutions.Length; i++)
        {
            var (w, h) = SettingsManager.Resolutions[i];
            if (w == settings.ResolutionWidth && h == settings.ResolutionHeight)
            {
                _resIndex = i;
                break;
            }
        }

        BuildLayout(screenW, screenH);
    }

    public void Resize(int screenW, int screenH)
    {
        if (_sw == screenW && _sh == screenH)
            return;

        _sw = screenW;
        _sh = screenH;
        BuildLayout(screenW, screenH);
    }

    public SettingsAction Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var mouse = Mouse.GetState();
        var kb = Keyboard.GetState();
        Point mp = mouse.Position;

        UpdateSlider(ref _sliderMaster, mouse);
        UpdateSlider(ref _sliderMusic, mouse);
        UpdateSlider(ref _sliderSfx, mouse);

        _sound.MasterVolume = _sliderMaster.Value;
        _sound.MusicVolume = _sliderMusic.Value;
        _sound.SfxVolume = _sliderSfx.Value;
        _settings.MasterVolume = _sliderMaster.Value;
        _settings.MusicVolume = _sliderMusic.Value;
        _settings.SfxVolume = _sliderSfx.Value;

        bool clicked = mouse.LeftButton == ButtonState.Released
                       && _prevMouse.LeftButton == ButtonState.Pressed;

        bool hBack = _btnBack.Contains(mp);
        _scaleBack = Lerp(_scaleBack, hBack ? 1.06f : 1f, 8f * dt);

        if (clicked)
        {
            if (_btnResolution.Contains(mp))
            {
                _resIndex = (_resIndex + 1) % SettingsManager.Resolutions.Length;
                var (w, h) = SettingsManager.Resolutions[_resIndex];
                _settings.ResolutionWidth = w;
                _settings.ResolutionHeight = h;
                ApplyResolution();
            }

            if (_checkFullscreen.Contains(mp))
            {
                _settings.IsFullscreen = !_settings.IsFullscreen;
                ApplyResolution();
            }

            if (hBack)
            {
                _settings.Save();
                return SettingsAction.Back;
            }
        }

        if (kb.IsKeyDown(Keys.Escape) && !_prevKb.IsKeyDown(Keys.Escape))
        {
            _settings.Save();
            _prevKb = kb;
            return SettingsAction.Back;
        }

        _prevMouse = mouse;
        _prevKb = kb;
        return SettingsAction.None;
    }

    public void Draw(SpriteBatch sb)
    {
        sb.Draw(_pixel, new Rectangle(0, 0, _sw, _sh), Color.Black * 0.55f);

        DrawRect(sb, _panelRect, ColPanel);
        DrawRectBorder(sb, _panelRect, new Color(80, 100, 160), 2);

        DrawTextCentered(sb, "SETTINGS", _panelRect.X, _panelRect.Y + 22,
            _panelRect.Width, 1.6f, ColText);

        var (rw, rh) = SettingsManager.Resolutions[_resIndex];
        DrawLabel(sb, $"Resolution: {rw} x {rh}", _btnResolution.X, _btnResolution.Y - 24);
        DrawButtonRect(sb, _btnResolution, $"{rw} x {rh}  >");

        DrawLabel(sb, "Fullscreen", _checkFullscreen.X + 34, _checkFullscreen.Y);
        DrawCheckbox(sb, _checkFullscreen, _settings.IsFullscreen);

        DrawSlider(sb, _sliderMaster, "Master volume");
        DrawSlider(sb, _sliderMusic, "Music");
        DrawSlider(sb, _sliderSfx, "Sound effects");

        DrawScaledButton(sb, _btnBack, "Back", _scaleBack);
    }

    private void BuildLayout(int sw, int sh)
    {
        int px = (sw - PanelW) / 2;
        int py = (sh - PanelH) / 2;
        _panelRect = new Rectangle(px, py, PanelW, PanelH);

        int cx = px + 40;
        int sliderW = PanelW - 80;
        int y = py + 95;

        _btnResolution = new Rectangle(cx, y, sliderW, 40);
        y += 72;

        _checkFullscreen = new Rectangle(cx, y, 24, 24);
        y += 62;

        _sliderMaster = new SliderState
        {
            Track = new Rectangle(cx, y, sliderW, 10),
            Value = _settings.MasterVolume
        };
        y += 58;

        _sliderMusic = new SliderState
        {
            Track = new Rectangle(cx, y, sliderW, 10),
            Value = _settings.MusicVolume
        };
        y += 58;

        _sliderSfx = new SliderState
        {
            Track = new Rectangle(cx, y, sliderW, 10),
            Value = _settings.SfxVolume
        };

        int bw = 160;
        int bh = 44;
        _btnBack = new Rectangle(px + (PanelW - bw) / 2, py + PanelH - 64, bw, bh);
    }

    private void ApplyResolution()
    {
        _graphics.PreferredBackBufferWidth = _settings.ResolutionWidth;
        _graphics.PreferredBackBufferHeight = _settings.ResolutionHeight;
        _graphics.IsFullScreen = _settings.IsFullscreen;
        _graphics.ApplyChanges();
        Resize(_graphics.GraphicsDevice.Viewport.Width,
            _graphics.GraphicsDevice.Viewport.Height);
    }

    private struct SliderState
    {
        public Rectangle Track;
        public float Value;
        public bool Dragging;
    }

    private void UpdateSlider(ref SliderState s, MouseState mouse)
    {
        int knobX = s.Track.X + (int)(s.Value * s.Track.Width);
        var knobRect = new Rectangle(knobX - 8, s.Track.Y - 8, 16, 26);

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            if (!s.Dragging && knobRect.Contains(mouse.Position))
                s.Dragging = true;

            if (s.Dragging)
            {
                float raw = (mouse.Position.X - s.Track.X) / (float)s.Track.Width;
                s.Value = MathHelper.Clamp(raw, 0f, 1f);
            }
        }
        else
        {
            s.Dragging = false;
        }
    }

    private void DrawSlider(SpriteBatch sb, SliderState s, string label)
    {
        int labelY = s.Track.Y - 24;
        DrawLabel(sb, label, s.Track.X, labelY);
        string pct = $"{(int)(s.Value * 100)}%";
        DrawLabel(sb, pct, s.Track.Right - 48, labelY, ColSubtext);

        DrawRect(sb, s.Track, ColBg);

        int fillW = (int)(s.Value * s.Track.Width);
        if (fillW > 0)
            DrawRect(sb, new Rectangle(s.Track.X, s.Track.Y, fillW, s.Track.Height), ColSlider);

        int kx = s.Track.X + fillW - 8;
        DrawRect(sb, new Rectangle(kx, s.Track.Y - 7, 16, 24), Color.White);
        DrawRectBorder(sb, new Rectangle(kx, s.Track.Y - 7, 16, 24), ColSlider, 1);
    }

    private void DrawCheckbox(SpriteBatch sb, Rectangle r, bool checked_)
    {
        DrawRectBorder(sb, r, ColSubtext, 2);
        if (checked_)
        {
            var inner = new Rectangle(r.X + 5, r.Y + 5, r.Width - 10, r.Height - 10);
            DrawRect(sb, inner, ColSlider);
        }
    }

    private void DrawButtonRect(SpriteBatch sb, Rectangle r, string text)
    {
        bool hover = r.Contains(Mouse.GetState().Position);
        DrawRect(sb, r, hover ? ColBtnHover : ColBtn);
        DrawRectBorder(sb, r, ColSlider, 1);
        DrawTextCentered(sb, text, r.X, r.Y + 10, r.Width, 1f, ColText);
    }

    private void DrawScaledButton(SpriteBatch sb, Rectangle r, string text, float scale)
    {
        int w = (int)(r.Width * scale);
        int h = (int)(r.Height * scale);
        int ox = (r.Width - w) / 2;
        int oy = (r.Height - h) / 2;
        var scaled = new Rectangle(r.X + ox, r.Y + oy, w, h);
        bool hover = r.Contains(Mouse.GetState().Position);
        DrawRect(sb, scaled, hover ? ColBtnHover : ColBtn);
        DrawRectBorder(sb, scaled, ColSlider, 1);
        DrawTextCentered(sb, text, scaled.X, scaled.Y + scaled.Height / 2 - 8,
            scaled.Width, 1f, ColText);
    }

    private void DrawRect(SpriteBatch sb, Rectangle r, Color c)
        => sb.Draw(_pixel, r, c);

    private void DrawRectBorder(SpriteBatch sb, Rectangle r, Color c, int thickness)
    {
        sb.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
        sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), c);
        sb.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
        sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), c);
    }

    private void DrawLabel(SpriteBatch sb, string text, int x, int y,
        Color? color = null)
    {
        var c = color ?? ColText;
        sb.DrawString(_font, text, new Vector2(x, y), c);
    }

    private void DrawTextCentered(SpriteBatch sb, string text,
        int rx, int ry, int rw, float scale, Color color)
    {
        Vector2 size = _font.MeasureString(text) * scale;
        float x = rx + (rw - size.X) / 2f;
        sb.DrawString(_font, text, new Vector2(x, ry), color, 0f,
            Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private static float Lerp(float a, float b, float t)
        => a + (b - a) * MathHelper.Clamp(t, 0f, 1f);
}
