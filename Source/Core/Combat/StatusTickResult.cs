namespace RPGGame.Core.Combat;

public partial class StatusTickResult {
	public float DamageFromEffects { get; set; }
	public float HealingFromEffects { get; set; }
	public List<StatusCondition> ExpiredConditions { get; } = [];
}
