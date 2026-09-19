namespace RPGGame.Core.Prototypes;

using System;

public abstract class Proto : IProto, IComparable<Proto>, IEquatable<Proto>
{
	public static Loc CreateText(string name, string description = "")
		=> new Loc(name, description);

    // Strongly-typed prototype identifier
    public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID>
    {
        public readonly string Value = value ?? string.Empty;

        public bool Equals(ID other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public int CompareTo(ID other) => string.CompareOrdinal(Value, other.Value);
        public override bool Equals(object? obj) => obj is ID id && Equals(id);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;

        public static bool operator ==(ID left, ID right) => left.Equals(right);
        public static bool operator !=(ID left, ID right) => !left.Equals(right);
    }

    public ID Id { get; }
    
    // This is the correct, self-documenting name
    public Loc DisplayText { get; }

    public static bool IdIsValid(ID id) => !string.IsNullOrWhiteSpace(id.Value);

    protected Proto(ID id, Loc display)
    {
        if (!IdIsValid(id))
            throw new ArgumentException("ID for Proto cannot be null or whitespace.", nameof(id));

        Id = id;
        DisplayText = display; // can be Loc.Empty if you want, or require non-empty in derived ctors
    }

    // Optional: allow derived classes to do extra init when loaded into GameDb
    protected virtual void OnInitialize(GameDb gameDb) { }
    public virtual void OnInitialize() { }

    // Equality & comparison based solely on ID (standard for data-driven prototypes)
    public bool Equals(Proto? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public int CompareTo(Proto? other) => other is null ? 1 : Id.CompareTo(other.Id);

    public override bool Equals(object? obj) => Equals(obj as Proto);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{Id} ({GetType().Name})";
}