using RPGGame.Core.Prototypes.Combat;

namespace RPGGame.Core.Combat;

/// <summary>
/// Runtime combat ability data.
/// </summary>
public class CombatAbility {
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public CombatAbilityType Type { get; set; } = CombatAbilityType.Attack;
	public TargetType TargetType { get; set; } = TargetType.SingleEnemy;
	public int ManaCost { get; set; }
	public int Cooldown { get; set; }
	public int CurrentCooldown { get; set; }
	public HitDice DamageDice { get; set; } = HitDice.None;
	public DamageType DamageType { get; set; } = DamageType.Physical;
	public List<AbilityEffect> Effects { get; set; } = [];
}
