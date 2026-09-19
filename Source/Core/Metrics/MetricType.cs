namespace RPGGame.Core.Metrics;

/// <summary>
/// All trackable metrics in the game.
/// </summary>
public enum MetricType {
	// ═══════════════════════════════════════════════════════════════
	// COMBAT METRICS
	// ═══════════════════════════════════════════════════════════════

	AttacksAttempted,
	AttacksHit,
	AttacksMissed,
	CriticalHits,
	CriticalHitsReceived,
	DamageDealt,
	DamageReceived,
	/// <summary>Damage blocked by armor/shields.</summary>
	DamageBlocked,
	/// <summary>Damage avoided through dodge/evasion.</summary>
	DamageDodged,
	/// <summary>Healing done to self or allies.</summary>
	HealingDone,
	/// <summary>Healing received from others.</summary>
	HealingReceived,
	/// <summary>Overhealing (healing beyond max HP).</summary>
	OverhealingDone,
	EnemiesDefeated,
	BossesDefeated,
	MiniBossesDefeated,
	EliteEnemiesDefeated,
	/// <summary>Times knocked to 0 HP but not killed.</summary>
	TimesDownedNotDead,
	CombatsWon,
	CombatsFled,
	/// <summary>Combats avoided through stealth/diplomacy.</summary>
	CombatsAvoided,
	CombatTurnsTaken,
	LongestCombatTurns,
	QuickestCombatTurns,
	/// <summary>Total combats initiated.</summary>
	CombatsInitiated,
	/// <summary>Combats where player struck first.</summary>
	AmbushesSuccessful,
	/// <summary>Combats where enemies struck first.</summary>
	TimesAmbushed,
	/// <summary>Enemies killed in a single hit.</summary>
	OneHitKills,
	/// <summary>Killing blows dealt.</summary>
	KillingBlows,
	/// <summary>Times reduced to 1 HP and survived.</summary>
	CloseCallsSurvived,
	/// <summary>Consecutive hits without missing.</summary>
	LongestHitStreak,
	/// <summary>Consecutive misses.</summary>
	LongestMissStreak,
	/// <summary>Times status effects were applied to enemies.</summary>
	StatusEffectsApplied,
	/// <summary>Times status effects were received.</summary>
	StatusEffectsReceived,
	/// <summary>Times status effects were cleansed.</summary>
	StatusEffectsCleansed,
	/// <summary>Damage dealt by status effects (poison, burn, etc).</summary>
	DamageFromStatusEffects,
	/// <summary>Times defended successfully.</summary>
	SuccessfulDefends,
	/// <summary>Damage mitigated by defending.</summary>
	DamageMitigatedByDefending,

	// ═══════════════════════════════════════════════════════════════
	// RESOURCE METRICS
	// ═══════════════════════════════════════════════════════════════

	GoldEarned,
	GoldSpent,
	GoldSpentOnEquipment,
	GoldSpentOnConsumables,
	GoldSpentOnUpgrades,
	/// <summary>Gold lost through theft, death, etc.</summary>
	GoldLost,
	/// <summary>Gold given away as bribes or gifts.</summary>
	GoldGivenAway,
	/// <summary>Highest gold held at once.</summary>
	PeakGoldHeld,
	ItemsPickedUp,
	ItemsCollected,
	ItemsUsed,
	ItemsCrafted,
	ItemsSold,
	ItemsDiscarded,
	ItemsPurchased,
	ItemsEquipped,
	/// <summary>Items broken/destroyed.</summary>
	ItemsBroken,
	/// <summary>Items repaired.</summary>
	ItemsRepaired,
	/// <summary>Items upgraded/enhanced.</summary>
	ItemsUpgraded,
	/// <summary>Unique items found.</summary>
	UniqueItemsFound,
	/// <summary>Legendary items found.</summary>
	LegendaryItemsFound,
	/// <summary>Epic items found.</summary>
	EpicItemsFound,
	/// <summary>Rare items found.</summary>
	RareItemsFound,
	FoodConsumed,
	PotionsConsumed,
	/// <summary>Health potions specifically.</summary>
	HealthPotionsConsumed,
	/// <summary>Mana potions specifically.</summary>
	ManaPotionsConsumed,
	/// <summary>Scrolls used.</summary>
	ScrollsUsed,
	ResourcesGathered,
	WoodGathered,
	StoneGathered,
	OreGathered,
	HerbsGathered,
	FishCaught,
	AnimalsHunted,
	/// <summary>Rare resources gathered.</summary>
	RareResourcesGathered,
	/// <summary>Total weight of items carried (cumulative).</summary>
	TotalWeightCarried,
	/// <summary>Times inventory was full.</summary>
	TimesInventoryFull,

	// ═══════════════════════════════════════════════════════════════
	// EXPLORATION METRICS
	// ═══════════════════════════════════════════════════════════════

	TilesTraveled,
	TilesExplored,
	/// <summary>Tiles revealed by clearing fog.</summary>
	TilesRevealed,
	/// <summary>New tiles discovered (first time visited).</summary>
	NewTilesDiscovered,
	DistanceTraveled,
	FurthestDistanceSingleRun,
	FurthestDistanceEver,
	FurthestDistance,
	LandmarksDiscovered,
	SecretsFound,
	HiddenAreasFound,
	ChestsOpened,
	/// <summary>Locked chests opened.</summary>
	LockedChestsOpened,
	/// <summary>Mimics encountered.</summary>
	MimicsEncountered,
	DoorsUnlocked,
	/// <summary>Locked doors picked.</summary>
	LocksPickedSuccessfully,
	/// <summary>Failed lockpick attempts.</summary>
	LockpicksFailed,
	/// <summary>Lockpicks broken.</summary>
	LockpicksBroken,
	TrapsTriggered,
	TrapsDisarmed,
	/// <summary>Traps spotted before triggering.</summary>
	TrapsSpotted,
	CampsSetUp,
	HoursRested,
	/// <summary>Full rests (8+ hours).</summary>
	FullRestsCompleted,
	/// <summary>Short rests completed.</summary>
	ShortRestsCompleted,
	PathBranchesEncountered,
	DeadEndsDiscovered,
	ExpeditionsStarted,
	/// <summary>Expeditions completed successfully.</summary>
	ExpeditionsCompleted,
	/// <summary>Expeditions abandoned/retreated.</summary>
	ExpeditionsAbandoned,
	/// <summary>Dungeons entered.</summary>
	DungeonsEntered,
	/// <summary>Dungeons cleared.</summary>
	DungeonsCleared,
	/// <summary>Dungeon floors explored.</summary>
	DungeonFloorsExplored,
	/// <summary>Shortcuts discovered.</summary>
	ShortcutsDiscovered,
	/// <summary>Fast travel points unlocked.</summary>
	FastTravelPointsUnlocked,
	/// <summary>Times fast traveled.</summary>
	TimesFastTraveled,
	/// <summary>Regions fully explored.</summary>
	RegionsFullyExplored,
	/// <summary>Points of interest discovered.</summary>
	PointsOfInterestDiscovered,

	// ═══════════════════════════════════════════════════════════════
	// CHOICE & EVENT METRICS
	// ═══════════════════════════════════════════════════════════════

	EventsEncountered,
	/// <summary>Unique events encountered.</summary>
	UniqueEventsEncountered,
	/// <summary>Story events completed.</summary>
	StoryEventsCompleted,
	/// <summary>Random events encountered.</summary>
	RandomEventsEncountered,
	ChoicesMade,
	PeacefulChoices,
	AggressiveChoices,
	/// <summary>Neutral/diplomatic choices.</summary>
	NeutralChoices,
	/// <summary>Risky choices taken.</summary>
	RiskyChoicesTaken,
	/// <summary>Safe choices taken.</summary>
	SafeChoicesTaken,
	BribesAttempted,
	BribesSuccessful,
	StealthAttempts,
	StealthSuccesses,
	PersuasionAttempts,
	PersuasionSuccesses,
	IntimidationAttempts,
	IntimidationSuccesses,
	/// <summary>Deception attempts.</summary>
	DeceptionAttempts,
	/// <summary>Successful deceptions.</summary>
	DeceptionSuccesses,
	SkillChecksAttempted,
	SkillChecksPassed,
	SkillChecksFailed,
	/// <summary>Skill checks passed by exactly 1.</summary>
	SkillChecksBarelySurvived,
	/// <summary>Skill checks failed by exactly 1.</summary>
	SkillChecksBarelyFailed,
	NPCsHelped,
	NPCsHarmed,
	NPCsIgnored,
	/// <summary>NPCs befriended.</summary>
	NPCsBefriended,
	/// <summary>NPCs betrayed.</summary>
	NPCsBetrayed,
	/// <summary>Companions recruited.</summary>
	CompanionsRecruited,
	/// <summary>Companions lost/dismissed.</summary>
	CompanionsLost,
	/// <summary>Quests accepted.</summary>
	QuestsAccepted,
	/// <summary>Quests completed.</summary>
	QuestsCompleted,
	/// <summary>Quests failed.</summary>
	QuestsFailed,
	/// <summary>Quests abandoned.</summary>
	QuestsAbandoned,
	/// <summary>Side quests completed.</summary>
	SideQuestsCompleted,

	// ═══════════════════════════════════════════════════════════════
	// SKILL/ABILITY METRICS
	// ═══════════════════════════════════════════════════════════════

	SkillsUsed,
	SpellsCast,
	/// <summary>Unique spells cast.</summary>
	UniqueSpellsCast,
	/// <summary>Spell damage dealt.</summary>
	SpellDamageDealt,
	/// <summary>Mana spent on spells.</summary>
	ManaSpentOnSpells,
	AbilitiesUsed,
	/// <summary>Ultimate/special abilities used.</summary>
	UltimateAbilitiesUsed,
	/// <summary>Abilities used on cooldown (wasted).</summary>
	AbilitiesWasted,
	LevelUpsEarned,
	HighestLevel,
	AttributePointsSpent,
	SkillPointsSpent,
	/// <summary>Abilities learned.</summary>
	AbilitiesLearned,
	/// <summary>Abilities upgraded.</summary>
	AbilitiesUpgraded,
	/// <summary>Times respecced.</summary>
	TimesRespecced,
	/// <summary>Experience points earned total.</summary>
	ExperienceEarned,
	/// <summary>Experience from combat.</summary>
	ExperienceFromCombat,
	/// <summary>Experience from exploration.</summary>
	ExperienceFromExploration,
	/// <summary>Experience from quests.</summary>
	ExperienceFromQuests,

	// ═══════════════════════════════════════════════════════════════
	// META/RUN METRICS
	// ═══════════════════════════════════════════════════════════════

	TotalRuns,
	RunsStarted,
	RunsCompleted,
	/// <summary>Deaths from being consumed by the fog.</summary>
	TimesCaughtInTime,
	DeathsTotal,
	/// <summary>Deaths from combat.</summary>
	DeathsFromCombat,
	/// <summary>Deaths from traps.</summary>
	DeathsFromTraps,
	/// <summary>Deaths from starvation.</summary>
	DeathsFromStarvation,
	/// <summary>Deaths from exhaustion.</summary>
	DeathsFromExhaustion,
	/// <summary>Deaths from status effects.</summary>
	DeathsFromStatusEffects,
	/// <summary>Successful fog clear victories.</summary>
	FogClears,
	VictoriesAchieved,
	/// <summary>Perfect victories (no deaths).</summary>
	PerfectVictories,
	/// <summary>Speedrun victories.</summary>
	SpeedrunVictories,
	LongestRunDays,
	ShortestVictoryDays,
	/// <summary>Average run length in days.</summary>
	AverageRunDays,
	VillageBuildingsBuilt,
	VillageBuildingsUpgraded,
	/// <summary>Village upgrade points earned total.</summary>
	VillageUpgradePointsEarned,
	/// <summary>Village upgrade points spent.</summary>
	VillageUpgradePointsSpent,
	LoreEntriesDiscovered,
	/// <summary>Total lore pages collected.</summary>
	LorePagesCollected,
	AchievementsUnlocked,
	ClassesUnlocked,
	/// <summary>Endings seen.</summary>
	EndingsSeen,
	/// <summary>True ending achieved.</summary>
	TrueEndingAchieved,
	/// <summary>Secret endings found.</summary>
	SecretEndingsFound,
	/// <summary>New game plus runs.</summary>
	NewGamePlusRuns,

	// ═══════════════════════════════════════════════════════════════
	// SAVE/LOAD METRICS
	// ═══════════════════════════════════════════════════════════════

	TimesSaved,
	TimesLoaded,
	/// <summary>Autosaves triggered.</summary>
	AutosavesTriggered,
	/// <summary>Save scum detected (loads after death).</summary>
	SaveScumAttempts,

	// ═══════════════════════════════════════════════════════════════
	// TIME METRICS
	// ═══════════════════════════════════════════════════════════════

	TotalPlayTimeSeconds,
	TimeInCombatSeconds,
	TimeTravelingSeconds,
	TimeInMenusSeconds,
	CurrentRunTimeSeconds,
	TotalDaysSurvived,
	CurrentRunDays,
	/// <summary>Time spent in village.</summary>
	TimeInVillageSeconds,
	/// <summary>Time spent camping.</summary>
	TimeCampingSeconds,
	/// <summary>Time spent in dungeons.</summary>
	TimeInDungeonsSeconds,
	/// <summary>Time spent reading lore/dialogue.</summary>
	TimeReadingSeconds,
	/// <summary>Longest single session.</summary>
	LongestSessionSeconds,
	/// <summary>Time spent idle.</summary>
	TimeIdleSeconds,

	// ═══════════════════════════════════════════════════════════════
	// WEATHER & ENVIRONMENT METRICS
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Hours traveled in rain.</summary>
	HoursTraveledInRain,
	/// <summary>Hours traveled in storm.</summary>
	HoursTraveledInStorm,
	/// <summary>Hours traveled in fog.</summary>
	HoursTraveledInFog,
	/// <summary>Hours traveled at night.</summary>
	HoursTraveledAtNight,
	/// <summary>Weather changes experienced.</summary>
	WeatherChangesExperienced,
	/// <summary>Times struck by environmental hazards.</summary>
	EnvironmentalHazardsTaken,

	// ═══════════════════════════════════════════════════════════════
	// ECONOMY METRICS
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Best trade profit (single transaction).</summary>
	BestTradeProfitSingle,
	/// <summary>Worst trade loss (single transaction).</summary>
	WorstTradeLossSingle,
	/// <summary>Total trade profit.</summary>
	TotalTradeProfit,
	/// <summary>Items bought from merchants.</summary>
	ItemsBoughtFromMerchants,
	/// <summary>Items sold to merchants.</summary>
	ItemsSoldToMerchants,
	/// <summary>Times haggled.</summary>
	TimesHaggled,
	/// <summary>Successful haggles.</summary>
	HagglesSuccessful,
	/// <summary>Donations made.</summary>
	DonationsMade,
	/// <summary>Taxes paid.</summary>
	TaxesPaid,

	// ═══════════════════════════════════════════════════════════════
	// MISCELLANEOUS METRICS
	// ═══════════════════════════════════════════════════════════════

	TimesOpenedInventory,
	TimesOpenedMap,
	TimesOpenedCharacterSheet,
	TimesOpenedCodex,
	TimesPaused,
	TimesChangedSpeed,
	DiceRolled,
	Natural20sRolled,
	Natural1sRolled,
	/// <summary>Highest single dice roll.</summary>
	HighestDiceRoll,
	/// <summary>Lowest single dice roll.</summary>
	LowestDiceRoll,
	StepsBackwards,
	TimesGotLost,
	TimesClickedAnchor,
	MaxDaysWithoutSleep,
	TimesMoraleHitZero,
	TimesRanOutOfFood,
	/// <summary>Times ran out of water.</summary>
	TimesRanOutOfWater,
	/// <summary>Times inventory was sorted.</summary>
	TimesInventorySorted,
	/// <summary>Screenshots taken (if supported).</summary>
	ScreenshotsTaken,
	/// <summary>Tutorial steps completed.</summary>
	TutorialStepsCompleted,
	/// <summary>Tutorial skipped.</summary>
	TutorialSkipped,
	/// <summary>Settings changed.</summary>
	SettingsChanged,
	/// <summary>Times difficulty changed.</summary>
	DifficultyChanged,
	/// <summary>Easter eggs found.</summary>
	EasterEggsFound,
	/// <summary>Jokes/puns encountered.</summary>
	JokesEncountered,
	/// <summary>Times pet was pet (if applicable).</summary>
	TimesPetWasPet,
	/// <summary>Chairs sat in.</summary>
	ChairsSatIn,
	/// <summary>Beds slept in.</summary>
	BedsSleptIn,
	/// <summary>Signs read.</summary>
	SignsRead,
	/// <summary>Books read.</summary>
	BooksRead,
	/// <summary>Notes found.</summary>
	NotesFound,
	/// <summary>Maps found.</summary>
	MapsFound
}