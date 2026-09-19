using System;
using System.Collections.Generic;
using System.Text;
using RPGGame.Core.Prototypes.Activities;

namespace RPGGame.Core;

public static partial class Ids {

	// ═══════════════════════════════════════════════════════════════════════
	// ACTIVITIES
	// ═══════════════════════════════════════════════════════════════════════

	public static class Activities {
		public static readonly ActivityProto.ID Fishing = newId("Fishing");
		public static readonly ActivityProto.ID Gathering = newId("Gathering");
		public static readonly ActivityProto.ID Cooking = newId("Cooking");
		public static readonly ActivityProto.ID Mining = newId("Mining");
		public static readonly ActivityProto.ID Woodcutting = newId("Woodcutting");
		public static readonly ActivityProto.ID Smithing = newId("Smithing");
		public static readonly ActivityProto.ID Crafting = newId("Crafting");
		public static readonly ActivityProto.ID Farming = newId("Farming");
		public static readonly ActivityProto.ID Magic = newId("Magic");
		public static readonly ActivityProto.ID Combat = newId("Combat");
		public static readonly ActivityProto.ID Thieving = newId("Thieving");

		private static ActivityProto.ID newId(string name) {
			return new ActivityProto.ID($"Activity_{name}");
		}
	}
}
