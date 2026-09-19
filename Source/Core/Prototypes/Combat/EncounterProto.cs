using RPGGame.Core.Prototypes.Events;

namespace RPGGame.Core.Prototypes.Combat;
#nullable disable

/// <summary>
/// Prototype for combat encounter definitions.
/// </summary>
public class EncounterProto : Proto {
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	public List<EncounterEnemy> Enemies { get; }
	public bool IsBoss { get; }
	public bool CanFlee { get; }
	public float DifficultyModifier { get; }
	public int BonusExperience { get; }
	public int BonusGold { get; }
	public List<LootEntry> GuaranteedLoot { get; }
	public EventProto.ID? OnVictoryEvent { get; }
	public EventProto.ID? OnDefeatEvent { get; }
	public string MusicId { get; }
	public string BackgroundId { get; }

	public EncounterProto(
		ID id,
		Loc text,
		List<EncounterEnemy> enemies,
		bool isBoss = false,
		bool canFlee = true,
		float difficultyModifier = 1f,
		int bonusExperience = 0,
		int bonusGold = 0,
		List<LootEntry> guaranteedLoot = null,
		EventProto.ID? onVictoryEvent = null,
		EventProto.ID? onDefeatEvent = null,
		string musicId = null,
		string backgroundId = null
	) : base(id, text) {
		Enemies = enemies ?? [];
		IsBoss = isBoss;
		CanFlee = canFlee;
		DifficultyModifier = difficultyModifier;
		BonusExperience = bonusExperience;
		BonusGold = bonusGold;
		GuaranteedLoot = guaranteedLoot ?? [];
		OnVictoryEvent = onVictoryEvent;
		OnDefeatEvent = onDefeatEvent;
		MusicId = musicId;
		BackgroundId = backgroundId;
	}
}

public class EncounterEnemy {
	public EnemyProto.ID EnemyId { get; }
	public int MinCount { get; }
	public int MaxCount { get; }

	public EncounterEnemy(EnemyProto.ID enemyId, int count = 1) {
		EnemyId = enemyId;
		MinCount = count;
		MaxCount = count;
	}

	public EncounterEnemy(EnemyProto.ID enemyId, int minCount, int maxCount) {
		EnemyId = enemyId;
		MinCount = minCount;
		MaxCount = maxCount;
	}
}