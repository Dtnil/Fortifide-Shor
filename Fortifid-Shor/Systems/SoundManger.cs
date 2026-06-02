using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Fortifid.World;

namespace Fortifid.Systems;

public class SoundManger
{
    private float _masterVolume = 1.0f;
    private float _musicVolume = 0.6f;
    private float _sfxVolume    = 0.8f;

    public float MasterVolume
    {
        get => _masterVolume;
        set { _masterVolume = MathHelper.Clamp(value, 0f, 1f); ApplyVolumes(); }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set { _musicVolume = MathHelper.Clamp(value, 0f, 1f); ApplyVolumes(); }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set { _sfxVolume = MathHelper.Clamp(value, 0f, 1f); }
    }
    
    private Song _currentSong;
    
    private TileType? _currentAmbientTile = null;

    private readonly Dictionary<TileType, Song> _ambientSongs = new();
    private readonly Dictionary<TileType, SoundEffect> _footstepSfx = new();
    
    private float _footstepTimer     = 0f;
    private const float FootstepInterval = 0.45f;

    public void LoadContent(ContentManager content)
    {
        // -- Ambient музика --
        TryLoadSong(content, TileType.Grass,  "Sounds/Environment/Forest");
        TryLoadSong(content, TileType.Shore,  "Sounds/Environment/Waves");
        TryLoadSong(content, TileType.Water,  "Sounds/Environment/Waves");
        TryLoadSong(content, TileType.Sand,   "Sounds/Environment/Waves");

        // -- Звуки кроків --
        TryLoadFootstep(content, TileType.Sand,  "Sounds/Walk/Footsteps_on_sand");
        TryLoadFootstep(content, TileType.Grass, "Sounds/Walk/Footsteps_on_grass");
        TryLoadFootstep(content, TileType.Shore, "Sounds/Walk/Footsteps_on_sand");

        // Налаштовуємо MediaPlayer
        MediaPlayer.IsRepeating = true;
        ApplyVolumes();
    }

    public void Update(TileType tileUnderPlayer, bool isMoving, float dt)
    {
        UpdateAmbient(tileUnderPlayer);
        if (isMoving)
            UpdateFootsteps(tileUnderPlayer, dt);
        else
            _footstepTimer = 0f;
    }

    public void PlaySfx(SoundEffect sfx, float pitchVariance = 0f)
    {
        if(sfx == null) return;
        float pitch = pitchVariance > 0f
            ? (float)(new Random().NextDouble() * pitchVariance * 2 - pitchVariance)
            : 0f;
        sfx.Play(_sfxVolume * _masterVolume, pitch, 0f);
    }

    public void StopMusic()
    {
        MediaPlayer.Stop();
        _currentSong = null;
        _currentAmbientTile = null;
    }

    private void UpdateAmbient(TileType tile)
    {
        TileType key = tile;
        
        if (!_ambientSongs.ContainsKey(key))
            key = TileType.Grass;
        
        if (_currentAmbientTile == key) return;
        _currentAmbientTile = key;

        if (_ambientSongs.TryGetValue(key, out Song song))
        {
            if (_currentSong == song) return;
            _currentSong = song;
            MediaPlayer.Play(song);
            ApplyVolumes();
        }
    }

    private void UpdateFootsteps(TileType tile, float dt)
    {
        _footstepTimer += dt;
        if (_footstepTimer < FootstepInterval) return;
        _footstepTimer = 0f;

        if (_footstepSfx.TryGetValue(tile, out SoundEffect sfx))
            PlaySfx(sfx, 0.15f);
        else if (_footstepSfx.TryGetValue(TileType.Sand, out SoundEffect fallback))
            PlaySfx(fallback, 0.15f);
    }
    
    private void ApplyVolumes()
    {
        MediaPlayer.Volume = _musicVolume * _masterVolume;
        SoundEffect.MasterVolume = _sfxVolume * _masterVolume;
    }

    private void TryLoadSong(ContentManager content, TileType tile, string path)
    {
        try   { _ambientSongs[tile] = content.Load<Song>(path); }
        catch { Console.WriteLine($"[SoundManager] Не знайдено пісню: {path}"); }
    }
    
    private void TryLoadFootstep(ContentManager content, TileType tile, string path)
    {
        try   { _footstepSfx[tile] = content.Load<SoundEffect>(path); }
        catch { Console.WriteLine($"[SoundManager] Не знайдено SFX: {path}"); }
    }
    
}