namespace RPGGame.Core.Activities;
#nullable disable

public readonly struct ActivityId(int value) : IEquatable<ActivityId>, IComparable<ActivityId> {
	public readonly int Value = value;

	[Dependency(RegistrationType.Singleton)]
	public class Factory : GameIdFactoryBase<ActivityId>
	{
		protected override ActivityId GenerateNewId(ActivityId previousId)
		{
			UnityEngine.Debug.Log($"Generating new activity with ID {previousId.Value+1}");
			return new ActivityId(previousId.Value + 1);  // Increment for next ActivityId
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is ActivityId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode() => Value;

	public bool Equals(ActivityId other) => Value == other.Value;
	public int CompareTo(ActivityId other) => Value.CompareTo(other.Value);

	public static bool operator ==(ActivityId lhs, ActivityId rhs) => lhs.Value == rhs.Value;
	public static bool operator !=(ActivityId lhs, ActivityId rhs) => lhs.Value != rhs.Value;
}
