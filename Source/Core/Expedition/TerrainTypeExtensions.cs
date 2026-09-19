using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Item.Resource;

namespace RPGGame.Core.Expedition;

///// <summary>
///// Extension methods for TerrainType.
///// </summary>
//[Obsolete]
//public static class TerrainTypeExtensions {

//	extension(TerrainType terrain) {
//		/// <summary>
//		/// Converts legacy TerrainType to new TerrainProto.ID.
//		/// Use this during migration; new code should use TerrainProto.ID directly.
//		/// </summary>
//		[Obsolete("Use TerrainProto.ID directly instead of converting from TerrainType.")]
//		public TerrainProto.ID ToTerrainId() {
//			return terrain switch {
//				Ids.Terrains.Roads.Road => Ids.Terrains.Roads.Road,
//				Ids.Terrains.Roads.Path => Ids.Terrains.Roads.Path,
//				TerrainType.Trail => Ids.Terrains.Roads.Trail,
//				TerrainType.Bridge => Ids.Terrains.Roads.Bridge,
//				Ids.Terrains.Plains.Grass => Ids.Terrains.Plains.Grass,
//				Ids.Terrains.Plains.Hills => Ids.Terrains.Plains.Hills,
//				TerrainType.Plains => Ids.Terrains.Plains.OpenPlains,
//				TerrainType.Meadow => Ids.Terrains.Plains.Meadow,
//				TerrainType.Farmland => Ids.Terrains.Plains.Farmland,
//				Ids.Terrains.Forests.Forest => Ids.Terrains.Forests.Forest,
//				Ids.Terrains.Forests.DeepForest => Ids.Terrains.Forests.DeepForest,
//				Ids.Terrains.Forests.AncientForest => Ids.Terrains.Forests.AncientForest,
//				Ids.Terrains.Forests.DeadForest => Ids.Terrains.Forests.DeadForest,
//				TerrainType.Jungle => Ids.Terrains.Forests.Jungle,
//				TerrainType.Orchard => Ids.Terrains.Forests.Orchard,
//				Ids.Terrains.Mountains.Mountain => Ids.Terrains.Mountains.Mountain,
//				TerrainType.Peak => Ids.Terrains.Mountains.Peak,
//				Ids.Terrains.Mountains.Cliff => Ids.Terrains.Mountains.Cliff,
//				TerrainType.Pass => Ids.Terrains.Mountains.Pass,
//				Ids.Terrains.Mountains.Highland => Ids.Terrains.Mountains.Highland,
//				Ids.Terrains.Water.River => Ids.Terrains.Water.River,
//				Ids.Terrains.Water.Lake => Ids.Terrains.Water.Lake,
//				TerrainType.Pond => Ids.Terrains.Water.Pond,
//				TerrainType.Water => Ids.Terrains.Water.Ocean,
//				TerrainType.Shallows => Ids.Terrains.Water.Shallows,
//				TerrainType.Waterfall => Ids.Terrains.Water.Waterfall,
//				Ids.Terrains.Water.HotSpring => Ids.Terrains.Water.HotSpring,
//				Ids.Terrains.Coastal.Beach => Ids.Terrains.Coastal.Beach,
//				TerrainType.Coast => Ids.Terrains.Coastal.RockyCoast,
//				TerrainType.TidalFlats => Ids.Terrains.Coastal.TidalFlats,
//				TerrainType.Reef => Ids.Terrains.Coastal.Reef,
//				Ids.Terrains.Wetlands.Swamp => Ids.Terrains.Wetlands.Swamp,
//				TerrainType.Marsh => Ids.Terrains.Wetlands.Marsh,
//				TerrainType.Bog => Ids.Terrains.Wetlands.Bog,
//				TerrainType.Mangrove => Ids.Terrains.Wetlands.Mangrove,
//				Ids.Terrains.Desert.OpenDesert => Ids.Terrains.Desert.OpenDesert,
//				TerrainType.Dunes => Ids.Terrains.Desert.Dunes,
//				TerrainType.Oasis => Ids.Terrains.Desert.Oasis,
//				Ids.Terrains.Desert.Badlands => Ids.Terrains.Desert.Badlands,
//				TerrainType.SaltFlats => Ids.Terrains.Desert.SaltFlats,
//				TerrainType.Canyon => Ids.Terrains.Desert.Canyon,
//				TerrainType.Mesa => Ids.Terrains.Desert.Mesa,
//				TerrainType.Snow => Ids.Terrains.Frozen.Snow,
//				TerrainType.Ice => Ids.Terrains.Frozen.Ice,
//				TerrainType.Glacier => Ids.Terrains.Frozen.Glacier,
//				TerrainType.FrozenLake => Ids.Terrains.Frozen.FrozenLake,
//				TerrainType.Permafrost => Ids.Terrains.Frozen.Permafrost,
//				TerrainType.Blizzard => Ids.Terrains.Frozen.Blizzard,
//				Ids.Terrains.Volcanic.VolcanicPlain => Ids.Terrains.Volcanic.VolcanicPlain,
//				TerrainType.LavaField => Ids.Terrains.Volcanic.LavaField,
//				TerrainType.AshWastes => Ids.Terrains.Volcanic.AshWastes,
//				Ids.Terrains.Volcanic.Geothermal => Ids.Terrains.Volcanic.Geothermal,
//				TerrainType.ObsidianField => Ids.Terrains.Volcanic.ObsidianField,
//				Ids.Terrains.Underground.Cave => Ids.Terrains.Underground.Cave,
//				Ids.Terrains.Underground.Cavern => Ids.Terrains.Underground.Cavern,
//				TerrainType.Tunnel => Ids.Terrains.Underground.Tunnel,
//				TerrainType.UndergroundLake => Ids.Terrains.Underground.UndergroundLake,
//				Ids.Terrains.Underground.CrystalCavern => Ids.Terrains.Underground.CrystalCavern,
//				Ids.Terrains.Underground.MushroomForest => Ids.Terrains.Underground.MushroomForest,
//				TerrainType.LavaTube => Ids.Terrains.Underground.LavaTube,
//				Ids.Terrains.Underground.Mine => Ids.Terrains.Underground.Mine,
//				Ids.Terrains.Underground.Catacombs => Ids.Terrains.Underground.Catacombs,
//				Ids.Terrains.Settlements.Village => Ids.Terrains.Settlements.Village,
//				TerrainType.Town => Ids.Terrains.Settlements.Town,
//				TerrainType.City => Ids.Terrains.Settlements.City,
//				TerrainType.Outpost => Ids.Terrains.Settlements.Outpost,
//				TerrainType.Fort => Ids.Terrains.Settlements.Fort,
//				TerrainType.Castle => Ids.Terrains.Settlements.Castle,
//				Ids.Terrains.Settlements.Temple => Ids.Terrains.Settlements.Temple,
//				Ids.Terrains.Settlements.Tower => Ids.Terrains.Settlements.Tower,
//				TerrainType.Settlement => Ids.Terrains.Settlements.Village,
//				Ids.Terrains.Ruins.OpenRuins => Ids.Terrains.Ruins.OpenRuins,
//				Ids.Terrains.Ruins.AbandonedVillage => Ids.Terrains.Ruins.AbandonedVillage,
//				Ids.Terrains.Ruins.Graveyard => Ids.Terrains.Ruins.Graveyard,
//				Ids.Terrains.Ruins.Battlefield => Ids.Terrains.Ruins.Battlefield,
//				TerrainType.Shipwreck => Ids.Terrains.Ruins.Shipwreck,
//				TerrainType.Dungeon => Ids.Terrains.Ruins.Dungeon,
//				TerrainType.Crypt => Ids.Terrains.Ruins.Crypt,
//				TerrainType.Monument => Ids.Terrains.Ruins.Monument,
//				Ids.Terrains.Supernatural.Corrupted => Ids.Terrains.Supernatural.Corrupted,
//				Ids.Terrains.Supernatural.Blighted => Ids.Terrains.Supernatural.Blighted,
//				Ids.Terrains.Supernatural.ShadowRealm => Ids.Terrains.Supernatural.ShadowRealm,
//				TerrainType.Wasteland => Ids.Terrains.Supernatural.Wasteland,
//				TerrainType.CursedGround => Ids.Terrains.Supernatural.CursedGround,
//				Ids.Terrains.Supernatural.Magical => Ids.Terrains.Supernatural.Magical,
//				TerrainType.FeyCrossing => Ids.Terrains.Supernatural.FeyCrossing,
//				Ids.Terrains.Supernatural.LeyLine => Ids.Terrains.Supernatural.LeyLine,
//				Ids.Terrains.Supernatural.VoidTouched => Ids.Terrains.Supernatural.VoidTouched,
//				TerrainType.Fog => Ids.Terrains.Special.Fog,
//				Ids.Terrains.Special.Blocked => Ids.Terrains.Special.Blocked,
//				TerrainType.Portal => Ids.Terrains.Special.Portal,
//				TerrainType.Sanctuary => Ids.Terrains.Special.Sanctuary,
//				TerrainType.Arena => Ids.Terrains.Special.Arena,
//				Ids.Terrains.Special.Crossroads => Ids.Terrains.Special.Crossroads,
//				TerrainType.Campsite => Ids.Terrains.Special.Campsite,
//				TerrainType.Wayshrine => Ids.Terrains.Special.Wayshrine,
//				_ => Ids.Terrains.Roads.Path // Default fallback
//			};
//		}
//		/// <summary>
//		/// Gets the movement speed multiplier for this terrain.
//		/// 1.0 = normal speed, lower = slower, higher = faster.
//		/// </summary>
//		public float GetMovementMultiplier() {
//			return terrain switch {
//				// Roads & Paths
//				Ids.Terrains.Roads.Road => 1.3f,
//				Ids.Terrains.Roads.Path => 1.0f,
//				TerrainType.Trail => 0.85f,
//				TerrainType.Bridge => 1.2f,

//				// Grasslands & Plains
//				Ids.Terrains.Plains.Grass => 0.9f,
//				Ids.Terrains.Plains.Hills => 0.6f,
//				TerrainType.Plains => 1.0f,
//				TerrainType.Meadow => 0.9f,
//				TerrainType.Farmland => 0.85f,

//				// Forests & Woodlands
//				Ids.Terrains.Forests.Forest => 0.7f,
//				Ids.Terrains.Forests.DeepForest => 0.5f,
//				Ids.Terrains.Forests.AncientForest => 0.45f,
//				Ids.Terrains.Forests.DeadForest => 0.6f,
//				TerrainType.Jungle => 0.4f,
//				TerrainType.Orchard => 0.8f,

//				// Mountains & Highlands
//				Ids.Terrains.Mountains.Mountain => 0.3f,
//				TerrainType.Peak => 0.2f,
//				Ids.Terrains.Mountains.Cliff => 0.1f,
//				TerrainType.Pass => 0.5f,
//				Ids.Terrains.Mountains.Highland => 0.55f,

//				// Water Features
//				Ids.Terrains.Water.River => 0.3f,
//				Ids.Terrains.Water.Lake => 0f,
//				TerrainType.Pond => 0.4f,
//				TerrainType.Water => 0f,
//				TerrainType.Shallows => 0.4f,
//				TerrainType.Waterfall => 0.2f,
//				Ids.Terrains.Water.HotSpring => 0.7f,

//				// Coastal
//				Ids.Terrains.Coastal.Beach => 0.7f,
//				TerrainType.Coast => 0.5f,
//				TerrainType.TidalFlats => 0.5f,
//				TerrainType.Reef => 0f,

//				// Wetlands
//				Ids.Terrains.Wetlands.Swamp => 0.4f,
//				TerrainType.Marsh => 0.45f,
//				TerrainType.Bog => 0.3f,
//				TerrainType.Mangrove => 0.35f,

//				// Arid & Desert
//				Ids.Terrains.Desert.OpenDesert => 0.7f,
//				TerrainType.Dunes => 0.4f,
//				TerrainType.Oasis => 0.8f,
//				Ids.Terrains.Desert.Badlands => 0.5f,
//				TerrainType.SaltFlats => 0.75f,
//				TerrainType.Canyon => 0.45f,
//				TerrainType.Mesa => 0.5f,

//				// Cold & Frozen
//				TerrainType.Snow => 0.5f,
//				TerrainType.Ice => 0.6f,
//				TerrainType.Glacier => 0.3f,
//				TerrainType.FrozenLake => 0.7f,
//				TerrainType.Permafrost => 0.55f,
//				TerrainType.Blizzard => 0.2f,

//				// Volcanic & Fire
//				Ids.Terrains.Volcanic.VolcanicPlain => 0.5f,
//				TerrainType.LavaField => 0.2f,
//				TerrainType.AshWastes => 0.55f,
//				Ids.Terrains.Volcanic.Geothermal => 0.6f,
//				TerrainType.ObsidianField => 0.4f,

//				// Underground
//				Ids.Terrains.Underground.Cave => 0.6f,
//				Ids.Terrains.Underground.Cavern => 0.65f,
//				TerrainType.Tunnel => 0.5f,
//				TerrainType.UndergroundLake => 0f,
//				Ids.Terrains.Underground.CrystalCavern => 0.6f,
//				Ids.Terrains.Underground.MushroomForest => 0.55f,
//				TerrainType.LavaTube => 0.4f,
//				Ids.Terrains.Underground.Mine => 0.6f,
//				Ids.Terrains.Underground.Catacombs => 0.5f,

//				// Settlements & Structures
//				Ids.Terrains.Settlements.Village => 1.0f,
//				TerrainType.Town => 1.0f,
//				TerrainType.City => 1.0f,
//				TerrainType.Outpost => 0.9f,
//				TerrainType.Fort => 0.85f,
//				TerrainType.Castle => 0.8f,
//				Ids.Terrains.Settlements.Temple => 0.9f,
//				Ids.Terrains.Settlements.Tower => 0.7f,
//				TerrainType.Settlement => 1.0f,

//				// Ruins & Abandoned
//				Ids.Terrains.Ruins.OpenRuins => 0.6f,
//				Ids.Terrains.Ruins.AbandonedVillage => 0.7f,
//				Ids.Terrains.Ruins.Graveyard => 0.75f,
//				Ids.Terrains.Ruins.Battlefield => 0.65f,
//				TerrainType.Shipwreck => 0.4f,
//				TerrainType.Dungeon => 0.5f,
//				TerrainType.Crypt => 0.55f,
//				TerrainType.Monument => 0.8f,

//				// Corrupted & Magical
//				Ids.Terrains.Supernatural.Corrupted => 0.5f,
//				Ids.Terrains.Supernatural.Blighted => 0.55f,
//				Ids.Terrains.Supernatural.ShadowRealm => 0.4f,
//				TerrainType.Wasteland => 0.6f,
//				TerrainType.CursedGround => 0.5f,
//				Ids.Terrains.Supernatural.Magical => 0.7f,
//				TerrainType.FeyCrossing => 0.6f,
//				Ids.Terrains.Supernatural.LeyLine => 0.8f,
//				Ids.Terrains.Supernatural.VoidTouched => 0.3f,

//				// Special
//				TerrainType.Fog => 0f,
//				Ids.Terrains.Special.Blocked => 0f,
//				TerrainType.Portal => 1.0f,
//				TerrainType.Sanctuary => 1.0f,
//				TerrainType.Arena => 0.9f,
//				Ids.Terrains.Special.Crossroads => 1.1f,
//				TerrainType.Campsite => 0.9f,
//				TerrainType.Wayshrine => 1.0f,

//				_ => 0.5f
//			};
//		}
//		/// <summary>
//		/// Gets an icon/emoji for this terrain type.
//		/// </summary>
//		public string GetIcon() {
//			return terrain switch {
//				// Roads & Paths
//				Ids.Terrains.Roads.Road => "🛤",
//				Ids.Terrains.Roads.Path => "🚶",
//				TerrainType.Trail => "👣",
//				TerrainType.Bridge => "🌉",

//				// Grasslands
//				Ids.Terrains.Plains.Grass => "🌿",
//				Ids.Terrains.Plains.Hills => "⛰",
//				TerrainType.Plains => "🌾",
//				TerrainType.Meadow => "🌼",
//				TerrainType.Farmland => "🌾",

//				// Forests
//				Ids.Terrains.Forests.Forest => "🌲",
//				Ids.Terrains.Forests.DeepForest => "🌳",
//				Ids.Terrains.Forests.AncientForest => "🌳",
//				Ids.Terrains.Forests.DeadForest => "🥀",
//				TerrainType.Jungle => "🌴",
//				TerrainType.Orchard => "🍎",

//				// Mountains
//				Ids.Terrains.Mountains.Mountain => "🏔",
//				TerrainType.Peak => "⛰",
//				Ids.Terrains.Mountains.Cliff => "🧗",
//				TerrainType.Pass => "🛤",
//				Ids.Terrains.Mountains.Highland => "🏔",

//				// Water
//				Ids.Terrains.Water.River => "🏞",
//				Ids.Terrains.Water.Lake => "💧",
//				TerrainType.Pond => "🌊",
//				TerrainType.Water => "🌊",
//				TerrainType.Shallows => "🐚",
//				TerrainType.Waterfall => "💦",
//				Ids.Terrains.Water.HotSpring => "♨",

//				// Coastal
//				Ids.Terrains.Coastal.Beach => "🏖",
//				TerrainType.Coast => "🌊",
//				TerrainType.TidalFlats => "🐚",
//				TerrainType.Reef => "🪸",

//				// Wetlands
//				Ids.Terrains.Wetlands.Swamp => "🐊",
//				TerrainType.Marsh => "🦆",
//				TerrainType.Bog => "💀",
//				TerrainType.Mangrove => "🌿",

//				// Desert
//				Ids.Terrains.Desert.OpenDesert => "🏜",
//				TerrainType.Dunes => "🏜",
//				TerrainType.Oasis => "🌴",
//				Ids.Terrains.Desert.Badlands => "🏜",
//				TerrainType.SaltFlats => "🧂",
//				TerrainType.Canyon => "🏜",
//				TerrainType.Mesa => "🏜",

//				// Cold
//				TerrainType.Snow => "❄",
//				TerrainType.Ice => "🧊",
//				TerrainType.Glacier => "🏔",
//				TerrainType.FrozenLake => "🧊",
//				TerrainType.Permafrost => "❄",
//				TerrainType.Blizzard => "🌨",

//				// Volcanic
//				Ids.Terrains.Volcanic.VolcanicPlain => "🌋",
//				TerrainType.LavaField => "🔥",
//				TerrainType.AshWastes => "🌫",
//				Ids.Terrains.Volcanic.Geothermal => "♨",
//				TerrainType.ObsidianField => "🖤",

//				// Underground
//				Ids.Terrains.Underground.Cave => "🕳",
//				Ids.Terrains.Underground.Cavern => "🦇",
//				TerrainType.Tunnel => "🚇",
//				TerrainType.UndergroundLake => "💧",
//				Ids.Terrains.Underground.CrystalCavern => "💎",
//				Ids.Terrains.Underground.MushroomForest => "🍄",
//				TerrainType.LavaTube => "🔥",
//				Ids.Terrains.Underground.Mine => "⛏",
//				Ids.Terrains.Underground.Catacombs => "💀",

//				// Settlements
//				Ids.Terrains.Settlements.Village => "🏘",
//				TerrainType.Town => "🏛",
//				TerrainType.City => "🏙",
//				TerrainType.Outpost => "🏚",
//				TerrainType.Fort => "🏰",
//				TerrainType.Castle => "🏰",
//				Ids.Terrains.Settlements.Temple => "⛩",
//				Ids.Terrains.Settlements.Tower => "🗼",
//				TerrainType.Settlement => "🏘",

//				// Ruins
//				Ids.Terrains.Ruins.OpenRuins => "🏚",
//				Ids.Terrains.Ruins.AbandonedVillage => "🏚",
//				Ids.Terrains.Ruins.Graveyard => "🪦",
//				Ids.Terrains.Ruins.Battlefield => "⚔",
//				TerrainType.Shipwreck => "🚢",
//				TerrainType.Dungeon => "🏰",
//				TerrainType.Crypt => "⚰",
//				TerrainType.Monument => "🗿",

//				// Corrupted
//				Ids.Terrains.Supernatural.Corrupted => "☠",
//				Ids.Terrains.Supernatural.Blighted => "🦠",
//				Ids.Terrains.Supernatural.ShadowRealm => "👻",
//				TerrainType.Wasteland => "💀",
//				TerrainType.CursedGround => "☠",
//				Ids.Terrains.Supernatural.Magical => "✨",
//				TerrainType.FeyCrossing => "🧚",
//				Ids.Terrains.Supernatural.LeyLine => "⚡",
//				Ids.Terrains.Supernatural.VoidTouched => "🕳",

//				// Special
//				TerrainType.Fog => "🌫",
//				Ids.Terrains.Special.Blocked => "🚫",
//				TerrainType.Portal => "🌀",
//				TerrainType.Sanctuary => "🏛",
//				TerrainType.Arena => "⚔",
//				Ids.Terrains.Special.Crossroads => "🔀",
//				TerrainType.Campsite => "🏕",
//				TerrainType.Wayshrine => "✨",

//				_ => "❓"
//			};
//		}
//		/// <summary>
//		/// Gets whether this terrain is passable on foot.
//		/// </summary>
//		public bool IsPassable() {
//			return terrain switch {
//				TerrainType.Water => false,
//				Ids.Terrains.Water.Lake => false,
//				TerrainType.Reef => false,
//				TerrainType.UndergroundLake => false,
//				TerrainType.Fog => false,
//				Ids.Terrains.Special.Blocked => false,
//				Ids.Terrains.Mountains.Cliff => false,
//				TerrainType.LavaField => false,
//				_ => true
//			};
//		}
//		/// <summary>
//		/// Gets whether this terrain requires special equipment or abilities to traverse.
//		/// </summary>
//		public bool RequiresSpecialAccess() {
//			return terrain switch {
//				Ids.Terrains.Mountains.Mountain => true,
//				TerrainType.Peak => true,
//				Ids.Terrains.Mountains.Cliff => true,
//				TerrainType.Water => true,
//				Ids.Terrains.Water.Lake => true,
//				TerrainType.Reef => true,
//				TerrainType.Glacier => true,
//				TerrainType.LavaField => true,
//				Ids.Terrains.Supernatural.VoidTouched => true,
//				Ids.Terrains.Supernatural.ShadowRealm => true,
//				_ => false
//			};
//		}
//		/// <summary>
//		/// Gets the morale modifier for traveling through this terrain.
//		/// Positive = morale boost, Negative = morale drain per tile.
//		/// </summary>
//		public float GetMoraleModifier() {
//			return terrain switch {
//				// Roads & Paths - Neutral to positive
//				Ids.Terrains.Roads.Road => 0.5f,
//				Ids.Terrains.Roads.Path => 0f,
//				TerrainType.Trail => -0.25f,
//				TerrainType.Bridge => 0f,

//				// Grasslands & Plains - Pleasant
//				Ids.Terrains.Plains.Grass => 0f,
//				Ids.Terrains.Plains.Hills => -0.5f,
//				TerrainType.Plains => 0.25f,
//				TerrainType.Meadow => 1f,
//				TerrainType.Farmland => 0.5f,

//				// Forests & Woodlands - Mixed
//				Ids.Terrains.Forests.Forest => -0.5f,
//				Ids.Terrains.Forests.DeepForest => -1.5f,
//				Ids.Terrains.Forests.AncientForest => 0f,
//				Ids.Terrains.Forests.DeadForest => -2f,
//				TerrainType.Jungle => -1.5f,
//				TerrainType.Orchard => 1f,

//				// Mountains & Highlands - Harsh
//				Ids.Terrains.Mountains.Mountain => -2f,
//				TerrainType.Peak => -3f,
//				Ids.Terrains.Mountains.Cliff => -2.5f,
//				TerrainType.Pass => -1f,
//				Ids.Terrains.Mountains.Highland => -0.5f,

//				// Water Features
//				Ids.Terrains.Water.River => -0.5f,
//				Ids.Terrains.Water.Lake => 0f,
//				TerrainType.Pond => 0.5f,
//				TerrainType.Water => -1f,
//				TerrainType.Shallows => -0.25f,
//				TerrainType.Waterfall => 1f,
//				Ids.Terrains.Water.HotSpring => 3f,

//				// Coastal - Generally pleasant
//				Ids.Terrains.Coastal.Beach => 1f,
//				TerrainType.Coast => 0.5f,
//				TerrainType.TidalFlats => -0.25f,
//				TerrainType.Reef => 0f,

//				// Wetlands - Unpleasant
//				Ids.Terrains.Wetlands.Swamp => -2f,
//				TerrainType.Marsh => -1.5f,
//				TerrainType.Bog => -2.5f,
//				TerrainType.Mangrove => -1.5f,

//				// Arid & Desert - Harsh
//				Ids.Terrains.Desert.OpenDesert => -1.5f,
//				TerrainType.Dunes => -2f,
//				TerrainType.Oasis => 3f,
//				Ids.Terrains.Desert.Badlands => -2f,
//				TerrainType.SaltFlats => -2.5f,
//				TerrainType.Canyon => -1f,
//				TerrainType.Mesa => -0.5f,

//				// Cold & Frozen - Very harsh
//				TerrainType.Snow => -1.5f,
//				TerrainType.Ice => -2f,
//				TerrainType.Glacier => -2.5f,
//				TerrainType.FrozenLake => -1.5f,
//				TerrainType.Permafrost => -1.5f,
//				TerrainType.Blizzard => -4f,

//				// Volcanic & Fire - Dangerous
//				Ids.Terrains.Volcanic.VolcanicPlain => -2f,
//				TerrainType.LavaField => -4f,
//				TerrainType.AshWastes => -2.5f,
//				Ids.Terrains.Volcanic.Geothermal => -1f,
//				TerrainType.ObsidianField => -1.5f,

//				// Underground - Dark and oppressive
//				Ids.Terrains.Underground.Cave => -1f,
//				Ids.Terrains.Underground.Cavern => -0.5f,
//				TerrainType.Tunnel => -1.5f,
//				TerrainType.UndergroundLake => -0.5f,
//				Ids.Terrains.Underground.CrystalCavern => 1f,
//				Ids.Terrains.Underground.MushroomForest => 0f,
//				TerrainType.LavaTube => -2f,
//				Ids.Terrains.Underground.Mine => -1f,
//				Ids.Terrains.Underground.Catacombs => -2.5f,

//				// Settlements & Structures - Safe and comfortable
//				Ids.Terrains.Settlements.Village => 5f,
//				TerrainType.Town => 6f,
//				TerrainType.City => 7f,
//				TerrainType.Outpost => 2f,
//				TerrainType.Fort => 3f,
//				TerrainType.Castle => 4f,
//				Ids.Terrains.Settlements.Temple => 5f,
//				Ids.Terrains.Settlements.Tower => 1f,
//				TerrainType.Settlement => 5f,

//				// Ruins & Abandoned - Eerie
//				Ids.Terrains.Ruins.OpenRuins => -1.5f,
//				Ids.Terrains.Ruins.AbandonedVillage => -2f,
//				Ids.Terrains.Ruins.Graveyard => -2.5f,
//				Ids.Terrains.Ruins.Battlefield => -3f,
//				TerrainType.Shipwreck => -1.5f,
//				TerrainType.Dungeon => -2f,
//				TerrainType.Crypt => -3f,
//				TerrainType.Monument => 0.5f,

//				// Corrupted & Magical - Disturbing
//				Ids.Terrains.Supernatural.Corrupted => -3f,
//				Ids.Terrains.Supernatural.Blighted => -2.5f,
//				Ids.Terrains.Supernatural.ShadowRealm => -4f,
//				TerrainType.Wasteland => -2f,
//				TerrainType.CursedGround => -3.5f,
//				Ids.Terrains.Supernatural.Magical => 0.5f,
//				TerrainType.FeyCrossing => 1f,
//				Ids.Terrains.Supernatural.LeyLine => 0.5f,
//				Ids.Terrains.Supernatural.VoidTouched => -5f,

//				// Special
//				TerrainType.Fog => -1f,
//				Ids.Terrains.Special.Blocked => 0f,
//				TerrainType.Portal => 0f,
//				TerrainType.Sanctuary => 10f,
//				TerrainType.Arena => -0.5f,
//				Ids.Terrains.Special.Crossroads => 0f,
//				TerrainType.Campsite => 3f,
//				TerrainType.Wayshrine => 2f,

//				_ => 0f
//			};
//		}
//		/// <summary>
//		/// Gets the encounter rate multiplier for this terrain.
//		/// Higher = more frequent encounters. 0 = no encounters.
//		/// </summary>
//		public float GetEncounterMultiplier() {
//			return terrain switch {
//				// Roads & Paths - Safe
//				Ids.Terrains.Roads.Road => 0.4f,
//				Ids.Terrains.Roads.Path => 0.6f,
//				TerrainType.Trail => 0.8f,
//				TerrainType.Bridge => 0.7f,

//				// Grasslands & Plains
//				Ids.Terrains.Plains.Grass => 1.0f,
//				Ids.Terrains.Plains.Hills => 1.2f,
//				TerrainType.Plains => 0.9f,
//				TerrainType.Meadow => 0.7f,
//				TerrainType.Farmland => 0.5f,

//				// Forests & Woodlands - Dangerous
//				Ids.Terrains.Forests.Forest => 1.5f,
//				Ids.Terrains.Forests.DeepForest => 2.0f,
//				Ids.Terrains.Forests.AncientForest => 1.8f,
//				Ids.Terrains.Forests.DeadForest => 1.7f,
//				TerrainType.Jungle => 2.2f,
//				TerrainType.Orchard => 0.4f,

//				// Mountains & Highlands
//				Ids.Terrains.Mountains.Mountain => 1.5f,
//				TerrainType.Peak => 1.2f,
//				Ids.Terrains.Mountains.Cliff => 0.8f,
//				TerrainType.Pass => 1.8f,
//				Ids.Terrains.Mountains.Highland => 1.3f,

//				// Water Features
//				Ids.Terrains.Water.River => 0.8f,
//				Ids.Terrains.Water.Lake => 0.5f,
//				TerrainType.Pond => 0.4f,
//				TerrainType.Water => 0.6f,
//				TerrainType.Shallows => 0.7f,
//				TerrainType.Waterfall => 0.6f,
//				Ids.Terrains.Water.HotSpring => 0.3f,

//				// Coastal
//				Ids.Terrains.Coastal.Beach => 0.6f,
//				TerrainType.Coast => 0.8f,
//				TerrainType.TidalFlats => 0.7f,
//				TerrainType.Reef => 1.0f,

//				// Wetlands - Treacherous
//				Ids.Terrains.Wetlands.Swamp => 1.8f,
//				TerrainType.Marsh => 1.5f,
//				TerrainType.Bog => 1.6f,
//				TerrainType.Mangrove => 1.7f,

//				// Arid & Desert
//				Ids.Terrains.Desert.OpenDesert => 0.8f,
//				TerrainType.Dunes => 0.7f,
//				TerrainType.Oasis => 1.2f,
//				Ids.Terrains.Desert.Badlands => 1.3f,
//				TerrainType.SaltFlats => 0.4f,
//				TerrainType.Canyon => 1.5f,
//				TerrainType.Mesa => 1.0f,

//				// Cold & Frozen
//				TerrainType.Snow => 0.7f,
//				TerrainType.Ice => 0.5f,
//				TerrainType.Glacier => 0.4f,
//				TerrainType.FrozenLake => 0.6f,
//				TerrainType.Permafrost => 0.6f,
//				TerrainType.Blizzard => 0.3f,

//				// Volcanic & Fire
//				Ids.Terrains.Volcanic.VolcanicPlain => 1.4f,
//				TerrainType.LavaField => 1.0f,
//				TerrainType.AshWastes => 1.2f,
//				Ids.Terrains.Volcanic.Geothermal => 1.1f,
//				TerrainType.ObsidianField => 0.9f,

//				// Underground - Very dangerous
//				Ids.Terrains.Underground.Cave => 2.5f,
//				Ids.Terrains.Underground.Cavern => 2.0f,
//				TerrainType.Tunnel => 2.2f,
//				TerrainType.UndergroundLake => 1.5f,
//				Ids.Terrains.Underground.CrystalCavern => 1.8f,
//				Ids.Terrains.Underground.MushroomForest => 1.6f,
//				TerrainType.LavaTube => 1.8f,
//				Ids.Terrains.Underground.Mine => 2.0f,
//				Ids.Terrains.Underground.Catacombs => 2.8f,

//				// Settlements & Structures - Safe
//				Ids.Terrains.Settlements.Village => 0.1f,
//				TerrainType.Town => 0.05f,
//				TerrainType.City => 0.02f,
//				TerrainType.Outpost => 0.3f,
//				TerrainType.Fort => 0.2f,
//				TerrainType.Castle => 0.15f,
//				Ids.Terrains.Settlements.Temple => 0.1f,
//				Ids.Terrains.Settlements.Tower => 0.5f,
//				TerrainType.Settlement => 0.1f,

//				// Ruins & Abandoned - Dangerous
//				Ids.Terrains.Ruins.OpenRuins => 2.5f,
//				Ids.Terrains.Ruins.AbandonedVillage => 2.0f,
//				Ids.Terrains.Ruins.Graveyard => 2.8f,
//				Ids.Terrains.Ruins.Battlefield => 2.2f,
//				TerrainType.Shipwreck => 1.5f,
//				TerrainType.Dungeon => 3.0f,
//				TerrainType.Crypt => 2.8f,
//				TerrainType.Monument => 1.2f,

//				// Corrupted & Magical - Very dangerous
//				Ids.Terrains.Supernatural.Corrupted => 2.5f,
//				Ids.Terrains.Supernatural.Blighted => 2.2f,
//				Ids.Terrains.Supernatural.ShadowRealm => 3.0f,
//				TerrainType.Wasteland => 1.8f,
//				TerrainType.CursedGround => 2.5f,
//				Ids.Terrains.Supernatural.Magical => 1.5f,
//				TerrainType.FeyCrossing => 1.8f,
//				Ids.Terrains.Supernatural.LeyLine => 1.2f,
//				Ids.Terrains.Supernatural.VoidTouched => 3.5f,

//				// Special
//				TerrainType.Fog => 0f,
//				Ids.Terrains.Special.Blocked => 0f,
//				TerrainType.Portal => 0.5f,
//				TerrainType.Sanctuary => 0f,
//				TerrainType.Arena => 0f,
//				Ids.Terrains.Special.Crossroads => 1.0f,
//				TerrainType.Campsite => 0.2f,
//				TerrainType.Wayshrine => 0.1f,

//				_ => 1.0f
//			};
//		}
//		/// <summary>
//		/// Gets environmental damage per turn for this terrain (if any).
//		/// Returns (damageAmount, damageType) or (0, null) if none.
//		/// </summary>
//		public (float damage, DamageType? type) GetEnvironmentalDamage() {
//			return terrain switch {
//				// Cold damage
//				TerrainType.Snow => (1f, DamageType.Cold),
//				TerrainType.Ice => (1.5f, DamageType.Cold),
//				TerrainType.Glacier => (2f, DamageType.Cold),
//				TerrainType.FrozenLake => (1f, DamageType.Cold),
//				TerrainType.Blizzard => (3f, DamageType.Cold),

//				// Fire/Heat damage
//				Ids.Terrains.Volcanic.VolcanicPlain => (1.5f, DamageType.Fire),
//				TerrainType.LavaField => (5f, DamageType.Fire),
//				TerrainType.AshWastes => (1f, DamageType.Fire),
//				Ids.Terrains.Volcanic.Geothermal => (0.5f, DamageType.Fire),
//				TerrainType.LavaTube => (2f, DamageType.Fire),
//				Ids.Terrains.Desert.OpenDesert => (0.5f, DamageType.Fire),

//				// Poison/Disease damage
//				Ids.Terrains.Wetlands.Swamp => (0.5f, DamageType.Poison),
//				TerrainType.Bog => (1f, DamageType.Poison),
//				Ids.Terrains.Supernatural.Blighted => (1.5f, DamageType.Poison),

//				// Necrotic/Dark damage
//				Ids.Terrains.Supernatural.Corrupted => (1f, DamageType.Necrotic),
//				TerrainType.CursedGround => (1.5f, DamageType.Necrotic),
//				Ids.Terrains.Supernatural.ShadowRealm => (2f, DamageType.Necrotic),
//				Ids.Terrains.Supernatural.VoidTouched => (3f, DamageType.Necrotic),
//				Ids.Terrains.Ruins.Graveyard => (0.5f, DamageType.Necrotic),
//				TerrainType.Crypt => (0.5f, DamageType.Necrotic),
//				Ids.Terrains.Underground.Catacombs => (0.5f, DamageType.Necrotic),

//				// Physical hazards
//				TerrainType.ObsidianField => (1f, DamageType.Slashing),

//				_ => (0f, null)
//			};
//		}
//		/// <summary>
//		/// Gets the visibility range modifier for this terrain.
//		/// 1.0 = normal visibility, lower = reduced.
//		/// </summary>
//		public float GetVisibilityModifier() {
//			return terrain switch {
//				// Excellent visibility
//				TerrainType.Plains => 1.5f,
//				Ids.Terrains.Desert.OpenDesert => 1.4f,
//				TerrainType.SaltFlats => 1.5f,
//				TerrainType.Peak => 2.0f,
//				Ids.Terrains.Mountains.Highland => 1.3f,
//				TerrainType.Mesa => 1.4f,

//				// Good visibility
//				Ids.Terrains.Roads.Road => 1.0f,
//				Ids.Terrains.Roads.Path => 1.0f,
//				Ids.Terrains.Plains.Grass => 1.0f,
//				Ids.Terrains.Plains.Hills => 1.1f,
//				Ids.Terrains.Coastal.Beach => 1.2f,
//				TerrainType.Coast => 1.1f,

//				// Reduced visibility
//				Ids.Terrains.Forests.Forest => 0.6f,
//				Ids.Terrains.Forests.DeepForest => 0.4f,
//				Ids.Terrains.Forests.AncientForest => 0.5f,
//				TerrainType.Jungle => 0.3f,
//				Ids.Terrains.Wetlands.Swamp => 0.5f,
//				TerrainType.Marsh => 0.6f,
//				TerrainType.Mangrove => 0.4f,

//				// Very poor visibility
//				Ids.Terrains.Underground.Cave => 0.3f,
//				Ids.Terrains.Underground.Cavern => 0.4f,
//				TerrainType.Tunnel => 0.2f,
//				Ids.Terrains.Underground.Mine => 0.3f,
//				Ids.Terrains.Underground.Catacombs => 0.2f,
//				TerrainType.Dungeon => 0.3f,
//				TerrainType.Crypt => 0.2f,

//				// Weather/Environmental reduced visibility
//				TerrainType.Blizzard => 0.1f,
//				TerrainType.AshWastes => 0.4f,
//				TerrainType.Fog => 0.1f,
//				Ids.Terrains.Supernatural.ShadowRealm => 0.3f,

//				// Magical lighting
//				Ids.Terrains.Underground.CrystalCavern => 0.7f,
//				Ids.Terrains.Underground.MushroomForest => 0.5f,
//				TerrainType.LavaField => 0.6f,
//				Ids.Terrains.Volcanic.VolcanicPlain => 0.7f,

//				_ => 1.0f
//			};
//		}
//		/// <summary>
//		/// Gets the display name for this terrain.
//		/// </summary>
//		public string GetDisplayName() {
//			return terrain switch {
//				Ids.Terrains.Forests.DeepForest => "Deep Forest",
//				Ids.Terrains.Forests.AncientForest => "Ancient Forest",
//				Ids.Terrains.Forests.DeadForest => "Dead Forest",
//				Ids.Terrains.Water.HotSpring => "Hot Spring",
//				TerrainType.TidalFlats => "Tidal Flats",
//				TerrainType.SaltFlats => "Salt Flats",
//				TerrainType.FrozenLake => "Frozen Lake",
//				TerrainType.LavaField => "Lava Field",
//				TerrainType.AshWastes => "Ash Wastes",
//				TerrainType.ObsidianField => "Obsidian Field",
//				TerrainType.UndergroundLake => "Underground Lake",
//				Ids.Terrains.Underground.CrystalCavern => "Crystal Cavern",
//				Ids.Terrains.Underground.MushroomForest => "Mushroom Forest",
//				TerrainType.LavaTube => "Lava Tube",
//				Ids.Terrains.Ruins.AbandonedVillage => "Abandoned Village",
//				Ids.Terrains.Supernatural.ShadowRealm => "Shadow Realm",
//				TerrainType.CursedGround => "Cursed Ground",
//				TerrainType.FeyCrossing => "Fey Crossing",
//				Ids.Terrains.Supernatural.LeyLine => "Ley Line",
//				Ids.Terrains.Supernatural.VoidTouched => "Void Touched",
//				_ => SplitCamelCase(terrain.ToString())
//			};
//		}
//		/// <summary>
//		/// Gets the terrain category for grouping purposes.
//		/// </summary>
//		public TerrainCategory GetCategory() {
//			return terrain switch {
//				Ids.Terrains.Roads.Road or Ids.Terrains.Roads.Path or TerrainType.Trail or TerrainType.Bridge
//					=> TerrainCategory.Road,

//				Ids.Terrains.Plains.Grass or Ids.Terrains.Plains.Hills or TerrainType.Plains or TerrainType.Meadow or TerrainType.Farmland
//					=> TerrainCategory.Grassland,

//				Ids.Terrains.Forests.Forest or Ids.Terrains.Forests.DeepForest or Ids.Terrains.Forests.AncientForest or 
//					Ids.Terrains.Forests.DeadForest or TerrainType.Jungle or TerrainType.Orchard
//					=> TerrainCategory.Forest,

//				Ids.Terrains.Mountains.Mountain or TerrainType.Peak or Ids.Terrains.Mountains.Cliff or 
//					TerrainType.Pass or Ids.Terrains.Mountains.Highland
//					=> TerrainCategory.Mountain,

//				Ids.Terrains.Water.River or Ids.Terrains.Water.Lake or TerrainType.Pond or TerrainType.Water or 
//					TerrainType.Shallows or TerrainType.Waterfall or Ids.Terrains.Water.HotSpring
//					=> TerrainCategory.Water,

//				Ids.Terrains.Coastal.Beach or TerrainType.Coast or TerrainType.TidalFlats or TerrainType.Reef
//					=> TerrainCategory.Coastal,

//				Ids.Terrains.Wetlands.Swamp or TerrainType.Marsh or TerrainType.Bog or TerrainType.Mangrove
//					=> TerrainCategory.Wetland,

//				Ids.Terrains.Desert.OpenDesert or TerrainType.Dunes or TerrainType.Oasis or Ids.Terrains.Desert.Badlands or 
//					TerrainType.SaltFlats or TerrainType.Canyon or TerrainType.Mesa
//					=> TerrainCategory.Desert,

//				TerrainType.Snow or TerrainType.Ice or TerrainType.Glacier or 
//					TerrainType.FrozenLake or TerrainType.Permafrost or TerrainType.Blizzard
//					=> TerrainCategory.Frozen,

//				Ids.Terrains.Volcanic.VolcanicPlain or TerrainType.LavaField or TerrainType.AshWastes or 
//					Ids.Terrains.Volcanic.Geothermal or TerrainType.ObsidianField
//					=> TerrainCategory.Volcanic,

//				Ids.Terrains.Underground.Cave or Ids.Terrains.Underground.Cavern or TerrainType.Tunnel or TerrainType.UndergroundLake or 
//					Ids.Terrains.Underground.CrystalCavern or Ids.Terrains.Underground.MushroomForest or TerrainType.LavaTube or 
//					Ids.Terrains.Underground.Mine or Ids.Terrains.Underground.Catacombs
//					=> TerrainCategory.Underground,

//				Ids.Terrains.Settlements.Village or TerrainType.Town or TerrainType.City or TerrainType.Outpost or 
//					TerrainType.Fort or TerrainType.Castle or Ids.Terrains.Settlements.Temple or 
//					Ids.Terrains.Settlements.Tower or TerrainType.Settlement
//					=> TerrainCategory.Settlement,

//				Ids.Terrains.Ruins.OpenRuins or Ids.Terrains.Ruins.AbandonedVillage or Ids.Terrains.Ruins.Graveyard or 
//					Ids.Terrains.Ruins.Battlefield or TerrainType.Shipwreck or TerrainType.Dungeon or 
//					TerrainType.Crypt or TerrainType.Monument
//					=> TerrainCategory.Ruins,

//				Ids.Terrains.Supernatural.Corrupted or Ids.Terrains.Supernatural.Blighted or Ids.Terrains.Supernatural.ShadowRealm or 
//					TerrainType.Wasteland or TerrainType.CursedGround or Ids.Terrains.Supernatural.Magical or 
//					TerrainType.FeyCrossing or Ids.Terrains.Supernatural.LeyLine or Ids.Terrains.Supernatural.VoidTouched
//					=> TerrainCategory.Magical,

//				TerrainType.Fog or Ids.Terrains.Special.Blocked or TerrainType.Portal or TerrainType.Sanctuary or 
//					TerrainType.Arena or Ids.Terrains.Special.Crossroads or TerrainType.Campsite or TerrainType.Wayshrine
//					=> TerrainCategory.Special,

//				_ => TerrainCategory.Special
//			};
//		}
//		/// <summary>
//		/// Gets the color for map display.
//		/// </summary>
//		public UnityEngine.Color GetMapColor() {
//			return terrain switch {
//				// Roads & Paths - Browns
//				Ids.Terrains.Roads.Road => new UnityEngine.Color(0.55f, 0.45f, 0.35f),
//				Ids.Terrains.Roads.Path => new UnityEngine.Color(0.65f, 0.55f, 0.45f),
//				TerrainType.Trail => new UnityEngine.Color(0.6f, 0.5f, 0.4f),
//				TerrainType.Bridge => new UnityEngine.Color(0.5f, 0.4f, 0.3f),

//				// Grasslands - Greens
//				Ids.Terrains.Plains.Grass => new UnityEngine.Color(0.4f, 0.7f, 0.3f),
//				Ids.Terrains.Plains.Hills => new UnityEngine.Color(0.5f, 0.65f, 0.4f),
//				TerrainType.Plains => new UnityEngine.Color(0.55f, 0.75f, 0.35f),
//				TerrainType.Meadow => new UnityEngine.Color(0.5f, 0.8f, 0.45f),
//				TerrainType.Farmland => new UnityEngine.Color(0.6f, 0.7f, 0.3f),

//				// Forests - Dark greens
//				Ids.Terrains.Forests.Forest => new UnityEngine.Color(0.2f, 0.5f, 0.2f),
//				Ids.Terrains.Forests.DeepForest => new UnityEngine.Color(0.1f, 0.35f, 0.1f),
//				Ids.Terrains.Forests.AncientForest => new UnityEngine.Color(0.15f, 0.4f, 0.25f),
//				Ids.Terrains.Forests.DeadForest => new UnityEngine.Color(0.35f, 0.3f, 0.25f),
//				TerrainType.Jungle => new UnityEngine.Color(0.1f, 0.45f, 0.15f),
//				TerrainType.Orchard => new UnityEngine.Color(0.35f, 0.55f, 0.25f),

//				// Mountains - Grays
//				Ids.Terrains.Mountains.Mountain => new UnityEngine.Color(0.5f, 0.5f, 0.5f),
//				TerrainType.Peak => new UnityEngine.Color(0.7f, 0.7f, 0.75f),
//				Ids.Terrains.Mountains.Cliff => new UnityEngine.Color(0.4f, 0.4f, 0.4f),
//				TerrainType.Pass => new UnityEngine.Color(0.55f, 0.5f, 0.45f),
//				Ids.Terrains.Mountains.Highland => new UnityEngine.Color(0.5f, 0.55f, 0.45f),

//				// Water - Blues
//				Ids.Terrains.Water.River => new UnityEngine.Color(0.3f, 0.5f, 0.8f),
//				Ids.Terrains.Water.Lake => new UnityEngine.Color(0.25f, 0.45f, 0.75f),
//				TerrainType.Pond => new UnityEngine.Color(0.35f, 0.55f, 0.7f),
//				TerrainType.Water => new UnityEngine.Color(0.2f, 0.4f, 0.7f),
//				TerrainType.Shallows => new UnityEngine.Color(0.4f, 0.6f, 0.8f),
//				TerrainType.Waterfall => new UnityEngine.Color(0.5f, 0.7f, 0.9f),
//				Ids.Terrains.Water.HotSpring => new UnityEngine.Color(0.4f, 0.6f, 0.7f),

//				// Coastal - Sandy blues
//				Ids.Terrains.Coastal.Beach => new UnityEngine.Color(0.9f, 0.85f, 0.65f),
//				TerrainType.Coast => new UnityEngine.Color(0.7f, 0.75f, 0.6f),
//				TerrainType.TidalFlats => new UnityEngine.Color(0.6f, 0.65f, 0.55f),
//				TerrainType.Reef => new UnityEngine.Color(0.3f, 0.5f, 0.6f),

//				// Wetlands - Murky greens
//				Ids.Terrains.Wetlands.Swamp => new UnityEngine.Color(0.3f, 0.4f, 0.25f),
//				TerrainType.Marsh => new UnityEngine.Color(0.35f, 0.45f, 0.3f),
//				TerrainType.Bog => new UnityEngine.Color(0.25f, 0.3f, 0.2f),
//				TerrainType.Mangrove => new UnityEngine.Color(0.25f, 0.35f, 0.25f),

//				// Desert - Tans/yellows
//				Ids.Terrains.Desert.OpenDesert => new UnityEngine.Color(0.9f, 0.8f, 0.5f),
//				TerrainType.Dunes => new UnityEngine.Color(0.95f, 0.85f, 0.55f),
//				TerrainType.Oasis => new UnityEngine.Color(0.4f, 0.65f, 0.45f),
//				Ids.Terrains.Desert.Badlands => new UnityEngine.Color(0.7f, 0.5f, 0.35f),
//				TerrainType.SaltFlats => new UnityEngine.Color(0.95f, 0.95f, 0.9f),
//				TerrainType.Canyon => new UnityEngine.Color(0.75f, 0.55f, 0.4f),
//				TerrainType.Mesa => new UnityEngine.Color(0.8f, 0.6f, 0.45f),

//				// Cold - White/light blues
//				TerrainType.Snow => new UnityEngine.Color(0.95f, 0.95f, 1f),
//				TerrainType.Ice => new UnityEngine.Color(0.85f, 0.9f, 0.98f),
//				TerrainType.Glacier => new UnityEngine.Color(0.75f, 0.85f, 0.95f),
//				TerrainType.FrozenLake => new UnityEngine.Color(0.7f, 0.8f, 0.9f),
//				TerrainType.Permafrost => new UnityEngine.Color(0.8f, 0.85f, 0.9f),
//				TerrainType.Blizzard => new UnityEngine.Color(0.9f, 0.92f, 0.95f),

//				// Volcanic - Reds/oranges
//				Ids.Terrains.Volcanic.VolcanicPlain => new UnityEngine.Color(0.5f, 0.3f, 0.25f),
//				TerrainType.LavaField => new UnityEngine.Color(0.9f, 0.3f, 0.1f),
//				TerrainType.AshWastes => new UnityEngine.Color(0.4f, 0.4f, 0.4f),
//				Ids.Terrains.Volcanic.Geothermal => new UnityEngine.Color(0.6f, 0.45f, 0.35f),
//				TerrainType.ObsidianField => new UnityEngine.Color(0.15f, 0.15f, 0.2f),

//				// Underground - Dark
//				Ids.Terrains.Underground.Cave => new UnityEngine.Color(0.25f, 0.25f, 0.25f),
//				Ids.Terrains.Underground.Cavern => new UnityEngine.Color(0.3f, 0.3f, 0.3f),
//				TerrainType.Tunnel => new UnityEngine.Color(0.2f, 0.2f, 0.2f),
//				TerrainType.UndergroundLake => new UnityEngine.Color(0.2f, 0.3f, 0.45f),
//				Ids.Terrains.Underground.CrystalCavern => new UnityEngine.Color(0.5f, 0.4f, 0.7f),
//				Ids.Terrains.Underground.MushroomForest => new UnityEngine.Color(0.4f, 0.35f, 0.5f),
//				TerrainType.LavaTube => new UnityEngine.Color(0.35f, 0.2f, 0.15f),
//				Ids.Terrains.Underground.Mine => new UnityEngine.Color(0.35f, 0.3f, 0.25f),
//				Ids.Terrains.Underground.Catacombs => new UnityEngine.Color(0.25f, 0.22f, 0.2f),

//				// Settlements - Warm browns
//				Ids.Terrains.Settlements.Village => new UnityEngine.Color(0.75f, 0.65f, 0.5f),
//				TerrainType.Town => new UnityEngine.Color(0.7f, 0.6f, 0.45f),
//				TerrainType.City => new UnityEngine.Color(0.65f, 0.55f, 0.4f),
//				TerrainType.Outpost => new UnityEngine.Color(0.6f, 0.5f, 0.4f),
//				TerrainType.Fort => new UnityEngine.Color(0.55f, 0.5f, 0.45f),
//				TerrainType.Castle => new UnityEngine.Color(0.5f, 0.5f, 0.5f),
//				Ids.Terrains.Settlements.Temple => new UnityEngine.Color(0.8f, 0.75f, 0.6f),
//				Ids.Terrains.Settlements.Tower => new UnityEngine.Color(0.55f, 0.55f, 0.55f),
//				TerrainType.Settlement => new UnityEngine.Color(0.75f, 0.65f, 0.5f),

//				// Ruins - Grays with hints
//				Ids.Terrains.Ruins.OpenRuins => new UnityEngine.Color(0.45f, 0.45f, 0.4f),
//				Ids.Terrains.Ruins.AbandonedVillage => new UnityEngine.Color(0.5f, 0.48f, 0.42f),
//				Ids.Terrains.Ruins.Graveyard => new UnityEngine.Color(0.4f, 0.42f, 0.45f),
//				Ids.Terrains.Ruins.Battlefield => new UnityEngine.Color(0.45f, 0.4f, 0.35f),
//				TerrainType.Shipwreck => new UnityEngine.Color(0.4f, 0.35f, 0.3f),
//				TerrainType.Dungeon => new UnityEngine.Color(0.3f, 0.28f, 0.32f),
//				TerrainType.Crypt => new UnityEngine.Color(0.35f, 0.33f, 0.35f),
//				TerrainType.Monument => new UnityEngine.Color(0.6f, 0.6f, 0.55f),

//				// Corrupted - Purples/sickly
//				Ids.Terrains.Supernatural.Corrupted => new UnityEngine.Color(0.4f, 0.25f, 0.45f),
//				Ids.Terrains.Supernatural.Blighted => new UnityEngine.Color(0.45f, 0.4f, 0.25f),
//				Ids.Terrains.Supernatural.ShadowRealm => new UnityEngine.Color(0.2f, 0.15f, 0.25f),
//				TerrainType.Wasteland => new UnityEngine.Color(0.5f, 0.45f, 0.4f),
//				TerrainType.CursedGround => new UnityEngine.Color(0.35f, 0.2f, 0.35f),
//				Ids.Terrains.Supernatural.Magical => new UnityEngine.Color(0.5f, 0.4f, 0.7f),
//				TerrainType.FeyCrossing => new UnityEngine.Color(0.6f, 0.7f, 0.55f),
//				Ids.Terrains.Supernatural.LeyLine => new UnityEngine.Color(0.45f, 0.55f, 0.75f),
//				Ids.Terrains.Supernatural.VoidTouched => new UnityEngine.Color(0.15f, 0.1f, 0.2f),

//				// Special
//				TerrainType.Fog => new UnityEngine.Color(0.6f, 0.6f, 0.65f, 0.7f),
//				Ids.Terrains.Special.Blocked => new UnityEngine.Color(0.2f, 0.2f, 0.2f),
//				TerrainType.Portal => new UnityEngine.Color(0.6f, 0.3f, 0.8f),
//				TerrainType.Sanctuary => new UnityEngine.Color(0.9f, 0.85f, 0.6f),
//				TerrainType.Arena => new UnityEngine.Color(0.7f, 0.5f, 0.4f),
//				Ids.Terrains.Special.Crossroads => new UnityEngine.Color(0.6f, 0.55f, 0.5f),
//				TerrainType.Campsite => new UnityEngine.Color(0.7f, 0.6f, 0.45f),
//				TerrainType.Wayshrine => new UnityEngine.Color(0.7f, 0.75f, 0.85f),

//				_ => UnityEngine.Color.magenta
//			};
//		}
//		/// <summary>
//		/// Gets resource node IDs that can spawn on this terrain.
//		/// </summary>
//		public ResourceProto.ID[] GetGatherableResources() {
//			return terrain switch {
//				// Grasslands
//				Ids.Terrains.Plains.Grass => [
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Foraging.BerryBush,
//					Ids.Resources.Hunting.RabbitWarren,
//					Ids.Resources.Foraging.BirdNest,
//					Ids.Resources.Environment.Anthill
//				],
//				Ids.Terrains.Plains.Hills => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Mining.CopperDeposit,
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Hunting.RabbitWarren
//				],
//				TerrainType.Plains => [
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Hunting.RabbitWarren,
//					Ids.Resources.Hunting.DeerTrail
//				],
//				TerrainType.Meadow => [
//					Ids.Resources.Herbalism.WildflowerMeadow,
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Foraging.HoneyBeehive,
//					Ids.Resources.Foraging.BerryBush
//				],
//				TerrainType.Farmland => [
//					Ids.Resources.Foraging.WildVegetables,
//					Ids.Resources.Foraging.AppleTree
//				],

//				// Forests
//				Ids.Terrains.Forests.Forest => [
//					Ids.Resources.Woodcutting.OakTree,
//					Ids.Resources.Woodcutting.PineTree,
//					Ids.Resources.Woodcutting.BirchTree,
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Foraging.BerryBush,
//					Ids.Resources.Foraging.MushroomCluster,
//					Ids.Resources.Hunting.RabbitWarren,
//					Ids.Resources.Hunting.DeerTrail,
//					Ids.Resources.Hunting.FoxDen,
//					Ids.Resources.Hunting.OwlNest,
//					Ids.Resources.Environment.FallenLog
//				],
//				Ids.Terrains.Forests.DeepForest => [
//					Ids.Resources.Woodcutting.OakTree,
//					Ids.Resources.Woodcutting.ElderTree,
//					Ids.Resources.Woodcutting.IronwoodTree,
//					Ids.Resources.Herbalism.MoonbloomPatch,
//					Ids.Resources.Herbalism.Nightshade,
//					Ids.Resources.Foraging.MushroomCluster,
//					Ids.Resources.Foraging.TruffleGround,
//					Ids.Resources.Hunting.BoarDen,
//					Ids.Resources.Hunting.WolfPack,
//					Ids.Resources.Hunting.BearCave,
//					Ids.Resources.Hunting.GiantSpiderNest
//				],
//				Ids.Terrains.Forests.AncientForest => [
//					Ids.Resources.Woodcutting.ElderTree,
//					Ids.Resources.Herbalism.MoonbloomPatch,
//					Ids.Resources.Herbalism.MandrakeRoot,
//					Ids.Resources.Foraging.MushroomCluster,
//					Ids.Resources.Special.ManaWell,
//					Ids.Resources.Special.AncientShrine
//				],
//				Ids.Terrains.Forests.DeadForest => [
//					Ids.Resources.Woodcutting.DeadTree,
//					Ids.Resources.Woodcutting.CorruptedTree,
//					Ids.Resources.Special.UndeadRemains,
//					Ids.Resources.Environment.FallenLog
//				],
//				TerrainType.Jungle => [
//					Ids.Resources.Woodcutting.OakTree,
//					Ids.Resources.Herbalism.HerbPatch,
//					Ids.Resources.Foraging.BerryBush,
//					Ids.Resources.Hunting.GiantSpiderNest,
//					Ids.Resources.Foraging.HoneyBeehive
//				],
//				TerrainType.Orchard => [
//					Ids.Resources.Foraging.AppleTree,
//					Ids.Resources.Foraging.NutTree,
//					Ids.Resources.Foraging.GrapeVine
//				],

//				// Mountains
//				Ids.Terrains.Mountains.Mountain => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Mining.CopperDeposit,
//					Ids.Resources.Mining.SilverVein,
//					Ids.Resources.Mining.GoldVein,
//					Ids.Resources.Mining.CoalSeam,
//					Ids.Resources.Herbalism.FrostLichen,
//					Ids.Resources.Hunting.WyvernNest,
//					Ids.Resources.Environment.IceBlock,
//					Ids.Resources.Special.DragonBones
//				],
//				TerrainType.Peak => [
//					Ids.Resources.Mining.MithrilVein,
//					Ids.Resources.Mining.CrystalFormation,
//					Ids.Resources.Environment.IceBlock,
//					Ids.Resources.Environment.Icicles,
//					Ids.Resources.Special.FallenStar
//				],
//				Ids.Terrains.Mountains.Highland => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Herbalism.HerbPatch
//				],
//				TerrainType.Pass => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein
//				],

//				// Water
//				Ids.Terrains.Water.River => [
//					Ids.Resources.Fishing.RiverFishingSpot,
//					Ids.Resources.Mining.ClayDeposit,
//					Ids.Resources.Mining.SandPocket,
//					Ids.Resources.Woodcutting.WillowTree,
//					Ids.Resources.Environment.Waterfall
//				],
//				Ids.Terrains.Water.Lake => [
//					Ids.Resources.Fishing.LakeFishingSpot,
//					Ids.Resources.Mining.ClayDeposit
//				],
//				TerrainType.Pond => [
//					Ids.Resources.Fishing.PondFishingSpot
//				],
//				TerrainType.Shallows => [
//					Ids.Resources.Fishing.RiverFishingSpot,
//					Ids.Resources.Foraging.ClamBed
//				],
//				TerrainType.Waterfall => [
//					Ids.Resources.Environment.Waterfall,
//					Ids.Resources.Environment.FreshwaterSpring
//				],
//				Ids.Terrains.Water.HotSpring => [
//					Ids.Resources.Environment.MineralHotSpring
//				],

//				// Coastal
//				Ids.Terrains.Coastal.Beach => [
//					Ids.Resources.Foraging.SeashellBeach,
//					Ids.Resources.Foraging.ClamBed,
//					Ids.Resources.Foraging.OysterRocks,
//					Ids.Resources.Mining.SandPocket,
//					Ids.Resources.Special.ShipwreckDebris,
//					Ids.Resources.Special.BuriedTreasure
//				],
//				TerrainType.Coast => [
//					Ids.Resources.Foraging.SeashellBeach,
//					Ids.Resources.Foraging.OysterRocks
//				],
//				TerrainType.TidalFlats => [
//					Ids.Resources.Foraging.ClamBed,
//					Ids.Resources.Foraging.SeashellBeach
//				],

//				// Wetlands
//				Ids.Terrains.Wetlands.Swamp => [
//					Ids.Resources.Fishing.SwampFishingSpot,
//					Ids.Resources.Herbalism.SwampMoss,
//					Ids.Resources.Herbalism.Nightshade,
//					Ids.Resources.Woodcutting.WillowTree,
//					Ids.Resources.Woodcutting.DeadTree,
//					Ids.Resources.Woodcutting.CorruptedTree,
//					Ids.Resources.Mining.ClayDeposit
//				],
//				TerrainType.Marsh => [
//					Ids.Resources.Herbalism.SwampMoss,
//					Ids.Resources.Fishing.SwampFishingSpot
//				],
//				TerrainType.Bog => [
//					Ids.Resources.Herbalism.SwampMoss,
//					Ids.Resources.Mining.ClayDeposit
//				],
//				TerrainType.Mangrove => [
//					Ids.Resources.Foraging.OysterRocks,
//					Ids.Resources.Fishing.SwampFishingSpot
//				],

//				// Desert
//				Ids.Terrains.Desert.OpenDesert => [
//					Ids.Resources.Mining.SandPocket,
//					Ids.Resources.Environment.CactusPlant,
//					Ids.Resources.Environment.DesertRuins,
//					Ids.Resources.Special.BuriedTreasure,
//					Ids.Resources.Special.FallenStar
//				],
//				TerrainType.Dunes => [
//					Ids.Resources.Mining.SandPocket
//				],
//				TerrainType.Oasis => [
//					Ids.Resources.Environment.Oasis,
//					Ids.Resources.Fishing.PondFishingSpot
//				],
//				Ids.Terrains.Desert.Badlands => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.ClayDeposit
//				],
//				TerrainType.Canyon => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Mining.CopperDeposit
//				],

//				// Cold
//				TerrainType.Snow => [
//					Ids.Resources.Environment.SnowPile,
//					Ids.Resources.Environment.IceBlock,
//					Ids.Resources.Herbalism.FrostLichen,
//					Ids.Resources.Woodcutting.FrozenPine,
//					Ids.Resources.Hunting.WolfPack
//				],
//				TerrainType.Ice => [
//					Ids.Resources.Environment.IceBlock,
//					Ids.Resources.Environment.Icicles
//				],
//				TerrainType.Glacier => [
//					Ids.Resources.Environment.IceBlock
//				],
//				TerrainType.FrozenLake => [
//					Ids.Resources.Fishing.IceFishingHole,
//					Ids.Resources.Environment.IceBlock
//				],

//				// Volcanic
//				Ids.Terrains.Volcanic.VolcanicPlain => [
//					Ids.Resources.Mining.ObsidianFormation,
//					Ids.Resources.Mining.SulfurDeposit,
//					Ids.Resources.Environment.LavaRock,
//					Ids.Resources.Environment.GeyserVent,
//					Ids.Resources.Herbalism.FireBloom,
//					Ids.Resources.Fishing.LavaFishingSpot
//				],
//				TerrainType.AshWastes => [
//					Ids.Resources.Environment.AshPile,
//					Ids.Resources.Environment.LavaRock
//				],
//				Ids.Terrains.Volcanic.Geothermal => [
//					Ids.Resources.Environment.GeyserVent,
//					Ids.Resources.Environment.MineralHotSpring,
//					Ids.Resources.Mining.SulfurDeposit
//				],
//				TerrainType.ObsidianField => [
//					Ids.Resources.Mining.ObsidianFormation
//				],

//				// Underground
//				Ids.Terrains.Underground.Cave => [
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Mining.CopperDeposit,
//					Ids.Resources.Mining.CoalSeam,
//					Ids.Resources.Herbalism.GlowMoss,
//					Ids.Resources.Foraging.MushroomCluster,
//					Ids.Resources.Hunting.GiantSpiderNest,
//					Ids.Resources.Environment.CobwebCorner
//				],
//				Ids.Terrains.Underground.Cavern => [
//					Ids.Resources.Mining.CrystalFormation,
//					Ids.Resources.Mining.SilverVein,
//					Ids.Resources.Mining.GoldVein,
//					Ids.Resources.Herbalism.GlowMoss
//				],
//				Ids.Terrains.Underground.CrystalCavern => [
//					Ids.Resources.Mining.CrystalFormation,
//					Ids.Resources.Special.ManaWell
//				],
//				Ids.Terrains.Underground.MushroomForest => [
//					Ids.Resources.Foraging.MushroomCluster,
//					Ids.Resources.Herbalism.GlowMoss
//				],
//				Ids.Terrains.Underground.Mine => [
//					Ids.Resources.Mining.IronVein,
//					Ids.Resources.Mining.CopperDeposit,
//					Ids.Resources.Mining.SilverVein,
//					Ids.Resources.Mining.GoldVein,
//					Ids.Resources.Mining.CoalSeam
//				],
//				Ids.Terrains.Underground.Catacombs => [
//					Ids.Resources.Special.UndeadRemains,
//					Ids.Resources.Environment.GraveSite,
//					Ids.Resources.Environment.CobwebCorner
//				],

//				// Ruins
//				Ids.Terrains.Ruins.OpenRuins => [
//					Ids.Resources.Special.AncientShrine,
//					Ids.Resources.Special.BuriedTreasure,
//					Ids.Resources.Mining.StoneOutcrop,
//					Ids.Resources.Herbalism.GlowMoss,
//					Ids.Resources.Herbalism.MoonbloomPatch,
//					Ids.Resources.Hunting.GiantSpiderNest,
//					Ids.Resources.Environment.CobwebCorner
//				],
//				Ids.Terrains.Ruins.AbandonedVillage => [
//					Ids.Resources.Special.AbandonedCart,
//					Ids.Resources.Environment.AbandonedCampfire
//				],
//				Ids.Terrains.Ruins.Graveyard => [
//					Ids.Resources.Environment.GraveSite,
//					Ids.Resources.Special.UndeadRemains,
//					Ids.Resources.Herbalism.MandrakeRoot,
//					Ids.Resources.Herbalism.Nightshade
//				],
//				Ids.Terrains.Ruins.Battlefield => [
//					Ids.Resources.Special.UndeadRemains
//				],
//				TerrainType.Shipwreck => [
//					Ids.Resources.Special.ShipwreckDebris
//				],
//				TerrainType.Dungeon => [
//					Ids.Resources.Special.UndeadRemains,
//					Ids.Resources.Hunting.GiantSpiderNest,
//					Ids.Resources.Environment.CobwebCorner,
//					Ids.Resources.Special.BuriedTreasure
//				],
//				TerrainType.Crypt => [
//					Ids.Resources.Special.UndeadRemains,
//					Ids.Resources.Environment.GraveSite
//				],

//				// Corrupted & Magical
//				Ids.Terrains.Supernatural.Corrupted => [
//					Ids.Resources.Woodcutting.CorruptedTree,
//					Ids.Resources.Special.FogFragment,
//					Ids.Resources.Special.DemonPortal
//				],
//				TerrainType.Wasteland => [
//					Ids.Resources.Special.FogFragment,
//					Ids.Resources.Special.DragonBones
//				],
//				Ids.Terrains.Supernatural.Magical => [
//					Ids.Resources.Special.ManaWell,
//					Ids.Resources.Mining.CrystalFormation
//				],
//				TerrainType.FeyCrossing => [
//					Ids.Resources.Herbalism.MoonbloomPatch,
//					Ids.Resources.Special.ManaWell
//				],
//				Ids.Terrains.Supernatural.LeyLine => [
//					Ids.Resources.Special.ManaWell,
//					Ids.Resources.Mining.CrystalFormation
//				],
//				Ids.Terrains.Supernatural.VoidTouched => [
//					Ids.Resources.Special.TimeCrack,
//					Ids.Resources.Special.FogFragment
//				],

//				// Special
//				Ids.Terrains.Special.Crossroads => [
//					Ids.Resources.Special.AbandonedCart
//				],
//				TerrainType.Campsite => [
//					Ids.Resources.Environment.AbandonedCampfire
//				],

//				_ => []
//			};
//		}
//		/// <summary>
//		/// Gets whether this terrain provides natural rest bonuses.
//		/// </summary>
//		public bool ProvidesRestBonus() {
//			return terrain switch {
//				Ids.Terrains.Settlements.Village => true,
//				TerrainType.Town => true,
//				TerrainType.City => true,
//				Ids.Terrains.Settlements.Temple => true,
//				TerrainType.Sanctuary => true,
//				TerrainType.Campsite => true,
//				TerrainType.Oasis => true,
//				Ids.Terrains.Water.HotSpring => true,
//				TerrainType.Meadow => true,
//				_ => false
//			};
//		}
//		/// <summary>
//		/// Gets the rest quality multiplier for this terrain.
//		/// 1.0 = normal rest, higher = better recovery.
//		/// </summary>
//		public float GetRestQualityMultiplier() {
//			return terrain switch {
//				// Excellent rest
//				TerrainType.City => 1.5f,
//				TerrainType.Town => 1.4f,
//				Ids.Terrains.Settlements.Village => 1.3f,
//				Ids.Terrains.Settlements.Temple => 1.5f,
//				TerrainType.Sanctuary => 2.0f,
//				Ids.Terrains.Water.HotSpring => 1.6f,

//				// Good rest
//				TerrainType.Campsite => 1.2f,
//				TerrainType.Oasis => 1.3f,
//				TerrainType.Meadow => 1.1f,
//				TerrainType.Fort => 1.2f,
//				TerrainType.Castle => 1.3f,
//				TerrainType.Outpost => 1.1f,

//				// Normal rest
//				Ids.Terrains.Roads.Road => 1.0f,
//				Ids.Terrains.Roads.Path => 1.0f,
//				Ids.Terrains.Plains.Grass => 1.0f,
//				Ids.Terrains.Forests.Forest => 0.9f,
//				TerrainType.Plains => 1.0f,
//				Ids.Terrains.Coastal.Beach => 1.0f,

//				// Poor rest
//				Ids.Terrains.Forests.DeepForest => 0.8f,
//				Ids.Terrains.Plains.Hills => 0.85f,
//				Ids.Terrains.Mountains.Mountain => 0.7f,
//				Ids.Terrains.Underground.Cave => 0.8f,
//				Ids.Terrains.Desert.OpenDesert => 0.7f,
//				TerrainType.Snow => 0.6f,

//				// Very poor rest
//				Ids.Terrains.Wetlands.Swamp => 0.5f,
//				TerrainType.Bog => 0.4f,
//				Ids.Terrains.Volcanic.VolcanicPlain => 0.4f,
//				TerrainType.Blizzard => 0.3f,
//				Ids.Terrains.Supernatural.Corrupted => 0.3f,
//				Ids.Terrains.Ruins.Graveyard => 0.4f,
//				Ids.Terrains.Underground.Catacombs => 0.3f,

//				// Impossible to rest well
//				TerrainType.LavaField => 0.1f,
//				Ids.Terrains.Supernatural.VoidTouched => 0.1f,
//				Ids.Terrains.Supernatural.ShadowRealm => 0.2f,

//				_ => 0.8f
//			};
//		}
//	}

//	/// <summary>
//	/// Helper to split camel case into words.
//	/// </summary>
//	private static string SplitCamelCase(string input) {
//		if (string.IsNullOrEmpty(input)) return input;

//		var result = new System.Text.StringBuilder();
//		for (int i = 0; i < input.Length; i++) {
//			if (i > 0 && char.IsUpper(input[i])) {
//				result.Append(' ');
//			}
//			result.Append(input[i]);
//		}
//		return result.ToString();
//	}
//}

