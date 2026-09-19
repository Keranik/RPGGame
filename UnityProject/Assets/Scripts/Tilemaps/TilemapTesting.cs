using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapFactoryMb : MonoBehaviour
	{
		/// <summary>
		/// Root game object that will store the main grid.
		/// </summary>
		private GameObject k_mainGameGridObject;
		private Grid k_mainGameGrid;
		private Tilemap k_worldTileMap;
		private TilemapRenderer k_worldTileMapRenderer;
		private Tile k_emptyTile;

		void Start()
		{
			// Create the main grid
			k_mainGameGridObject = new GameObject("MainGameGrid");
			k_mainGameGrid = k_mainGameGridObject.AddComponent<Grid>();
			k_mainGameGrid.cellLayout = GridLayout.CellLayout.Isometric;

			// Create a new Tilemap
			GameObject tileMapObject = new GameObject("Tilemap_World");
			tileMapObject.transform.SetParent(k_mainGameGridObject.transform);
			k_worldTileMap = tileMapObject.AddComponent<Tilemap>();
			k_worldTileMapRenderer = tileMapObject.AddComponent<TilemapRenderer>();

			// Load the default tile sprite
			Sprite blankTileSprite = Resources.Load<Sprite>("Sprites/Tilemap/Cobblestone");
			if (blankTileSprite == null) {
				Debug.LogError("Failed to load default tile sprite");
			}

			// Create the default tile
			k_emptyTile = ScriptableObject.CreateInstance<Tile>();
			k_emptyTile.sprite = blankTileSprite;
			k_emptyTile.color = Color.green;
			k_emptyTile.colliderType = Tile.ColliderType.None;			

			// Set tiles on the Tilemap
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					k_worldTileMap.SetTile(new Vector3Int(x, y, 0), k_emptyTile); // Assign a tile at this position
				}
			}

			// Ensure the camera is positioned correctly
			Camera.main.transform.position = new Vector3(5, 5, -10); // Adjust camera position to view the grid
			Camera.main.orthographic = true; // Set camera to orthographic mode
		}
	}