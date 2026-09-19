using RPGGame.Core.Effects;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Activities;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Item.Resource;
using RPGGame.Core.Prototypes.Locations;
using RPGGame.Core.Prototypes.Lore;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Prototypes.Village;

namespace RPGGame.Core;

/// <summary>
/// Partial class containing  B u i l d i n g s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
	// BUILDINGS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Buildings {
		// Core Buildings
		public static readonly BuildingProto.ID Anchor = newId("Anchor");
		public static readonly BuildingProto.ID Gate = newId("Gate");

		// Starter Buildings
		public static readonly BuildingProto.ID Blacksmith = newId("Blacksmith");
		public static readonly BuildingProto.ID GeneralStore = newId("GeneralStore");
		public static readonly BuildingProto.ID Tavern = newId("Tavern");
		public static readonly BuildingProto.ID Carpenter = newId("Carpenter");

		// Unlockable Buildings
		public static readonly BuildingProto.ID Herbalist = newId("Herbalist");
		public static readonly BuildingProto.ID Cartographer = newId("Cartographer");
		public static readonly BuildingProto.ID TrainingGrounds = newId("TrainingGrounds");
		public static readonly BuildingProto.ID Library = newId("Library");
		public static readonly BuildingProto.ID Stables = newId("Stables");
		public static readonly BuildingProto.ID HuntersLodge = newId("HuntersLodge");
		public static readonly BuildingProto.ID Shrine = newId("Shrine");
		public static readonly BuildingProto.ID Workshop = newId("Workshop");
		public static readonly BuildingProto.ID Watchtower = newId("Watchtower");
		public static readonly BuildingProto.ID Farm = newId("Farm");
		public static readonly BuildingProto.ID Well = newId("Well");
		public static readonly BuildingProto.ID Monument = newId("Monument");
		public static readonly BuildingProto.ID Graveyard = newId("Graveyard");

		// Late Game Buildings
		public static readonly BuildingProto.ID ArcaneTower = newId("ArcaneTower");
		public static readonly BuildingProto.ID Arena = newId("Arena");
		public static readonly BuildingProto.ID Observatory = newId("Observatory");
		public static readonly BuildingProto.ID PortalChamber = newId("PortalChamber");

		// Special Buildings
		public static readonly BuildingProto.ID Home = newId("Home");
		public static readonly BuildingProto.ID GuildHall = newId("GuildHall");

		// ═══════════════════════════════════════════════════════════════
		// HELPER COLLECTIONS
		// ═══════════════════════════════════════════════════════════════

		/// <summary>
		/// Core buildings that cannot be moved or demolished.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> CoreBuildings => [
			Anchor,
			Gate
		];

		/// <summary>
		/// Starter buildings available from the beginning.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> StarterBuildings => [
			Blacksmith,
			GeneralStore,
			Tavern,
			Carpenter
		];

		/// <summary>
		/// Unlockable buildings.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> UnlockableBuildings => [
			Herbalist,
			Cartographer,
			TrainingGrounds,
			Library,
			Stables,
			HuntersLodge,
			Shrine,
			Workshop,
			Watchtower,
			Farm,
			Well,
			Monument,
			Graveyard
		];

		/// <summary>
		/// Late game buildings requiring prerequisites.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> LateGameBuildings => [
			ArcaneTower,
			Arena,
			Observatory,
			PortalChamber
		];

		/// <summary>
		/// Special buildings unlocked through achievements/events.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> SpecialBuildings => [
			Home,
			GuildHall
		];

		/// <summary>
		/// All building IDs.
		/// </summary>
		public static IReadOnlyList<BuildingProto.ID> AllBuildings => [
			// Core
			Anchor, Gate,
			// Starter
			Blacksmith, GeneralStore, Tavern, Carpenter,
			// Unlockable
			Herbalist, Cartographer, TrainingGrounds, Library,
			Stables, HuntersLodge, Shrine, Workshop,
			Watchtower, Farm, Well, Monument, Graveyard,
			// Late Game
			ArcaneTower, Arena, Observatory, PortalChamber,
			// Special
			Home, GuildHall
		];

		/// <summary>
		/// Checks if a building ID is a core building.
		/// </summary>
		public static bool IsCore(BuildingProto.ID id) => CoreBuildings.Contains(id);

		/// <summary>
		/// Checks if a building ID is a starter building.
		/// </summary>
		public static bool IsStarter(BuildingProto.ID id) => StarterBuildings.Contains(id);

		private static BuildingProto.ID newId(string name) {
			return new BuildingProto.ID($"Building_{name}");
		}
	}
}
