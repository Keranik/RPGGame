using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Balanced generation strategy - even distribution across all areas.
/// </summary>
public class BalancedStrategy : BaseGenerationStrategy {
	public BalancedStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

	public override CharacterUpgrade? SelectUpgrade(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	) {
		var affordable = GetAffordableUpgrades(state, classProto, manager);
		if (affordable.Count == 0) return null;

		// Weight everything equally
		var weighted = affordable.Select(u => (u, 1f)).ToList();
		return SelectWeighted(weighted);
	}
}