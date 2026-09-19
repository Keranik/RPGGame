namespace RPGGame.Core.Expedition;

///// <summary>
///// Types of terrain that affect movement, encounters, and resource spawning.
///// </summary>
//[Obsolete("Use TerrainProto.ID from Ids.Terrains instead. All terrain data is now data-driven via TerrainProto.")]
//public enum TerrainType {
//	// ═══════════════════════════════════════════════════════════════
//	// ROADS & PATHS
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Paved road - fastest travel, safest.</summary>
//	Road,

//	/// <summary>Dirt path - slightly slower than road.</summary>
//	Path,

//	/// <summary>Trail - narrow path through wilderness.</summary>
//	Trail,

//	/// <summary>Bridge - crossing over water or chasm.</summary>
//	Bridge,

//	// ═══════════════════════════════════════════════════════════════
//	// GRASSLANDS & PLAINS
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Open grassland - easy travel.</summary>
//	Grass,

//	/// <summary>Rolling hills - slower movement, good visibility.</summary>
//	Hills,

//	/// <summary>Open plains - fast travel, exposed.</summary>
//	Plains,

//	/// <summary>Meadow - grassland with flowers, good foraging.</summary>
//	Meadow,

//	/// <summary>Farmland - cultivated fields near settlements.</summary>
//	Farmland,

//	// ═══════════════════════════════════════════════════════════════
//	// FORESTS & WOODLANDS
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Light forest - slower, more cover.</summary>
//	Forest,

//	/// <summary>Dense forest - very slow, dangerous, rich resources.</summary>
//	DeepForest,

//	/// <summary>Ancient forest - magical, rare resources, spirits.</summary>
//	AncientForest,

//	/// <summary>Dead forest - corrupted or burned, hazardous.</summary>
//	DeadForest,

//	/// <summary>Jungle - dense tropical forest, very slow.</summary>
//	Jungle,

//	/// <summary>Orchard - cultivated fruit trees.</summary>
//	Orchard,

//	// ═══════════════════════════════════════════════════════════════
//	// MOUNTAINS & HIGHLANDS
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Mountain terrain - very slow, requires equipment.</summary>
//	Mountain,

//	/// <summary>Mountain peak - extreme conditions, rare resources.</summary>
//	Peak,

//	/// <summary>Cliff face - impassable without climbing gear.</summary>
//	Cliff,

//	/// <summary>Mountain pass - navigable route through mountains.</summary>
//	Pass,

//	/// <summary>Highland - elevated plateau, windy.</summary>
//	Highland,

//	// ═══════════════════════════════════════════════════════════════
//	// WATER FEATURES
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>River - may need bridge or ford to cross.</summary>
//	River,

//	/// <summary>Lake - large body of fresh water.</summary>
//	Lake,

//	/// <summary>Pond - small body of water, easy to navigate around.</summary>
//	Pond,

//	/// <summary>Ocean/deep water - impassable without boat.</summary>
//	Water,

//	/// <summary>Shallow water - wadeable, slows movement.</summary>
//	Shallows,

//	/// <summary>Waterfall - scenic, sometimes hides caves.</summary>
//	Waterfall,

//	/// <summary>Hot spring - healing properties, rest bonus.</summary>
//	HotSpring,

//	// ═══════════════════════════════════════════════════════════════
//	// COASTAL
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Sandy beach - coastal area, shells and driftwood.</summary>
//	Beach,

//	/// <summary>Rocky coast - difficult terrain, tide pools.</summary>
//	Coast,

//	/// <summary>Tidal flats - changes with tides, clams and crabs.</summary>
//	TidalFlats,

//	/// <summary>Coral reef - underwater, requires diving.</summary>
//	Reef,

//	// ═══════════════════════════════════════════════════════════════
//	// WETLANDS
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Swamp - slow movement, morale penalty, disease risk.</summary>
//	Swamp,

//	/// <summary>Marsh - wet grassland, difficult terrain.</summary>
//	Marsh,

//	/// <summary>Bog - treacherous, risk of sinking.</summary>
//	Bog,

//	/// <summary>Mangrove - coastal swamp with tangled roots.</summary>
//	Mangrove,

//	// ═══════════════════════════════════════════════════════════════
//	// ARID & DESERT
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Desert - requires water management, heat damage.</summary>
//	Desert,

//	/// <summary>Dunes - shifting sand, very slow travel.</summary>
//	Dunes,

//	/// <summary>Oasis - water source in desert, rest area.</summary>
//	Oasis,

//	/// <summary>Badlands - eroded terrain, difficult navigation.</summary>
//	Badlands,

//	/// <summary>Salt flats - blinding glare, no resources.</summary>
//	SaltFlats,

//	/// <summary>Canyon - deep ravine, limited paths.</summary>
//	Canyon,

//	/// <summary>Mesa - flat-topped elevation in desert.</summary>
//	Mesa,

//	// ═══════════════════════════════════════════════════════════════
//	// COLD & FROZEN
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Snow/tundra - cold damage without protection.</summary>
//	Snow,

//	/// <summary>Ice field - slippery, risk of falling.</summary>
//	Ice,

//	/// <summary>Glacier - slow movement, crevasse danger.</summary>
//	Glacier,

//	/// <summary>Frozen lake - may crack under weight.</summary>
//	FrozenLake,

//	/// <summary>Permafrost - frozen ground, hard to dig.</summary>
//	Permafrost,

//	/// <summary>Blizzard zone - extreme cold, low visibility.</summary>
//	Blizzard,

//	// ═══════════════════════════════════════════════════════════════
//	// VOLCANIC & FIRE
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Volcanic terrain - heat damage, rare minerals.</summary>
//	Volcanic,

//	/// <summary>Lava field - extremely dangerous, fire damage.</summary>
//	LavaField,

//	/// <summary>Ash wastes - volcanic aftermath, poor visibility.</summary>
//	AshWastes,

//	/// <summary>Geothermal area - hot springs and geysers.</summary>
//	Geothermal,

//	/// <summary>Obsidian fields - sharp volcanic glass terrain.</summary>
//	ObsidianField,

//	// ═══════════════════════════════════════════════════════════════
//	// UNDERGROUND
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Cave entrance - leads to underground.</summary>
//	Cave,

//	/// <summary>Cavern - large underground chamber.</summary>
//	Cavern,

//	/// <summary>Tunnel - narrow underground passage.</summary>
//	Tunnel,

//	/// <summary>Underground lake - subterranean water.</summary>
//	UndergroundLake,

//	/// <summary>Crystal cavern - magical crystals, light sources.</summary>
//	CrystalCavern,

//	/// <summary>Mushroom forest - giant fungi, bioluminescent.</summary>
//	MushroomForest,

//	/// <summary>Lava tube - volcanic tunnel, extreme heat.</summary>
//	LavaTube,

//	/// <summary>Mine - excavated tunnels, ore deposits.</summary>
//	Mine,

//	/// <summary>Catacombs - underground burial chambers.</summary>
//	Catacombs,

//	// ═══════════════════════════════════════════════════════════════
//	// SETTLEMENTS & STRUCTURES
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Village or settlement - safe zone, services.</summary>
//	Village,

//	/// <summary>Town - larger settlement, more services.</summary>
//	Town,

//	/// <summary>City - major settlement, full services.</summary>
//	City,

//	/// <summary>Outpost - small defensive structure.</summary>
//	Outpost,

//	/// <summary>Fort - military installation.</summary>
//	Fort,

//	/// <summary>Castle - fortified noble residence.</summary>
//	Castle,

//	/// <summary>Temple - religious structure, healing.</summary>
//	Temple,

//	/// <summary>Tower - wizard tower or watchtower.</summary>
//	Tower,

//	/// <summary>Settlement - generic inhabited area.</summary>
//	Settlement,

//	// ═══════════════════════════════════════════════════════════════
//	// RUINS & ABANDONED
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Ruins - explorable ancient structures.</summary>
//	Ruins,

//	/// <summary>Abandoned village - deserted settlement.</summary>
//	AbandonedVillage,

//	/// <summary>Graveyard - burial ground, undead activity.</summary>
//	Graveyard,

//	/// <summary>Battlefield - old war site, ghosts and relics.</summary>
//	Battlefield,

//	/// <summary>Shipwreck - wrecked vessel, salvage.</summary>
//	Shipwreck,

//	/// <summary>Dungeon - dangerous underground complex.</summary>
//	Dungeon,

//	/// <summary>Crypt - underground tomb.</summary>
//	Crypt,

//	/// <summary>Ancient monument - standing stones, magical.</summary>
//	Monument,

//	// ═══════════════════════════════════════════════════════════════
//	// CORRUPTED & MAGICAL
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Corrupted land - tainted by dark magic.</summary>
//	Corrupted,

//	/// <summary>Blighted - diseased and dying land.</summary>
//	Blighted,

//	/// <summary>Shadow realm - partial overlay with shadow plane.</summary>
//	ShadowRealm,

//	/// <summary>Wasteland - destroyed, lifeless terrain.</summary>
//	Wasteland,

//	/// <summary>Cursed ground - negative magical effects.</summary>
//	CursedGround,

//	/// <summary>Magical - infused with arcane energy.</summary>
//	Magical,

//	/// <summary>Fey crossing - thin barrier to fairy realm.</summary>
//	FeyCrossing,

//	/// <summary>Ley line - magical power concentration.</summary>
//	LeyLine,

//	/// <summary>Void touched - reality is unstable.</summary>
//	VoidTouched,

//	// ═══════════════════════════════════════════════════════════════
//	// SPECIAL
//	// ═══════════════════════════════════════════════════════════════

//	/// <summary>Fog boundary - edge of explored area.</summary>
//	Fog,

//	/// <summary>Blocked/impassable terrain.</summary>
//	Blocked,

//	/// <summary>Portal - magical gateway.</summary>
//	Portal,

//	/// <summary>Sanctuary - protected safe zone.</summary>
//	Sanctuary,

//	/// <summary>Arena - combat area.</summary>
//	Arena,

//	/// <summary>Crossroads - intersection, often magical.</summary>
//	Crossroads,

//	/// <summary>Campsite - established rest area.</summary>
//	Campsite,

//	/// <summary>Wayshrine - fast travel point.</summary>
//	Wayshrine
//}
