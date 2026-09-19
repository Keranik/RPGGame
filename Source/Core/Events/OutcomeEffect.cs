using RPGGame.Core.Expedition;

namespace RPGGame.Core.Events;

public partial class OutcomeEffect {
	public OutcomeEffectType Type { get; init; }
	public string Parameter { get; init; } = "";
	public int Value { get; init; }
	public float FloatValue { get; init; }
	public bool ShowInUI { get; init; } = true;
	public string? DisplayText { get; init; }

	public static OutcomeEffect GainGold(int amount) => new() { Type = OutcomeEffectType.GainGold, Value = amount };
	public static OutcomeEffect LoseGold(int amount) => new() { Type = OutcomeEffectType.LoseGold, Value = amount };
	public static OutcomeEffect GainFood(int amount) => new() { Type = OutcomeEffectType.GainFood, Value = amount };
	public static OutcomeEffect LoseFood(int amount) => new() { Type = OutcomeEffectType.LoseFood, Value = amount };
	public static OutcomeEffect GainItem(string itemId, int count = 1) => new()
		{ Type = OutcomeEffectType.GainItem, Parameter = itemId, Value = count };
	public static OutcomeEffect LoseItem(string itemId, int count = 1) => new()
		{ Type = OutcomeEffectType.LoseItem, Parameter = itemId, Value = count };
	public static OutcomeEffect Heal(int amount) => new() { Type = OutcomeEffectType.Heal, Value = amount };
	public static OutcomeEffect Damage(int amount) => new() { Type = OutcomeEffectType.Damage, Value = amount };
	public static OutcomeEffect RestoreMana(int amount)
		=> new() { Type = OutcomeEffectType.RestoreMana, Value = amount };
	public static OutcomeEffect DrainMana(int amount) => new() { Type = OutcomeEffectType.DrainMana, Value = amount };
	public static OutcomeEffect GainExperience(int amount)
		=> new() { Type = OutcomeEffectType.GainExperience, Value = amount };
	public static OutcomeEffect GainMorale(int amount) => new() { Type = OutcomeEffectType.GainMorale, Value = amount };
	public static OutcomeEffect LoseMorale(int amount) => new() { Type = OutcomeEffectType.LoseMorale, Value = amount };
	public static OutcomeEffect AddFatigue(int amount) => new() { Type = OutcomeEffectType.AddFatigue, Value = amount };
	public static OutcomeEffect ApplyBuff(string buffId, int duration) => new()
		{ Type = OutcomeEffectType.ApplyBuff, Parameter = buffId, Value = duration };
	public static OutcomeEffect RemoveBuff(string buffId)
		=> new() { Type = OutcomeEffectType.RemoveBuff, Parameter = buffId };
	public static OutcomeEffect ApplyDebuff(string debuffId, int duration) => new()
		{ Type = OutcomeEffectType.ApplyDebuff, Parameter = debuffId, Value = duration };
	public static OutcomeEffect SetFlag(string flagId) => new()
		{ Type = OutcomeEffectType.SetFlag, Parameter = flagId, ShowInUI = false };
	public static OutcomeEffect ClearFlag(string flagId) => new()
		{ Type = OutcomeEffectType.ClearFlag, Parameter = flagId, ShowInUI = false };
	public static OutcomeEffect UnlockLore(string loreId)
		=> new() { Type = OutcomeEffectType.UnlockLore, Parameter = loreId };
	public static OutcomeEffect StartCombat(string encounterId) => new()
		{ Type = OutcomeEffectType.StartCombat, Parameter = encounterId, ShowInUI = false };
	public static OutcomeEffect TriggerEvent(string eventId) => new()
		{ Type = OutcomeEffectType.TriggerEvent, Parameter = eventId, ShowInUI = false };
	public static OutcomeEffect AdvanceTime(int hours) => new() { Type = OutcomeEffectType.AdvanceTime, Value = hours };
	public static OutcomeEffect Teleport(string locationId) => new()
		{ Type = OutcomeEffectType.Teleport, Parameter = locationId, ShowInUI = false };
	public static OutcomeEffect UnlockClass(string classId)
		=> new() { Type = OutcomeEffectType.UnlockClass, Parameter = classId };
	public static OutcomeEffect UnlockBuilding(string buildingId)
		=> new() { Type = OutcomeEffectType.UnlockBuilding, Parameter = buildingId };
	public static OutcomeEffect GainUpgradePoints(int amount)
		=> new() { Type = OutcomeEffectType.GainUpgradePoints, Value = amount };

	/// <summary>Grants a class-appropriate weapon of the specified tier.</summary>
	public static OutcomeEffect GainClassWeapon(int tier) => new() {
		Type = OutcomeEffectType.GainClassWeapon,
		Value = tier
	};

	/// <summary>Grants a class-appropriate armor of the specified tier.</summary>
	public static OutcomeEffect GainClassArmor(int tier) => new() {
		Type = OutcomeEffectType.GainClassArmor,
		Value = tier
	};

	/// <summary>Grants a class-appropriate spell of the specified tier.</summary>
	public static OutcomeEffect GainClassSpell(int tier) => new() {
		Type = OutcomeEffectType.GainClassSpell,
		Value = tier
	};

	/// <summary>Adds a run modifier (bonus or penalty).</summary>
	public static OutcomeEffect AddRunModifier(RunModifierType type, float value) => new() {
		Type = OutcomeEffectType.AddRunModifier,
		Parameter = type.ToString(),
		FloatValue = value
	};

	/// <summary>
	/// Gets display text for this effect.
	/// </summary>
	public string GetDisplayText() {
		if (!string.IsNullOrEmpty(DisplayText)) {
			return DisplayText;
		}

		return Type switch {
			OutcomeEffectType.GainGold => $"+{Value} gold",
			OutcomeEffectType.LoseGold => $"-{Value} gold",
			OutcomeEffectType.GainFood => $"+{Value} food",
			OutcomeEffectType.LoseFood => $"-{Value} food",
			OutcomeEffectType.GainItem => $"+{Value} {Parameter}",
			OutcomeEffectType.LoseItem => $"-{Value} {Parameter}",
			OutcomeEffectType.Heal => $"+{Value} HP",
			OutcomeEffectType.Damage => $"-{Value} HP",
			OutcomeEffectType.RestoreMana => $"+{Value} MP",
			OutcomeEffectType.DrainMana => $"-{Value} MP",
			OutcomeEffectType.GainExperience => $"+{Value} XP",
			OutcomeEffectType.GainMorale => $"+{Value} morale",
			OutcomeEffectType.LoseMorale => $"-{Value} morale",
			OutcomeEffectType.AddFatigue => Value >= 0 ? $"+{Value} fatigue" : $"{Value} fatigue",
			OutcomeEffectType.ApplyBuff => $"Gained: {Parameter}",
			OutcomeEffectType.RemoveBuff => $"Lost: {Parameter}",
			OutcomeEffectType.ApplyDebuff => $"Afflicted: {Parameter}",
			OutcomeEffectType.UnlockLore => $"Discovered: {Parameter}",
			OutcomeEffectType.AdvanceTime => $"{Value} hours pass",
			OutcomeEffectType.UnlockClass => $"Class unlocked: {Parameter}",
			OutcomeEffectType.UnlockBuilding => $"Building unlocked: {Parameter}",
			OutcomeEffectType.GainUpgradePoints => $"+{Value} upgrade points",
			OutcomeEffectType.GainClassWeapon => $"Received tier {Value} weapon",
			OutcomeEffectType.GainClassArmor => $"Received tier {Value} armor",
			OutcomeEffectType.GainClassSpell => $"Learned tier {Value} spell",
			OutcomeEffectType.AddRunModifier => $"Run modifier: {Parameter}",
			_ => ""
		};
	}
}