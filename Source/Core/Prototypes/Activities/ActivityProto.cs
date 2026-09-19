#nullable disable
namespace RPGGame.Core.Prototypes.Activities;

public abstract class ActivityProto : Proto, IActivityProto
{
	// ActivityProto-specific ID struct
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {

		public readonly string Value = value;

		// activity-specific prefix added automatically

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs)
		{
			return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}
		
		public static bool operator ==(ID lhs, Proto.ID rhs)
		{
			return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}

		public static bool operator !=(Proto.ID lhs, ID rhs)
		{
			return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}

		public static bool operator !=(ID lhs, Proto.ID rhs)
		{
			return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}

		public static bool operator ==(ID lhs, ID rhs)
		{
			return string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}

		public static bool operator !=(ID lhs, ID rhs)
		{
			return !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		}

		public static implicit operator Proto.ID(ID itemId)
		{
			return new Proto.ID(itemId.Value);
		}
	}
	
	// Override the ID with activity-specific logic
	new public ID Id => new(base.Id.Value);

	public abstract Type ActivityType { get; }
	public double EnergyPerStep;
	public double TotalSteps;

	// Constructor for ActivityProto using the item-specific ID
	public ActivityProto(ID id, Loc text, double energyPerStep, double totalSteps) : base(id, text)	{
		EnergyPerStep = energyPerStep;
		TotalSteps = totalSteps;
	}	
}