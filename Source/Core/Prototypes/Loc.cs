// File: RPGGame/Core/Prototypes/Loc.cs
namespace RPGGame.Core.Prototypes;

using System;

/// <summary>
/// Represents display name + description that will eventually be localized.
/// This is a fundamental value type — used by items, skills, NPCs, quests, UI, etc.
/// </summary>
public readonly struct Loc : IEquatable<Loc>
{
	public string Name { get; }
	public string Description { get; }

	public Loc(string name, string description = "")
	{
		Name = name ?? string.Empty;
		Description = description ?? string.Empty;
	}

	public static Loc Empty => default;

	public bool IsEmpty => Name.Length == 0 && Description.Length == 0;

	public bool Equals(Loc other) =>
		string.Equals(Name, other.Name, StringComparison.Ordinal) &&
		string.Equals(Description, other.Description, StringComparison.Ordinal);

	public override bool Equals(object? obj) => obj is Loc other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(Name, Description);
	public override string ToString() => string.IsNullOrEmpty(Name) ? "<unnamed>" : Name;

	public static bool operator ==(Loc left, Loc right) => left.Equals(right);
	public static bool operator !=(Loc left, Loc right) => !left.Equals(right);

	// Nice helper for fluent creation
	public static Loc Of(string name, string description = "") => new(name, description);
}
