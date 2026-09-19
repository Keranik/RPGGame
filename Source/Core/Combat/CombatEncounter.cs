using RPGGame.Core.Characters;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Combat;

/// <summary>
/// A runtime combat encounter instance created from an EncounterProto.
/// This is the mutable state for an active or pending combat.
/// </summary>
public class CombatEncounter {
	#region Properties

	/// <summary>
	/// The encounter ID (from the proto).
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Display name for the encounter.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Description of the encounter.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Whether this is a boss encounter.
	/// </summary>
	public bool IsBoss { get; }

	/// <summary>
	/// Whether the player can flee from this encounter.
	/// </summary>
	public bool CanFlee { get; }

	/// <summary>
	/// Difficulty modifier for scaling.
	/// </summary>
	public float DifficultyModifier { get; }

	/// <summary>
	/// Bonus experience awarded on victory.
	/// </summary>
	public int BonusExperience { get; }

	/// <summary>
	/// Bonus gold awarded on victory.
	/// </summary>
	public int BonusGold { get; }

	/// <summary>
	/// Guaranteed loot drops on victory (uses consolidated LootEntry).
	/// </summary>
	public List<LootEntry> GuaranteedLoot { get; }

	/// <summary>
	/// Enemy spawn definitions.
	/// </summary>
	public List<EncounterEnemy> EnemySpawns { get; }

	/// <summary>
	/// Event to trigger on victory.
	/// </summary>
	public string? OnVictoryEventId { get; }

	/// <summary>
	/// Event to trigger on defeat.
	/// </summary>
	public string? OnDefeatEventId { get; }

	/// <summary>
	/// Combat music ID.
	/// </summary>
	public string MusicId { get; }

	/// <summary>
	/// Combat background ID.
	/// </summary>
	public string BackgroundId { get; }

	#endregion

	#region Constructor

	private CombatEncounter(
		string id,
		string name,
		string description,
		bool isBoss,
		bool canFlee,
		float difficultyModifier,
		int bonusExperience,
		int bonusGold,
		List<LootEntry> guaranteedLoot,
		List<EncounterEnemy> enemySpawns,
		string? onVictoryEventId,
		string? onDefeatEventId,
		string musicId,
		string backgroundId
	) {
		Id = id;
		Name = name;
		Description = description;
		IsBoss = isBoss;
		CanFlee = canFlee;
		DifficultyModifier = difficultyModifier;
		BonusExperience = bonusExperience;
		BonusGold = bonusGold;
		GuaranteedLoot = guaranteedLoot;
		EnemySpawns = enemySpawns;
		OnVictoryEventId = onVictoryEventId;
		OnDefeatEventId = onDefeatEventId;
		MusicId = musicId;
		BackgroundId = backgroundId;
	}

	#endregion

	#region Factory

	/// <summary>
	/// Creates a CombatEncounter from an EncounterProto.
	/// </summary>
	public static CombatEncounter FromProto(EncounterProto proto) {
		// GuaranteedLoot is already List<LootEntry> in the proto
		var guaranteedLoot = proto.GuaranteedLoot?.ToList() ?? [];

		// Copy enemy spawns directly - EncounterEnemy is immutable so we can reuse them
		var enemySpawns = proto.Enemies?.ToList() ?? [];

		return new CombatEncounter(
			id: proto.Id.Value,
			name: proto.DisplayText.Name,
			description: proto.DisplayText.Description,
			isBoss: proto.IsBoss,
			canFlee: proto.CanFlee,
			difficultyModifier: proto.DifficultyModifier,
			bonusExperience: proto.BonusExperience,
			bonusGold: proto.BonusGold,
			guaranteedLoot: guaranteedLoot,
			enemySpawns: enemySpawns,
			onVictoryEventId: proto.OnVictoryEvent?.Value,
			onDefeatEventId: proto.OnDefeatEvent?.Value,
			musicId: proto.MusicId,
			backgroundId: proto.BackgroundId
		);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates enemy LiveCharacters for this encounter.
	/// </summary>
	public List<LiveCharacter> CreateEnemies(GameDb gameDb) {
		var enemies = new List<LiveCharacter>();

		foreach (var spawn in EnemySpawns) {
			if (!gameDb.TryGetProto<EnemyProto>(spawn.EnemyId, out var enemyProto)) {
				UnityEngine.Debug.LogWarning($"Enemy proto not found: {spawn.EnemyId}");
				continue;
			}

			int count = UnityEngine.Random.Range(spawn.MinCount, spawn.MaxCount + 1);

			for (int i = 0; i < count; i++) {
				var enemy = LiveCharacter.CreateEnemy(enemyProto);

				// Apply difficulty modifier to health
				if (DifficultyModifier != 1f) {
					float scaledHealth = enemy.MaxHealth * DifficultyModifier;
					enemy.BaseStats.Set(Ids.Stats.Resource.MaxHealth, scaledHealth);
					enemy.BaseStats.Set(Ids.Stats.Resource.CurrentHealth, scaledHealth);
				}

				// Suffix name if multiple
				if (count > 1) {
					enemy.Name = $"{enemy.Name} {i + 1}";
				}

				enemies.Add(enemy);
			}
		}

		return enemies;
	}

	/// <summary>
	/// Rolls loot drops for this encounter (encounter-level guaranteed loot).
	/// </summary>
	public List<(LootEntry entry, int quantity)> RollLoot(Random random, int playerLevel, HashSet<string>? flags = null, bool isFirstClear = false) {
		var drops = new List<(LootEntry entry, int quantity)>();

		foreach (var entry in GuaranteedLoot) {
			if (!entry.CanDrop(playerLevel, flags, isFirstClear)) {
				continue;
			}

			if (entry.RollDrop(random)) {
				int quantity = entry.RollQuantity(random);
				drops.Add((entry, quantity));
			}
		}

		return drops;
	}

	#endregion
}