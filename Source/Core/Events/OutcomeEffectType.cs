namespace RPGGame.Core.Events;

public enum OutcomeEffectType {
	// Resources
	GainGold,
	LoseGold,
	GainFood,
	LoseFood,
	GainItem,
	LoseItem,

	// Character
	Heal,
	Damage,
	RestoreMana,
	DrainMana,
	GainExperience,
	GainMorale,
	LoseMorale,
	AddFatigue,

	// Buffs
	ApplyBuff,
	RemoveBuff,
	ApplyDebuff,

	// Game State
	SetFlag,
	ClearFlag,
	UnlockLore,
	StartCombat,
	TriggerEvent,
	AdvanceTime,
	Teleport,

	// Meta
	UnlockClass,
	UnlockBuilding,
	GainUpgradePoints,

	/// <summary>Grants class-appropriate weapon.</summary>
	GainClassWeapon,

	/// <summary>Grants class-appropriate armor.</summary>
	GainClassArmor,

	/// <summary>Grants class-appropriate spell.</summary>
	GainClassSpell,

	/// <summary>Adds a run modifier.</summary>
	AddRunModifier
}
