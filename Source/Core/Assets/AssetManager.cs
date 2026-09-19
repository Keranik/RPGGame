using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Spells;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace RPGGame.Core.Assets;

/// <summary>
/// Asset categories for organization and preloading.
/// </summary>
public enum AssetCategory {
	Icons,
	Portraits,
	Backgrounds,
	Audio,
	Cursors,
	Fonts,
	Sprites,
	Effects,
	Terrain
}

/// <summary>
/// Centralized asset management for the game.
/// Handles loading, caching, and providing access to all game assets.
/// </summary>
[Dependency(registrationType: RegistrationType.Singleton)]
public class AssetManager {
	#region Fields

	private readonly Dictionary<string, Texture2D> k_textures = [];
	private readonly Dictionary<string, Sprite> k_sprites = [];
	private readonly Dictionary<string, AudioClip> k_audioClips = [];
	private readonly Dictionary<string, Font> k_fonts = [];
	private readonly Dictionary<string, StyleSheet> k_styleSheets = [];
	private readonly Dictionary<string, VectorImage> k_vectorImages = [];
	private readonly Dictionary<string, FontAsset> k_fontAssets = [];

	// Terrain graphics cache - keyed by TerrainProto.ID
	private readonly Dictionary<TerrainProto.ID, TerrainTileGfx> k_terrainGfx = [];
	private bool k_terrainGfxLoaded;

	private readonly HashSet<string> k_preloadedCategories = [];
	private readonly Dictionary<string, Texture2D> k_fallbackTextures = [];

	private bool k_isInitialized;

	#endregion

	#region Paths

	// Base paths for different asset types
	private const string ICONS_PATH = "Icons/";
	private const string PORTRAITS_PATH = "Portraits/";
	private const string BACKGROUNDS_PATH = "Backgrounds/";
	private const string AUDIO_PATH = "Audio/";
	private const string CURSORS_PATH = "Cursors/";
	private const string FONTS_PATH = "Fonts/";
	private const string SPRITES_PATH = "Sprites/";
	private const string EFFECTS_PATH = "Effects/";
	private const string TERRAIN_TILES_PATH = "Sprites/Isometric/Tiles/";

	#endregion

	#region Initialization

	public AssetManager() {
		Debug.Log("AssetManager: Initialized");
	}

	/// <summary>
	/// Initializes the asset manager and creates fallback assets.
	/// Call this during game startup.
	/// </summary>
	public void Initialize() {
		if (k_isInitialized) {
			return;
		}

		CreateFallbackAssets();
		k_isInitialized = true;

		Debug.Log("AssetManager: Ready");
	}

	private void CreateFallbackAssets() {
		// Create a simple fallback texture (magenta square for missing textures)
		var fallback = new Texture2D(32, 32);
		var pixels = new Color[32 * 32];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = new Color(1f, 0f, 1f, 1f); // Magenta
		}
		fallback.SetPixels(pixels);
		fallback.Apply();
		k_fallbackTextures["default"] = fallback;

		// Transparent fallback
		var transparent = new Texture2D(1, 1);
		transparent.SetPixel(0, 0, Color.clear);
		transparent.Apply();
		k_fallbackTextures["transparent"] = transparent;
	}

	#endregion

	#region Preloading

	/// <summary>
	/// Preloads all assets in a category for faster access.
	/// </summary>
	public void PreloadCategory(AssetCategory category) {
		string categoryName = category.ToString();
		if (k_preloadedCategories.Contains(categoryName)) {
			return;
		}

		string path = GetCategoryPath(category);

		switch (category) {
			case AssetCategory.Icons:
			case AssetCategory.Portraits:
			case AssetCategory.Backgrounds:
			case AssetCategory.Sprites:
				PreloadSprites(path);
				break;
			case AssetCategory.Audio:
				PreloadAudio(path);
				break;
			case AssetCategory.Fonts:
				PreloadFonts(path);
				break;
			case AssetCategory.Terrain:
				PreloadAllTerrainGraphics();
				break;
		}

		k_preloadedCategories.Add(categoryName);
		Debug.Log($"AssetManager: Preloaded category {category}");
	}

	/// <summary>
	/// Preloads commonly used assets for UI.
	/// </summary>
	public void PreloadCommonAssets() {
		PreloadCategory(AssetCategory.Icons);
		PreloadCategory(AssetCategory.Fonts);
	}

	private void PreloadSprites(string path) {
		var sprites = UnityEngine.Resources.LoadAll<Sprite>(path);
		foreach (var sprite in sprites) {
			string key = $"{path}{sprite.name}";
			k_sprites[key] = sprite;
			if (sprite.texture != null) {
				k_textures[key] = sprite.texture;
			}
		}
	}

	private void PreloadAudio(string path) {
		var clips = UnityEngine.Resources.LoadAll<AudioClip>(path);
		foreach (var clip in clips) {
			k_audioClips[$"{path}{clip.name}"] = clip;
		}
	}

	private void PreloadFonts(string path) {
		var fonts = UnityEngine.Resources.LoadAll<Font>(path);
		foreach (var font in fonts) {
			k_fonts[$"{path}{font.name}"] = font;
		}
	}

	private static string GetCategoryPath(AssetCategory category) => category switch {
		AssetCategory.Icons => ICONS_PATH,
		AssetCategory.Portraits => PORTRAITS_PATH,
		AssetCategory.Backgrounds => BACKGROUNDS_PATH,
		AssetCategory.Audio => AUDIO_PATH,
		AssetCategory.Cursors => CURSORS_PATH,
		AssetCategory.Fonts => FONTS_PATH,
		AssetCategory.Sprites => SPRITES_PATH,
		AssetCategory.Effects => EFFECTS_PATH,
		AssetCategory.Terrain => TERRAIN_TILES_PATH,
		_ => ""
	};

	#endregion

	#region Terrain Graphics

	/// <summary>
	/// Preloads all terrain graphics from the Tiles folder structure.
	/// Call this during game initialization for best performance.
	/// </summary>
	public void PreloadAllTerrainGraphics() {
		if (k_terrainGfxLoaded) {
			return;
		}

		Debug.Log("AssetManager: Loading terrain graphics...");
		int loadedCount = 0;

		// Load all sprites from the terrain tiles path
		var allSprites = UnityEngine.Resources.LoadAll<Sprite>(TERRAIN_TILES_PATH);

		// Group sprites by terrain folder name (first part before _Top_ or _Side_)
		var spritesByTerrain = new Dictionary<string, List<Sprite>>();

		foreach (var sprite in allSprites) {
			// Parse the sprite path to extract terrain name
			// Sprites are named like: Grass_Top_1, Grass_Side_L1, DeepForest_Top_2, etc.
			string spriteName = sprite.name;
			
			// Find the terrain name by looking for _Top_ or _Side_
			string terrainName = ExtractTerrainNameFromSprite(spriteName);
			if (string.IsNullOrEmpty(terrainName)) {
				continue;
			}

			if (!spritesByTerrain.TryGetValue(terrainName, out var spriteList)) {
				spriteList = [];
				spritesByTerrain[terrainName] = spriteList;
			}
			spriteList.Add(sprite);
		}

		// Process each terrain's sprites
		foreach (var (terrainName, sprites) in spritesByTerrain) {
			// Build the TerrainProto.ID from the folder name
			var terrainId = BuildTerrainIdFromFolderName(terrainName);

			var topSprites = new List<Sprite>();
			var sideLeftSprites = new List<Sprite>();
			var sideRightSprites = new List<Sprite>();

			foreach (var sprite in sprites) {
				string spriteName = sprite.name;

				if (spriteName.Contains("_Top_")) {
					topSprites.Add(sprite);
				} else if (spriteName.Contains("_Side_L")) {
					sideLeftSprites.Add(sprite);
				} else if (spriteName.Contains("_Side_R")) {
					sideRightSprites.Add(sprite);
				}
			}

			// Sort by variant number for consistent ordering
			topSprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
			sideLeftSprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
			sideRightSprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

			var gfx = new TerrainTileGfx(
				terrainId,
				topSprites.ToArray(),
				sideLeftSprites.ToArray(),
				sideRightSprites.ToArray()
			);

			k_terrainGfx[terrainId] = gfx;
			loadedCount++;
		}

		k_terrainGfxLoaded = true;
		Debug.Log($"AssetManager: Loaded {loadedCount} terrain graphics sets");
	}

	/// <summary>
	/// Extracts the terrain name from a sprite filename.
	/// E.g., "Grass_Top_1" -> "Grass", "DeepForest_Side_L2" -> "DeepForest"
	/// </summary>
	private static string ExtractTerrainNameFromSprite(string spriteName) {
		// Find _Top_ or _Side_ and take everything before it
		int topIndex = spriteName.IndexOf("_Top_", StringComparison.Ordinal);
		if (topIndex > 0) {
			return spriteName[..topIndex];
		}

		int sideIndex = spriteName.IndexOf("_Side_", StringComparison.Ordinal);
		if (sideIndex > 0) {
			return spriteName[..sideIndex];
		}

		return string.Empty;
	}

	/// <summary>
	/// Builds a TerrainProto.ID from a folder/sprite name.
	/// The folder name IS the last segment of the ID.
	/// E.g., "Grass" -> TerrainProto.ID where the folder is looked up.
	/// 
	/// This works because we store by the ID we build, and retrieve by the same ID
	/// that comes from Ids.Terrains which follows the pattern "Terrain_Category_Name".
	/// </summary>
	private static TerrainProto.ID BuildTerrainIdFromFolderName(string folderName) {
		// We create an ID that will match when looked up.
		// The actual TerrainProto.ID from Ids.Terrains will have format "Terrain_Category_Name"
		// But we don't know the category from just the folder name.
		// 
		// Solution: Store a reverse lookup OR use just the terrain name as a secondary key.
		// For now, we'll create a simple ID and rely on the lookup method to handle matching.
		//
		// When GetTerrainGraphics is called with a real ID like "Terrain_Plains_Grass",
		// we extract "Grass" and look it up.
		
		// Store with a normalized key that can be matched
		return new TerrainProto.ID($"Terrain_{folderName}");
	}

	/// <summary>
	/// Loads terrain graphics for a specific terrain on-demand.
	/// </summary>
	/// <param name="terrainId">The terrain ID to load graphics for.</param>
	/// <returns>The loaded terrain graphics, or Empty if not found.</returns>
	public TerrainTileGfx LoadTerrainGraphics(TerrainProto.ID terrainId) {
		// Check cache first with exact ID
		if (k_terrainGfx.TryGetValue(terrainId, out var cached)) {
			return cached;
		}

		// Extract the folder name from the terrain ID and try that
		string folderName = ExtractTerrainFolderName(terrainId);
		if (string.IsNullOrEmpty(folderName)) {
			Debug.LogWarning($"AssetManager: Could not determine folder name for terrain '{terrainId.Value}'");
			return TerrainTileGfx.Empty;
		}

		// Try lookup by folder name (for preloaded terrains with simplified IDs)
		var simplifiedId = new TerrainProto.ID($"Terrain_{folderName}");
		if (k_terrainGfx.TryGetValue(simplifiedId, out cached)) {
			// Cache under the full ID for faster future lookups
			k_terrainGfx[terrainId] = cached;
			return cached;
		}

		// Not preloaded, try to load on-demand
		string basePath = $"{TERRAIN_TILES_PATH}{folderName}/";

		// Load top sprites
		var topSprites = UnityEngine.Resources.LoadAll<Sprite>($"{basePath}Top")
			.OrderBy(s => s.name)
			.ToArray();

		// Load side sprites
		var sideSprites = UnityEngine.Resources.LoadAll<Sprite>($"{basePath}Side");
		var sideLeft = sideSprites
			.Where(s => s.name.Contains("_L"))
			.OrderBy(s => s.name)
			.ToArray();
		var sideRight = sideSprites
			.Where(s => s.name.Contains("_R"))
			.OrderBy(s => s.name)
			.ToArray();

		var gfx = new TerrainTileGfx(terrainId, topSprites, sideLeft, sideRight);
		
		// Cache under both the full ID and simplified ID
		k_terrainGfx[terrainId] = gfx;
		k_terrainGfx[simplifiedId] = gfx;

		if (!gfx.IsValid) {
			Debug.LogWarning($"AssetManager: No graphics found for terrain '{terrainId.Value}' at path '{basePath}'");
		}

		return gfx;
	}

	/// <summary>
	/// Gets terrain graphics by ID. Returns cached or loads on-demand.
	/// </summary>
	/// <param name="terrainId">The terrain ID.</param>
	/// <returns>The terrain graphics, or Empty if not found.</returns>
	public TerrainTileGfx GetTerrainGraphics(TerrainProto.ID terrainId) {
		if (k_terrainGfx.TryGetValue(terrainId, out var gfx)) {
			return gfx;
		}
		return LoadTerrainGraphics(terrainId);
	}

	/// <summary>
	/// Tries to get terrain graphics by ID.
	/// </summary>
	/// <param name="terrainId">The terrain ID.</param>
	/// <param name="gfx">The terrain graphics if found.</param>
	/// <returns>True if found and valid, false otherwise.</returns>
	public bool TryGetTerrainGraphics(TerrainProto.ID terrainId, out TerrainTileGfx gfx) {
		gfx = GetTerrainGraphics(terrainId);
		return gfx.IsValid;
	}

	/// <summary>
	/// Gets all loaded terrain graphics.
	/// </summary>
	public IReadOnlyDictionary<TerrainProto.ID, TerrainTileGfx> AllTerrainGraphics => k_terrainGfx;

	/// <summary>
	/// Gets the count of loaded terrain graphics.
	/// </summary>
	public int TerrainGraphicsCount => k_terrainGfx.Count;

	/// <summary>
	/// Extracts the folder name (last segment) from a TerrainProto.ID.
	/// IDs follow the pattern "Terrain_Category_Name" (e.g., "Terrain_Plains_Grass" -> "Grass").
	/// Handles any depth of underscores by always taking the last segment.
	/// </summary>
	private static string ExtractTerrainFolderName(TerrainProto.ID terrainId) {
		string idValue = terrainId.Value;
		if (string.IsNullOrEmpty(idValue)) {
			return string.Empty;
		}

		// Find the last underscore and take everything after it
		int lastUnderscore = idValue.LastIndexOf('_');
		if (lastUnderscore >= 0 && lastUnderscore < idValue.Length - 1) {
			return idValue[(lastUnderscore + 1)..];
		}

		// No underscore found, return the whole value
		return idValue;
	}

	#endregion

	#region Icons

	/// <summary>
	/// Gets an icon by name. Returns fallback if not found.
	/// </summary>
	public Sprite? GetIcon(string iconName) {
		if (string.IsNullOrEmpty(iconName)) {
			return null;
		}

		string key = $"{ICONS_PATH}{iconName}";

		if (k_sprites.TryGetValue(key, out var cached)) {
			return cached;
		}

		var sprite = UnityEngine.Resources.Load<Sprite>(key);
		if (sprite != null) {
			k_sprites[key] = sprite;
			return sprite;
		}

		Debug.LogWarning($"AssetManager: Icon not found: {iconName}");
		return null;
	}

	/// <summary>
	/// Gets an icon texture for use in UI Toolkit.
	/// </summary>
	public Texture2D? GetIconTexture(string iconName) {
		var sprite = GetIcon(iconName);
		return sprite?.texture;
	}

	/// <summary>
	/// Gets a VectorImage icon for UI Toolkit (SVG-based icons).
	/// </summary>
	public VectorImage? GetVectorIcon(string iconName) {
		if (string.IsNullOrEmpty(iconName)) {
			return null;
		}

		string key = $"{ICONS_PATH}{iconName}";

		if (k_vectorImages.TryGetValue(key, out var cached)) {
			return cached;
		}

		var vector = UnityEngine.Resources.Load<VectorImage>(key);
		if (vector != null) {
			k_vectorImages[key] = vector;
			return vector;
		}

		return null;
	}

	#endregion

	#region Portraits

	/// <summary>
	/// Gets a character portrait by name.
	/// </summary>
	public Sprite? GetPortrait(string portraitName) {
		if (string.IsNullOrEmpty(portraitName)) {
			return null;
		}

		string key = $"{PORTRAITS_PATH}{portraitName}";

		if (k_sprites.TryGetValue(key, out var cached)) {
			return cached;
		}

		var sprite = UnityEngine.Resources.Load<Sprite>(key);
		if (sprite != null) {
			k_sprites[key] = sprite;
			return sprite;
		}

		Debug.LogWarning($"AssetManager: Portrait not found: {portraitName}");
		return null;
	}

	/// <summary>
	/// Gets a portrait texture for UI Toolkit.
	/// </summary>
	public Texture2D? GetPortraitTexture(string portraitName) {
		var sprite = GetPortrait(portraitName);
		return sprite?.texture;
	}

	#endregion

	#region Backgrounds

	/// <summary>
	/// Gets a background image by name.
	/// </summary>
	public Texture2D? GetBackground(string backgroundName) {
		if (string.IsNullOrEmpty(backgroundName)) {
			return null;
		}

		string key = $"{BACKGROUNDS_PATH}{backgroundName}";

		if (k_textures.TryGetValue(key, out var cached)) {
			return cached;
		}

		var texture = UnityEngine.Resources.Load<Texture2D>(key);
		if (texture != null) {
			k_textures[key] = texture;
			return texture;
		}

		Debug.LogWarning($"AssetManager: Background not found: {backgroundName}");
		return null;
	}

	#endregion

	#region Audio

	/// <summary>
	/// Gets an audio clip by name.
	/// </summary>
	public AudioClip? GetAudioClip(string clipName) {
		if (string.IsNullOrEmpty(clipName)) {
			return null;
		}

		string key = $"{AUDIO_PATH}{clipName}";

		if (k_audioClips.TryGetValue(key, out var cached)) {
			return cached;
		}

		var clip = UnityEngine.Resources.Load<AudioClip>(key);
		if (clip != null) {
			k_audioClips[key] = clip;
			return clip;
		}

		Debug.LogWarning($"AssetManager: Audio clip not found: {clipName}");
		return null;
	}

	/// <summary>
	/// Gets a UI sound effect.
	/// </summary>
	public AudioClip? GetUISfx(string sfxName) {
		return GetAudioClip($"UI/{sfxName}");
	}

	/// <summary>
	/// Gets a combat sound effect.
	/// </summary>
	public AudioClip? GetCombatSfx(string sfxName) {
		return GetAudioClip($"Combat/{sfxName}");
	}

	/// <summary>
	/// Gets an ambient audio clip.
	/// </summary>
	public AudioClip? GetAmbient(string ambientName) {
		return GetAudioClip($"Ambient/{ambientName}");
	}

	/// <summary>
	/// Gets a music track.
	/// </summary>
	public AudioClip? GetMusic(string musicName) {
		return GetAudioClip($"Music/{musicName}");
	}

	#endregion

	#region Cursors

	/// <summary>
	/// Gets a cursor texture by name.
	/// </summary>
	public Texture2D? GetCursor(string cursorName) {
		if (string.IsNullOrEmpty(cursorName)) {
			return null;
		}

		string key = $"{CURSORS_PATH}{cursorName}";

		if (k_textures.TryGetValue(key, out var cached)) {
			return cached;
		}

		var texture = UnityEngine.Resources.Load<Texture2D>(key);
		if (texture != null) {
			k_textures[key] = texture;
			return texture;
		}

		return null;
	}

	/// <summary>
	/// Sets the game cursor.
	/// </summary>
	public void SetCursor(string cursorName, Vector2 hotspot = default) {
		var texture = GetCursor(cursorName);
		if (texture != null) {
			UnityEngine.Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
		}
	}

	/// <summary>
	/// Resets to the default system cursor.
	/// </summary>
	public void ResetCursor() {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	#endregion

	#region Fonts

	/// <summary>
	/// Gets a font by name.
	/// </summary>
	public Font? GetFont(string fontName) {
		if (string.IsNullOrEmpty(fontName)) {
			return null;
		}

		string key = $"{FONTS_PATH}{fontName}";

		if (k_fonts.TryGetValue(key, out var cached)) {
			return cached;
		}

		var font = UnityEngine.Resources.Load<Font>(key);
		if (font != null) {
			k_fonts[key] = font;
			return font;
		}

		return null;
	}

	#endregion

	#region Font Assets (SDF/TextMeshPro)

	// Add a cache for font assets by category for faster lookup
private readonly Dictionary<(FontCategory, FontVariant), FontAsset> k_fontAssetsByCategory = [];

/// <summary>
/// Gets a FontAsset (SDF font) by path for UI Toolkit.
/// </summary>
public FontAsset? GetFontAsset(string path) {
	if (string.IsNullOrEmpty(path)) {
		return null;
	}

	if (k_fontAssets.TryGetValue(path, out var cached)) {
		return cached;
	}

	var fontAsset = UnityEngine.Resources.Load<FontAsset>(path);
	if (fontAsset != null) {
		k_fontAssets[path] = fontAsset;
		return fontAsset;
	}

	Debug.LogWarning($"AssetManager: FontAsset not found: {path}");
	return null;
}

/// <summary>
/// Gets a font asset by category and variant.
/// Uses the current theme's typography settings for path lookup.
/// </summary>
public FontAsset? GetFontAsset(FontCategory category, FontVariant variant = FontVariant.Regular) {
	// Check category cache first
	var key = (category, variant);
	if (k_fontAssetsByCategory.TryGetValue(key, out var cached)) {
		return cached;
	}

	var typography = GameTheme.Current?.Typography;
	if (typography == null) return null;

	string path = typography.GetFontPath(category, variant);
	var fontAsset = GetFontAsset(path);

	if (fontAsset != null) {
		k_fontAssetsByCategory[key] = fontAsset;
	}

	return fontAsset;
}

/// <summary>
/// Gets the body font asset (Bookinsanity).
/// </summary>
public FontAsset? GetBodyFontAsset(FontVariant variant = FontVariant.Regular) {
	return GetFontAsset(FontCategory.Body, variant);
}

/// <summary>
/// Gets the heading font asset (Mr Eaves Small Caps).
/// </summary>
public FontAsset? GetHeadingFontAsset() {
	return GetFontAsset(FontCategory.Heading);
}

/// <summary>
/// Gets the table/stats font asset (Scaly Sans).
/// </summary>
public FontAsset? GetTableFontAsset(FontVariant variant = FontVariant.Regular) {
	return GetFontAsset(FontCategory.Table, variant);
}

/// <summary>
/// Gets the title font asset (Nodesto Caps Condensed).
/// </summary>
public FontAsset? GetTitleFontAsset(FontVariant variant = FontVariant.Regular) {
	return GetFontAsset(FontCategory.Title, variant);
}

/// <summary>
/// Gets the table header font asset (Zatanna Misdirection).
/// </summary>
public FontAsset? GetTableHeaderFontAsset() {
	return GetFontAsset(FontCategory.TableHeader);
}

/// <summary>
/// Gets the drop cap font asset (Solbera Imitation).
/// </summary>
public FontAsset? GetDropCapFontAsset() {
	return GetFontAsset(FontCategory.DropCap);
}

/// <summary>
/// Gets the monospace font asset (JetBrains Mono).
/// </summary>
public FontAsset? GetMonospaceFontAsset() {
	return GetFontAsset(FontCategory.Monospace);
}

/// <summary>
/// Preloads all font assets defined in the current theme.
/// Call this during game initialization for smoother UI.
/// </summary>
public void PreloadAllFonts() {
	var typography = GameTheme.Current?.Typography;
	if (typography == null) return;

	Debug.Log("AssetManager: Preloading fonts...");

	// Body fonts (Bookinsanity)
	GetFontAsset(typography.BodyFontPath);
	GetFontAsset(typography.BodyBoldFontPath);
	GetFontAsset(typography.BodyItalicFontPath);
	GetFontAsset(typography.BodyBoldItalicFontPath);

	// Heading font (Mr Eaves)
	GetFontAsset(typography.HeadingFontPath);

	// Table fonts (Scaly Sans)
	GetFontAsset(typography.TableFontPath);
	GetFontAsset(typography.TableBoldFontPath);
	GetFontAsset(typography.TableItalicFontPath);
	GetFontAsset(typography.TableBoldItalicFontPath);
	GetFontAsset(typography.TableCapsFontPath);

	// Title fonts (Nodesto)
	GetFontAsset(typography.TitleFontPath);
	GetFontAsset(typography.TitleBoldFontPath);

	// Special fonts
	GetFontAsset(typography.TableHeaderFontPath);
	GetFontAsset(typography.DropCapFontPath);
	GetFontAsset(typography.DropCapAltFontPath);
	GetFontAsset(typography.MonospaceFontAssetPath);

	Debug.Log($"AssetManager: Preloaded {k_fontAssets.Count} font assets");
}

/// <summary>
/// Clears the font category cache (call when theme changes).
/// </summary>
public void ClearFontCategoryCache() {
	k_fontAssetsByCategory.Clear();
}


	#endregion

	#region Sprites (General)

	/// <summary>
	/// Gets a sprite by full path.
	/// </summary>
	public Sprite? GetSprite(string path) {
		if (string.IsNullOrEmpty(path)) {
			return null;
		}

		if (k_sprites.TryGetValue(path, out var cached)) {
			return cached;
		}

		var sprite = UnityEngine.Resources.Load<Sprite>(path);
		if (sprite != null) {
			k_sprites[path] = sprite;
			return sprite;
		}

		return null;
	}

	/// <summary>
	/// Gets a texture by full path.
	/// </summary>
	public Texture2D? GetTexture(string path) {
		if (string.IsNullOrEmpty(path)) {
			return null;
		}

		if (k_textures.TryGetValue(path, out var cached)) {
			return cached;
		}

		var texture = UnityEngine.Resources.Load<Texture2D>(path);
		if (texture != null) {
			k_textures[path] = texture;
			return texture;
		}

		return null;
	}

	#endregion

	#region StyleSheets

	/// <summary>
	/// Gets a USS stylesheet by name.
	/// </summary>
	public StyleSheet? GetStyleSheet(string styleName) {
		if (string.IsNullOrEmpty(styleName)) {
			return null;
		}

		if (k_styleSheets.TryGetValue(styleName, out var cached)) {
			return cached;
		}

		var style = UnityEngine.Resources.Load<StyleSheet>($"Styles/{styleName}");
		if (style != null) {
			k_styleSheets[styleName] = style;
			return style;
		}

		return null;
	}

	#endregion

	#region Rarity Assets

	/// <summary>
	/// Gets an item frame sprite for a rarity.
	/// </summary>
	public Sprite? GetRarityFrame(RarityType rarity) {
		string frameName = rarity switch {
			RarityType.Common => "frame_common",
			RarityType.Uncommon => "frame_uncommon",
			RarityType.Rare => "frame_rare",
			RarityType.Epic => "frame_epic",
			RarityType.Legendary => "frame_legendary",
			RarityType.Mythic => "frame_mythic",
			_ => "frame_common"
		};

		return GetIcon(frameName);
	}

	/// <summary>
	/// Gets an item background for a rarity.
	/// </summary>
	public Sprite? GetRarityBackground(RarityType rarity) {
		string bgName = rarity switch {
			RarityType.Common => "bg_common",
			RarityType.Uncommon => "bg_uncommon",
			RarityType.Rare => "bg_rare",
			RarityType.Epic => "bg_epic",
			RarityType.Legendary => "bg_legendary",
			RarityType.Mythic => "bg_mythic",
			_ => "bg_common"
		};

		return GetIcon(bgName);
	}

	#endregion

	#region Item/Equipment Icons

	/// <summary>
	/// Gets an item icon by item type and name.
	/// </summary>
	public Sprite? GetItemIcon(ItemType itemType, string iconName) {
		string prefix = itemType switch {
			ItemType.Weapon => "weapon",
			ItemType.Armor => "armor",
			ItemType.Consumable => "consumable",
			ItemType.Material => "material",
			ItemType.Quest => "quest",
			ItemType.Miscellaneous => "misc",
			_ => "item"
		};

		return GetIcon($"items/{prefix}_{iconName}");
	}

	/// <summary>
	/// Gets a weapon type icon.
	/// </summary>
	public Sprite? GetWeaponTypeIcon(WeaponType weaponType) {
		string iconName = weaponType.ToString().ToLower();
		return GetIcon($"items/weapon_{iconName}");
	}

	/// <summary>
	/// Gets an armor type icon.
	/// </summary>
	public Sprite? GetArmorTypeIcon(ArmorType armorType) {
		string iconName = armorType.ToString().ToLower();
		return GetIcon($"items/armor_{iconName}");
	}

	/// <summary>
	/// Gets an equipment slot icon.
	/// </summary>
	public Sprite? GetSlotIcon(SlotType slotType) {
		string iconName = slotType.ToString().ToLower();
		return GetIcon($"slots/slot_{iconName}");
	}

	#endregion

	#region Spell/Skill Icons

	/// <summary>
	/// Gets a spell school icon.
	/// </summary>
	public Sprite? GetSpellSchoolIcon(SpellSchool school) {
		string iconName = school.ToString().ToLower();
		return GetIcon($"schools/{iconName}");
	}

	/// <summary>
	/// Gets a spell icon by name.
	/// </summary>
	public Sprite? GetSpellIcon(string spellIconName) {
		return GetIcon($"spells/{spellIconName}");
	}

	/// <summary>
	/// Gets a skill icon by name.
	/// </summary>
	public Sprite? GetSkillIcon(string skillIconName) {
		return GetIcon($"skills/{skillIconName}");
	}

	#endregion

	#region Status/Condition Icons

	/// <summary>
	/// Gets a status condition icon.
	/// </summary>
	public Sprite? GetStatusIcon(StatusCondition condition) {
		string iconName = condition.ToString().ToLower();
		return GetIcon($"status/{iconName}");
	}

	/// <summary>
	/// Gets a damage type icon.
	/// </summary>
	public Sprite? GetDamageTypeIcon(DamageType damageType) {
		string iconName = damageType.ToString().ToLower();
		return GetIcon($"damage/{iconName}");
	}

	#endregion

	#region Class Icons

	/// <summary>
	/// Gets a character class icon.
	/// </summary>
	public Sprite? GetClassIcon(string classId) {
		return GetIcon($"classes/{classId}");
	}

	#endregion

	#region Cache Management

	/// <summary>
	/// Clears all cached assets to free memory.
	/// </summary>
	public void ClearCache() {
		k_textures.Clear();
		k_sprites.Clear();
		k_audioClips.Clear();
		k_fonts.Clear();
		k_fontAssets.Clear();
		k_styleSheets.Clear();
		k_vectorImages.Clear();
		k_terrainGfx.Clear();
		k_terrainGfxLoaded = false;
		k_preloadedCategories.Clear();

		CreateFallbackAssets();

		UnityEngine.Resources.UnloadUnusedAssets();
		Debug.Log("AssetManager: Cache cleared");
	}

	/// <summary>
	/// Clears cached assets for a specific category.
	/// </summary>
	public void ClearCategoryCache(AssetCategory category) {
		string path = GetCategoryPath(category);

		// Remove matching entries
		var keysToRemove = new List<string>();

		foreach (var key in k_sprites.Keys) {
			if (key.StartsWith(path)) {
				keysToRemove.Add(key);
			}
		}
		foreach (var key in keysToRemove) {
			k_sprites.Remove(key);
		}

		keysToRemove.Clear();
		foreach (var key in k_textures.Keys) {
			if (key.StartsWith(path)) {
				keysToRemove.Add(key);
			}
		}
		foreach (var key in keysToRemove) {
			k_textures.Remove(key);
		}

		keysToRemove.Clear();
		foreach (var key in k_audioClips.Keys) {
			if (key.StartsWith(path)) {
				keysToRemove.Add(key);
			}
		}
		foreach (var key in keysToRemove) {
			k_audioClips.Remove(key);
		}

		if (category == AssetCategory.Terrain) {
			k_terrainGfx.Clear();
			k_terrainGfxLoaded = false;
		}

		k_preloadedCategories.Remove(category.ToString());

		Debug.Log($"AssetManager: Cleared cache for {category}");
	}

	/// <summary>
	/// Gets cache statistics for debugging.
	/// </summary>
	public (int textures, int sprites, int audio, int fonts, int terrain) GetCacheStats() {
		return (k_textures.Count, k_sprites.Count, k_audioClips.Count, k_fonts.Count, k_terrainGfx.Count);
	}

	#endregion

	#region Async Loading (Future)

	// TODO: Add async loading support for large assets
	// public async Task<Sprite?> GetIconAsync(string iconName) { ... }
	// public async Task PreloadCategoryAsync(AssetCategory category) { ... }

	#endregion
}