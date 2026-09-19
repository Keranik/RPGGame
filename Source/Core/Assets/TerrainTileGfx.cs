using RPGGame.Core.Prototypes.Expedition;
using UnityEngine;

namespace RPGGame.Core.Assets;

/// <summary>
/// Graphics data for a terrain tile, containing top surface and side sprites.
/// Immutable struct for efficient storage and access.
/// </summary>
public readonly struct TerrainTileGfx {
	#region Fields

	/// <summary>Top surface sprite variations.</summary>
	public readonly Sprite[] TopVariants;

	/// <summary>Left side sprite variations.</summary>
	public readonly Sprite[] SideLeftVariants;

	/// <summary>Right side sprite variations.</summary>
	public readonly Sprite[] SideRightVariants;

	/// <summary>The terrain ID this graphics data belongs to.</summary>
	public readonly TerrainProto.ID TerrainId;

	#endregion

	#region Properties

	/// <summary>Whether this terrain has valid graphics loaded.</summary>
	public bool IsValid => TopVariants is { Length: > 0 };

	/// <summary>Number of top surface variations.</summary>
	public int TopVariantCount => TopVariants?.Length ?? 0;

	/// <summary>Number of side variations (left and right should match).</summary>
	public int SideVariantCount => SideLeftVariants?.Length ?? 0;

	/// <summary>Whether this terrain has side graphics.</summary>
	public bool HasSides => SideLeftVariants is { Length: > 0 } && SideRightVariants is { Length: > 0 };

	#endregion

	#region Constructor

	public TerrainTileGfx(
		TerrainProto.ID terrainId,
		Sprite[]? topVariants,
		Sprite[]? sideLeftVariants,
		Sprite[]? sideRightVariants
	) {
		TerrainId = terrainId;
		TopVariants = topVariants ?? [];
		SideLeftVariants = sideLeftVariants ?? [];
		SideRightVariants = sideRightVariants ?? [];
	}

	#endregion

	#region Accessors

	/// <summary>
	/// Gets a random top sprite variant.
	/// </summary>
	public Sprite? GetRandomTop(ref RandomStream rng) {
		if (TopVariants == null || TopVariants.Length == 0) return null;
		return TopVariants[rng.NextInt(TopVariants.Length)];
	}

	/// <summary>
	/// Gets a top sprite by index (wraps if out of range).
	/// </summary>
	public Sprite? GetTop(int index) {
		if (TopVariants == null || TopVariants.Length == 0) return null;
		return TopVariants[index % TopVariants.Length];
	}

	/// <summary>
	/// Gets a random side sprite pair (left and right).
	/// </summary>
	public (Sprite? Left, Sprite? Right) GetRandomSides(ref RandomStream rng) {
		if (!HasSides) return (null, null);
		int index = rng.NextInt(SideLeftVariants.Length);
		return (SideLeftVariants[index], SideRightVariants[index]);
	}

	/// <summary>
	/// Gets side sprites by index (wraps if out of range).
	/// </summary>
	public (Sprite? Left, Sprite? Right) GetSides(int index) {
		if (!HasSides) return (null, null);
		int wrappedIndex = index % SideLeftVariants.Length;
		return (SideLeftVariants[wrappedIndex], SideRightVariants[wrappedIndex]);
	}

	/// <summary>
	/// Gets the left side sprite by index.
	/// </summary>
	public Sprite? GetSideLeft(int index) {
		if (SideLeftVariants == null || SideLeftVariants.Length == 0) return null;
		return SideLeftVariants[index % SideLeftVariants.Length];
	}

	/// <summary>
	/// Gets the right side sprite by index.
	/// </summary>
	public Sprite? GetSideRight(int index) {
		if (SideRightVariants == null || SideRightVariants.Length == 0) return null;
		return SideRightVariants[index % SideRightVariants.Length];
	}

	#endregion

	#region Static

	/// <summary>Empty/invalid terrain graphics.</summary>
	public static readonly TerrainTileGfx Empty = new(new TerrainProto.ID("Empty"), [], [], []);

	#endregion
}