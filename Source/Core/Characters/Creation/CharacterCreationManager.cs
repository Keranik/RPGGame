using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Spells;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Manages character creation logic, validation, and random generation.
/// </summary>
public class CharacterCreationManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly Random k_random;

	private CharacterCreationState k_currentState;
	private readonly Stack<CharacterCreationState> k_undoStack = new();
	private readonly Stack<CharacterCreationState> k_redoStack = new();

	#endregion

	#region Properties

	/// <summary>Current creation state.</summary>
	public CharacterCreationState CurrentState => k_currentState;

	/// <summary>Whether undo is available.</summary>
	public bool CanUndo => k_undoStack.Count > 0 && !k_currentState.IsRandomReroll;

	/// <summary>Whether redo is available.</summary>
	public bool CanRedo => k_redoStack.Count > 0 && !k_currentState.IsRandomReroll;

	/// <summary>Selected class proto (if any).</summary>
	public CharacterClassProto? SelectedClass =>
		k_currentState.SelectedClassId.HasValue
			? k_gameDb.Get<CharacterClassProto>(k_currentState.SelectedClassId.Value)
			: null;

	/// <summary>
	/// Whether modifications are currently locked (random reroll mode).
	/// When true, user must Reset or Reroll Again - no manual changes allowed.
	/// </summary>
	public bool IsModificationLocked => k_currentState.IsRandomReroll;

	#endregion

	#region Events

	/// <summary>Fired when state changes.</summary>
	public event Action<CharacterCreationState>? OnStateChanged;

	/// <summary>Fired when validation fails.</summary>
	public event Action<string>? OnValidationFailed;

	#endregion

	#region Constructor

	public CharacterCreationManager(GameDb gameDb, Random? random = null) {
		k_gameDb = gameDb;
		k_random = random ?? new Random();
		k_currentState = new CharacterCreationState();
	}

	#endregion

	#region Class Selection

	/// <summary>
	/// Selects a class and initializes the creation state.
	/// </summary>
	public bool SelectClass(CharacterClassProto.ID classId, bool isRandomReroll = false) {
		var classProto = k_gameDb.Get<CharacterClassProto>(classId);
		if (classProto == null) {
			OnValidationFailed?.Invoke("Invalid class selected.");
			return false;
		}

		k_undoStack.Clear();
		k_redoStack.Clear();

		k_currentState = CharacterCreationState.CreateForClass(classProto, isRandomReroll);
		OnStateChanged?.Invoke(k_currentState);
		return true;
	}

	/// <summary>
	/// Resets to class defaults, removing any random reroll bonus.
	/// This allows manual customization after viewing a random build.
	/// </summary>
	public bool ResetToClassDefaults() {
		var classProto = SelectedClass;
		if (classProto == null) {
			OnValidationFailed?.Invoke("No class selected.");
			return false;
		}

		k_undoStack.Clear();
		k_redoStack.Clear();

		// Reset WITHOUT random reroll bonus - user forfeits the bonus
		k_currentState = CharacterCreationState.CreateForClass(classProto, isRandomReroll: false);
		OnStateChanged?.Invoke(k_currentState);
		return true;
	}

	/// <summary>
	/// Gets all available classes (unlocked based on meta progression).
	/// </summary>
	public List<CharacterClassProto> GetAvailableClasses(MetaProgression meta) {
		return k_gameDb.GetAll<CharacterClassProto>()
			.Where(c => c.StartsUnlocked || meta.UnlockedClasses.Contains(c.Id.Value))
			.OrderBy(c => c.Difficulty)
			.ThenBy(c => c.DisplayText.Name)
			.ToList();
	}

	#endregion

	#region Attribute Management

	/// <summary>
	/// Gets the cost to increase an attribute.
	/// </summary>
	public int GetAttributeIncreaseCost(StatProto.ID attribute) {
		int currentValue = k_currentState.Attributes.GetValueOrDefault(attribute, 10);
		return CreationPointCosts.GetAttributeIncreaseCost(currentValue);
	}

	/// <summary>
	/// Gets the refund for decreasing an attribute.
	/// </summary>
	public int GetAttributeDecreaseRefund(StatProto.ID attribute) {
		int currentValue = k_currentState.Attributes.GetValueOrDefault(attribute, 10);
		// Refund is cost of the previous level
		return CreationPointCosts.GetAttributeIncreaseCost(currentValue - 1);
	}

	/// <summary>
	/// Checks if an attribute can be increased.
	/// </summary>
	public bool CanIncreaseAttribute(StatProto.ID attribute) {
		// Locked during random reroll
		if (IsModificationLocked) {
			return false;
		}

		int currentValue = k_currentState.Attributes.GetValueOrDefault(attribute, 10);
		int cost = GetAttributeIncreaseCost(attribute);

		return currentValue < CreationPointCosts.MAX_CREATION_ATTRIBUTE_VALUE &&
			   k_currentState.PointsRemaining >= cost;
	}

	/// <summary>
	/// Checks if an attribute can be decreased (above class minimum).
	/// </summary>
	public bool CanDecreaseAttribute(StatProto.ID attribute) {
		// Locked during random reroll
		if (IsModificationLocked) {
			return false;
		}

		var classProto = SelectedClass;
		if (classProto == null) {
			return false;
		}

		int currentValue = k_currentState.Attributes.GetValueOrDefault(attribute, 10);
		int minValue = GetClassBaseAttribute(classProto, attribute);

		return currentValue > minValue;
	}

	/// <summary>
	/// Increases an attribute by 1.
	/// </summary>
	public bool IncreaseAttribute(StatProto.ID attribute) {
		if (!CanIncreaseAttribute(attribute)) {
			if (IsModificationLocked) {
				OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			} else {
				OnValidationFailed?.Invoke("Cannot increase this attribute.");
			}
			return false;
		}

		int cost = GetAttributeIncreaseCost(attribute);
		ApplyStateChange(k_currentState.WithAttributeIncreased(attribute, cost));
		return true;
	}

	/// <summary>
	/// Decreases an attribute by 1 (refunds points).
	/// </summary>
	public bool DecreaseAttribute(StatProto.ID attribute) {
		if (!CanDecreaseAttribute(attribute)) {
			if (IsModificationLocked) {
				OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			} else {
				OnValidationFailed?.Invoke("Cannot decrease below class minimum.");
			}
			return false;
		}

		int refund = GetAttributeDecreaseRefund(attribute);
		ApplyStateChange(k_currentState.WithAttributeDecreased(attribute, refund));
		return true;
	}

	private int GetClassBaseAttribute(CharacterClassProto classProto, StatProto.ID attribute) {
		if (attribute == Ids.Stats.Attributes.Strength) {
			return classProto.BaseAttributes.Strength;
		}
		if (attribute == Ids.Stats.Attributes.Dexterity) {
			return classProto.BaseAttributes.Dexterity;
		}
		if (attribute == Ids.Stats.Attributes.Constitution) {
			return classProto.BaseAttributes.Constitution;
		}
		if (attribute == Ids.Stats.Attributes.Intelligence) {
			return classProto.BaseAttributes.Intelligence;
		}
		if (attribute == Ids.Stats.Attributes.Wisdom) {
			return classProto.BaseAttributes.Wisdom;
		}
		if (attribute == Ids.Stats.Attributes.Charisma) {
			return classProto.BaseAttributes.Charisma;
		}
		return 10;
	}

	#endregion

	#region Skill Management

	/// <summary>
	/// Gets all skills available to the current class.
	/// </summary>
	public List<SkillProto> GetAvailableSkills() {
		var classProto = SelectedClass;
		if (classProto == null) {
			return [];
		}

		var accessibleSchools = GetAccessibleSkillSchools();

		return k_gameDb.GetAll<SkillProto>()
			.Where(s => CanAccessSkill(s, accessibleSchools))
			.Where(s => s.RequiredLevel <= 1) // Only level 1 skills during creation
			.OrderBy(s => s.BaseCost)
			.ThenBy(s => s.DisplayText.Name)
			.ToList();
	}

	/// <summary>
	/// Gets skill schools accessible to the current class.
	/// </summary>
	public HashSet<TagProto.ID> GetAccessibleSkillSchools() {
		var classProto = SelectedClass;
		if (classProto == null) {
			return [];
		}

		var schools = new HashSet<TagProto.ID>(classProto.StartingSkillSchools);
		foreach (var unlocked in k_currentState.UnlockedSkillSchools) {
			schools.Add(unlocked);
		}
		return schools;
	}

	/// <summary>
	/// Checks if a skill can be accessed based on school requirements.
	/// </summary>
	public bool CanAccessSkill(SkillProto skill, HashSet<TagProto.ID>? accessibleSchools = null) {
		accessibleSchools ??= GetAccessibleSkillSchools();

		// Check if skill requires specific tags we don't have
		foreach (var requiredTag in skill.RequiredTags) {
			if (!accessibleSchools.Contains(requiredTag) &&
				!k_currentState.SelectedTags.Contains(requiredTag)) {
				return false;
			}
		}

		// Check if skill has any school tag we have access to
		// (skills without school tags are universally accessible)
		var skillSchoolTags = skill.Tags.Where(IsSchoolTag).ToList();
		if (skillSchoolTags.Count == 0) {
			return true;
		}

		return skillSchoolTags.Any(t => accessibleSchools.Contains(t));
	}

	/// <summary>
	/// Gets the cost to add a skill or increase its rank.
	/// </summary>
	public int GetSkillCost(SkillProto.ID skillId) {
		var skill = k_gameDb.Get<SkillProto>(skillId);
		if (skill == null) {
			return int.MaxValue;
		}

		int currentRank = k_currentState.SelectedSkills.GetValueOrDefault(skillId, 0);
		return skill.GetRankCost(currentRank);
	}

	/// <summary>
	/// Checks if a skill can be added or ranked up.
	/// </summary>
	public bool CanAddSkill(SkillProto.ID skillId) {
		// Locked during random reroll
		if (IsModificationLocked) {
			return false;
		}

		var skill = k_gameDb.Get<SkillProto>(skillId);
		if (skill == null) {
			return false;
		}

		int currentRank = k_currentState.SelectedSkills.GetValueOrDefault(skillId, 0);
		if (currentRank >= CreationPointCosts.MAX_CREATION_SKILL_RANK) {
			return false;
		}

		int cost = GetSkillCost(skillId);
		if (k_currentState.PointsRemaining < cost) {
			return false;
		}

		return CanAccessSkill(skill);
	}

	/// <summary>
	/// Adds a skill or increases its rank.
	/// </summary>
	public bool AddSkill(SkillProto.ID skillId) {
		if (!CanAddSkill(skillId)) {
			if (IsModificationLocked) {
				OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			} else {
				OnValidationFailed?.Invoke("Cannot add this skill.");
			}
			return false;
		}

		int cost = GetSkillCost(skillId);
		ApplyStateChange(k_currentState.WithSkillAdded(skillId, cost));
		return true;
	}

	/// <summary>
	/// Removes a skill rank (refunds points).
	/// </summary>
	public bool RemoveSkill(SkillProto.ID skillId) {
		// Locked during random reroll
		if (IsModificationLocked) {
			OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			return false;
		}

		var classProto = SelectedClass;
		if (classProto == null) {
			return false;
		}

		// Can't remove starting skills
		if (classProto.StartingSkills.Contains(skillId)) {
			OnValidationFailed?.Invoke("Cannot remove starting skills.");
			return false;
		}

		if (!k_currentState.SelectedSkills.ContainsKey(skillId)) {
			return false;
		}

		int currentRank = k_currentState.SelectedSkills[skillId];
		var skill = k_gameDb.Get<SkillProto>(skillId);
		int refund = skill?.GetRankCost(currentRank - 1) ?? 0;

		ApplyStateChange(k_currentState.WithSkillRemoved(skillId, refund));
		return true;
	}

	#endregion

	#region Spell Management

	/// <summary>
	/// Gets all spells available to the current class.
	/// </summary>
	public List<SpellProto> GetAvailableSpells() {
		var classProto = SelectedClass;
		if (classProto == null || !classProto.CanCastSpells) {
			return [];
		}

		var accessibleSchools = GetAccessibleSpellSchools();

		return k_gameDb.GetAll<SpellProto>()
			.Where(s => CanAccessSpell(s, accessibleSchools))
			.Where(s => s.Level <= CreationPointCosts.MAX_CREATION_SPELL_LEVEL)
			.Where(s => s.RequiredLevel <= 1)
			.OrderBy(s => s.Level)
			.ThenBy(s => CreationPointCosts.GetSpellLearnCost(s.Level))
			.ThenBy(s => s.DisplayText.Name)
			.ToList();
	}

	/// <summary>
	/// Gets spell schools accessible to the current class.
	/// </summary>
	public HashSet<TagProto.ID> GetAccessibleSpellSchools() {
		var classProto = SelectedClass;
		if (classProto == null) {
			return [];
		}

		var schools = new HashSet<TagProto.ID>(classProto.StartingSpellSchools);
		foreach (var unlocked in k_currentState.UnlockedSpellSchools) {
			schools.Add(unlocked);
		}
		return schools;
	}

	/// <summary>
	/// Checks if a spell can be accessed based on school/class restrictions.
	/// </summary>
	public bool CanAccessSpell(SpellProto spell, HashSet<TagProto.ID>? accessibleSchools = null) {
		var classProto = SelectedClass;
		if (classProto == null) {
			return false;
		}

		// Check class restrictions
		if (spell.ClassRestrictions.Count > 0 &&
			!spell.ClassRestrictions.Any(c => c == classProto.Id)) {
			return false;
		}

		accessibleSchools ??= GetAccessibleSpellSchools();

		// Map spell school to tag
		var schoolTag = GetSpellSchoolTag(spell.School);
		if (schoolTag.HasValue) {
			return accessibleSchools.Contains(schoolTag.Value);
		}

		return true; // No school requirement
	}

	/// <summary>
	/// Gets the cost to learn a spell.
	/// </summary>
	public int GetSpellCost(SpellProto.ID spellId) {
		var spell = k_gameDb.Get<SpellProto>(spellId);
		if (spell == null) {
			return int.MaxValue;
		}

		return CreationPointCosts.GetSpellLearnCost(spell.Level);
	}

	/// <summary>
	/// Checks if a spell can be added.
	/// </summary>
	public bool CanAddSpell(SpellProto.ID spellId) {
		// Locked during random reroll
		if (IsModificationLocked) {
			return false;
		}

		var spell = k_gameDb.Get<SpellProto>(spellId);
		if (spell == null) {
			return false;
		}

		// Already have it
		if (k_currentState.SelectedSpells.Contains(spellId)) {
			return false;
		}

		// Max spells
		if (k_currentState.SelectedSpells.Count >= CreationPointCosts.MAX_CREATION_SPELLS) {
			return false;
		}

		int cost = GetSpellCost(spellId);
		if (k_currentState.PointsRemaining < cost) {
			return false;
		}

		return CanAccessSpell(spell);
	}

	/// <summary>
	/// Adds a spell.
	/// </summary>
	public bool AddSpell(SpellProto.ID spellId) {
		if (!CanAddSpell(spellId)) {
			if (IsModificationLocked) {
				OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			} else {
				OnValidationFailed?.Invoke("Cannot add this spell.");
			}
			return false;
		}

		int cost = GetSpellCost(spellId);
		ApplyStateChange(k_currentState.WithSpellAdded(spellId, cost));
		return true;
	}

	/// <summary>
	/// Removes a spell (refunds points).
	/// </summary>
	public bool RemoveSpell(SpellProto.ID spellId) {
		// Locked during random reroll
		if (IsModificationLocked) {
			OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			return false;
		}

		var classProto = SelectedClass;
		if (classProto == null) {
			return false;
		}

		// Can't remove starting spells
		if (classProto.StartingSpells.Contains(spellId)) {
			OnValidationFailed?.Invoke("Cannot remove starting spells.");
			return false;
		}

		if (!k_currentState.SelectedSpells.Contains(spellId)) {
			return false;
		}

		var spell = k_gameDb.Get<SpellProto>(spellId);
		int refund = spell != null ? CreationPointCosts.GetSpellLearnCost(spell.Level) : 0;

		ApplyStateChange(k_currentState.WithSpellRemoved(spellId, refund));
		return true;
	}

	#endregion

	#region School Unlocking

	/// <summary>
	/// Unlocks a skill school.
	/// </summary>
	public bool UnlockSkillSchool(TagProto.ID school) {
		// Locked during random reroll
		if (IsModificationLocked) {
			OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			return false;
		}

		if (k_currentState.UnlockedSkillSchools.Contains(school)) {
			return false;
		}
		if (k_currentState.PointsRemaining < CreationPointCosts.SkillCategoryCost) {
			OnValidationFailed?.Invoke("Not enough points to unlock skill school.");
			return false;
		}

		ApplyStateChange(k_currentState.WithSkillSchoolUnlocked(school, CreationPointCosts.SkillCategoryCost));
		return true;
	}

	/// <summary>
	/// Unlocks a spell school.
	/// </summary>
	public bool UnlockSpellSchool(TagProto.ID school) {
		// Locked during random reroll
		if (IsModificationLocked) {
			OnValidationFailed?.Invoke("Cannot modify random build. Reset to customize manually.");
			return false;
		}

		if (k_currentState.UnlockedSpellSchools.Contains(school)) {
			return false;
		}
		if (k_currentState.PointsRemaining < CreationPointCosts.SpellSchoolCost) {
			OnValidationFailed?.Invoke("Not enough points to unlock spell school.");
			return false;
		}

		ApplyStateChange(k_currentState.WithSpellSchoolUnlocked(school, CreationPointCosts.SpellSchoolCost));
		return true;
	}

	#endregion

	#region Random Reroll

	
	/// <summary>
	/// Generates a random character build for the selected class.
	/// Uses the 20% bonus points from random reroll.
	/// After calling this, modifications are locked until Reset is called.
	/// </summary>
	public void RandomReroll() {
		RandomReroll(null);
	}

	public void RandomReroll(CharacterClassProto? randomClass) {
		var classProto = randomClass ?? SelectedClass;

		if (classProto == null) {
			return;
		}


		// IMPORTANT: Create state WITHOUT IsRandomReroll first, so strategies can use CanXxx methods
		// We give the bonus points but don't lock yet
		k_currentState = CharacterCreationState.CreateForClass(classProto, isRandomReroll: false);
		
		// Manually add the bonus points (20% of base)
		int bonusPoints = classProto.GetCreationPoints(true);
		k_currentState = k_currentState with { TotalPoints = k_currentState.TotalPoints + bonusPoints };

		// Generate random name
		k_currentState = k_currentState.WithName(GenerateRandomName());

		// Apply random strategy (this works now because IsRandomReroll is false)
		var strategy = SelectRandomStrategy(classProto);
		ApplyGenerationStrategy(strategy);

		// NOW lock the build by setting IsRandomReroll = true
		k_currentState = k_currentState with { IsRandomReroll = true };

		k_redoStack.Clear();
		k_undoStack.Clear();
		OnStateChanged?.Invoke(k_currentState);
	}

	private BaseGenerationStrategy SelectRandomStrategy(CharacterClassProto classProto) {
		// Weight strategies based on class role
		var strategies = new List<(BaseGenerationStrategy strategy, float weight)>();

		switch (classProto.Role) {
			case ClassRole.Melee:
			case ClassRole.Tank:
				strategies.Add((new OffensiveStrategy(k_gameDb, k_random), 0.5f));
				strategies.Add((new DefensiveStrategy(k_gameDb, k_random), 0.3f));
				strategies.Add((new BalancedStrategy(k_gameDb, k_random), 0.2f));
				break;

			case ClassRole.Caster:
				strategies.Add((new SpellHeavyStrategy(k_gameDb, k_random), 0.5f));
				strategies.Add((new BalancedStrategy(k_gameDb, k_random), 0.3f));
				strategies.Add((new DefensiveStrategy(k_gameDb, k_random), 0.2f));
				break;

			case ClassRole.Ranged:
				strategies.Add((new OffensiveStrategy(k_gameDb, k_random), 0.4f));
				strategies.Add((new SkillHeavyStrategy(k_gameDb, k_random), 0.3f));
				strategies.Add((new BalancedStrategy(k_gameDb, k_random), 0.3f));
				break;

			case ClassRole.Support:
			case ClassRole.Hybrid:
			default:
				strategies.Add((new BalancedStrategy(k_gameDb, k_random), 0.4f));
				strategies.Add((new SpellHeavyStrategy(k_gameDb, k_random), 0.3f));
				strategies.Add((new DefensiveStrategy(k_gameDb, k_random), 0.3f));
				break;
		}

		float totalWeight = strategies.Sum(s => s.weight);
		float roll = (float)k_random.NextDouble() * totalWeight;
		float cumulative = 0;

		foreach (var (strategy, weight) in strategies) {
			cumulative += weight;
			if (roll <= cumulative) {
				return strategy;
			}
		}

		return new BalancedStrategy(k_gameDb, k_random);
	}

	private void ApplyGenerationStrategy(BaseGenerationStrategy strategy) {
		int iterations = 0;
		const int maxIterations = 200;

		while (k_currentState.PointsRemaining > 0 && iterations < maxIterations) {
			iterations++;

			var upgrade = strategy.SelectUpgrade(k_currentState, SelectedClass!, this);
			if (upgrade == null) {
				break;
			}

			if (!TryApplyUpgradeInternal(upgrade)) {
				break;
			}
		}
	}

	/// <summary>
	/// Internal upgrade application used during random generation.
	/// </summary>
	private bool TryApplyUpgradeInternal(CharacterUpgrade upgrade) {
		switch (upgrade.Type) {
			case CharacterUpgradeType.Attribute:
				if (upgrade.AttributeId.HasValue) {
					int currentValue = k_currentState.Attributes.GetValueOrDefault(upgrade.AttributeId.Value, 10);
					int cost = CreationPointCosts.GetAttributeIncreaseCost(currentValue);
					if (currentValue < CreationPointCosts.MAX_CREATION_ATTRIBUTE_VALUE && k_currentState.PointsRemaining >= cost) {
						k_currentState = k_currentState.WithAttributeIncreased(upgrade.AttributeId.Value, cost);
						return true;
					}
				}
				break;

			case CharacterUpgradeType.Skill:
				if (upgrade.SkillId.HasValue) {
					var skill = k_gameDb.Get<SkillProto>(upgrade.SkillId.Value);
					if (skill != null) {
						int currentRank = k_currentState.SelectedSkills.GetValueOrDefault(upgrade.SkillId.Value, 0);
						if (currentRank < CreationPointCosts.MAX_CREATION_SKILL_RANK) {
							int cost = skill.GetRankCost(currentRank);
							if (k_currentState.PointsRemaining >= cost && CanAccessSkill(skill)) {
								k_currentState = k_currentState.WithSkillAdded(upgrade.SkillId.Value, cost);
								return true;
							}
						}
					}
				}
				break;

			case CharacterUpgradeType.Spell:
				if (upgrade.SpellId.HasValue) {
					var spell = k_gameDb.Get<SpellProto>(upgrade.SpellId.Value);
					if (spell != null && !k_currentState.SelectedSpells.Contains(upgrade.SpellId.Value)) {
						if (k_currentState.SelectedSpells.Count < CreationPointCosts.MAX_CREATION_SPELLS) {
							int cost = CreationPointCosts.GetSpellLearnCost(spell.Level);
							if (k_currentState.PointsRemaining >= cost && CanAccessSpell(spell)) {
								k_currentState = k_currentState.WithSpellAdded(upgrade.SpellId.Value, cost);
								return true;
							}
						}
					}
				}
				break;

			case CharacterUpgradeType.BonusHealth:
				if (k_currentState.PointsRemaining >= CreationPointCosts.HealthCost) {
					k_currentState = k_currentState.WithBonusHealth(5, CreationPointCosts.HealthCost);
					return true;
				}
				break;

			case CharacterUpgradeType.BonusMana:
				if (k_currentState.PointsRemaining >= CreationPointCosts.ManaCost) {
					k_currentState = k_currentState.WithBonusMana(5, CreationPointCosts.ManaCost);
					return true;
				}
				break;
		}

		return false;
	}

	private string GenerateRandomName() {
		var prefixes = new[] { "Eld", "Thor", "Kal", "Ven", "Mor", "Ash", "Dra", "Fen", "Gor", "Lyn" };
		var suffixes = new[] { "ric", "wyn", "dan", "tor", "gar", "ia", "ius", "ara", "orn", "eth" };

		return prefixes[k_random.Next(prefixes.Length)] + suffixes[k_random.Next(suffixes.Length)];
	}

	#endregion

	#region Undo/Redo

	/// <summary>
	/// Undoes the last change.
	/// </summary>
	public bool Undo() {
		if (!CanUndo) {
			return false;
		}

		k_redoStack.Push(k_currentState);
		k_currentState = k_undoStack.Pop();
		OnStateChanged?.Invoke(k_currentState);
		return true;
	}

	/// <summary>
	/// Redoes the last undone change.
	/// </summary>
	public bool Redo() {
		if (!CanRedo) {
			return false;
		}

		k_undoStack.Push(k_currentState);
		k_currentState = k_redoStack.Pop();
		OnStateChanged?.Invoke(k_currentState);
		return true;
	}

	private void ApplyStateChange(CharacterCreationState newState) {
		k_undoStack.Push(k_currentState);
		k_redoStack.Clear();
		k_currentState = newState;
		OnStateChanged?.Invoke(k_currentState);
	}

	#endregion

	#region Finalization

	/// <summary>
	/// Validates the current build is complete and valid.
	/// </summary>
	public (bool isValid, List<string> errors) ValidateBuild() {
		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(k_currentState.Name)) {
			errors.Add("Character must have a name.");
		}

		if (!k_currentState.SelectedClassId.HasValue) {
			errors.Add("Must select a class.");
		}

		return (errors.Count == 0, errors);
	}

	/// <summary>
	/// Creates the final character data from the current state.
	/// </summary>
	public CharacterCreationResult CreateCharacter() {
		var (isValid, errors) = ValidateBuild();
		if (!isValid) {
			return new CharacterCreationResult {
				Success = false,
				Errors = errors
			};
		}

		return new CharacterCreationResult {
			Success = true,
			State = k_currentState,
			WasRandomReroll = k_currentState.IsRandomReroll
		};
	}

	#endregion

	#region Helpers

	private bool IsSchoolTag(TagProto.ID tagId) {
		return tagId.Value.StartsWith("Tag_School_") ||
			   tagId.Value.StartsWith("Tag_Meta_");
	}

	private TagProto.ID? GetSpellSchoolTag(SpellSchool school) {
		return school switch {
			SpellSchool.Evocation => Ids.Tags.School.Evocation,
			SpellSchool.Abjuration => Ids.Tags.School.Abjuration,
			SpellSchool.Conjuration => Ids.Tags.School.Conjuration,
			SpellSchool.Divination => Ids.Tags.School.Divination,
			SpellSchool.Enchantment => Ids.Tags.School.Enchantment,
			SpellSchool.Illusion => Ids.Tags.School.Illusion,
			SpellSchool.Necromancy => Ids.Tags.School.Necromancy,
			SpellSchool.Transmutation => Ids.Tags.School.Transmutation,
			SpellSchool.Holy => Ids.Tags.School.Holy,
			SpellSchool.Nature => Ids.Tags.School.Nature,
			SpellSchool.Temporal => Ids.Tags.School.Temporal,
			_ => null
		};
	}

	#endregion
}