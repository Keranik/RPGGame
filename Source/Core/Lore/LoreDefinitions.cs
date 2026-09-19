using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Lore;

namespace RPGGame.Core.Lore;

/// <summary>
/// Defines all lore entries using the Proto system.
/// </summary>
public class LoreDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterWorldLore(gameDatabase);
		RegisterFogLore(gameDatabase);
		RegisterHistoryLore(gameDatabase);
		RegisterVillageLore(gameDatabase);
		RegisterBestiaryLore(gameDatabase);
		RegisterMagicLore(gameDatabase);
	}

	#region World Lore

	private void RegisterWorldLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.World.Introduction,
			text: Proto.CreateText("The World", "An overview of the world as it exists today."),
			category: LoreCategory.World,
			summary: "An overview of the world as it exists today.",
			content: "The world was once vast and filled with wonder. Kingdoms rose and fell, magic flowed freely, and the people prospered under the watchful eyes of ancient powers.\n\nThat was before the Fog came.\n\nNow, reality itself seems to fray at the edges. Time moves strangely. The dead don't always stay dead. And the villages that remain cling to their Anchors—artifacts of old magic that hold back the encroaching mist.\n\nYou are a Wanderer, one of the brave few who ventures beyond the Anchor's protection. You seek the source of the Fog. You seek an end to the endless night.\n\nBut time loops, and the Fog remembers. Every failure is a lesson. Every death is a new beginning.\n\nHow many times have you walked this path before?",
			quote: "The Fog does not consume. It erases.",
			quoteSource: "Elder's Warning",
			startsDiscovered: true,
			sortOrder: 0,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.World.Anchors,
			text: Proto.CreateText("The Anchors", "Ancient artifacts that protect settlements from the Fog."),
			category: LoreCategory.World,
			summary: "Ancient artifacts that protect settlements from the Fog.",
			content: "The Anchors are relics of an age before the Fog. Their origin is lost to time, but their purpose is clear: they create zones of temporal stability where the Fog cannot reach.\n\nEach Anchor appears as a crystalline pillar at the heart of a village, pulsing with a warm, golden light. Those within its radius experience time normally. They remember. They persist.\n\nBut the Anchors are weakening. Their light dims with each passing year, their range shrinks ever so slightly. The Elders say that one day, they will fail entirely.\n\nThat is why the Wanderers venture forth. To find answers. To find a way to restore the Anchors—or to end the threat of the Fog forever.",
			quote: "Stay in the light, child. The light remembers who you are.",
			quoteSource: "Village Proverb",
			startsDiscovered: true,
			sortOrder: 1,
			relatedEntries: [Ids.Lore.Village.TheAnchor]
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.World.Wanderers,
			text: Proto.CreateText("The Wanderers", "Those who venture beyond the Anchor's protection."),
			category: LoreCategory.World,
			summary: "Those who venture beyond the Anchor's protection.",
			content: "Wanderers are those touched by the Anchor's light who venture beyond its protection. Unlike ordinary folk, Wanderers seem to retain fragments of memory across time loops—a blessing and a curse.\n\nMost who leave the Anchor's radius simply... cease to exist. The Fog takes them, unmakes them, and reality forgets they ever were.\n\nBut Wanderers return. Again and again. Each time they die in the Fog, they awaken at the Anchor, memories fragmenting like dreams upon waking. Skilled Wanderers learn to piece together their past experiences, growing stronger with each cycle.\n\nNo one knows why Wanderers are different. Some say the Anchor chooses them. Others believe they are touched by whatever force created the Fog. A few whisper that Wanderers are not truly alive at all—merely echoes of people who once existed, playing out the same doomed journey forever.",
			quote: "How many times have I stood here, ready to leave? How many times have I never returned?",
			quoteSource: "Wanderer's Journal",
			startsDiscovered: true,
			sortOrder: 2
		));
	}

	#endregion

	#region Fog Lore

	private void RegisterFogLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Nature01,
			text: Proto.CreateText("The Nature of the Fog", "Initial observations about the Fog's behavior."),
			category: LoreCategory.TheFog,
			summary: "Initial observations about the Fog's behavior.",
			content: "The Fog is not merely mist or vapor. It is an absence—a void where reality should be. Within it, time flows backwards, sideways, or not at all.\n\nThose caught in the Fog do not simply die. They are erased from existence, removed from the timeline as if they never were. Only those who have touched the Anchor's light can resist this erasure, and even then, not entirely.\n\nThe Fog seems drawn to life, to memory, to consciousness. It creeps toward settlements slowly but inexorably, held back only by the Anchors' failing light.\n\nSome scholars believe the Fog is alive—or at least, that something within it is aware. The shapes that move in its depths, the whispers that echo from its edges... these suggest an intelligence of some kind.\n\nBut what does it want? Why does it consume?",
			discoveryHint: "Venture deep into the Fog's edge.",
			sortOrder: 0,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Origin01,
			text: Proto.CreateText("Origins of the Fog", "Fragments of knowledge about where the Fog came from."),
			category: LoreCategory.TheFog,
			summary: "Fragments of knowledge about where the Fog came from.",
			content: "The Fog appeared suddenly, without warning, on a day the histories call the Unmaking. One moment, the world was whole. The next, vast swathes of reality simply... vanished.\n\nNo one remembers the exact date. The Fog took that too.\n\nAncient texts, those few that survived, speak of a great magical experiment gone wrong. Others mention a war between powers beyond mortal understanding. A rare few whisper of something deliberately unleashed—a weapon designed to end not just lives, but the very concept of existence.\n\nWhatever its origin, the Fog continues to spread. Year by year, inch by inch, it consumes more of the world. The Anchors slow it, but they cannot stop it.\n\nUnless someone finds the source.",
			prerequisites: [Ids.Lore.Fog.Nature01],
			discoveryHint: "Defeat one who has emerged from the Fog.",
			sortOrder: 1,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.TimeLoop01,
			text: Proto.CreateText("The Loop", "Understanding the nature of the time loop."),
			category: LoreCategory.TheFog,
			summary: "Understanding the nature of the time loop.",
			content: "Time does not flow normally for Wanderers. When we die in the Fog—or when the Fog finally consumes all—we awaken again at the Anchor, at the beginning of our journey.\n\nMost forget. The memories shatter like glass, leaving only fragments. But with each loop, some things remain: skills honed through repetition, instincts sharpened by forgotten failures, and occasionally, flashes of déjà vu.\n\nI have met others who remember more than they should. They speak of meeting the same people, fighting the same battles, dying the same deaths—dozens, hundreds, perhaps thousands of times.\n\nIs this salvation, or damnation? We cannot truly die, but we cannot truly live, either. We are prisoners of the loop, playing out the same tragedy until...\n\nUntil what? What breaks the cycle?",
			quote: "You look like you've walked this road before, Wanderer.",
			quoteSource: "A Stranger at the Crossroads",
			prerequisites: [Ids.Lore.Fog.Nature01],
			discoveryHint: "Encounter one who exists outside normal time.",
			sortOrder: 2,
			requiredFogClears: 1,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Clue01,
			text: Proto.CreateText("Whispers in the Mist", "Strange voices heard at the Fog's edge."),
			category: LoreCategory.TheFog,
			summary: "Strange voices heard at the Fog's edge.",
			content: "At the boundary where the Anchor's light meets the Fog, if you listen carefully, you can hear them. Voices.\n\nNot screams—that would be merciful. These are conversations, laughter, mundane words spoken by people going about their daily lives. Echoes of those the Fog has taken, still playing out their final moments in an endless loop.\n\nSometimes, if you listen long enough, you hear your own voice among them.\n\nThe voices seem to come from everywhere and nowhere. Some Wanderers claim to have heard prophecies in the whispers, hints about the Fog's nature or weakness. Others warn against listening too long, lest the Fog notice you listening back.\n\nI have heard my name, spoken by a voice I almost recognize. It told me to find the Herald.",
			discoveryHint: "Listen at the Fog's boundary.",
			sortOrder: 3
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Clue02,
			text: Proto.CreateText("The Herald", "A being that seems to command the Fog."),
			category: LoreCategory.TheFog,
			summary: "A being that seems to command the Fog.",
			content: "They appear at the deepest point of the Fog—a figure wreathed in mist, wearing a shape that is almost human but not quite. The Herald.\n\nSome say the Herald is the source of the Fog, the entity that creates and controls it. Others believe it to be merely a servant, an emissary of something greater lurking in the depths of unreality.\n\nWhat is known: the Herald can be confronted. It can be fought. And when it falls, the Fog recedes—temporarily. The world gets a reprieve, a chance to breathe before the mist closes in once more.\n\nBut the Herald always returns. Different, sometimes, wearing different forms, speaking in different voices. Yet always, unmistakably, the Herald.\n\nTo end the Fog, must we end the Herald? Or is there something else we must do?",
			prerequisites: [Ids.Lore.Fog.Clue01],
			discoveryHint: "Learn the Herald's name.",
			sortOrder: 4,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Clue03,
			text: Proto.CreateText("Fog-Touched", "Those who have been changed by the Fog."),
			category: LoreCategory.TheFog,
			summary: "Those who have been changed by the Fog.",
			content: "Not all who enter the Fog are erased. Some return... changed.\n\nThe Fog-Touched bear marks of their exposure: eyes that glow faintly in darkness, skin that seems to shimmer between moments, voices that echo strangely. They speak of visions—glimpses of other times, other possibilities, other versions of themselves.\n\nMost Fog-Touched are pitied. Their minds are fractured, unable to distinguish between what is, what was, and what might be. They wander the edges of villages, muttering prophecies no one understands.\n\nBut a few... a few gain something from their exposure. Power. Knowledge. The ability to step outside time itself, if only for a moment.\n\nThe Ascended, they call themselves. And they claim to know how the cycle can be broken.",
			prerequisites: [Ids.Lore.Fog.Nature01],
			discoveryHint: "Meet one who survived the Fog's touch.",
			sortOrder: 5,
			requiredFogClears: 2
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Clue04,
			text: Proto.CreateText("The Source", "Rumors of where the Fog originates."),
			category: LoreCategory.TheFog,
			summary: "Rumors of where the Fog originates.",
			content: "The Fog must come from somewhere. It must have a source.\n\nThe oldest tales speak of a place at the center of the world—or what used to be the center, before the Fog consumed it. A city of impossible architecture, where time itself was first bent and broken.\n\nThey call it the Unraveling. The point where reality began to fray.\n\nNo Wanderer has ever reached it and returned. The Fog is too thick, too hungry, too aware. But the Ascended claim that reaching the Unraveling is the only way to end the cycle.\n\nTo unmake the Unmaking.\n\nBut at what cost?",
			prerequisites: [Ids.Lore.Fog.Clue02, Ids.Lore.Fog.Clue03],
			discoveryHint: "Piece together the fragments of truth.",
			sortOrder: 6,
			requiredFogClears: 3,
			isImportant: true,
			isSpoiler: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Fog.Clue05,
			text: Proto.CreateText("The Truth", "The final revelation about the Fog's nature."),
			category: LoreCategory.TheFog,
			summary: "The final revelation about the Fog's nature.",
			content: "I understand now. The Fog is not an enemy. It is not a weapon.\n\nIt is a wound.\n\nReality itself was torn—by what, I cannot say. War? Hubris? An accident beyond mortal comprehension? It doesn't matter. What matters is that the wound bleeds, and the blood is the Fog.\n\nThe Anchors don't hold back the Fog. They hold reality together, preventing the wound from spreading. But they are failing. One by one, they dim and die, and with each failure, the wound grows.\n\nThe Herald is not the source. The Herald is a symptom—a manifestation of reality's pain, given form and purpose.\n\nTo heal the wound... we must find where it began. We must go to the Unraveling.\n\nAnd we must stitch reality back together.\n\nOr watch everything unravel forever.",
			prerequisites: [Ids.Lore.Fog.Clue04],
			discoveryHint: "Defeat the Herald and survive.",
			sortOrder: 7,
			requiredFogClears: 5,
			isImportant: true,
			isSpoiler: true
		));
	}

	#endregion

	#region History Lore

	private void RegisterHistoryLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.History.BeforeFog,
			text: Proto.CreateText("The World Before", "What little is known of the world before the Fog."),
			category: LoreCategory.History,
			summary: "What little is known of the world before the Fog.",
			content: "The world before the Fog is mostly speculation now. The mist took more than people—it took records, artifacts, entire libraries of knowledge.\n\nWhat remains suggests a world of wonders: great cities of gleaming stone, magic used freely for everyday tasks, travel between distant lands in the blink of an eye. The kingdoms traded, warred, and made peace, much as nations always have.\n\nThere were warnings, supposedly. Seers and oracles who spoke of coming darkness, of an unraveling at the edges of reality. But prophecies are common, and most are wrong. By the time anyone realized these warnings were different, it was too late.\n\nThe Fog came. And the world that was... wasn't anymore.",
			discoveryHint: "Find records from before the Fog.",
			sortOrder: 0
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.History.Unmaking,
			text: Proto.CreateText("The Unmaking", "The day the Fog first appeared."),
			category: LoreCategory.History,
			summary: "The day the Fog first appeared.",
			content: "No one remembers the Unmaking clearly—the Fog saw to that. But fragments persist, passed down through those who touched the Anchors in time.\n\nIt began at the edges of the world, they say. Places far from the centers of power, where few would notice or care. Villages vanished. Trade routes became impassable. Travelers spoke of roads that led nowhere, of landmarks that had never existed.\n\nBy the time the great cities took notice, it was too late. The Fog moved faster than any could flee, swallowing kingdoms in hours. Those who reached the Anchors survived. Those who didn't...\n\nWe don't speak of what happened to those who didn't.",
			prerequisites: [Ids.Lore.History.BeforeFog],
			discoveryHint: "Speak with one who witnessed the Unmaking.",
			sortOrder: 1,
			isImportant: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.History.FirstWanderers,
			text: Proto.CreateText("The First Wanderers", "Those who first ventured into the Fog."),
			category: LoreCategory.History,
			summary: "Those who first ventured into the Fog.",
			content: "In the early days after the Unmaking, no one dared leave the Anchors' protection. The Fog was too new, too terrifying, too unknown.\n\nBut supplies ran low. Questions needed answers. And some simply couldn't bear to wait.\n\nThe first Wanderers were volunteers—or fools, depending on who tells the tale. They walked into the Fog expecting never to return.\n\nAnd yet... some did. Changed. Shaken. But alive.\n\nThey brought back knowledge: the Fog could be navigated. It could be survived. And most importantly, death within it was not the end. The Anchors pulled them back, reset them, gave them another chance.\n\nThe time loop had been discovered. And with it, hope.",
			prerequisites: [Ids.Lore.History.Unmaking],
			discoveryHint: "Complete your first expedition.",
			sortOrder: 2
		));
	}

	#endregion

	#region Village Lore

	private void RegisterVillageLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Village.Overview,
			text: Proto.CreateText("Haven Village", "Your home and base of operations."),
			category: LoreCategory.Village,
			summary: "Your home and base of operations.",
			content: "Haven is small, as villages go—perhaps two hundred souls clustered around their Anchor. But in a world where most settlements have been consumed by the Fog, it represents hope.\n\nThe villagers are a hardy folk, accustomed to hardship and uncertainty. They farm what land remains within the Anchor's radius, craft what they need, and try not to think too hard about what lies beyond the light.\n\nThey view Wanderers with a mixture of reverence and unease. We are their protectors, their hope for salvation—but also a reminder that the world beyond is hostile and hungry. Every Wanderer who leaves might not return. Every Wanderer who does return carries the taint of the Fog with them.\n\nStill, they welcome us. They feed us, equip us, and send us forth with their blessings.\n\nWe try not to disappoint them.",
			startsDiscovered: true,
			sortOrder: 0
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Village.TheAnchor,
			text: Proto.CreateText("The Haven Anchor", "The crystalline pillar that protects the village."),
			category: LoreCategory.Village,
			summary: "The crystalline pillar that protects the village.",
			content: "At Haven's heart stands the Anchor—a pillar of pale crystal twice the height of a person, pulsing with warm golden light. Around it, time flows normally. Memory persists. The Fog cannot reach.\n\nNo one knows who created the Anchors or how they work. The village elders maintain them through rituals passed down through generations, though even they admit the rituals might be meaningless. The Anchors seem to sustain themselves, drawing power from... somewhere.\n\nThe light dims slowly over the years. When I was young, it reached to the old mill on the hill. Now, it barely covers the village square. At this rate, in another generation...\n\nBut Wanderers bring hope. Every time we push back the Fog, every Herald we fell, the Anchor seems to strengthen. Perhaps we can restore it to its former glory.\n\nPerhaps we can save more than just Haven.",
			startsDiscovered: true,
			sortOrder: 1,
			relatedEntries: [Ids.Lore.World.Anchors]
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Village.Elder,
			text: Proto.CreateText("The Elder", "The village's spiritual leader and keeper of knowledge."),
			category: LoreCategory.Village,
			summary: "The village's spiritual leader and keeper of knowledge.",
			content: "The Elder is old—older than anyone else in Haven, older than the village's records indicate any human should be. Some whisper that she is as old as the Anchor itself, sustained by its light.\n\nShe never confirms or denies. She simply smiles, offers cryptic advice, and sends Wanderers on their way.\n\nWhat she does share freely is knowledge. The Elder remembers things from before the Fog—or claims to. She speaks of the old world with a familiarity that should be impossible. She knows of places long swallowed by the mist, of magic lost to time, of enemies we will face before we face them.\n\nI asked her once how she knows so much. She looked at me with ancient eyes and said:\n\n\"I have watched you walk this path many times, Wanderer. I remember every step.\"",
			discoveryHint: "Speak with the Elder after your first expedition.",
			sortOrder: 2
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Village.Blacksmith,
			text: Proto.CreateText("The Blacksmith", "The village's weapon and armor crafter."),
			category: LoreCategory.Village,
			summary: "The village's weapon and armor crafter.",
			content: "Kael the Blacksmith is a mountain of a man with arms like tree trunks and hands scarred by countless burns. He speaks little, but his work speaks volumes.\n\nThe weapons and armor he crafts are more than functional—they're works of art, each piece perfectly balanced and suited to its wielder. He claims no special magic, just skill and experience.\n\nBut I've seen him work. The way the metal seems to flow under his hammer, the way he knows exactly how hot the forge needs to be without checking... there's something more than mundane craftsmanship at work.\n\nWhen I asked him about it, he just smiled and said, \"The metal remembers what it wants to be. I just help it get there.\"\n\nPerhaps that's magic enough.",
			discoveryHint: "Upgrade equipment at the Blacksmith.",
			sortOrder: 3
		));
	}

	#endregion

	#region Bestiary Lore

	private void RegisterBestiaryLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Bestiary.FogTouched,
			text: Proto.CreateText("Fog-Touched Creatures", "Creatures corrupted by proximity to the Fog."),
			category: LoreCategory.Bestiary,
			summary: "Creatures corrupted by proximity to the Fog.",
			content: "Animals and beasts that wander too close to the Fog's edge sometimes return... changed. We call them Fog-Touched.\n\nThe transformation varies. Some merely become aggressive, their minds clouded by the mist's influence. Others warp physically—growing extra limbs, developing unnatural abilities, or twisting into shapes that defy normal biology.\n\nAll are dangerous. The Fog's touch grants no kindness.\n\nInterestingly, Fog-Touched creatures avoid the Anchor's light instinctively. They will prowl the boundary, waiting for foolish prey to stray beyond protection, but they will not enter. Whether this is due to the light's power or some deeper command from the Fog itself, we do not know.",
			discoveryHint: "Encounter a creature touched by the Fog.",
			sortOrder: 0
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Bestiary.Undead,
			text: Proto.CreateText("The Risen Dead", "Those who died in the Fog and returned."),
			category: LoreCategory.Bestiary,
			summary: "Those who died in the Fog and returned.",
			content: "Death works differently near the Fog. Those who die within its influence sometimes rise again, their bodies animated by forces unknown. Skeletons, zombies, ghosts—the Fog creates them all.\n\nThey are not the people they were. Whatever consciousness remains is warped, fragmented, hostile. They attack the living on sight, as if offended by the presence of those who still draw breath.\n\nStrangely, the risen dead seem bound to specific locations—the places where they died, perhaps, or where they were taken by the Fog. They do not wander far. This makes them predictable, at least.\n\nHoly magic is particularly effective against them. The light of faith seems to mimic the Anchor's power, driving back whatever force animates their corpses.",
			prerequisites: [Ids.Lore.Bestiary.FogTouched],
			discoveryHint: "Face the risen dead and survive.",
			sortOrder: 1
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Bestiary.Demons,
			text: Proto.CreateText("Demons", "Entities from beyond the material world."),
			category: LoreCategory.Bestiary,
			summary: "Entities from beyond the material world.",
			content: "The Fog is not the only threat beyond the Anchor's light. Where reality frays, other things slip through—beings from planes of existence never meant to touch our own.\n\nWe call them demons, though the name is imprecise. Some are mindless forces of destruction. Others are cunning, even charming, offering bargains that seem too good to be true.\n\nThey always are.\n\nDemons feed on the chaos the Fog creates. They are drawn to the wounds in reality like sharks to blood. Some say they were here before the Fog, waiting. Others believe the Fog itself is a demon of sorts—the largest and hungriest of them all.\n\nWhatever their origin, they are enemies. Trust nothing that emerges from the mist with a smile and a promise.",
			prerequisites: [Ids.Lore.Bestiary.Undead],
			discoveryHint: "Encounter a being from beyond.",
			sortOrder: 2
		));
	}

	#endregion

	#region Magic Lore

	private void RegisterMagicLore(GameDb db) {
		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Magic.Overview,
			text: Proto.CreateText("Magic in the Fog Age", "How magic has changed since the Fog appeared."),
			category: LoreCategory.Magic,
			summary: "How magic has changed since the Fog appeared.",
			content: "Magic existed before the Fog, and it persists still—but changed. Twisted.\n\nThe old magic drew power from the world itself: from ley lines, from natural places of power, from the ambient energy of existence. The Fog disrupted all of this. Ley lines severed or corrupted, places of power consumed or tainted, ambient energy drained into the endless void.\n\nWhat remains is harder, requiring more will and sacrifice. Spells that once came easily now demand intense concentration. Rituals that once required mere words now need blood, pain, or worse.\n\nSome mages have adapted, learning to draw power from the Fog itself. This is... inadvisable. The Fog's power comes with a price, always. Those who use it too freely become something less than human. Or perhaps something more.\n\nThe safest magic, they say, is that which draws from the Anchor's light. Holy magic, healing magic, protective wards—these still function cleanly. Perhaps because they serve the same purpose as the Anchors themselves: to push back against the darkness.",
			discoveryHint: "Study the nature of magic.",
			sortOrder: 0
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Magic.Temporal,
			text: Proto.CreateText("Temporal Magic", "The forbidden art of manipulating time."),
			category: LoreCategory.Magic,
			summary: "The forbidden art of manipulating time.",
			content: "Before the Fog, temporal magic was the rarest and most dangerous school. Only a handful of mages ever mastered it, and most met unfortunate ends—erased from time by their own experiments.\n\nNow, it is something else entirely.\n\nThe Fog exists outside normal time. It consumes past, present, and future simultaneously. To study temporal magic is to touch the same forces that power the Fog itself.\n\nSome Wanderers develop temporal abilities naturally, perhaps as a side effect of the time loop. They glimpse possible futures, echo their actions across moments, or even briefly step outside the flow of time entirely.\n\nThese abilities are powerful but come with risks. The Fog notices those who manipulate time. It remembers them. And eventually, it comes for them.\n\nThe true ending, they say, requires mastering time itself. But at what cost?",
			prerequisites: [Ids.Lore.Magic.Overview, Ids.Lore.Fog.TimeLoop01],
			discoveryHint: "Unlock the secrets of temporal magic.",
			sortOrder: 1,
			requiredFogClears: 3,
			isSpoiler: true
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Magic.Holy,
			text: Proto.CreateText("Holy Magic", "The power of faith and light."),
			category: LoreCategory.Magic,
			summary: "The power of faith and light.",
			content: "Holy magic is the most reliable form of spellcasting in the Fog Age. While other schools falter and fail, the power of faith remains constant.\n\nThis is not coincidence. Holy magic draws from the same source as the Anchors—a force of stability, of memory, of persistence. Those who wield it become conduits for reality's resistance to the Fog.\n\nHealers are precious in Haven. Their magic can mend wounds that would otherwise fester, cure diseases that mundane medicine cannot touch, and even restore vitality drained by the Fog itself.\n\nBut holy magic has limits. It cannot bring back those the Fog has taken. It cannot restore memories lost to the loop. And it cannot harm the Fog directly—only hold it at bay.\n\nTo truly defeat the Fog, we need something more.",
			prerequisites: [Ids.Lore.Magic.Overview],
			discoveryHint: "Witness the power of faith.",
			sortOrder: 2
		));

		db.RegisterProto(new LoreProto(
			id: Ids.Lore.Magic.Arcane,
			text: Proto.CreateText("Arcane Magic", "The scholarly pursuit of magical power."),
			category: LoreCategory.Magic,
			summary: "The scholarly pursuit of magical power.",
			content: "Arcane magic is the magic of study, of formulas, of precise manipulation of reality's underlying structure. Before the Fog, it was the dominant school—academies trained thousands of mages in its arts.\n\nNow, it is diminished but not destroyed.\n\nArcane mages still exist, though they are fewer and their power is harder-won. The formulas that once worked flawlessly now require constant adjustment, compensation for the way the Fog has warped the underlying fabric of reality.\n\nSome see this as a challenge to be overcome. Others have abandoned arcane study entirely, turning to other schools or giving up magic altogether.\n\nBut for those who persist, the rewards are significant. Arcane magic remains versatile, powerful, and—crucially—does not require faith or bargains with dark powers. It requires only knowledge, will, and the patience to relearn what the Fog has unwritten.",
			prerequisites: [Ids.Lore.Magic.Overview],
			discoveryHint: "Study with the scholars of Haven.",
			sortOrder: 3
		));
	}

	#endregion
}