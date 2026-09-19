# Create-TerrainFolders.ps1
# Creates simple Top/ and Side/ folders for every terrain

$terrains = @(
    "Road","Path","Trail","Bridge",
    "Grass","Hills","OpenPlains","Meadow","Farmland",
    "Forest","DeepForest","AncientForest","DeadForest","Jungle","Orchard",
    "Mountain","Peak","Cliff","Pass","Highland",
    "River","Lake","Pond","Ocean","Shallows","Waterfall","HotSpring",
    "Beach","RockyCoast","TidalFlats","Reef",
    "Swamp","Marsh","Bog","Mangrove",
    "OpenDesert","Dunes","Oasis","Badlands","SaltFlats","Canyon","Mesa",
    "Snow","Ice","Glacier","FrozenLake","Permafrost","Blizzard","Tundra",
    "VolcanicPlain","LavaField","AshWastes","Geothermal","ObsidianField",
    "Cave","Cavern","Tunnel","UndergroundLake","CrystalCavern","MushroomForest","LavaTube","Mine","Catacombs",
    "Village","Town","City","Outpost","Fort","Castle","Temple","Tower",
    "OpenRuins","AbandonedVillage","Graveyard","Battlefield","Shipwreck","Dungeon","Crypt","Monument",
    "Corrupted","Blighted","ShadowRealm","Wasteland","CursedGround","Magical","FeyCrossing","LeyLine","VoidTouched",
    "Fog","Blocked","Portal","Sanctuary","Arena","Crossroads","Campsite","Wayshrine"
)

foreach ($t in $terrains) {
    $root = Join-Path . $t
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    
    New-Item -ItemType Directory -Force -Path (Join-Path $root "Top")  | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $root "Side") | Out-Null
    
    Write-Host "Created: $t/Top   and   $t/Side"
}

Write-Host "`nAll done! Simple structure ready:"
Write-Host "   • Drop files like Grass_Top_1.png, Grass_Top_2.png into Top/"
Write-Host "   • Drop files like Grass_Side_L1.png, Grass_Side_R1.png into Side/"
pause