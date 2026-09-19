namespace RPGGame.Core.Prototypes.Item.Resource;

public partial class ResourceDrop {
	public ItemProto.ID ItemId { get; init; }
	public float DropChance { get; init; } = 1f;
	public int MinCount { get; init; } = 1;
	public int MaxCount { get; init; } = 1;
	public ResourceDrop(ItemProto.ID itemId, float dropChance = 1f, int minCount = 1, int maxCount = 1) {
		ItemId = itemId;
		DropChance = dropChance;
		MinCount = minCount;
		MaxCount = maxCount;
	}
}
