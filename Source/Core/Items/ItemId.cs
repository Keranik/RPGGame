namespace RPGGame.Core.Items;
#nullable disable

public readonly struct ItemId(int value) : IEquatable<ItemId>, IComparable<ItemId> {
	public readonly int Value = value;

	[Dependency(RegistrationType.Singleton)]
	public class Factory : GameIdFactoryBase<ItemId>
	{
		protected override ItemId GenerateNewId(ItemId previousId)
		{
			return new ItemId(previousId.Value + 1);  // Increment for next ItemId
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is ItemId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode() => Value;

	public bool Equals(ItemId other) => Value == other.Value;
	public int CompareTo(ItemId other) => Value.CompareTo(other.Value);

	public static bool operator ==(ItemId lhs, ItemId rhs) => lhs.Value == rhs.Value;
	public static bool operator !=(ItemId lhs, ItemId rhs) => lhs.Value != rhs.Value;
}