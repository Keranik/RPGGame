using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Manages fog of war - revealed and hidden tiles.
/// </summary>
public class FogOfWar {
	#region Fields

	private readonly HashSet<Vector2Int> k_revealedTiles = [];
	private readonly HashSet<Vector2Int> k_visibleTiles = [];
	private readonly HashSet<Vector2Int> k_permanentlyRevealed;

	private int k_baseRevealRadius = 2;

	#endregion

	#region Properties

	/// <summary>
	/// Number of tiles revealed this session.
	/// </summary>
	public int RevealedCount => k_revealedTiles.Count;

	/// <summary>
	/// Number of tiles currently visible.
	/// </summary>
	public int VisibleCount => k_visibleTiles.Count;

	/// <summary>
	/// All revealed tiles (including from previous runs).
	/// </summary>
	public IReadOnlyCollection<Vector2Int> RevealedTiles => k_revealedTiles;

	/// <summary>
	/// Currently visible tiles (within vision range).
	/// </summary>
	public IReadOnlyCollection<Vector2Int> VisibleTiles => k_visibleTiles;

	/// <summary>
	/// Base reveal radius (modified by stats).
	/// </summary>
	public int BaseRevealRadius {
		get => k_baseRevealRadius;
		set => k_baseRevealRadius = Math.Max(1, value);
	}

	#endregion

	#region Events

	/// <summary>
	/// Fired when new tiles are revealed.
	/// </summary>
	public event Action<IEnumerable<Vector2Int>>? OnTilesRevealed;

	/// <summary>
	/// Fired when visibility updates.
	/// </summary>
	public event Action? OnVisibilityChanged;

	#endregion

	#region Constructor

	public FogOfWar(IEnumerable<Vector2Int>? permanentlyRevealed = null) {
		k_permanentlyRevealed = permanentlyRevealed?.ToHashSet() ?? [];

		// Start with permanently revealed tiles
		foreach (var tile in k_permanentlyRevealed) {
			k_revealedTiles.Add(tile);
		}
	}

	#endregion

	#region Visibility

	/// <summary>
	/// Updates visibility based on player position.
	/// </summary>
	public void UpdateVisibility(Vector2Int playerPosition, int visionRadius) {
		k_visibleTiles.Clear();
		var newlyRevealed = new List<Vector2Int>();

		// Calculate visible tiles in a circle
		for (int dx = -visionRadius; dx <= visionRadius; dx++) {
			for (int dy = -visionRadius; dy <= visionRadius; dy++) {
				// Circular check
				if (dx * dx + dy * dy <= visionRadius * visionRadius) {
					var tile = new Vector2Int(playerPosition.x + dx, playerPosition.y + dy);
					k_visibleTiles.Add(tile);

					// Reveal if not already revealed
					if (k_revealedTiles.Add(tile)) {
						newlyRevealed.Add(tile);
					}
				}
			}
		}

		if (newlyRevealed.Count > 0) {
			OnTilesRevealed?.Invoke(newlyRevealed);
		}

		OnVisibilityChanged?.Invoke();
	}

	/// <summary>
	/// Reveals tiles around a position (e.g., from a reveal map item).
	/// </summary>
	public int RevealArea(Vector2Int center, int radius) {
		var newlyRevealed = new List<Vector2Int>();

		for (int dx = -radius; dx <= radius; dx++) {
			for (int dy = -radius; dy <= radius; dy++) {
				if (dx * dx + dy * dy <= radius * radius) {
					var tile = new Vector2Int(center.x + dx, center.y + dy);
					if (k_revealedTiles.Add(tile)) {
						newlyRevealed.Add(tile);
					}
				}
			}
		}

		if (newlyRevealed.Count > 0) {
			OnTilesRevealed?.Invoke(newlyRevealed);
		}

		return newlyRevealed.Count;
	}

	/// <summary>
	/// Reveals a specific tile.
	/// </summary>
	public bool RevealTile(Vector2Int tile) {
		if (k_revealedTiles.Add(tile)) {
			OnTilesRevealed?.Invoke([tile]);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Reveals a path between two points.
	/// </summary>
	public int RevealPath(Vector2Int from, Vector2Int to, int width = 1) {
		var newlyRevealed = new List<Vector2Int>();

		// Bresenham's line algorithm
		int dx = Math.Abs(to.x - from.x);
		int dy = Math.Abs(to.y - from.y);
		int sx = from.x < to.x ? 1 : -1;
		int sy = from.y < to.y ? 1 : -1;
		int err = dx - dy;

		int x = from.x;
		int y = from.y;

		while (true) {
			// Reveal tiles in width
			for (int wx = -width; wx <= width; wx++) {
				for (int wy = -width; wy <= width; wy++) {
					var tile = new Vector2Int(x + wx, y + wy);
					if (k_revealedTiles.Add(tile)) {
						newlyRevealed.Add(tile);
					}
				}
			}

			if (x == to.x && y == to.y) {
				break;
			}

			int e2 = 2 * err;
			if (e2 > -dy) {
				err -= dy;
				x += sx;
			}
			if (e2 < dx) {
				err += dx;
				y += sy;
			}
		}

		if (newlyRevealed.Count > 0) {
			OnTilesRevealed?.Invoke(newlyRevealed);
		}

		return newlyRevealed.Count;
	}

	#endregion

	#region Queries

	/// <summary>
	/// Checks if a tile has been revealed.
	/// </summary>
	public bool IsRevealed(Vector2Int tile) {
		return k_revealedTiles.Contains(tile);
	}

	/// <summary>
	/// Checks if a tile is currently visible.
	/// </summary>
	public bool IsVisible(Vector2Int tile) {
		return k_visibleTiles.Contains(tile);
	}

	/// <summary>
	/// Gets the visibility state of a tile.
	/// </summary>
	public FogState GetFogState(Vector2Int tile) {
		if (k_visibleTiles.Contains(tile)) {
			return FogState.Visible;
		}
		if (k_revealedTiles.Contains(tile)) {
			return FogState.Revealed;
		}
		return FogState.Hidden;
	}

	/// <summary>
	/// Gets tiles that were revealed this run (not from meta-progression).
	/// </summary>
	public IEnumerable<Vector2Int> GetNewlyRevealedTiles() {
		return k_revealedTiles.Except(k_permanentlyRevealed);
	}

	/// <summary>
	/// Gets the number of new tiles revealed this run.
	/// </summary>
	public int GetNewTilesCount() {
		return k_revealedTiles.Count - k_permanentlyRevealed.Count;
	}

	/// <summary>
	/// Checks if any adjacent tile is unrevealed (edge of explored area).
	/// </summary>
	public bool IsAtFogEdge(Vector2Int tile) {
		var neighbors = new Vector2Int[] {
			new(tile.x - 1, tile.y),
			new(tile.x + 1, tile.y),
			new(tile.x, tile.y - 1),
			new(tile.x, tile.y + 1)
		};

		return neighbors.Any(n => !k_revealedTiles.Contains(n));
	}

	/// <summary>
	/// Gets all tiles at the edge of revealed territory.
	/// </summary>
	public IEnumerable<Vector2Int> GetFogEdgeTiles() {
		return k_revealedTiles.Where(IsAtFogEdge);
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Gets all revealed tiles for saving.
	/// </summary>
	public HashSet<Vector2Int> GetAllRevealedTiles() {
		return [.. k_revealedTiles];
	}

	/// <summary>
	/// Loads revealed tiles from save data.
	/// </summary>
	public void LoadRevealedTiles(IEnumerable<Vector2Int> tiles) {
		foreach (var tile in tiles) {
			k_revealedTiles.Add(tile);
		}
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Visibility state of a tile.
/// </summary>
public enum FogState {
	/// <summary>Never seen, completely hidden.</summary>
	Hidden,
	/// <summary>Previously seen but not currently visible.</summary>
	Revealed,
	/// <summary>Currently visible.</summary>
	Visible
}

#endregion