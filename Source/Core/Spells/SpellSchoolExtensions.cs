using RPGGame.Core.Generation;

namespace RPGGame.Core.Spells;

public static partial class SpellSchoolExtensions {
	extension(SpellSchool school) {
		/// <summary>
		/// Gets the display name for this school.
		/// </summary>
		public string GetDisplayName() {
			return school switch {
				SpellSchool.Evocation => "Evocation",
				SpellSchool.Abjuration => "Abjuration",
				SpellSchool.Conjuration => "Conjuration",
				SpellSchool.Transmutation => "Transmutation",
				SpellSchool.Divination => "Divination",
				SpellSchool.Enchantment => "Enchantment",
				SpellSchool.Illusion => "Illusion",
				SpellSchool.Necromancy => "Necromancy",
				SpellSchool.Holy => "Holy",
				SpellSchool.Nature => "Nature",
				SpellSchool.Temporal => "Temporal",
				_ => school.ToString()
			};
		}

		/// <summary>
		/// Gets the color associated with this school.
		/// </summary>
		public UnityEngine.Color GetColor() {
			return school switch {
				SpellSchool.Evocation => new UnityEngine.Color(1f, 0.4f, 0.2f), // Orange-red
				SpellSchool.Abjuration => new UnityEngine.Color(0.3f, 0.6f, 1f), // Blue
				SpellSchool.Conjuration => new UnityEngine.Color(0.6f, 0.3f, 0.9f), // Purple
				SpellSchool.Transmutation => new UnityEngine.Color(0.2f, 0.8f, 0.4f), // Green
				SpellSchool.Divination => new UnityEngine.Color(1f, 1f, 0.5f), // Yellow
				SpellSchool.Enchantment => new UnityEngine.Color(1f, 0.5f, 0.8f), // Pink
				SpellSchool.Illusion => new UnityEngine.Color(0.7f, 0.7f, 0.9f), // Light purple
				SpellSchool.Necromancy => new UnityEngine.Color(0.4f, 0.2f, 0.4f), // Dark purple
				SpellSchool.Holy => new UnityEngine.Color(1f, 0.95f, 0.6f), // Gold
				SpellSchool.Nature => new UnityEngine.Color(0.3f, 0.7f, 0.3f), // Forest green
				SpellSchool.Temporal => new UnityEngine.Color(0.5f, 0.8f, 1f), // Cyan
				_ => UnityEngine.Color.white
			};
		}

		/// <summary>
		/// Gets the icon name for this school.
		/// </summary>
		public string GetIconName() {
			return $"icon_school_{school.ToString().ToLower()}";
		}

		/// <summary>
		/// Gets the TagProto.ID for this spell school.
		/// Maps to Ids.Tags.SpellSchool.* tags.
		/// </summary>
		public TagProto.ID ToTagId() {
			return school switch {
				SpellSchool.Evocation => Ids.Tags.School.Evocation,
				SpellSchool.Abjuration => Ids.Tags.School.Abjuration,
				SpellSchool.Conjuration => Ids.Tags.School.Conjuration,
				SpellSchool.Transmutation => Ids.Tags.School.Transmutation,
				SpellSchool.Divination => Ids.Tags.School.Divination,
				SpellSchool.Enchantment => Ids.Tags.School.Enchantment,
				SpellSchool.Illusion => Ids.Tags.School.Illusion,
				SpellSchool.Necromancy => Ids.Tags.School.Necromancy,
				SpellSchool.Holy => Ids.Tags.School.Holy,
				SpellSchool.Nature => Ids.Tags.School.Nature,
				SpellSchool.Temporal => Ids.Tags.School.Temporal,
				_ => new TagProto.ID($"Tag_SpellSchool_{school}")
			};
		}

		/// <summary>
		/// Gets whether this is an arcane school.
		/// </summary>
		public bool IsArcane() {
			return school is SpellSchool.Evocation or SpellSchool.Abjuration or
				SpellSchool.Conjuration or SpellSchool.Transmutation or
				SpellSchool.Divination or SpellSchool.Enchantment or
				SpellSchool.Illusion or SpellSchool.Necromancy;
		}

		/// <summary>
		/// Gets whether this is a divine school.
		/// </summary>
		public bool IsDivine() {
			return school is SpellSchool.Holy or SpellSchool.Nature;
		}
	}
}