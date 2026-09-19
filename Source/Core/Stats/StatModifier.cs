using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Stats;

/// <summary>
/// A modifier that affects a stat value.
/// </summary>
public class StatModifier {
	/// <summary>Unique identifier for this modifier.</summary>
	public string Id { get; }

	/// <summary>The stat being modified.</summary>
	public StatProto.ID StatId { get; }

	/// <summary>The modification value.</summary>
	public float Value { get; }

	/// <summary>How the value is applied.</summary>
	public ModifierOperation Operation { get; }

	/// <summary>How long this modifier lasts.</summary>
	public StatModifierDuration Duration { get; }

	/// <summary>Source of this modifier (item, buff, etc.).</summary>
	public string Source { get; }

	/// <summary>Remaining ticks for temporary modifiers.</summary>
	public Duration RemainingTicks { get; private set; }

	/// <summary>Condition required for this modifier to be active.</summary>
	public string? Condition { get; }

	/// <summary>Whether this modifier is currently active.</summary>
	public bool IsActive { get; set; } = true;

	/// <summary>Whether this modifier can stack.</summary>
	public bool Stackable { get; }

	/// <summary>Maximum stacks if stackable.</summary>
	public int MaxStacks { get; }

	/// <summary>Current stack count.</summary>
	public int CurrentStacks { get; private set; } = 1;

	public bool IsExpired => Duration == StatModifierDuration.Temporary && RemainingTicks <= 0;

	public StatModifier(
		string id,
		StatProto.ID statId,
		float value,
		ModifierOperation operation = ModifierOperation.FlatAdd,
		StatModifierDuration duration = StatModifierDuration.Permanent,
		string source = "",
		Duration? ticks = null,
		string? condition = null,
		bool stackable = false,
		int maxStacks = 1) {
		
		Id = id;
		StatId = statId;
		Value = value;
		Operation = operation;
		Duration = duration;
		Source = source;
		RemainingTicks = ticks ?? Core.Duration.Infinite;
		Condition = condition;
		Stackable = stackable;
		MaxStacks = maxStacks;
	}

	#region Factory Methods

	/// <summary>
	/// Creates a meta bonus modifier that persists across runs.
	/// Uses flat addition by default.
	/// </summary>
	public static StatModifier CreateMetaBonus(
		StatProto.ID statId,
		float value,
		string source) {
		return new StatModifier(
			id: $"meta_{source}_{statId.Value}",
			statId: statId,
			value: value,
			operation: ModifierOperation.FlatAdd,
			duration: StatModifierDuration.Meta,
			source: source
		);
	}

	/// <summary>
	/// Creates a meta bonus modifier with a specific operation.
	/// </summary>
	public static StatModifier CreateMetaBonus(
		StatProto.ID statId,
		float value,
		string source,
		ModifierOperation operation) {
		return new StatModifier(
			id: $"meta_{source}_{statId.Value}",
			statId: statId,
			value: value,
			operation: operation,
			duration: StatModifierDuration.Meta,
			source: source
		);
	}

	/// <summary>
	/// Creates a permanent modifier that lasts for the current run.
	/// </summary>
	public static StatModifier CreatePermanent(
		StatProto.ID statId,
		float value,
		string source,
		ModifierOperation operation = ModifierOperation.FlatAdd) {
		return new StatModifier(
			id: $"perm_{source}_{statId.Value}",
			statId: statId,
			value: value,
			operation: operation,
			duration: StatModifierDuration.Permanent,
			source: source
		);
	}

	/// <summary>
	/// Creates a temporary modifier that expires after a number of ticks.
	/// </summary>
	public static StatModifier CreateTemporary(
		StatProto.ID statId,
		float value,
		string source,
		Duration inTicks,
		ModifierOperation operation = ModifierOperation.FlatAdd) {
		return new StatModifier(
			id: $"temp_{source}_{statId.Value}_{Guid.NewGuid():N}",
			statId: statId,
			value: value,
			operation: operation,
			duration: StatModifierDuration.Temporary,
			source: source,
			ticks: inTicks
		);
	}

	/// <summary>
	/// Creates an equipment modifier that is removed when unequipped.
	/// </summary>
	public static StatModifier CreateEquipment(
		StatProto.ID statId,
		float value,
		string itemId,
		ModifierOperation operation = ModifierOperation.FlatAdd) {
		return new StatModifier(
			id: $"equip_{itemId}_{statId.Value}",
			statId: statId,
			value: value,
			operation: operation,
			duration: StatModifierDuration.Equipment,
			source: itemId
		);
	}

	#endregion

	#region Instance Methods

	/// <summary>Ticks down duration. Returns false if expired.</summary>
	public bool Tick() {
		if (Duration != StatModifierDuration.Temporary) return true;
		RemainingTicks -= Core.Duration.OneTick;
		return RemainingTicks > 0;
	}

	/// <summary>Adds a stack if possible.</summary>
	public bool AddStack() {
		if (!Stackable || CurrentStacks >= MaxStacks) return false;
		CurrentStacks++;
		return true;
	}

	/// <summary>Gets effective value considering stacks.</summary>
	public float GetEffectiveValue() => Value * CurrentStacks;

	/// <summary>Converts to ValueModifier for calculation.</summary>
	public ValueModifier ToValueModifier() {
		return ValueModifier.Create(Operation, GetEffectiveValue());
	}

	/// <summary>Checks if can stack with another modifier.</summary>
	public bool CanStackWith(StatModifier other) {
		return Stackable && StatId == other.StatId && Source == other.Source;
	}

	#endregion

	#region Serialization

	/// <summary>Converts to serializable data.</summary>
	public StatModifierData ToData() {
		return new StatModifierData {
			Id = Id,
			StatId = StatId.Value,
			Value = Value,
			Operation = Operation,
			Duration = Duration,
			Source = Source,
			RemainingTicks = RemainingTicks,
			Condition = Condition,
			Stackable = Stackable,
			MaxStacks = MaxStacks,
			CurrentStacks = CurrentStacks,
			IsActive = IsActive
		};
	}

	public static StatModifier FromData(StatModifierData data) {
		var modifier = new StatModifier(
				id: data.Id,
				statId: new StatProto.ID(data.StatId),
				value: data.Value,
				operation: data.Operation,
				duration: data.Duration,
				source: data.Source,
				ticks: data.RemainingTicks,
				condition: data.Condition,
				stackable: data.Stackable,
				maxStacks: data.MaxStacks
			);

		// Restore runtime state
		modifier.IsActive = data.IsActive;
		while (modifier.CurrentStacks < data.CurrentStacks) {
			modifier.AddStack();
		}

		return modifier;
	}

	#endregion
}

/// <summary>
/// Serializable stat modifier data.
/// </summary>
public class StatModifierData {
	public string Id { get; set; } = "";
	public string StatId { get; set; } = "";
	public float Value { get; set; }
	public ModifierOperation Operation { get; set; }
	public StatModifierDuration Duration { get; set; }
	public string Source { get; set; } = "";
	public Duration RemainingTicks { get; set; }
	public string? Condition { get; set; }
	public bool Stackable { get; set; }
	public int MaxStacks { get; set; }
	public int CurrentStacks { get; set; }
	public bool IsActive { get; set; }
}