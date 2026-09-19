using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;

namespace RPGGame.Core.Characters.LevelUp;

/// <summary>
/// Immutable state for level-up point allocation.
/// Tracks pending changes that haven't been applied yet.
/// Uses the same creation points system as character creation.
/// </summary>
public record LevelUpState {
	/// <summary>Total points available for this level-up (includes unspent from previous levels).</summary>
	public int TotalPoints { get; init; }

	/// <summary>Points spent so far.</summary>
	public int PointsSpent { get; init; }

	/// <summary>Points remaining to spend.</summary>
	public int PointsRemaining => TotalPoints - PointsSpent;

	/// <summary>Current attribute values (base + pending increases).</summary>
	public Dictionary<StatProto.ID, int> Attributes { get; init; } = [];

	/// <summary>Base attribute values before this level-up (for calculating min/refund).</summary>
	public Dictionary<StatProto.ID, int> BaseAttributes { get; init; } = [];

	/// <summary>Bonus HP from this level-up.</summary>
	public int BonusHealth { get; init; }

	/// <summary>Bonus MP from this level-up.</summary>
	public int BonusMana { get; init; }

	/// <summary>Bonus Stamina from this level-up.</summary>
	public int BonusStamina { get; init; }

	/// <summary>Skills already known (with ranks).</summary>
	public Dictionary<SkillProto.ID, int> Skills { get; init; } = [];

	/// <summary>New skills added this level-up (with rank increases).</summary>
	public Dictionary<SkillProto.ID, int> NewSkillRanks { get; init; } = [];

	/// <summary>Spells already known.</summary>
	public HashSet<SpellProto.ID> Spells { get; init; } = [];

	/// <summary>New spells added this level-up.</summary>
	public HashSet<SpellProto.ID> NewSpells { get; init; } = [];

	/// <summary>
	/// Creates a level-up state from the current run.
	/// Points are calculated from the CreationPointsPerLevel stat and any unspent points.
	/// </summary>
	public static LevelUpState CreateFromRun(RunState run) {
		int pendingLevels = run.PendingLevelUps;
		
		// Get points per level from stats (can be modified by equipment, buffs, etc.)
		int pointsPerLevel = run.Stats.GetInt(Ids.Stats.Meta.CreationPointsPerLevel);
		if (pointsPerLevel <= 0) pointsPerLevel = 10; // Fallback default
		
		// Get unspent points from previous level-ups
		int unspentPoints = run.Stats.GetInt(Ids.Stats.Meta.CreationPointsUnspent);
		
		int totalPoints = (pendingLevels * pointsPerLevel) + unspentPoints;

		var character = run.Character;

		// Capture current attribute values
		var attributes = new Dictionary<StatProto.ID, int> {
			[Ids.Stats.Attributes.Strength] = run.Stats.GetInt(Ids.Stats.Attributes.Strength),
			[Ids.Stats.Attributes.Dexterity] = run.Stats.GetInt(Ids.Stats.Attributes.Dexterity),
			[Ids.Stats.Attributes.Constitution] = run.Stats.GetInt(Ids.Stats.Attributes.Constitution),
			[Ids.Stats.Attributes.Intelligence] = run.Stats.GetInt(Ids.Stats.Attributes.Intelligence),
			[Ids.Stats.Attributes.Wisdom] = run.Stats.GetInt(Ids.Stats.Attributes.Wisdom),
			[Ids.Stats.Attributes.Charisma] = run.Stats.GetInt(Ids.Stats.Attributes.Charisma)
		};

		// Copy for base values (can't decrease below these)
		var baseAttributes = new Dictionary<StatProto.ID, int>(attributes);

		// Copy existing skills with ranks
		var skills = new Dictionary<SkillProto.ID, int>(character.Skills);

		// Copy existing spells
		var spells = new HashSet<SpellProto.ID>(character.KnownSpells);

		return new LevelUpState {
			TotalPoints = totalPoints,
			PointsSpent = 0,
			Attributes = attributes,
			BaseAttributes = baseAttributes,
			Skills = skills,
			Spells = spells,
			NewSkillRanks = [],
			NewSpells = [],
			BonusHealth = 0,
			BonusMana = 0,
			BonusStamina = 0
		};
	}

	#region Attribute Mutations

	public LevelUpState WithAttributeIncreased(StatProto.ID attribute, int cost) {
		var newAttrs = new Dictionary<StatProto.ID, int>(Attributes);
		newAttrs[attribute] = newAttrs.GetValueOrDefault(attribute, 10) + 1;

		return this with {
			Attributes = newAttrs,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithAttributeDecreased(StatProto.ID attribute, int refund) {
		var newAttrs = new Dictionary<StatProto.ID, int>(Attributes);
		newAttrs[attribute] = newAttrs.GetValueOrDefault(attribute, 10) - 1;

		return this with {
			Attributes = newAttrs,
			PointsSpent = PointsSpent - refund
		};
	}

	#endregion

	#region Resource Mutations

	public LevelUpState WithBonusHealth(int amount, int cost) {
		return this with {
			BonusHealth = BonusHealth + amount,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithBonusHealthRemoved(int amount, int refund) {
		return this with {
			BonusHealth = BonusHealth - amount,
			PointsSpent = PointsSpent - refund
		};
	}

	public LevelUpState WithBonusMana(int amount, int cost) {
		return this with {
			BonusMana = BonusMana + amount,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithBonusManaRemoved(int amount, int refund) {
		return this with {
			BonusMana = BonusMana - amount,
			PointsSpent = PointsSpent - refund
		};
	}

	public LevelUpState WithBonusStamina(int amount, int cost) {
		return this with {
			BonusStamina = BonusStamina + amount,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithBonusStaminaRemoved(int amount, int refund) {
		return this with {
			BonusStamina = BonusStamina - amount,
			PointsSpent = PointsSpent - refund
		};
	}

	#endregion

	#region Skill Mutations

	public LevelUpState WithSkillRankIncreased(SkillProto.ID skillId, int cost) {
		var newRanks = new Dictionary<SkillProto.ID, int>(NewSkillRanks);
		newRanks[skillId] = newRanks.GetValueOrDefault(skillId, 0) + 1;

		return this with {
			NewSkillRanks = newRanks,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithSkillRankDecreased(SkillProto.ID skillId, int refund) {
		var newRanks = new Dictionary<SkillProto.ID, int>(NewSkillRanks);
		int currentPending = newRanks.GetValueOrDefault(skillId, 0);
		
		if (currentPending <= 1) {
			newRanks.Remove(skillId);
		} else {
			newRanks[skillId] = currentPending - 1;
		}

		return this with {
			NewSkillRanks = newRanks,
			PointsSpent = PointsSpent - refund
		};
	}

	/// <summary>Gets the total rank for a skill (base + pending).</summary>
	public int GetTotalSkillRank(SkillProto.ID skillId) {
		int baseRank = Skills.GetValueOrDefault(skillId, 0);
		int pendingRank = NewSkillRanks.GetValueOrDefault(skillId, 0);
		return baseRank + pendingRank;
	}

	/// <summary>Gets just the pending rank increase for a skill.</summary>
	public int GetPendingSkillRank(SkillProto.ID skillId) {
		return NewSkillRanks.GetValueOrDefault(skillId, 0);
	}

	#endregion

	#region Spell Mutations

	public LevelUpState WithSpellAdded(SpellProto.ID spellId, int cost) {
		var newSpells = new HashSet<SpellProto.ID>(NewSpells) { spellId };

		return this with {
			NewSpells = newSpells,
			PointsSpent = PointsSpent + cost
		};
	}

	public LevelUpState WithSpellRemoved(SpellProto.ID spellId, int refund) {
		var newSpells = new HashSet<SpellProto.ID>(NewSpells);
		newSpells.Remove(spellId);

		return this with {
			NewSpells = newSpells,
			PointsSpent = PointsSpent - refund
		};
	}

	#endregion
}