using RPGGame.Core.Isometric;
using RPGGame.Core.Prototypes.Expedition;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Represents a node in the travel path network.
/// Paths connect nodes, and events can occur at nodes.
/// </summary>
public class PathNode {
    public PathNodeId Id { get; }
    public IsoPos IsoPosition { get; private set; }
    public Vector2Int Position => new(IsoPosition.X, IsoPosition.Y);
    public PathNodeType Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationProtoId { get; set; }
    public TerrainProto.ID TerrainId { get; set; }
    public BiomeProto.ID BiomeId { get; set; }
    public float DangerLevel { get; set; }
    public List<PathConnection> Connections { get; } = [];
    public string? EventId { get; set; }
    public bool IsVisited { get; set; }
    public bool IsVisible { get; set; }
    public bool IsCurrentLocation { get; set; }

    public PathNode(PathNodeId id, Vector2Int position, PathNodeType type = PathNodeType.Waypoint) {
        Id = id;
        IsoPosition = new IsoPos(position.x, position.y, IsoLevel.Ground);
        Type = type;
        TerrainId = Ids.Terrains.Roads.Path;
    }

    public PathNode(PathNodeId id, IsoPos position, PathNodeType type = PathNodeType.Waypoint) {
        Id = id;
        IsoPosition = position;
        Type = type;
        TerrainId = Ids.Terrains.Roads.Path;
    }

    public IsoPos GetIsoPosition() => IsoPosition;
    public void SetIsoPosition(IsoPos pos) => IsoPosition = pos;

    public void AddConnection(PathNode target, float distance, TerrainProto.ID terrainId, bool isBidirectional = true) {
        if (Connections.Any(c => c.TargetNodeId == target.Id)) return;

        Connections.Add(new PathConnection {
            TargetNodeId = target.Id,
            Distance = distance,
            TerrainId = terrainId
        });

        if (isBidirectional) {
            target.AddConnection(this, distance, terrainId, false);
        }
    }

    public bool RemoveConnection(PathNodeId targetNodeId) {
        return Connections.RemoveAll(c => c.TargetNodeId == targetNodeId) > 0;
    }

    public bool ConnectsTo(PathNodeId targetNodeId) {
        return Connections.Any(c => c.TargetNodeId == targetNodeId);
    }

    public PathConnection? GetConnectionTo(PathNodeId targetNodeId) {
        return Connections.FirstOrDefault(c => c.TargetNodeId == targetNodeId);
    }

    public PathNodeData ToData() {
        return new PathNodeData {
            Id = Id.Value,
            PositionX = IsoPosition.X,
            PositionY = IsoPosition.Y,
            PositionLevel = IsoPosition.Level.Value,
            Type = Type,
            Name = Name,
            Description = Description,
            LocationProtoId = LocationProtoId,
            TerrainIdValue = TerrainId.Value,
            BiomeIdValue = BiomeId.Value,
            DangerLevel = DangerLevel,
            EventId = EventId,
            IsVisited = IsVisited,
            Connections = Connections.Select(c => c.ToData()).ToList()
        };
    }

	public static PathNode FromData(PathNodeData data) {
		var isoPos = new IsoPos(data.PositionX, data.PositionY, new IsoLevel(data.PositionLevel));
		var node = new PathNode(new PathNodeId(data.Id), isoPos, data.Type) {
			Name = data.Name,
			Description = data.Description,
			LocationProtoId = data.LocationProtoId,
			TerrainId = new TerrainProto.ID(data.TerrainIdValue ?? "Terrain_Road_Path"),
			BiomeId = new BiomeProto.ID(data.BiomeIdValue ?? ""),
			DangerLevel = data.DangerLevel,
			EventId = data.EventId,
			IsVisited = data.IsVisited
		};

		foreach (var connData in data.Connections) {
			node.Connections.Add(PathConnection.FromData(connData));
		}

		return node;
	}
}

#region Supporting Types

/// <summary>
/// Types of path nodes.
/// </summary>
public enum PathNodeType {
	/// <summary>Simple waypoint on the path.</summary>
	Waypoint,
	/// <summary>Path intersection/branch point.</summary>
	Intersection,
	/// <summary>Named landmark.</summary>
	Landmark,
	/// <summary>Camp site - can rest here.</summary>
	CampSite,
	/// <summary>Resource gathering spot.</summary>
	ResourceNode,
	/// <summary>Event location.</summary>
	EventLocation,
	/// <summary>Settlement/village.</summary>
	Settlement,
	/// <summary>Dungeon/cave entrance.</summary>
	Dungeon,
	/// <summary>Boss location.</summary>
	BossLocation,
	/// <summary>The fog source (final destination).</summary>
	FogSource,
	/// <summary>Starting village.</summary>
	Village
}

/// <summary>
/// Extension methods for PathNodeType.
/// </summary>
public static class PathNodeTypeExtensions {
	public static string GetIconName(this PathNodeType type) {
		return type switch {
			PathNodeType.Waypoint => "icon_waypoint",
			PathNodeType.Intersection => "icon_intersection",
			PathNodeType.Landmark => "icon_landmark",
			PathNodeType.CampSite => "icon_campfire",
			PathNodeType.ResourceNode => "icon_resource",
			PathNodeType.EventLocation => "icon_event",
			PathNodeType.Settlement => "icon_settlement",
			PathNodeType.Dungeon => "icon_dungeon",
			PathNodeType.BossLocation => "icon_boss",
			PathNodeType.FogSource => "icon_fog_source",
			PathNodeType.Village => "icon_village",
			_ => "icon_unknown"
		};
	}

	public static string GetDisplayName(this PathNodeType type) {
		return type switch {
			PathNodeType.CampSite => "Camp Site",
			PathNodeType.ResourceNode => "Resource Node",
			PathNodeType.EventLocation => "Point of Interest",
			PathNodeType.BossLocation => "Dangerous Area",
			PathNodeType.FogSource => "???",
			_ => type.ToString()
		};
	}

	public static bool CanCamp(this PathNodeType type) {
		return type switch {
			PathNodeType.CampSite => true,
			PathNodeType.Settlement => true,
			PathNodeType.Village => true,
			_ => false
		};
	}

	public static bool IsDangerous(this PathNodeType type) {
		return type switch {
			PathNodeType.Dungeon => true,
			PathNodeType.BossLocation => true,
			PathNodeType.FogSource => true,
			_ => false
		};
	}

	public static bool IsInteractive(this PathNodeType type) {
		return type switch {
			PathNodeType.Waypoint => false,
			PathNodeType.Intersection => false,
			_ => true
		};
	}
}

/// <summary>
/// A connection between two path nodes.
/// </summary>
public class PathConnection {
	public PathNodeId TargetNodeId { get; set; }
	public float Distance { get; set; }
	public TerrainProto.ID TerrainId { get; set; }
	public bool IsExplored { get; set; }
	public bool IsBlocked { get; set; }
	public string? BlockedReason { get; set; }

	public PathConnectionData ToData() {
		return new PathConnectionData {
			TargetNodeId = TargetNodeId.Value,
			Distance = Distance,
			TerrainIdValue = TerrainId.Value,
			IsExplored = IsExplored,
			IsBlocked = IsBlocked,
			BlockedReason = BlockedReason
		};
	}

	public static PathConnection FromData(PathConnectionData data) {
		return new PathConnection {
			TargetNodeId = new PathNodeId(data.TargetNodeId),
			Distance = data.Distance,
			TerrainId = new TerrainProto.ID(data.TerrainIdValue ?? "Terrain_Road_Path"),
			IsExplored = data.IsExplored,
			IsBlocked = data.IsBlocked,
			BlockedReason = data.BlockedReason
		};
	}
}

#endregion

#region Serialization Data

public class PathNodeData {
	public int Id { get; set; }  // Changed from string
	public int PositionX { get; set; }
	public int PositionY { get; set; }
	public int PositionLevel { get; set; }
	public PathNodeType Type { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? LocationProtoId { get; set; }
	public string? TerrainIdValue { get; set; }
	public string? BiomeIdValue { get; set; }
	public float DangerLevel { get; set; }
	public string? EventId { get; set; }
	public bool IsVisited { get; set; }
	public List<PathConnectionData> Connections { get; set; } = [];
}

public class PathConnectionData {
	public int TargetNodeId { get; set; }  // Changed from string
	public float Distance { get; set; }
	public string? TerrainIdValue { get; set; }
	public bool IsExplored { get; set; }
	public bool IsBlocked { get; set; }
	public string? BlockedReason { get; set; }
}

#endregion