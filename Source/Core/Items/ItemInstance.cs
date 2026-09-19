using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Items;

/// <summary>
/// An instance of an item in a player's inventory.
/// </summary>
public class ItemInstance {
	#region Properties

	/// <summary>
	/// Unique instance ID.
	/// </summary>
	public string InstanceId { get; }

	/// <summary>
	/// Reference to the item proto.
	/// </summary>
	public ItemProto Prototype { get; }

	/// <summary>
	/// Current stack count.
	/// </summary>
	public int Count { get; set; } = 1;

	/// <summary>
	/// Current durability (if applicable).
	/// </summary>
	public int? Durability { get; set; }

	/// <summary>
	/// Maximum durability.
	/// </summary>
	public int? MaxDurability { get; set; }

	/// <summary>
	/// Remaining uses (for consumables).
	/// </summary>
	public int? RemainingUses { get; set; }

	/// <summary>
	/// Whether this item is equipped.
	/// </summary>
	public bool IsEquipped { get; set; }

	/// <summary>
	/// Custom name (if renamed).
	/// </summary>
	public string? CustomName { get; set; }

	/// <summary>
	/// Display name.
	/// </summary>
	public string DisplayName => CustomName ?? Prototype.DisplayText.Name;

	/// <summary>
	/// Additional enchantments/modifiers.
	/// </summary>
	public List<ItemEnchantment> Enchantments { get; } = [];

	/// <summary>
	/// Generated item data for procedurally created items.
	/// Contains bonus stats, enchantments, etc. from procedural generation.
	/// </summary>
	public Generation.GeneratedItem? GeneratedData { get; set; }

	/// <summary>
	/// Whether this item is marked as favorite.
	/// </summary>
	public bool IsFavorite { get; set; }

	/// <summary>
	/// Whether this item is locked (cannot sell/drop).
	/// </summary>
	public bool IsLocked { get; set; }

	#endregion

	#region Computed Properties

	/// <summary>
	/// Total sell value.
	/// </summary>
	public int TotalSellValue => Prototype.SellPrice * Count;

	/// <summary>
	/// Durability percentage (0-1).
	/// </summary>
	public float DurabilityPercent =>
		Durability.HasValue && MaxDurability.HasValue && MaxDurability.Value > 0
			? (float)Durability.Value / MaxDurability.Value
			: 1f;

	/// <summary>
	/// Whether durability is low.
	/// </summary>
	public bool IsDurabilityLow => DurabilityPercent < 0.25f;

	/// <summary>
	/// Whether this item is broken.
	/// </summary>
	public bool IsBroken => Durability.HasValue && Durability.Value <= 0;

	/// <summary>
	/// Effective item level (with enchantments).
	/// </summary>
	public int EffectiveItemLevel => Prototype.ItemLevel + Enchantments.Sum(e => e.LevelBonus);

	#endregion

	#region Constructor

	public ItemInstance(ItemProto proto, int count = 1) {
		InstanceId = Guid.NewGuid().ToString();
		Prototype = proto;
		Count = Math.Clamp(count, 1, proto.MaxStackSize);

		// Initialize durability for equipment
		if (proto.IsEquippable) {
			MaxDurability = 100;
			Durability = 100;
		}

		// Initialize uses for consumables
		if (proto.MaxUses > 0) {
			RemainingUses = proto.MaxUses;
		}
	}

	/// <summary>
	/// Creates from save data.
	/// </summary>
	public ItemInstance(ItemProto proto, ItemInstanceData saveData) {
		InstanceId = saveData.InstanceId;
		Prototype = proto;
		Count = saveData.Count;
		Durability = saveData.Durability;
		MaxDurability = saveData.MaxDurability;
		RemainingUses = saveData.RemainingUses;
		IsEquipped = saveData.IsEquipped;
		CustomName = saveData.CustomName;
		IsFavorite = saveData.IsFavorite;
		IsLocked = saveData.IsLocked;

		foreach (var enchData in saveData.Enchantments) {
			Enchantments.Add(new ItemEnchantment {
				Id = enchData.Id,
				Name = enchData.Name,
				LevelBonus = enchData.LevelBonus
			});
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Attempts to add to stack. Returns amount that couldn't be added.
	/// </summary>
	public int TryAddToStack(int amount) {
		if (!Prototype.IsStackable) {
			return amount;
		}

		int canAdd = Math.Min(amount, Prototype.MaxStackSize - Count);
		Count += canAdd;
		return amount - canAdd;
	}

	/// <summary>
	/// Removes from stack. Returns amount actually removed.
	/// </summary>
	public int RemoveFromStack(int amount) {
		int toRemove = Math.Min(amount, Count);
		Count -= toRemove;
		return toRemove;
	}

	/// <summary>
	/// Uses the item. Returns true if use succeeded.
	/// </summary>
	public bool Use() {
		if (!Prototype.IsUsable) {
			return false;
		}

		if (RemainingUses.HasValue) {
			if (RemainingUses.Value <= 0) {
				return false;
			}
			RemainingUses--;
		}

		return true;
	}

	/// <summary>
	/// Reduces durability.
	/// </summary>
	public void ReduceDurability(int amount) {
		if (!Durability.HasValue) {
			return;
		}
		Durability = Math.Max(0, Durability.Value - amount);
	}

	/// <summary>
	/// Repairs durability.
	/// </summary>
	public void Repair(int amount) {
		if (!Durability.HasValue || !MaxDurability.HasValue) {
			return;
		}
		Durability = Math.Min(MaxDurability.Value, Durability.Value + amount);
	}

	/// <summary>
	/// Fully repairs durability.
	/// </summary>
	public void FullRepair() {
		if (MaxDurability.HasValue) {
			Durability = MaxDurability.Value;
		}
	}

	/// <summary>
	/// Gets all stat modifiers from this item.
	/// </summary>
	public IEnumerable<StatModifier> GetStatModifiers() {
		// Base equipment stats (only stats)
		foreach (var equipStat in Prototype.EquipStats) {
			if (!equipStat.Target.Value.StartsWith("Stat_")) {
				continue;
			}

			var statId = new StatProto.ID(equipStat.Target.Value);
			yield return StatModifier.CreateEquipment(
				statId,
				equipStat.Value,
				InstanceId,
				equipStat.Operation
			);
		}

		// Enchantment stats
		foreach (var enchant in Enchantments) {
			foreach (var equipStat in enchant.Stats) {
				if (!equipStat.Target.Value.StartsWith("Stat_")) {
					continue;
				}

				var statId = new StatProto.ID(equipStat.Target.Value);
				yield return StatModifier.CreateEquipment(
					statId,
					equipStat.Value,
					$"{InstanceId}_{enchant.Id}",
					equipStat.Operation
				);
			}
		}

		// Durability penalty
		if (IsDurabilityLow && !IsBroken) {
			yield return StatModifier.CreateEquipment(
				Ids.Stats.Combat.DamBonus,
				2,
				$"{InstanceId}_durability",
				ModifierOperation.FlatSubtract
			);
		}
	}

	/// <summary>
	/// Gets all equipment stats that modify skills.
	/// </summary>
	public IEnumerable<EquipmentStat> GetSkillModifiers() {
		// Base equipment skills
		foreach (var equipStat in Prototype.EquipStats) {
			if (equipStat.Target.Value.StartsWith("Skill_")) {
				yield return equipStat;
			}
		}

		// Enchantment skills
		foreach (var enchant in Enchantments) {
			foreach (var equipStat in enchant.Stats) {
				if (equipStat.Target.Value.StartsWith("Skill_")) {
					yield return equipStat;
				}
			}
		}
	}

	/// <summary>
	/// Gets all equipment stats that modify spells.
	/// </summary>
	public IEnumerable<EquipmentStat> GetSpellModifiers() {
		// Base equipment spells
		foreach (var equipStat in Prototype.EquipStats) {
			if (equipStat.Target.Value.StartsWith("Spell_")) {
				yield return equipStat;
			}
		}

		// Enchantment spells
		foreach (var enchant in Enchantments) {
			foreach (var equipStat in enchant.Stats) {
				if (equipStat.Target.Value.StartsWith("Spell_")) {
					yield return equipStat;
				}
			}
		}
	}

	/// <summary>
	/// Gets all equipment modifiers (stats, skills, spells) as raw EquipmentStat objects.
	/// Useful when you need to process all modifier types uniformly.
	/// </summary>
	public IEnumerable<EquipmentStat> GetAllEquipmentModifiers() {
		// Base equipment modifiers
		foreach (var equipStat in Prototype.EquipStats) {
			yield return equipStat;
		}

		// Enchantment modifiers
		foreach (var enchant in Enchantments) {
			foreach (var equipStat in enchant.Stats) {
				yield return equipStat;
			}
		}
	}

	/// <summary>
	/// Gets the total bonus this item provides to a specific target (stat, skill, or spell).
	/// </summary>
	public float GetBonusTo(Proto.ID target) {
		float total = 0f;

		foreach (var equipStat in GetAllEquipmentModifiers()) {
			if (equipStat.Target == target) {
				// For simplicity, just sum flat values
				// A more complete implementation would use ModifierCalculator
				total += equipStat.Operation switch {
					ModifierOperation.FlatAdd => equipStat.Value,
					ModifierOperation.FlatSubtract => -equipStat.Value,
					_ => 0 // Percentage modifiers need base value context
				};
			}
		}

		return total;
	}

	/// <summary>
	/// Checks if item can be equipped by a class.
	/// </summary>
	public bool CanEquip(string classId) {
		if (!Prototype.IsEquippable) {
			return false;
		}
		if (IsBroken) {
			return false;
		}

		if (Prototype.ClassRestrictions.Count > 0 &&
			!Prototype.ClassRestrictions.Any(r => r.Value == classId)) {
			return false;
		}

		return true;
	}

	#endregion

	#region Serialization

	public ItemInstanceData ToData() {
		return new ItemInstanceData {
			InstanceId = InstanceId,
			ItemId = Prototype.Id.Value,
			Count = Count,
			Durability = Durability,
			MaxDurability = MaxDurability,
			RemainingUses = RemainingUses,
			IsEquipped = IsEquipped,
			CustomName = CustomName,
			IsFavorite = IsFavorite,
			IsLocked = IsLocked,
			Enchantments = Enchantments.Select(e => e.ToData()).ToList()
		};
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// An enchantment on an item.
/// </summary>
public class ItemEnchantment {
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public int LevelBonus { get; set; }
	public List<EquipmentStat> Stats { get; set; } = [];

	public EnchantmentData ToData() => new() {
		Id = Id,
		Name = Name,
		LevelBonus = LevelBonus
	};
}

#endregion

#region Serialization Data

public class ItemInstanceData {
	public string InstanceId { get; set; } = "";
	public string ItemId { get; set; } = "";
	public int Count { get; set; }
	public int? Durability { get; set; }
	public int? MaxDurability { get; set; }
	public int? RemainingUses { get; set; }
	public bool IsEquipped { get; set; }
	public string? CustomName { get; set; }
	public bool IsFavorite { get; set; }
	public bool IsLocked { get; set; }
	public List<EnchantmentData> Enchantments { get; set; } = [];
}

public class EnchantmentData {
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public int LevelBonus { get; set; }
}

#endregion