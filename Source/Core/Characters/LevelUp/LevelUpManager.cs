using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;

namespace RPGGame.Core.Characters.LevelUp;

/// <summary>
/// Manages level-up logic using the same point-buy system as character creation.
/// Players receive creation points per level and can spend them on attributes, skills, spells, or resources.
/// All costs use CreationPointCosts for consistency with character creation.
/// </summary>
public class LevelUpManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly RunState k_runState;
	private readonly CharacterClassProto? k_classProto;

	private LevelUpState k_currentState;
	private readonly Stack<LevelUpState> k_undoStack = new();

	#endregion

	#region Properties

	/// <summary>Current level-up state.</summary>
	public LevelUpState CurrentState => k_currentState;

	/// <summary>The character being leveled up.</summary>
	public LiveCharacter Character => k_runState.Character;

	/// <summary>The character's class proto.</summary>
	public CharacterClassProto? ClassProto => k_classProto;

	/// <summary>Whether undo is available.</summary>
	public bool CanUndo => k_undoStack.Count > 0;

	/// <summary>Points remaining to spend.</summary>
	public int PointsRemaining => k_currentState.PointsRemaining;

	/// <summary>Whether the character can learn spells.</summary>
	public bool CanCastSpells => k_classProto?.CanCastSpells ?? false;

	/// <summary>The new level after applying pending level-ups.</summary>
	public int NewLevel => k_runState.Level + k_runState.PendingLevelUps;

	#endregion

	#region Events

	/// <summary>Fired when state changes.</summary>
	public event Action<LevelUpState>? OnStateChanged;

	/// <summary>Fired when validation fails.</summary>
	public event Action<string>? OnValidationFailed;

	#endregion

	#region Constructor

	public LevelUpManager(GameDb gameDb, RunState runState) {
		k_gameDb = gameDb;
		k_runState = runState;

		// Look up class proto
		k_classProto = runState.Character.ClassId.HasValue
			? gameDb.GetOrNull<CharacterClassProto>(runState.Character.ClassId.Value)
			: null;

		// Initialize state from current character
		k_currentState = LevelUpState.CreateFromRun(runState);
	}

	#endregion

	#region Attribute Management

	/// <summary>
	/// Gets the cost to increase an attribute from its current value.
	/// Uses the same cost curve as character creation.
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
		return CreationPointCosts.GetAttributeIncreaseCost(currentValue - 1);
	}

	/// <summary>
	/// Checks if an attribute can be increased.
	/// </summary>
	public bool CanIncreaseAttribute(StatProto.ID attribute) {
		int cost = GetAttributeIncreaseCost(attribute);
		return k_currentState.PointsRemaining >= cost;
	}

	/// <summary>
	/// Checks if an attribute can be decreased (not below base value from before level-up).
	/// </summary>
	public bool CanDecreaseAttribute(StatProto.ID attribute) {
		int currentValue = k_currentState.Attributes.GetValueOrDefault(attribute, 10);
		int baseValue = k_currentState.BaseAttributes.GetValueOrDefault(attribute, 10);
		return currentValue > baseValue;
	}

	/// <summary>
	/// Increases an attribute by 1.
	/// </summary>
	public bool IncreaseAttribute(StatProto.ID attribute) {
		if (!CanIncreaseAttribute(attribute)) {
			OnValidationFailed?.Invoke("Not enough points to increase this attribute.");
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
			OnValidationFailed?.Invoke("Cannot decrease below starting value.");
			return false;
		}

		int refund = GetAttributeDecreaseRefund(attribute);
		ApplyStateChange(k_currentState.WithAttributeDecreased(attribute, refund));
		return true;
	}

	#endregion

	#region Resource Management (HP/MP/Stamina)

	/// <summary>Gets the cost to increase max health by 5.</summary>
	public int GetHealthIncreaseCost() => CreationPointCosts.HealthCost;

	/// <summary>Gets the cost to increase max mana by 5.</summary>
	public int GetManaIncreaseCost() => CreationPointCosts.ManaCost;

	/// <summary>Gets the cost to increase max stamina by 5.</summary>
	public int GetStaminaIncreaseCost() => CreationPointCosts.StaminaCost;

	/// <summary>Checks if health can be increased.</summary>
	public bool CanIncreaseHealth() => k_currentState.PointsRemaining >= GetHealthIncreaseCost();

	/// <summary>Checks if mana can be increased.</summary>
	public bool CanIncreaseMana() => k_currentState.PointsRemaining >= GetManaIncreaseCost();

	/// <summary>Checks if stamina can be increased.</summary>
	public bool CanIncreaseStamina() => k_currentState.PointsRemaining >= GetStaminaIncreaseCost();

	/// <summary>Checks if health can be decreased.</summary>
	public bool CanDecreaseHealth() => k_currentState.BonusHealth > 0;

	/// <summary>Checks if mana can be decreased.</summary>
	public bool CanDecreaseMana() => k_currentState.BonusMana > 0;

	/// <summary>Checks if stamina can be decreased.</summary>
	public bool CanDecreaseStamina() => k_currentState.BonusStamina > 0;

	/// <summary>Increases max health by 5.</summary>
	public bool IncreaseHealth() {
		if (!CanIncreaseHealth()) {
			OnValidationFailed?.Invoke("Not enough points.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusHealth(5, GetHealthIncreaseCost()));
		return true;
	}

	/// <summary>Decreases max health by 5.</summary>
	public bool DecreaseHealth() {
		if (!CanDecreaseHealth()) {
			OnValidationFailed?.Invoke("Cannot decrease below starting value.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusHealthRemoved(5, GetHealthIncreaseCost()));
		return true;
	}

	/// <summary>Increases max mana by 5.</summary>
	public bool IncreaseMana() {
		if (!CanIncreaseMana()) {
			OnValidationFailed?.Invoke("Not enough points.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusMana(5, GetManaIncreaseCost()));
		return true;
	}

	/// <summary>Decreases max mana by 5.</summary>
	public bool DecreaseMana() {
		if (!CanDecreaseMana()) {
			OnValidationFailed?.Invoke("Cannot decrease below starting value.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusManaRemoved(5, GetManaIncreaseCost()));
		return true;
	}

	/// <summary>Increases max stamina by 5.</summary>
	public bool IncreaseStamina() {
		if (!CanIncreaseStamina()) {
			OnValidationFailed?.Invoke("Not enough points.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusStamina(5, GetStaminaIncreaseCost()));
		return true;
	}

	/// <summary>Decreases max stamina by 5.</summary>
	public bool DecreaseStamina() {
		if (!CanDecreaseStamina()) {
			OnValidationFailed?.Invoke("Cannot decrease below starting value.");
			return false;
		}
		ApplyStateChange(k_currentState.WithBonusStaminaRemoved(5, GetStaminaIncreaseCost()));
		return true;
	}

	#endregion

	#region Skill Management

	/// <summary>
	/// Gets all skills available to learn or rank up at the new level.
	/// </summary>
	public List<SkillProto> GetAvailableSkills() {
		return k_gameDb.GetAll<SkillProto>()
			.Where(s => s.RequiredLevel <= NewLevel)
			.OrderBy(s => s.RequiredLevel)
			.ThenBy(s => s.BaseCost)
			.ThenBy(s => s.DisplayText.Name)
			.ToList();
	}

	/// <summary>
	/// Gets the cost to increase a skill's rank.
	/// </summary>
	public int GetSkillRankCost(SkillProto.ID skillId) {
		var skill = k_gameDb.GetOrNull<SkillProto>(skillId);
		if (skill == null) return int.MaxValue;

		int totalRank = k_currentState.GetTotalSkillRank(skillId);
		return skill.GetRankCost(totalRank);
	}

	/// <summary>
	/// Gets the refund for decreasing a skill's rank.
	/// </summary>
	public int GetSkillRankRefund(SkillProto.ID skillId) {
		var skill = k_gameDb.GetOrNull<SkillProto>(skillId);
		if (skill == null) return 0;

		int totalRank = k_currentState.GetTotalSkillRank(skillId);
		return skill.GetRankCost(totalRank - 1);
	}

	/// <summary>
	/// Checks if a skill's rank can be increased.
	/// </summary>
	public bool CanIncreaseSkillRank(SkillProto.ID skillId) {
		var skill = k_gameDb.GetOrNull<SkillProto>(skillId);
		if (skill == null) return false;

		int totalRank = k_currentState.GetTotalSkillRank(skillId);
		if (totalRank >= CreationPointCosts.MAX_SKILL_RANK) return false;

		int cost = GetSkillRankCost(skillId);
		return k_currentState.PointsRemaining >= cost;
	}

	/// <summary>
	/// Checks if a skill's rank can be decreased (only pending ranks can be removed).
	/// </summary>
	public bool CanDecreaseSkillRank(SkillProto.ID skillId) {
		return k_currentState.GetPendingSkillRank(skillId) > 0;
	}

	/// <summary>
	/// Increases a skill's rank by 1.
	/// </summary>
	public bool IncreaseSkillRank(SkillProto.ID skillId) {
		if (!CanIncreaseSkillRank(skillId)) {
			OnValidationFailed?.Invoke("Not enough points for this skill.");
			return false;
		}

		int cost = GetSkillRankCost(skillId);
		ApplyStateChange(k_currentState.WithSkillRankIncreased(skillId, cost));
		return true;
	}

	/// <summary>
	/// Decreases a skill's rank by 1 (only pending ranks, refunds points).
	/// </summary>
	public bool DecreaseSkillRank(SkillProto.ID skillId) {
		if (!CanDecreaseSkillRank(skillId)) {
			OnValidationFailed?.Invoke("Cannot remove ranks gained before this level-up.");
			return false;
		}

		int refund = GetSkillRankRefund(skillId);
		ApplyStateChange(k_currentState.WithSkillRankDecreased(skillId, refund));
		return true;
	}

	#endregion

	#region Spell Management

	/// <summary>
	/// Gets all spells available to learn at the new level.
	/// </summary>
	public List<SpellProto> GetAvailableSpells() {
		if (!CanCastSpells) return [];

		var classId = Character.ClassId;

		return k_gameDb.GetAll<SpellProto>()
			.Where(s => s.RequiredLevel <= NewLevel)
			.Where(s => !k_currentState.Spells.Contains(s.Id)) // Not already known
			.Where(s => !k_currentState.NewSpells.Contains(s.Id)) // Not pending
			.Where(s => s.ClassRestrictions.Count == 0 ||
				(classId.HasValue && s.ClassRestrictions.Contains(classId.Value)))
			.OrderBy(s => s.School)
			.ThenBy(s => s.Level)
			.ThenBy(s => s.DisplayText.Name)
			.ToList();
	}

	/// <summary>
	/// Gets the cost to learn a spell.
	/// </summary>
	public int GetSpellCost(SpellProto.ID spellId) {
		var spell = k_gameDb.GetOrNull<SpellProto>(spellId);
		if (spell == null) return int.MaxValue;

		return CreationPointCosts.GetSpellLearnCost(spell.Level);
	}

	/// <summary>
	/// Checks if a spell can be learned.
	/// </summary>
	public bool CanAddSpell(SpellProto.ID spellId) {
		if (!CanCastSpells) return false;

		var spell = k_gameDb.GetOrNull<SpellProto>(spellId);
		if (spell == null) return false;

		// Already known
		if (k_currentState.Spells.Contains(spellId)) return false;
		if (k_currentState.NewSpells.Contains(spellId)) return false;

		int cost = GetSpellCost(spellId);
		return k_currentState.PointsRemaining >= cost;
	}

	/// <summary>
	/// Checks if a pending spell can be removed.
	/// </summary>
	public bool CanRemoveSpell(SpellProto.ID spellId) {
		return k_currentState.NewSpells.Contains(spellId);
	}

	/// <summary>
	/// Learns a spell.
	/// </summary>
	public bool AddSpell(SpellProto.ID spellId) {
		if (!CanAddSpell(spellId)) {
			OnValidationFailed?.Invoke("Not enough points for this spell.");
			return false;
		}

		int cost = GetSpellCost(spellId);
		ApplyStateChange(k_currentState.WithSpellAdded(spellId, cost));
		return true;
	}

	/// <summary>
	/// Removes a pending spell (refunds points).
	/// </summary>
	public bool RemoveSpell(SpellProto.ID spellId) {
		if (!CanRemoveSpell(spellId)) {
			OnValidationFailed?.Invoke("Cannot remove spells learned before this level-up.");
			return false;
		}

		int refund = GetSpellCost(spellId);
		ApplyStateChange(k_currentState.WithSpellRemoved(spellId, refund));
		return true;
	}

	#endregion

	#region Undo / Reset

	/// <summary>
	/// Undoes the last change.
	/// </summary>
	public bool Undo() {
		if (!CanUndo) return false;

		k_currentState = k_undoStack.Pop();
		OnStateChanged?.Invoke(k_currentState);
		return true;
	}

	/// <summary>
	/// Resets all pending changes.
	/// </summary>
	public void Reset() {
		k_undoStack.Clear();
		k_currentState = LevelUpState.CreateFromRun(k_runState);
		OnStateChanged?.Invoke(k_currentState);
	}

	private void ApplyStateChange(LevelUpState newState) {
		k_undoStack.Push(k_currentState);
		k_currentState = newState;
		OnStateChanged?.Invoke(k_currentState);
	}

	#endregion

	#region Finalization

	/// <summary>
	/// Applies all pending changes to the character.
	/// </summary>
	public void ApplyLevelUp() {
		var character = k_runState.Character;
		var stats = k_runState.Stats;

		// Apply level increases
		while (k_runState.PendingLevelUps > 0) {
			k_runState.PendingLevelUps--;
			stats.Add(Ids.Stats.Meta.Level, 1);
		}
		k_runState.HasPendingLevelUp = k_runState.PendingLevelUps > 0;

		// Apply attribute changes (delta from base)
		foreach (var (attrId, newValue) in k_currentState.Attributes) {
			int baseValue = k_currentState.BaseAttributes.GetValueOrDefault(attrId, 10);
			int delta = newValue - baseValue;
			if (delta > 0) {
				stats.Add(attrId, delta);
			}
		}

		// Apply resource bonuses
		if (k_currentState.BonusHealth > 0) {
			stats.Add(Ids.Stats.Resource.MaxHealth, k_currentState.BonusHealth);
		}
		if (k_currentState.BonusMana > 0) {
			stats.Add(Ids.Stats.Resource.MaxMana, k_currentState.BonusMana);
		}
		if (k_currentState.BonusStamina > 0) {
			stats.Add(Ids.Stats.Resource.MaxStamina, k_currentState.BonusStamina);
		}

		// Apply new skill ranks
		foreach (var (skillId, rankIncrease) in k_currentState.NewSkillRanks) {
			if (character.Skills.ContainsKey(skillId)) {
				character.Skills[skillId] += rankIncrease;
			} else {
				character.Skills[skillId] = rankIncrease;
			}
		}

		// Apply new spells
		foreach (var spellId in k_currentState.NewSpells) {
			character.KnownSpells.Add(spellId);
			character.PreparedSpells.Add(spellId);
		}

		// Store unspent points in stats for future level-ups
		stats.Set(Ids.Stats.Meta.CreationPointsUnspent, k_currentState.PointsRemaining);

		// Heal to full on level up
		stats.Set(Ids.Stats.Resource.CurrentHealth, k_runState.MaxHealth);
		stats.Set(Ids.Stats.Resource.CurrentMana, k_runState.MaxMana);
	}

	#endregion
}