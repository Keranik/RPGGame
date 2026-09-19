namespace RPGGame.Core;

/// <summary>
/// Types of damage that can be dealt or resisted.
/// </summary>
public enum DamageType {
	// ═══════════════════════════════════════════════════════════════
	// PHYSICAL
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Generic physical damage.</summary>
	Physical,

	/// <summary>Cutting damage from bladed weapons.</summary>
	Slashing,

	/// <summary>Puncturing damage from pointed weapons.</summary>
	Piercing,

	/// <summary>Impact damage from blunt weapons.</summary>
	Bludgeoning,

	// ═══════════════════════════════════════════════════════════════
	// ELEMENTAL
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Heat and flame damage.</summary>
	Fire,

	/// <summary>Frost and ice damage.</summary>
	Cold,

	/// <summary>Electrical damage.</summary>
	Lightning,

	/// <summary>Sonic and concussive damage.</summary>
	Thunder,

	/// <summary>Corrosive damage.</summary>
	Acid,

	// ═══════════════════════════════════════════════════════════════
	// MAGICAL
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Raw magical energy.</summary>
	Arcane,

	/// <summary>Pure magical force.</summary>
	Force,

	/// <summary>Mind-affecting damage.</summary>
	Psychic,

	// ═══════════════════════════════════════════════════════════════
	// DIVINE/UNHOLY
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Holy/divine damage (extra vs undead/fiends).</summary>
	Holy,

	/// <summary>Divine radiant damage.</summary>
	Radiant,

	/// <summary>Death and decay damage.</summary>
	Necrotic,

	// ═══════════════════════════════════════════════════════════════
	// OTHER
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Toxic damage.</summary>
	Poison,

	/// <summary>True damage (ignores resistances).</summary>
	True
}

/// <summary>
/// Extension methods for DamageType.
/// </summary>
public static class DamageTypeExtensions {
	extension(DamageType type) {
		/// <summary>
		/// Gets whether this is a physical damage type.
		/// </summary>
		public bool IsPhysical() {
			return type is DamageType.Physical or DamageType.Slashing or
				DamageType.Piercing or DamageType.Bludgeoning;
		}
		/// <summary>
		/// Gets whether this is an elemental damage type.
		/// </summary>
		public bool IsElemental() {
			return type is DamageType.Fire or DamageType.Cold or
				DamageType.Lightning or DamageType.Thunder or DamageType.Acid;
		}
		/// <summary>
		/// Gets whether this is a magical damage type.
		/// </summary>
		public bool IsMagical() {
			return type is DamageType.Arcane or DamageType.Force or
				DamageType.Psychic or DamageType.Holy or
				DamageType.Radiant or DamageType.Necrotic;
		}
		/// <summary>
		/// Gets the color associated with this damage type.
		/// </summary>
		public UnityEngine.Color GetColor() {
			return type switch {
				DamageType.Physical or DamageType.Slashing or
					DamageType.Piercing or DamageType.Bludgeoning => new UnityEngine.Color(0.8f, 0.8f, 0.8f),

				DamageType.Fire => new UnityEngine.Color(1f, 0.4f, 0.1f),
				DamageType.Cold => new UnityEngine.Color(0.5f, 0.8f, 1f),
				DamageType.Lightning => new UnityEngine.Color(1f, 1f, 0.3f),
				DamageType.Thunder => new UnityEngine.Color(0.6f, 0.5f, 0.8f),
				DamageType.Acid => new UnityEngine.Color(0.5f, 0.9f, 0.2f),

				DamageType.Arcane => new UnityEngine.Color(0.6f, 0.3f, 0.9f),
				DamageType.Force => new UnityEngine.Color(0.9f, 0.9f, 1f),
				DamageType.Psychic => new UnityEngine.Color(1f, 0.4f, 0.8f),

				DamageType.Holy or DamageType.Radiant => new UnityEngine.Color(1f, 0.95f, 0.6f),
				DamageType.Necrotic => new UnityEngine.Color(0.4f, 0.2f, 0.4f),

				DamageType.Poison => new UnityEngine.Color(0.3f, 0.7f, 0.2f),
				DamageType.True => new UnityEngine.Color(1f, 1f, 1f),

				_ => UnityEngine.Color.white
			};
		}
		/// <summary>
		/// Gets the icon name for this damage type.
		/// </summary>
		public string GetIconName() {
			return $"icon_damage_{type.ToString().ToLower()}";
		}
		/// <summary>
		/// Gets whether this damage type bypasses resistances.
		/// </summary>
		public bool IgnoresResistance() {
			return type == DamageType.True;
		}
	}

}