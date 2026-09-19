using System;
using System.Collections.Generic;
using System.Text;

namespace RPGGame.Core.Expedition;

public readonly struct TerrainStatsLite {
	public static readonly TerrainStatsLite Default = new(1f, 1f, 1f, ColorRPG.FromHex("#4a7c4e"));
	
	public float MovementMultiplier { get; }
	public float StaminaDrain { get; }
	public float FatigueRate { get; }
	public ColorRPG TerrainColor { get; }
	
	public TerrainStatsLite(float movement, float stamina, float fatigue, ColorRPG terrainColor = default) {
		MovementMultiplier = movement;
		StaminaDrain = stamina;
		FatigueRate = fatigue;
		TerrainColor = terrainColor;
	}
}
