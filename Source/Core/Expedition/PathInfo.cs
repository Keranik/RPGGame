using RPGGame.Core.Prototypes.Expedition;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Immutable info about a path connection for UI display.
/// </summary>
public readonly struct PathInfo {
	public PathNodeId TargetNodeId { get; }
	public float Distance { get; }
	public TerrainProto.ID TerrainId { get; }
	public PathNodeType TargetType { get; }
	public string? TargetName { get; }

	public static readonly PathInfo Empty = new();

	public PathInfo(PathConnection connection, PathNode target) {
		TargetNodeId = connection.TargetNodeId;
		Distance = connection.Distance;
		TerrainId = connection.TerrainId;
		TargetType = target.Type;
		TargetName = target.Name;
	}

	public bool IsEmpty => TargetNodeId == PathNodeId.Invalid;
}
