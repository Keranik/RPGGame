namespace RPGGame.Core.Prototypes;

/// <summary>
/// Brings back your beloved Proto.CreateText("Name", "Desc") syntax — but now even better!
/// Put this in the same file or a separate LocExtensions.cs
/// </summary>
public static class LocExtensions
{
	public static Loc CreateText(this Proto _, string name, string description = "")
		=> new Loc(name, description);

	public static Loc Text(this string name, string description = "")
		=> new Loc(name, description);

	public static Loc Name(this string name) => new Loc(name);
}