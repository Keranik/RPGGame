using RPGGame.Core.Stats;

namespace RPGGame.Core.Characters;

/// <summary>
/// Represents the player's character data.
/// This is a simple data container, not a manager.
/// </summary>
public class Character {
	#region Identity

	/// <summary>
	/// Character name.
	/// </summary>
	public string Name { get; set; } = "Adventurer";

	/// <summary>
	/// Character class ID.
	/// </summary>
	public string ClassId { get; set; } = "fighter";

	/// <summary>
	/// Portrait resource name.
	/// </summary>
	public string PortraitName { get; set; } = "portrait_default";

	#endregion

	#region Level

	/// <summary>
	/// Current level.
	/// </summary>
	public int Level { get; set; } = 1;

	#endregion

	#region Attributes

	/// <summary>
	/// Base attributes (before modifiers).
	/// </summary>
	public ClassAttributes Attributes { get; set; } = new();

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a character from a class prototype.
	/// </summary>
	public static Character CreateFromClass(
		string name,
		string classId,
		ClassAttributes baseAttributes,
		string portraitName
	) {
		return new Character {
			Name = name,
			ClassId = classId,
			PortraitName = portraitName,
			Level = 1,
			Attributes = new ClassAttributes {
				Strength = baseAttributes.Strength,
				Dexterity = baseAttributes.Dexterity,
				Constitution = baseAttributes.Constitution,
				Intelligence = baseAttributes.Intelligence,
				Wisdom = baseAttributes.Wisdom,
				Charisma = baseAttributes.Charisma
			}
		};
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Converts to serializable data.
	/// </summary>
	public CharacterData ToData() {
		return new CharacterData {
			Name = Name,
			ClassId = ClassId,
			PortraitName = PortraitName,
			Level = Level,
			Strength = Attributes.Strength,
			Dexterity = Attributes.Dexterity,
			Constitution = Attributes.Constitution,
			Intelligence = Attributes.Intelligence,
			Wisdom = Attributes.Wisdom,
			Charisma = Attributes.Charisma
		};
	}

	/// <summary>
	/// Creates from serialized data.
	/// </summary>
	public static Character FromData(CharacterData data) {
		return new Character {
			Name = data.Name,
			ClassId = data.ClassId,
			PortraitName = data.PortraitName,
			Level = data.Level,
			Attributes = new ClassAttributes {
				Strength = data.Strength,
				Dexterity = data.Dexterity,
				Constitution = data.Constitution,
				Intelligence = data.Intelligence,
				Wisdom = data.Wisdom,
				Charisma = data.Charisma
			}
		};
	}

	#endregion
}

#region Serialization Data

/// <summary>
/// Serializable character data.
/// </summary>
public class CharacterData {
	public string Name { get; set; } = "";
	public string ClassId { get; set; } = "";
	public string PortraitName { get; set; } = "";
	public int Level { get; set; }
	public int Strength { get; set; }
	public int Dexterity { get; set; }
	public int Constitution { get; set; }
	public int Intelligence { get; set; }
	public int Wisdom { get; set; }
	public int Charisma { get; set; }
}

#endregion