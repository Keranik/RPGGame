using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
#nullable disable

namespace RPGGame.Core.Maps.Intro;
internal sealed class WorldMap {
	private readonly GameObject k_tilemapObject;
	public GameObject TileMapObject => k_tilemapObject;
	private readonly Tilemap k_tilemap;
	public Tilemap Tilemap => k_tilemap;
	private readonly TilemapRenderer k_tilemapRenderer;
	public Renderer Renderer => k_tilemapRenderer;

	public WorldMap(TileBase defaultTile) {
		GameObject parentObject = GameObject.Find("Tilemaps_World");

		if (parentObject == null) {
			UnityEngine.Debug.LogError("Failed to find parent object for world map tilemap.");
			return;
		}

		k_tilemapObject = new GameObject("Intro_WorldMap");
		k_tilemap = k_tilemapObject.AddComponent<Tilemap>();
		k_tilemapRenderer = k_tilemapObject.AddComponent<TilemapRenderer>();
		
		k_tilemapObject.transform.parent = parentObject.transform;

		for (int x = 0; x < 10; x++) {
			for (int y = 0; y < 10; y++) {
				k_tilemap.SetTile(new Vector3Int(x, y, 0), defaultTile); // Assign a tile at this position
			}
		}		
	}
}
