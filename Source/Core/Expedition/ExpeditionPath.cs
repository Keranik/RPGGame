using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Legacy expedition path - wrapper around node dictionary.
/// Used for compatibility with existing TravelData.
/// </summary>
public class ExpeditionPath {
	public int Seed { get; }
	public Dictionary<PathNodeId, PathNode> Nodes { get; } = [];
	public PathNode? StartNode { get; private set; }
	public PathNode? CurrentNode { get; private set; }
	public IsoPathGenerator.GenerationState? GenerationState { get; set; }

	public int NodeCount => Nodes.Count;

	public ExpeditionPath(int seed) {
		Seed = seed;
	}

	public void AddNode(PathNode node) {
		Nodes[node.Id] = node;
	}

	public PathNode? GetNode(PathNodeId id) {
		return Nodes.TryGetValue(id, out var node) ? node : null;
	}

	public PathNode? GetNodeAt(Vector2Int pos) {
		return Nodes.Values.FirstOrDefault(n => n.Position == pos);
	}

	public void SetStartNode(PathNode node) {
		StartNode = node;
		CurrentNode = node;
	}

	public void SetCurrentNode(PathNode node) {
		CurrentNode = node;
	}
}
