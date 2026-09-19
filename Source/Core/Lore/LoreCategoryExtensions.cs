namespace RPGGame.Core.Lore;

public static class LoreCategoryExtensions {
	extension(LoreCategory category) {
		/// <summary>
		/// Gets the display name for this category.
		/// </summary>
		public string GetDisplayName() {
			return category switch {
				LoreCategory.World => "The World",
				LoreCategory.TheFog => "The Fog",
				LoreCategory.History => "History",
				LoreCategory.Locations => "Locations",
				LoreCategory.Factions => "Factions",
				LoreCategory.Bestiary => "Bestiary",
				LoreCategory.Characters => "Characters",
				LoreCategory.Artifacts => "Artifacts",
				LoreCategory.Magic => "Magic",
				LoreCategory.Village => "The Village",
				LoreCategory.AncientTexts => "Ancient Texts",
				LoreCategory.Notes => "Notes",
				_ => category.ToString()
			};
		}
		/// <summary>
		/// Gets the icon name for this category.
		/// </summary>
		public string GetIconName() {
			return $"icon_lore_{category.ToString().ToLower()}";
		}
		/// <summary>
		/// Gets the description for this category.
		/// </summary>
		public string GetDescription() {
			return category switch {
				LoreCategory.World => "General knowledge about the world and its nature.",
				LoreCategory.TheFog => "Understanding the creeping fog and its connection to time.",
				LoreCategory.History => "Events of the past that shaped the present.",
				LoreCategory.Locations => "Places of interest discovered during expeditions.",
				LoreCategory.Factions => "Groups and organizations operating in the region.",
				LoreCategory.Bestiary => "Creatures encountered in the wilds.",
				LoreCategory.Characters => "Notable individuals met on your journey.",
				LoreCategory.Artifacts => "Powerful items and their histories.",
				LoreCategory.Magic => "The nature and schools of magical power.",
				LoreCategory.Village => "The Anchor village and its inhabitants.",
				LoreCategory.AncientTexts => "Fragments of ancient knowledge.",
				LoreCategory.Notes => "Personal observations and theories.",
				_ => ""
			};
		}
	}

}
