using RPGGame.Core.Characters;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Rewards;

/// <summary>
/// Manages item rewards, procedural generation, and the gacha/spinner reveal flow.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class RewardManager {
	#region Configuration Statics

	private static readonly int GEN_AMT_CLASS_ARMOR = 12;
	private static readonly int GEN_AMT_CLASS_WEAPON = 12;

	#endregion

	#region Fields

	private readonly GameDb k_gameDb;
	private readonly InventoryManager k_inventoryManager;
	private readonly ProceduralItemFactory k_itemFactory;

	#endregion

	#region Events

	public event Action<GeneratedItem, ItemInstance>? OnRewardClaimed;

	#endregion

	#region Constructor

	public RewardManager(GameDb gameDb, InventoryManager inventoryManager) {
		k_gameDb = gameDb;
		k_inventoryManager = inventoryManager;
		k_itemFactory = new ProceduralItemFactory(gameDb);

		Debug.Log("RewardManager initialized");
	}

	#endregion

	#region Class Weapon Generation

	public PendingReward GenerateClassWeaponReward(string classId, int tier) {
		var classProto = k_gameDb.Get<CharacterClassProto>(new CharacterClassProto.ID(classId));
		if (classProto == null) {
			Debug.LogError($"Class not found: {classId}");
			return CreateEmptyReward();
		}

		var preferredTypes = GetPreferredWeaponTypes(classProto);

		int minPoints = GetMinPointsForTier(tier);
		int maxPoints = GetMaxPointsForTier(tier);
		var maxRarity = GetMaxRarityForTier(tier);

		// Use deterministic random stream for rewards
		var rng = GameRandom.For("Rewards.Weapon", tier);

		var candidates = new List<GeneratedItem>();
		int winningIndex = rng.NextInt(GEN_AMT_CLASS_WEAPON);

		for (int i = 0; i < GEN_AMT_CLASS_WEAPON; i++) {
			int itemMinPoints = i == winningIndex ? minPoints + 5 : minPoints - 5;
			int itemMaxPoints = i == winningIndex ? maxPoints + 10 : maxPoints;
			var itemMaxRarity = i == winningIndex ? maxRarity : RarityType.Uncommon;

			var weaponType = rng.Pick(preferredTypes);
			var weapon = k_itemFactory.GenerateRandomWeapon(
				weaponType,
				Math.Max(5, itemMinPoints),
				itemMaxPoints,
				itemMaxRarity
			);

			if (weapon != null) {
				candidates.Add(weapon);
			} else {
				weapon = k_itemFactory.GenerateRandomWeapon(null, 5, maxPoints, itemMaxRarity);
				if (weapon != null) {
					candidates.Add(weapon);
				}
			}
		}

		if (candidates.Count == 0) {
			Debug.LogError("Failed to generate any weapons");
			return CreateEmptyReward();
		}

		winningIndex = Math.Min(winningIndex, candidates.Count - 1);

		return new PendingReward {
			Type = RewardType.Weapon,
			Candidates = candidates,
			WinningIndex = winningIndex,
			Tier = tier
		};
	}

	public PendingReward GenerateClassArmorReward(string classId, int tier) {
		var classProto = k_gameDb.Get<CharacterClassProto>(new CharacterClassProto.ID(classId));
		if (classProto == null) {
			Debug.LogError($"Class not found: {classId}");
			return CreateEmptyReward();
		}

		var preferredTypes = GetPreferredArmorTypes(classProto);
		var preferredSlot = SlotType.Chest;

		int minPoints = GetMinPointsForTier(tier);
		int maxPoints = GetMaxPointsForTier(tier);
		var maxRarity = GetMaxRarityForTier(tier);

		// Use deterministic random stream for rewards
		var rng = GameRandom.For("Rewards.Armor", tier);

		var candidates = new List<GeneratedItem>();
		int winningIndex = rng.NextInt(GEN_AMT_CLASS_ARMOR);

		for (int i = 0; i < GEN_AMT_CLASS_ARMOR; i++) {
			int itemMinPoints = i == winningIndex ? minPoints + 5 : minPoints - 5;
			int itemMaxPoints = i == winningIndex ? maxPoints + 10 : maxPoints;
			var itemMaxRarity = i == winningIndex ? maxRarity : RarityType.Uncommon;

			var armorType = rng.Pick(preferredTypes);
			var armor = k_itemFactory.GenerateRandomArmor(
				armorType,
				preferredSlot,
				Math.Max(5, itemMinPoints),
				itemMaxPoints,
				itemMaxRarity
			);

			if (armor != null) {
				candidates.Add(armor);
			} else {
				armor = k_itemFactory.GenerateRandomArmor(null, null, 5, maxPoints, itemMaxRarity);
				if (armor != null) {
					candidates.Add(armor);
				}
			}
		}

		if (candidates.Count == 0) {
			Debug.LogError("Failed to generate any armor");
			return CreateEmptyReward();
		}

		winningIndex = Math.Min(winningIndex, candidates.Count - 1);

		return new PendingReward {
			Type = RewardType.Armor,
			Candidates = candidates,
			WinningIndex = winningIndex,
			Tier = tier
		};
	}

	#endregion

	#region Reward Claiming

	/// <summary>
	/// Claims a pending reward and adds it to inventory.
	/// Returns the created item instance.
	/// </summary>
	public ItemInstance? ClaimReward(PendingReward reward) {
		if (reward.WinningIndex < 0 || reward.WinningIndex >= reward.Candidates.Count) {
			Debug.LogError("Invalid winning index");
			return null;
		}

		var generatedItem = reward.Candidates[reward.WinningIndex];

		// Convert GeneratedItem to ItemInstance
		var instance = ConvertToItemInstance(generatedItem);
		if (instance == null) {
			Debug.LogError("Failed to convert generated item to instance");
			return null;
		}

		// Add to inventory
		if (k_inventoryManager.AddItem(instance)) {
			Debug.Log($"Reward claimed: {instance.DisplayName}");
			OnRewardClaimed?.Invoke(generatedItem, instance);
			return instance;
		}

		Debug.LogWarning("Failed to add reward to inventory (full?)");
		return null;
	}

	/// <summary>
	/// Claims and auto-equips a reward.
	/// </summary>
	public ItemInstance? ClaimAndEquipReward(PendingReward reward) {
		var instance = ClaimReward(reward);
		if (instance != null && instance.Prototype.IsEquippable) {
			k_inventoryManager.EquipItem(instance);
		}
		return instance;
	}

	#endregion

	#region Helper Methods

	private List<WeaponType> GetPreferredWeaponTypes(CharacterClassProto classProto) {
		// Map class roles to preferred weapon types
		return classProto.Role switch {
			ClassRole.Melee => [WeaponType.Sword, WeaponType.Axe, WeaponType.Dagger, WeaponType.Mace],
			ClassRole.Ranged => [WeaponType.Bow, WeaponType.Crossbow],
			ClassRole.Caster => [WeaponType.Staff],
			ClassRole.Tank => [WeaponType.Sword, WeaponType.Mace, WeaponType.Shield],
			ClassRole.Support => [WeaponType.Staff, WeaponType.Mace],
			ClassRole.Hybrid => [WeaponType.Sword, WeaponType.Bow, WeaponType.Staff],
			_ => [WeaponType.Sword, WeaponType.Axe, WeaponType.Bow]
		};
	}

	private List<ArmorType> GetPreferredArmorTypes(CharacterClassProto classProto) {
		return classProto.Role switch {
			ClassRole.Melee => [ArmorType.Light, ArmorType.Medium],
			ClassRole.Ranged => [ArmorType.Light],
			ClassRole.Caster => [ArmorType.Cloth],
			ClassRole.Tank => [ArmorType.Heavy, ArmorType.Medium],
			ClassRole.Support => [ArmorType.Light, ArmorType.Cloth],
			ClassRole.Hybrid => [ArmorType.Medium, ArmorType.Light],
			_ => [ArmorType.Light, ArmorType.Medium]
		};
	}

	private int GetMinPointsForTier(int tier) {
		return tier switch {
			1 => 5,
			2 => 15,
			3 => 30,
			4 => 50,
			5 => 75,
			_ => 10
		};
	}

	private int GetMaxPointsForTier(int tier) {
		return tier switch {
			1 => 15,
			2 => 35,
			3 => 60,
			4 => 90,
			5 => 120,
			_ => 25
		};
	}

	private RarityType GetMaxRarityForTier(int tier) {
		return tier switch {
			1 => RarityType.Uncommon,
			2 => RarityType.Rare,
			3 => RarityType.Epic,
			4 => RarityType.Legendary,
			5 => RarityType.Mythic,
			_ => RarityType.Rare
		};
	}

	private ItemInstance? ConvertToItemInstance(GeneratedItem generatedItem) {
		// Get the base proto
		var protoId = new Prototypes.Proto.ID(generatedItem.BaseProtoId);

		if (!k_gameDb.TryGetProto<Prototypes.Item.ItemProto>(protoId, out var baseProto)) {
			Debug.LogError($"Base proto not found: {generatedItem.BaseProtoId}");
			return null;
		}

		// Create instance with generated stats
		var instance = new ItemInstance(baseProto, 1) {
			GeneratedData = generatedItem
		};

		return instance;
	}

	private PendingReward CreateEmptyReward() {
		return new PendingReward {
			Type = RewardType.None,
			Candidates = [],
			WinningIndex = -1,
			Tier = 0
		};
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// A pending reward ready to be shown and claimed.
/// </summary>
public class PendingReward {
	public RewardType Type { get; init; }
	public List<GeneratedItem> Candidates { get; init; } = [];
	public int WinningIndex { get; init; }
	public int Tier { get; init; }

	public bool IsValid => Candidates.Count > 0 && WinningIndex >= 0;
	public GeneratedItem? WinningItem => IsValid ? Candidates[WinningIndex] : null;
}

/// <summary>
/// Types of rewards.
/// </summary>
public enum RewardType {
	None,
	Weapon,
	Armor,
	Accessory,
	Consumable,
	Gold,
	Spell
}

#endregion