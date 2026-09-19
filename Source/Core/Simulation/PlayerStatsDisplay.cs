namespace RPGGame.Core.Simulation;

public partial class PlayerStatsDisplay {
	public string Name { get; init; } = "";
	public string ClassName { get; init; } = "";
	public int Level { get; init; }
	public float CurrentHealth { get; init; }
	public float MaxHealth { get; init; }
	public float CurrentMana { get; init; }
	public float MaxMana { get; init; }
	public int Experience { get; init; }
	public int ExperienceToLevel { get; init; }
	public int Gold { get; init; }
	public int Morale { get; init; }
	public int Fatigue { get; init; }
	public int ArmorClass { get; init; }
	public int AttackBonus { get; init; }
	public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
	public float ManaPercent => MaxMana > 0 ? CurrentMana / MaxMana : 0;
	public float ExperiencePercent => ExperienceToLevel > 0 ? (float)Experience / ExperienceToLevel : 0;
}
