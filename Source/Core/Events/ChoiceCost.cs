namespace RPGGame.Core.Events;

/// <summary>
/// Cost to select a choice.
/// </summary>
public class ChoiceCost {
	public ChoiceCostType Type { get; init; }
	public string? ItemId { get; init; }
	public int Amount { get; init; }

	public static ChoiceCost Gold(int amount) => new() { Type = ChoiceCostType.Gold, Amount = amount };
	public static ChoiceCost Food(int amount) => new() { Type = ChoiceCostType.Food, Amount = amount };
	public static ChoiceCost Item(string itemId, int amount = 1) => new() { Type = ChoiceCostType.Item, ItemId = itemId, Amount = amount };
	public static ChoiceCost Health(int amount) => new() { Type = ChoiceCostType.Health, Amount = amount };
	public static ChoiceCost Mana(int amount) => new() { Type = ChoiceCostType.Mana, Amount = amount };
}