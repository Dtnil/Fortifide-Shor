using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;
using System.IO;

namespace Fortifid.Systems;
/// <summary>
/// Підгружає текстури з дериккторії Content
///  та надае до них доступ за назвою ключа.
/// </summary>
public class TextureManager
{
  /// <summary>
  /// Приватний словник текстур
  /// </summary>
  private readonly Dictionary<string, Texture2D> _textures= new();

  private const string TextureFolder = "Textures";

  public TextureManager(ContentManager content)
  {
    LoadALLTextures(content);
  }

  public Texture2D Get(string name)
  {
    if (_textures.TryGetValue(name, out Texture2D? tex))
      return tex;
    
    throw new KeyNotFoundException(
      $"[TextureManager] Текстура '{{name}}' не знайдена.");
  }
  public Texture2D? TryGet(string name) => _textures.GetValueOrDefault(name);
  
  public bool Has(string name) => _textures.ContainsKey(name);
  
  public int Count => _textures.Count;

  private void LoadALLTextures(ContentManager content)
  {
    string contentRoot = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      content.RootDirectory);
    
    string texturesRoot = Path.Combine(contentRoot, TextureFolder);

    if (!Directory.Exists(texturesRoot))
    {
      Console.WriteLine($"[TextureManager] Папака не знайдена: {texturesRoot}");
    }
    
    string[] xnbFiles = Directory.GetFiles(
      texturesRoot, "*.xnb", 
      SearchOption.AllDirectories);

    foreach (string fullPath in xnbFiles)
    {
      try
      {
        string relativePath = Path.GetRelativePath(contentRoot, fullPath);
        
        string assetPath = Path.Combine(relativePath, null)
          .Replace('\\', '/');
        
        string key = Path.GetFileNameWithoutExtension(fullPath);

        if (_textures.ContainsKey(key))
        {
          Console.WriteLine($"[TextureManager] дублікат імені '{key}'");
          continue;
        }
        
        _textures[key] = content.Load<Texture2D>(assetPath);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"TextureManager] помилка завантаження '{fullPath}': {ex.Message}'");
      }
    }
    Console.WriteLine($"TextureManager] Завантаженно {_textures.Count} Текстур");
  }
}