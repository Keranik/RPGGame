namespace RPGGame.Core.Village;

/// <summary>
/// Extension methods for BuildingSize enum.
/// </summary>
public static class BuildingSizeExtensions {
	/// <summary>
	/// Gets the tile dimensions for a building size.
	/// </summary>
	public static (int width, int height) GetDimensions(this BuildingSize size) {
		return size switch {
			BuildingSize.Small => (1, 1),
			BuildingSize.Medium => (2, 2),
			BuildingSize.Large => (3, 3),
			BuildingSize.Special => (2, 2), // Special buildings define their own size
			_ => (1, 1)
		};
	}

	/// <summary>
	/// Gets the tile width for a building size.
	/// </summary>
	public static int GetWidth(this BuildingSize size) => GetDimensions(size).width;

	/// <summary>
	/// Gets the tile height for a building size.
	/// </summary>
	public static int GetHeight(this BuildingSize size) => GetDimensions(size).height;

	/// <summary>
	/// Gets the total tile count for a building size.
	/// </summary>
	public static int GetTileCount(this BuildingSize size) {
		var (width, height) = GetDimensions(size);
		return width * height;
	}

	/// <summary>
	/// Gets a display name for the building size.
	/// </summary>
	public static string GetDisplayName(this BuildingSize size) {
		return size switch {
			BuildingSize.Small => "Small (1×1)",
			BuildingSize.Medium => "Medium (2×2)",
			BuildingSize.Large => "Large (3×3)",
			BuildingSize.Special => "Special",
			_ => "Unknown"
		};
	}
}