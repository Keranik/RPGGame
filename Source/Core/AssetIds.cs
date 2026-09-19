namespace RPGGame.Core;

/// <summary>
/// Strongly-typed asset identifiers for common assets.
/// </summary>
public static class AssetIds {
	public static class Icons {
		public static class UI {
			public const string Close = "ui/close";
			public const string Menu = "ui/menu";
			public const string Settings = "ui/settings";
			public const string Back = "ui/back";
			public const string Forward = "ui/forward";
			public const string Check = "ui/check";
			public const string Cross = "ui/cross";
			public const string Plus = "ui/plus";
			public const string Minus = "ui/minus";
			public const string Info = "ui/info";
			public const string Warning = "ui/warning";
			public const string Error = "ui/error";
			public const string Lock = "ui/lock";
			public const string Unlock = "ui/unlock";
		}

		public static class Stats {
			public const string Strength = "stats/strength";
			public const string Dexterity = "stats/dexterity";
			public const string Constitution = "stats/constitution";
			public const string Intelligence = "stats/intelligence";
			public const string Wisdom = "stats/wisdom";
			public const string Charisma = "stats/charisma";
			public const string Health = "stats/health";
			public const string Mana = "stats/mana";
			public const string Stamina = "stats/stamina";
			public const string ArmorClass = "stats/armor_class";
			public const string Attack = "stats/attack";
			public const string Damage = "stats/damage";
		}

		public static class Actions {
			public const string Attack = "actions/attack";
			public const string Defend = "actions/defend";
			public const string Flee = "actions/flee";
			public const string UseItem = "actions/use_item";
			public const string CastSpell = "actions/cast_spell";
			public const string Rest = "actions/rest";
			public const string Travel = "actions/travel";
		}

		public static class Resources {
			public const string Gold = "resources/gold";
			public const string Food = "resources/food";
			public const string Wood = "resources/wood";
			public const string Stone = "resources/stone";
			public const string Iron = "resources/iron";
		}
	}

	public static class Cursors {
		public const string Default = "default";
		public const string Pointer = "pointer";
		public const string Grab = "grab";
		public const string Attack = "attack";
		public const string Loot = "loot";
		public const string Talk = "talk";
		public const string Blocked = "blocked";
	}

	public static class Audio {
		public static class UI {
			public const string Click = "UI/click";
			public const string Hover = "UI/hover";
			public const string Open = "UI/open";
			public const string Close = "UI/close";
			public const string Error = "UI/error";
			public const string Success = "UI/success";
			public const string LevelUp = "UI/level_up";
		}

		public static class Combat {
			public const string Hit = "Combat/hit";
			public const string Miss = "Combat/miss";
			public const string Critical = "Combat/critical";
			public const string Block = "Combat/block";
			public const string Dodge = "Combat/dodge";
			public const string Death = "Combat/death";
		}

		public static class Ambient {
			public const string Village = "Ambient/village";
			public const string Forest = "Ambient/forest";
			public const string Dungeon = "Ambient/dungeon";
			public const string Cave = "Ambient/cave";
			public const string Camp = "Ambient/camp";
		}

		public static class Music {
			public const string MainMenu = "Music/main_menu";
			public const string Village = "Music/village";
			public const string Exploration = "Music/exploration";
			public const string Combat = "Music/combat";
			public const string Boss = "Music/boss";
			public const string Victory = "Music/victory";
			public const string Defeat = "Music/defeat";
		}
	}

	public static class Backgrounds {
		public const string MainMenu = "main_menu";
		public const string Village = "village";
		public const string Forest = "forest";
		public const string Dungeon = "dungeon";
		public const string Camp = "camp";
		public const string Combat = "combat";
	}
}