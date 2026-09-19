using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Expedition;

public partial class ExpeditionManager {
	/// <summary>
	/// Pure data container for travel state during an expedition.
	/// Contains position, nodes, progress, and speed data.
	/// 
	/// NO GAME LOGIC - all game decisions are made by ExpeditionManager.
	/// NO EVENTS - all events are fired by ExpeditionManager.
	/// 
	/// This is purely a data structure that ExpeditionManager reads and writes.
	/// </summary>
	public class TravelData {
		#region Fields

		private readonly Dictionary<PathNodeId, PathNode> k_nodes = [];
		private float k_travelProgress;

		#endregion

		#region Position & Node Properties

		/// <summary>
		/// Current position in tile coordinates.
		/// </summary>
		public Vector2Int CurrentPosition { get; internal set; }

		/// <summary>
		/// Current path node the player is at or traveling from.
		/// </summary>
		public PathNode? CurrentNode { get; internal set; }

		/// <summary>
		/// The node we just came from.
		/// </summary>
		public PathNode? PreviousNode { get; internal set; }

		/// <summary>
		/// Target path node the player is traveling to (null if stationary).
		/// </summary>
		public PathNode? TargetNode { get; internal set; }

		/// <summary>
		/// Info about the current path being traveled (empty if not traveling).
		/// </summary>
		public PathInfo CurrentPathInfo { get; internal set; }

		#endregion

		#region Travel State Properties

		/// <summary>
		/// Current travel speed setting.
		/// </summary>
		public TravelSpeed Speed { get; internal set; } = TravelSpeed.Normal;

		/// <summary>
		/// Progress between current and target node (0-1).
		/// </summary>
		public float TravelProgress {
			get => k_travelProgress;
			internal set => k_travelProgress = value;
		}

		/// <summary>
		/// Current terrain type.
		/// </summary>
		public TerrainProto.ID CurrentTerrain { get; internal set; } = Ids.Terrains.Roads.Path;

		/// <summary>
		/// Current region ID.
		/// </summary>
		public string CurrentRegionId { get; set; } = "starting_road";

		#endregion

		#region Distance Tracking

		/// <summary>
		/// Total distance traveled from village.
		/// </summary>
		public float DistanceFromVillage { get; internal set; }

		/// <summary>
		/// Total distance traveled this run.
		/// </summary>
		public float TotalDistanceTraveled { get; internal set; }

		#endregion

		#region Speed Modifiers

		/// <summary>
		/// Base visual travel speed: tiles per real-second at 1x speed.
		/// </summary>
		public float BaseTravelSpeed { get; set; } = 5.0f;

		/// <summary>
		/// Movement speed stat modifier (from character stats).
		/// </summary>
		public float MovementSpeedModifier { get; set; } = 1.0f;

		/// <summary>
		/// Cached stats for the current terrain (set when terrain changes).
		/// </summary>
		public TerrainStatsLite CurrentTerrainStats { get; internal set; } = TerrainStatsLite.Default;

		/// <summary>
		/// Calculated effective travel speed (tiles per real-second).
		/// </summary>
		public float EffectiveTravelSpeed {
			get {
				float terrainMod = CurrentTerrainStats.MovementMultiplier;
				float speedMod = Speed.GetMovementMultiplier();
				return BaseTravelSpeed * MovementSpeedModifier * terrainMod * speedMod;
			}
		}

		#endregion

		#region Rendering Properties

		/// <summary>
		/// Direction of travel (for rendering).
		/// </summary>
		public Vector2 TravelDirection { get; internal set; }

		/// <summary>
		/// Visual position for smooth rendering.
		/// </summary>
		public Vector2 VisualPosition { get; internal set; }

		#endregion

		#region Computed Properties

		/// <summary>
		/// Whether currently traveling (has target and not paused).
		/// </summary>
		public bool IsTraveling => TargetNode != null && Speed != TravelSpeed.Paused;

		/// <summary>
		/// Whether we have a pending destination (even if paused).
		/// </summary>
		public bool HasPendingDestination => TargetNode != null;

		/// <summary>
		/// Whether travel is paused.
		/// </summary>
		public bool IsPaused => Speed == TravelSpeed.Paused;

		/// <summary>
		/// Estimated time to reach the target node (in real seconds).
		/// </summary>
		public float EstimatedTimeToNextNode {
			get {
				if (TargetNode == null || CurrentNode == null) {
					return 0;
				}
				var connection = CurrentNode.GetConnectionTo(TargetNode.Id);
				if (connection == null) {
					return 0;
				}
				float remainingDistance = connection.Distance * (1f - k_travelProgress);
				return EffectiveTravelSpeed > 0 ? remainingDistance / EffectiveTravelSpeed : 0;
			}
		}

		#endregion

		#region Constructor

		public TravelData() {
		}

		public TravelData(Vector2Int startPosition) {
			CurrentPosition = startPosition;
			VisualPosition = startPosition;
		}

		#endregion

		#region Node Registry (Pure Data Access)

		/// <summary>
		/// Registers a path node.
		/// </summary>
		public void RegisterNode(PathNode node) {
			k_nodes[node.Id] = node;
		}

		/// <summary>
		/// Gets a node by ID.
		/// </summary>
		public PathNode? GetNode(PathNodeId nodeId) {
			return k_nodes.TryGetValue(nodeId, out var node) ? node : null;
		}

		/// <summary>
		/// Gets all registered nodes.
		/// </summary>
		public IEnumerable<PathNode> GetAllNodes() => k_nodes.Values;

		/// <summary>
		/// Gets the number of registered nodes.
		/// </summary>
		public int NodeCount => k_nodes.Count;

		#endregion

		#region Data Mutation (Internal - Called by ExpeditionManager)

		/// <summary>
		/// Sets the current node position. Does NOT fire events.
		/// Call this for teleport/initial placement.
		/// </summary>
		internal void SetCurrentNode(PathNode node) {
			// Clear previous current location flag
			if (CurrentNode != null) {
				CurrentNode.IsCurrentLocation = false;
			}

			PreviousNode = null;
			CurrentNode = node;
			CurrentNode.IsCurrentLocation = true;
			CurrentNode.IsVisited = true;
			CurrentPosition = node.Position;
			VisualPosition = node.Position;
			TargetNode = null;
			CurrentPathInfo = PathInfo.Empty;
			k_travelProgress = 0;
		}

		/// <summary>
		/// Sets up travel to a target node. Returns false if invalid connection.
		/// Does NOT fire events - that's ExpeditionManager's job.
		/// </summary>
		internal bool SetupTravelTo(PathNodeId targetNodeId) {
			if (CurrentNode == null) return false;

			var connection = CurrentNode.GetConnectionTo(targetNodeId);
			if (connection == null || connection.IsBlocked) return false;

			var target = GetNode(targetNodeId);
			if (target == null) return false;

			TargetNode = target;
			k_travelProgress = 0;
			CurrentTerrain = connection.TerrainId;
			TravelDirection = ((Vector2)(target.Position - CurrentNode.Position)).normalized;
			connection.IsExplored = true;
			CurrentPathInfo = new PathInfo(connection, target);

			return true;
		}

		/// <summary>
		/// Updates travel progress. Returns true if arrived at destination.
		/// Does NOT handle arrival - that's ExpeditionManager's job.
		/// </summary>
		internal bool UpdateTravelProgress(float deltaTime) {
			if (!IsTraveling || CurrentNode == null || TargetNode == null) {
				return false;
			}

			var connection = CurrentNode.GetConnectionTo(TargetNode.Id);
			if (connection == null) {
				return false;
			}

			float distance = connection.Distance;
			float speed = EffectiveTravelSpeed;

			if (speed <= 0 || distance <= 0) {
				return false;
			}

			float progressPerSecond = speed / distance;
			k_travelProgress += progressPerSecond * deltaTime;

			VisualPosition = Vector2.Lerp(CurrentNode.Position, TargetNode.Position, Mathf.Clamp01(k_travelProgress));

			return k_travelProgress >= 1.0f;
		}

		/// <summary>
		/// Completes arrival at the target node. Updates all position data.
		/// Does NOT fire events - that's ExpeditionManager's job.
		/// </summary>
		internal float CompleteArrival() {
			if (TargetNode == null || CurrentNode == null) {
				return 0;
			}

			var connection = CurrentNode.GetConnectionTo(TargetNode.Id);
			float distanceTraveled = connection?.Distance ?? 0;

			TotalDistanceTraveled += distanceTraveled;
			DistanceFromVillage = CalculateDistanceFromVillage();

			PreviousNode = CurrentNode;
			CurrentNode.IsCurrentLocation = false;

			CurrentNode = TargetNode;
			CurrentNode.IsCurrentLocation = true;
			CurrentNode.IsVisited = true;
			CurrentPosition = CurrentNode.Position;
			VisualPosition = CurrentNode.Position;
			TargetNode = null;
			CurrentPathInfo = PathInfo.Empty;
			k_travelProgress = 0;

			return distanceTraveled;
		}

		/// <summary>
		/// Sets the travel speed.
		/// </summary>
		internal void SetSpeed(TravelSpeed speed) {
			Speed = speed;
		}

		/// <summary>
		/// Pauses travel.
		/// </summary>
		internal void Pause() => Speed = TravelSpeed.Paused;

		/// <summary>
		/// Resumes travel at normal speed.
		/// </summary>
		internal void Resume() => Speed = TravelSpeed.Normal;

		#endregion

		#region Queries (Pure Data - No Game Logic)

		/// <summary>
		/// Gets all forward connections from current node.
		/// Filters out the previous node AND any nodes that are behind us (lower X position).
		/// This ensures we only show forward paths at intersections.
		/// </summary>
		public List<PathConnection> GetForwardConnections() {
			if (CurrentNode == null) {
				return [];
			}

			return CurrentNode.Connections
				.Where(c => {
					if (c.IsBlocked) {
						return false;
					}

					// Always exclude the node we just came from
					if (c.TargetNodeId == PreviousNode?.Id) {
						return false;
					}
            
					return true;
				})
				.ToList();
		}

		/// <summary>
		/// Gets available forward paths with their target nodes.
		/// </summary>
		public List<(PathNode node, PathConnection connection)> GetAvailablePaths() {
			var result = new List<(PathNode, PathConnection)>();

			foreach (var conn in GetForwardConnections()) {
				var node = GetNode(conn.TargetNodeId);
				if (node != null) {
					result.Add((node, conn));
				}
			}

			return result;
		}

		/// <summary>
		/// Gets PathInfo for all available forward paths.
		/// </summary>
		public List<PathInfo> GetAvailablePathInfos() {
			var result = new List<PathInfo>();
			foreach (var conn in GetForwardConnections()) {
				var node = GetNode(conn.TargetNodeId);
				if (node != null) {
					result.Add(new PathInfo(conn, node));
				}
			}
			return result;
		}

		/// <summary>
		/// Gets estimated travel time to a target node (in real seconds).
		/// Uses current terrain stats as approximation.
		/// </summary>
		public float GetEstimatedTravelTime(PathNodeId targetNodeId) {
			if (CurrentNode == null) return float.MaxValue;
			var connection = CurrentNode.GetConnectionTo(targetNodeId);
			if (connection == null) return float.MaxValue;
			float speed = BaseTravelSpeed * MovementSpeedModifier * CurrentTerrainStats.MovementMultiplier;
			return speed <= 0 ? float.MaxValue : connection.Distance / speed;
		}

		/// <summary>
		/// Calculates straight-line distance from the village.
		/// </summary>
		private float CalculateDistanceFromVillage() {
			var village = k_nodes.Values.FirstOrDefault(n => n.Type == PathNodeType.Village);
			return village == null ? 0 : Vector2Int.Distance(CurrentPosition, village.Position);
		}

		#endregion

		#region Serialization

		public TravelDataSaveState ToSaveState() {
			return new TravelDataSaveState {
				CurrentPositionX = CurrentPosition.x,
				CurrentPositionY = CurrentPosition.y,
				CurrentNodeId = CurrentNode?.Id,
				PreviousNodeId = PreviousNode?.Id,
				TargetNodeId = TargetNode?.Id,
				TravelProgress = k_travelProgress,
				Speed = Speed,
				CurrentTerrain = CurrentTerrain,
				DistanceFromVillage = DistanceFromVillage,
				TotalDistanceTraveled = TotalDistanceTraveled,
				CurrentRegionId = CurrentRegionId,
				BaseTravelSpeed = BaseTravelSpeed,
				MovementSpeedModifier = MovementSpeedModifier,
				CurrentTerrainStats = CurrentTerrainStats,
				Nodes = k_nodes.Values.Select(n => n.ToData()).ToList()
			};
		}

		public static TravelData FromSaveState(TravelDataSaveState data) {
			var state = new TravelData {
				CurrentPosition = new Vector2Int(data.CurrentPositionX, data.CurrentPositionY),
				k_travelProgress = data.TravelProgress,
				Speed = data.Speed,
				CurrentTerrain = data.CurrentTerrain,
				DistanceFromVillage = data.DistanceFromVillage,
				TotalDistanceTraveled = data.TotalDistanceTraveled,
				CurrentRegionId = data.CurrentRegionId,
				BaseTravelSpeed = data.BaseTravelSpeed > 0 ? data.BaseTravelSpeed : 5.0f,
				MovementSpeedModifier = data.MovementSpeedModifier > 0 ? data.MovementSpeedModifier : 1f,
				CurrentTerrainStats = data.CurrentTerrainStats
			};

			state.VisualPosition = state.CurrentPosition;

			foreach (var nodeData in data.Nodes) {
				var node = PathNode.FromData(nodeData);
				state.k_nodes[node.Id] = node;
			}

			if (data.CurrentNodeId.HasValue) {
				state.CurrentNode = state.GetNode(data.CurrentNodeId.Value);
				if (state.CurrentNode != null) {
					state.CurrentNode.IsCurrentLocation = true;
				}
			}

			if (data.PreviousNodeId.HasValue) {
				state.PreviousNode = state.GetNode(data.PreviousNodeId.Value);
			}

			if (data.TargetNodeId.HasValue) {
				state.TargetNode = state.GetNode(data.TargetNodeId.Value);
				if (state.TargetNode != null && state.CurrentNode != null) {
					var conn = state.CurrentNode.GetConnectionTo(state.TargetNode.Id);
					if (conn != null) {
						state.CurrentPathInfo = new PathInfo(conn, state.TargetNode);
					}
				}
			}

			return state;
		}

		#endregion
	}

	/// <summary>
	/// Serialization data for TravelData.
	/// </summary>
	public class TravelDataSaveState {
		public int CurrentPositionX { get; set; }
		public int CurrentPositionY { get; set; }
		public PathNodeId? CurrentNodeId { get; set; }
		public PathNodeId? PreviousNodeId { get; set; }
		public PathNodeId? TargetNodeId { get; set; }
		public float TravelProgress { get; set; }
		public TravelSpeed Speed { get; set; }
		public TerrainProto.ID CurrentTerrain { get; set; }
		public float DistanceFromVillage { get; set; }
		public float TotalDistanceTraveled { get; set; }
		public string CurrentRegionId { get; set; } = "";
		public float BaseTravelSpeed { get; set; }
		public float MovementSpeedModifier { get; set; }
		public TerrainStatsLite CurrentTerrainStats { get; set; } = TerrainStatsLite.Default;
		public List<PathNodeData> Nodes { get; set; } = [];

	}
}