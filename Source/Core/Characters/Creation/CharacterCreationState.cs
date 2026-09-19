using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Tracks all state during character creation.
/// Immutable selections create new instances to allow undo/redo.
/// </summary>
public record CharacterCreationState {
	#region Identity

	/// <summary>Character name.</summary>
	public string Name { get; init; } = "Hero";

	/// <summary>Selected class ID.</summary>
	public CharacterClassProto.ID? SelectedClassId { get; init; }

	/// <summary>Whether this build used random reroll (gets bonus points).</summary>
	public bool IsRandomReroll { get; init; }

	#endregion

	#region Attributes

	/// <summary>
	/// Current attribute values. Keys are StatProto.ID for STR, DEX, etc.
	/// These start at class base values and can only be increased.
	/// </summary>
	public Dictionary<StatProto.ID, int> Attributes { get; init; } = [];

	#endregion

	#region Derived Stats

	/// <summary>Bonus health points purchased.</summary>
	public int BonusHealth { get; init; }

	/// <summary>Bonus mana points purchased.</summary>
	public int BonusMana { get; init; }

	/// <summary>Bonus stamina points purchased.</summary>
	public int BonusStamina { get; init; }

	#endregion

	#region Skills

	/// <summary>
	/// Skills selected and their ranks.
	/// Key = SkillProto.ID, Value = rank (1+).
	/// </summary>
	public Dictionary<SkillProto.ID, int> SelectedSkills { get; init; } = [];

	/// <summary>
	/// Additional skill schools unlocked (beyond class starting schools).
	/// </summary>
	public HashSet<TagProto.ID> UnlockedSkillSchools { get; init; } = [];

	#endregion

	#region Spells

	/// <summary>
	/// Spells selected.
	/// </summary>
	public HashSet<SpellProto.ID> SelectedSpells { get; init; } = [];

	/// <summary>
	/// Additional spell schools unlocked (beyond class starting schools).
	/// </summary>
	public HashSet<TagProto.ID> UnlockedSpellSchools { get; init; } = [];

	#endregion

	#region Tags / Packages

	/// <summary>
	/// Character tags purchased (like "Large", "Quick", etc.).
	/// </summary>
	public HashSet<TagProto.ID> SelectedTags { get; init; } = [];

	#endregion

	#region Appearance (Stubbed)

	/// <summary>Portrait/appearance selection index.</summary>
	public int PortraitIndex { get; init; }

	/// <summary>Hair style index.</summary>
	public int HairStyleIndex { get; init; }

	/// <summary>Hair color index.</summary>
	public int HairColorIndex { get; init; }

	/// <summary>Skin tone index.</summary>
	public int SkinToneIndex { get; init; }

	#endregion

	#region Points

	/// <summary>Total points available (class base + reroll bonus).</summary>
	public int TotalPoints { get; init; }

	/// <summary>Points spent so far.</summary>
	public int PointsSpent { get; init; }

	/// <summary>Points remaining.</summary>
	public int PointsRemaining => TotalPoints - PointsSpent;

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates initial state for a class selection.
	/// </summary>
	public static CharacterCreationState CreateForClass(
		CharacterClassProto classProto,
		bool isRandomReroll = false
	) {
		var attributes = new Dictionary<StatProto.ID, int> {
			{ Ids.Stats.Attributes.Strength, classProto.BaseAttributes.Strength },
			{ Ids.Stats.Attributes.Dexterity, classProto.BaseAttributes.Dexterity },
			{ Ids.Stats.Attributes.Constitution, classProto.BaseAttributes.Constitution },
			{ Ids.Stats.Attributes.Intelligence, classProto.BaseAttributes.Intelligence },
			{ Ids.Stats.Attributes.Wisdom, classProto.BaseAttributes.Wisdom },
			{ Ids.Stats.Attributes.Charisma, classProto.BaseAttributes.Charisma }
		};

		// Add starting skills at rank 1
		var startingSkills = new Dictionary<SkillProto.ID, int>();
		foreach (var skillId in classProto.StartingSkills) {
			startingSkills[skillId] = 1;
		}

		// Add starting spells
		var startingSpells = new HashSet<SpellProto.ID>(classProto.StartingSpells);

		return new CharacterCreationState {
			SelectedClassId = classProto.Id,
			IsRandomReroll = isRandomReroll,
			Attributes = attributes,
			SelectedSkills = startingSkills,
			SelectedSpells = startingSpells,
			TotalPoints = classProto.GetCreationPoints(isRandomReroll),
			PointsSpent = 0
		};
	}

	#endregion

	#region Mutation Methods (Return New State)

	/// <summary>
	/// Creates a new state with the name changed.
	/// </summary>
	public CharacterCreationState WithName(string name) {
		return this with { Name = name };
	}

	/// <summary>
	/// Creates a new state with an attribute increased by 1.
	/// </summary>
	public CharacterCreationState WithAttributeIncreased(StatProto.ID attribute, int cost) {
		var newAttributes = new Dictionary<StatProto.ID, int>(Attributes);
		newAttributes[attribute] = newAttributes.GetValueOrDefault(attribute, 10) + 1;

		return this with {
			Attributes = newAttributes,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with an attribute decreased by 1 (refund).
	/// Only valid if above class minimum.
	/// </summary>
	public CharacterCreationState WithAttributeDecreased(StatProto.ID attribute, int refund) {
		var newAttributes = new Dictionary<StatProto.ID, int>(Attributes);
		newAttributes[attribute] = newAttributes.GetValueOrDefault(attribute, 10) - 1;

		return this with {
			Attributes = newAttributes,
			PointsSpent = PointsSpent - refund
		};
	}

	/// <summary>
	/// Creates a new state with bonus health purchased.
	/// </summary>
	public CharacterCreationState WithBonusHealth(int amount, int cost) {
		return this with {
			BonusHealth = BonusHealth + amount,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with bonus mana purchased.
	/// </summary>
	public CharacterCreationState WithBonusMana(int amount, int cost) {
		return this with {
			BonusMana = BonusMana + amount,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with a skill added or rank increased.
	/// </summary>
	public CharacterCreationState WithSkillAdded(SkillProto.ID skill, int cost) {
		var newSkills = new Dictionary<SkillProto.ID, int>(SelectedSkills);
		newSkills[skill] = newSkills.GetValueOrDefault(skill, 0) + 1;

		return this with {
			SelectedSkills = newSkills,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with a skill removed.
	/// </summary>
	public CharacterCreationState WithSkillRemoved(SkillProto.ID skill, int refund) {
		var newSkills = new Dictionary<SkillProto.ID, int>(SelectedSkills);

		if (newSkills.TryGetValue(skill, out int rank)) {
			if (rank <= 1) {
				newSkills.Remove(skill);
			} else {
				newSkills[skill] = rank - 1;
			}
		}

		return this with {
			SelectedSkills = newSkills,
			PointsSpent = PointsSpent - refund
		};
	}

	/// <summary>
	/// Creates a new state with a skill school unlocked.
	/// </summary>
	public CharacterCreationState WithSkillSchoolUnlocked(TagProto.ID school, int cost) {
		var newSchools = new HashSet<TagProto.ID>(UnlockedSkillSchools) { school };

		return this with {
			UnlockedSkillSchools = newSchools,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with a spell added.
	/// </summary>
	public CharacterCreationState WithSpellAdded(SpellProto.ID spell, int cost) {
		var newSpells = new HashSet<SpellProto.ID>(SelectedSpells) { spell };

		return this with {
			SelectedSpells = newSpells,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with a spell removed.
	/// </summary>
	public CharacterCreationState WithSpellRemoved(SpellProto.ID spell, int refund) {
		var newSpells = new HashSet<SpellProto.ID>(SelectedSpells);
		newSpells.Remove(spell);

		return this with {
			SelectedSpells = newSpells,
			PointsSpent = PointsSpent - refund
		};
	}

	/// <summary>
	/// Creates a new state with a spell school unlocked.
	/// </summary>
	public CharacterCreationState WithSpellSchoolUnlocked(TagProto.ID school, int cost) {
		var newSchools = new HashSet<TagProto.ID>(UnlockedSpellSchools) { school };

		return this with {
			UnlockedSpellSchools = newSchools,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with a tag added.
	/// </summary>
	public CharacterCreationState WithTagAdded(TagProto.ID tag, int cost) {
		var newTags = new HashSet<TagProto.ID>(SelectedTags) { tag };

		return this with {
			SelectedTags = newTags,
			PointsSpent = PointsSpent + cost
		};
	}

	/// <summary>
	/// Creates a new state with appearance changed.
	/// </summary>
	public CharacterCreationState WithAppearance(
		int? portraitIndex = null,
		int? hairStyleIndex = null,
		int? hairColorIndex = null,
		int? skinToneIndex = null
	) {
		return this with {
			PortraitIndex = portraitIndex ?? PortraitIndex,
			HairStyleIndex = hairStyleIndex ?? HairStyleIndex,
			HairColorIndex = hairColorIndex ?? HairColorIndex,
			SkinToneIndex = skinToneIndex ?? SkinToneIndex
		};
	}

	#endregion
}