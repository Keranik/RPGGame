using RPGGame.Core.Items;

namespace RPGGame.Core.Generation;

/// <summary>
/// Manages creation point budgets for procedural generation.
/// Rarity adds bonus points on top of the base budget.
/// </summary>
public class CreationPointBudget {
	#region Constants

	/// <summary>Bonus creation points per rarity tier.</summary>
	public static readonly Dictionary<RarityType, int> RarityBonusPoints = new() {
		{ RarityType.Common, 0 },
		{ RarityType.Uncommon, 15 },
		{ RarityType.Rare, 35 },
		{ RarityType.Epic, 60 },
		{ RarityType.Legendary, 100 },
		{ RarityType.Mythic, 150 }
	};

	/// <summary>Rarity roll weights (higher = more common).</summary>
	public static readonly Dictionary<RarityType, float> RarityWeights = new() {
		{ RarityType.Common, 100f },
		{ RarityType.Uncommon, 40f },
		{ RarityType.Rare, 15f },
		{ RarityType.Epic, 5f },
		{ RarityType.Legendary, 1f },
		{ RarityType.Mythic, 0.1f }
	};

	/// <summary>Shared random instance for convenience.</summary>
	private static readonly Random SharedRandom = new();

	#endregion

	#region Properties

	/// <summary>Base creation points before rarity bonus.</summary>
	public int BasePoints { get; }

	/// <summary>Rolled rarity for this generation.</summary>
	public RarityType Rarity { get; }

	/// <summary>Bonus points from rarity.</summary>
	public int RarityBonus => RarityBonusPoints[Rarity];

	/// <summary>Total available creation points.</summary>
	public int TotalPoints => BasePoints + RarityBonus;

	/// <summary>Points remaining to spend.</summary>
	public int RemainingPoints { get; private set; }

	/// <summary>Points already spent.</summary>
	public int SpentPoints => TotalPoints - RemainingPoints;

	#endregion

	#region Constructor

	public CreationPointBudget(int basePoints, RarityType rarity) {
		BasePoints = basePoints;
		Rarity = rarity;
		RemainingPoints = TotalPoints;
	}

	#endregion

	#region Methods

	/// <summary>Attempts to spend points. Returns true if successful.</summary>
	public bool TrySpend(int points) {
		if (points > RemainingPoints) {
			return false;
		}
		RemainingPoints -= points;
		return true;
	}

	/// <summary>Spends points, allowing overspend (for mandatory costs).</summary>
	public void Spend(int points) {
		RemainingPoints -= points;
	}

	/// <summary>Checks if we can afford a cost.</summary>
	public bool CanAfford(int cost) => RemainingPoints >= cost;

	/// <summary>Returns remaining points without spending.</summary>
	public int Peek() => RemainingPoints;

	#endregion

	#region Static Factory Methods

	/// <summary>
	/// Creates a budget with random base points in range and rolled rarity.
	/// </summary>
	public static CreationPointBudget Create(
		int minPoints,
		int maxPoints,
		RarityType maxRarity,
		Random? random = null) {
		random ??= SharedRandom;

		int basePoints = random.Next(minPoints, maxPoints + 1);
		RarityType rarity = RollRarity(maxRarity, random);

		return new CreationPointBudget(basePoints, rarity);
	}

	/// <summary>
	/// Rolls for rarity up to the specified maximum.
	/// </summary>
	public static RarityType RollRarity(RarityType maxRarity, Random? random = null) {
		random ??= SharedRandom;

		// Build weighted list up to max rarity
		var validRarities = new List<(RarityType rarity, float weight)>();
		float totalWeight = 0;

		// Use non-generic Enum.GetValues for .NET Standard 2.1 compatibility
		foreach (RarityType rarity in Enum.GetValues(typeof(RarityType))) {
			if (rarity <= maxRarity) {
				float weight = RarityWeights[rarity];
				validRarities.Add((rarity, weight));
				totalWeight += weight;
			}
		}

		// Roll
		float roll = (float)(random.NextDouble() * totalWeight);
		float cumulative = 0;

		foreach (var (rarity, weight) in validRarities) {
			cumulative += weight;
			if (roll <= cumulative) {
				return rarity;
			}
		}

		return RarityType.Common;
	}

	#endregion
}