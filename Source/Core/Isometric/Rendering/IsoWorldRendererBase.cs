using UnityEngine;

namespace RPGGame.Core.Isometric.Rendering;

/// <summary>
/// Abstract base class for isometric world renderers.
/// Provides common coordinate conversion and configuration for all iso renderers.
/// Uses <see cref="TilesRPG"/> for tile measurements and <see cref="ColorRPG"/> for colors.
/// 
/// <para>
/// This is a pure logic class (NOT a MonoBehaviour) designed for dependency injection.
/// Concrete implementations handle specific world types (Village, Expedition, Combat, etc.)
/// </para>
/// 
/// <para>
/// Uses simple grid positioning where the camera angle provides the isometric look.
/// Grid X → World X, Grid Y → World Z, Level → World Y.
/// </para>
/// </summary>
public abstract class IsoWorldRendererBase {
    #region Configuration
	protected LightController? k_lightController;

    /// <summary>Width of one tile in world units.</summary>
    protected TilesRPG TileWidth { get; set; } = TilesRPG.One;

    /// <summary>Height of one tile in world units.</summary>
    protected TilesRPG TileHeight { get; set; } = TilesRPG.One;

    /// <summary>Height of one level in world units.</summary>
    protected TilesRPG LevelHeight { get; set; } = TilesRPG.One;

    /// <summary>Base sorting order multiplier per level.</summary>
    protected int SortingOrderPerLevel { get; set; } = 10000;

    /// <summary>Whether the renderer is currently active.</summary>
    public bool IsActive { get; protected set; }

    /// <summary>Whether the renderer has been initialized.</summary>
    public bool IsInitialized { get; protected set; }

    #endregion

    #region State

    /// <summary>Currently hovered position, if any.</summary>
    protected IsoPos? HoveredPosition { get; set; }

    /// <summary>Currently selected position, if any.</summary>
    protected IsoPos? SelectedPosition { get; set; }

    /// <summary>Root GameObject for all rendered objects.</summary>
    protected GameObject? RootObject { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new IsoWorldRendererBase with default configuration.
    /// </summary>
    protected IsoWorldRendererBase() { }

    /// <summary>
    /// Creates a new IsoWorldRendererBase with custom configuration.
    /// </summary>
    /// <param name="tileWidth">Width of one tile in world units.</param>
    /// <param name="tileHeight">Height of one tile in world units.</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    protected IsoWorldRendererBase(TilesRPG tileWidth, TilesRPG tileHeight, TilesRPG levelHeight) {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        LevelHeight = levelHeight;
    }

	/// <summary>
	/// Sets the light controller for this renderer.
	/// Called by GameInitializer after lighting system is set up.
	/// </summary>
	public void SetLightController(LightController controller) {
		k_lightController = controller;
		Debug.Log($"{GetType().Name}: LightController assigned");
	}
    
	/// <summary>
	/// Adds a light source at the specified position.
	/// </summary>
	protected int AddLight(IsoLightSource source) {
		if (k_lightController == null) {
			Debug.LogWarning($"{GetType().Name}: Cannot add light - no controller");
			return -1;
		}
		return k_lightController.AddLightSource(source);
	}
    
	/// <summary>
	/// Removes a previously added light source.
	/// </summary>
	protected void RemoveLight(int handle) {
		k_lightController?.RemoveLightSource(handle);
	}
    
	/// <summary>
	/// Updates a light source position.
	/// </summary>
	protected void UpdateLightPosition(int handle, IsoPos position) {
		k_lightController?.UpdateLightPosition(handle, position);
	}

    #endregion

    #region Coordinate Conversion

    /// <summary>
    /// Converts an IsoPos to world coordinates using simple grid positioning.
    /// Grid X → World X, Grid Y → World Z, Level → World Y.
    /// </summary>
    /// <param name="pos">The isometric position to convert.</param>
    /// <returns>World position suitable for Transform.position.</returns>
    protected Vector3 IsoToWorld(IsoPos pos) =>
        pos.ToWorldGridCenter(TileWidth, LevelHeight);

    /// <summary>
    /// Converts world coordinates to an IsoPos using simple grid positioning.
    /// </summary>
    /// <param name="worldPos">World position.</param>
    /// <returns>The corresponding isometric position.</returns>
    protected IsoPos WorldToIso(Vector3 worldPos) {
        int gridX = Mathf.FloorToInt(worldPos.x / TileWidth);
        int gridY = Mathf.FloorToInt(worldPos.z / TileHeight);
        var level = IsoLevel.FromWorldY(worldPos.y, LevelHeight);
        return new IsoPos(gridX, gridY, level);
    }

    /// <summary>
    /// Converts screen coordinates to an IsoPos using ground plane intersection.
    /// </summary>
    /// <param name="screenPos">Screen position in pixels.</param>
    /// <param name="camera">Camera to use for conversion.</param>
    /// <param name="groundLevel">Assumed ground level for intersection.</param>
    /// <returns>The corresponding isometric position, or null if no intersection.</returns>
    protected IsoPos? ScreenToIso(Vector2 screenPos, Camera camera, IsoLevel? groundLevel = null) {
        var ray = camera.ScreenPointToRay(screenPos);
        float planeY = groundLevel?.ToWorldY(LevelHeight) ?? 0f;
        var plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));

        if (plane.Raycast(ray, out float distance)) {
            var worldPoint = ray.GetPoint(distance);
            var isoPos = WorldToIso(worldPoint);
            return groundLevel.HasValue ? isoPos.WithLevel(groundLevel.Value) : isoPos;
        }

        return null;
    }

    /// <summary>
    /// Calculates the sorting order for an isometric position.
    /// Higher values render on top (in front).
    /// </summary>
    /// <param name="pos">The isometric position.</param>
    /// <param name="sublayer">Optional sublayer offset within the same tile.</param>
    /// <returns>Sorting order value for SpriteRenderer.</returns>
    protected int CalculateSortingOrder(IsoPos pos, int sublayer = 0) {
        // Level has highest priority, then depth (X + Y), then sublayer
        return pos.Level.Value * SortingOrderPerLevel + (pos.X + pos.Y) * 10 + sublayer;
    }

    /// <summary>
    /// Calculates the sorting order for a world Y position.
    /// </summary>
    protected int CalculateSortingOrder(Vector3 worldPos, int sublayer = 0) {
        var isoPos = WorldToIso(worldPos);
        return CalculateSortingOrder(isoPos, sublayer);
    }

    /// <summary>
    /// Calculates the distance in tiles between two isometric positions.
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Ending position.</param>
    /// <returns>Distance in tiles.</returns>
    protected TilesRPG CalculateDistance(IsoPos from, IsoPos to) {
        int dx = Mathf.Abs(to.X - from.X);
        int dy = Mathf.Abs(to.Y - from.Y);
        int dz = Mathf.Abs(to.Level.Value - from.Level.Value);
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz).Tiles();
    }

    /// <summary>
    /// Checks if an IsoPos is within the specified bounds.
    /// </summary>
    /// <param name="pos">Position to check.</param>
    /// <param name="bounds">Bounds to check against.</param>
    /// <returns>True if position is within bounds.</returns>
    protected bool IsInBounds(IsoPos pos, IsoBounds bounds) {
        return bounds.Contains(pos);
    }

    #endregion

    #region Rendering Helpers

    /// <summary>
    /// Creates or gets the root GameObject for this renderer.
    /// </summary>
    /// <param name="name">Name for the root object.</param>
    /// <returns>The root GameObject.</returns>
    protected GameObject GetOrCreateRoot(string name) {
        if (RootObject == null) {
            RootObject = new GameObject(name);
        }
        return RootObject;
    }

    /// <summary>
    /// Creates a sprite GameObject at the specified position.
    /// </summary>
    /// <param name="name">Name for the GameObject.</param>
    /// <param name="pos">Isometric position.</param>
    /// <param name="sprite">Sprite to render.</param>
    /// <param name="color">Color tint using ColorRPG.</param>
    /// <param name="parent">Parent transform.</param>
    /// <param name="sublayer">Sorting sublayer.</param>
    /// <returns>The created GameObject.</returns>
    protected GameObject CreateSpriteObject(string name, IsoPos pos, Sprite? sprite,
        ColorRPG color, Transform? parent = null, int sublayer = 0) {
        var go = new GameObject(name);
        go.transform.SetParent(parent ?? RootObject?.transform);
        go.transform.position = IsoToWorld(pos);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color; // Implicit conversion to Unity Color
        sr.sortingOrder = CalculateSortingOrder(pos, sublayer);

        return go;
    }

    /// <summary>
    /// Creates a colored quad at the specified position (for tiles without sprites).
    /// </summary>
    /// <param name="name">Name for the GameObject.</param>
    /// <param name="pos">Isometric position.</param>
    /// <param name="color">Color using ColorRPG.</param>
    /// <param name="parent">Parent transform.</param>
    /// <param name="sublayer">Sorting sublayer.</param>
    /// <returns>The created GameObject.</returns>
    protected GameObject CreateColoredQuad(string name, IsoPos pos, ColorRPG color,
        Transform? parent = null, int sublayer = 0) {
        var go = new GameObject(name);
        go.transform.SetParent(parent ?? RootObject?.transform);
        go.transform.position = IsoToWorld(pos);

        // Create a simple quad mesh
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        mf.mesh = CreateFlatQuadMesh();
        mr.material = CreateColorMaterial(color);
        mr.sortingOrder = CalculateSortingOrder(pos, sublayer);

        return go;
    }

    /// <summary>
    /// Creates a flat quad mesh lying on XZ plane.
    /// </summary>
    protected Mesh CreateFlatQuadMesh() {
        var mesh = new Mesh();

        TilesRPG halfWidth = TileWidth / 2;
        TilesRPG halfHeight = TileHeight / 2;

        mesh.vertices = new Vector3[] {
            new(-halfWidth, 0, -halfHeight),
            new(-halfWidth, 0, halfHeight),
            new(halfWidth, 0, halfHeight),
            new(halfWidth, 0, -halfHeight)
        };

        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.uv = new Vector2[] {
            new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f)
        };

        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>
    /// Creates a simple unlit color material.
    /// </summary>
    /// <param name="color">Color using ColorRPG.</param>
    /// <returns>A Unity Material with the specified color.</returns>
    protected Material CreateColorMaterial(ColorRPG color) {
        // Try multiple shaders in order of preference
        var shaderNames = new[] {
            "Sprites/Default",
            "Universal Render Pipeline/Unlit",
            "Unlit/Color",
            "Standard"
        };

        Shader? shader = null;
        foreach (var shaderName in shaderNames) {
            shader = Shader.Find(shaderName);
            if (shader != null) break;
        }

        Material mat;
        if (shader != null) {
            mat = new Material(shader);
        } else {
            // Ultimate fallback
            var tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mat = new Material(tempQuad.GetComponent<MeshRenderer>().sharedMaterial);
            UnityEngine.Object.DestroyImmediate(tempQuad);
        }

        mat.color = color; // Implicit conversion

        // Handle URP base color
        if (mat.HasProperty("_BaseColor")) {
            mat.SetColor("_BaseColor", color);
        }

        // Handle transparency
        if (color.Alpha < 1f) {
            SetMaterialTransparent(mat);
        }

        return mat;
    }

    /// <summary>
    /// Configures a material for transparency.
    /// </summary>
    private void SetMaterialTransparent(Material mat) {
        if (mat.HasProperty("_Mode")) {
            mat.SetFloat("_Mode", 3); // Transparent
        }

        if (mat.HasProperty("_Surface")) {
            mat.SetFloat("_Surface", 1); // Transparent for URP
        }

        if (mat.HasProperty("_SrcBlend")) {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }
        if (mat.HasProperty("_DstBlend")) {
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        if (mat.HasProperty("_ZWrite")) {
            mat.SetInt("_ZWrite", 0);
        }

        mat.renderQueue = 3000;
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }

    /// <summary>
    /// Destroys all child objects of the root.
    /// </summary>
    protected void ClearAllObjects() {
        if (RootObject == null) return;

        // Destroy all children
        for (int i = RootObject.transform.childCount - 1; i >= 0; i--) {
            var child = RootObject.transform.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Destroys the root object and all children.
    /// </summary>
    protected void DestroyRoot() {
        if (RootObject != null) {
            UnityEngine.Object.Destroy(RootObject);
            RootObject = null;
        }
    }

    #endregion

    #region Volume and Bounds Helpers

    /// <summary>
    /// Creates an IsoVolume at the specified position with given dimensions.
    /// </summary>
    /// <param name="origin">Origin position of the volume.</param>
    /// <param name="width">Width in tiles.</param>
    /// <param name="depth">Depth in tiles.</param>
    /// <param name="height">Height in levels.</param>
    /// <returns>A new IsoVolume.</returns>
    protected IsoVolume CreateVolume(IsoPos origin, int width, int depth, int height) {
        return new IsoVolume(origin, width, depth, height);
    }

    /// <summary>
    /// Checks if a ray intersects with a volume.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="volume">The volume to test against.</param>
    /// <returns>True if the ray intersects the volume.</returns>
    protected bool RayIntersectsVolume(IsoRay ray, IsoVolume volume) {
        return volume.Contains(ray.Origin);
    }

    /// <summary>
    /// Gets the world bounds of an IsoVolume.
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <returns>Unity Bounds in world space.</returns>
    protected Bounds GetWorldBounds(IsoVolume volume) {
        return volume.ToWorldBounds(LevelHeight);
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Initializes the renderer. Called before first Render().
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Renders the world. Creates/updates all visual elements.
    /// </summary>
    public abstract void Render();

    /// <summary>
    /// Updates the renderer each frame (hover, animations, etc.).
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public virtual void Update(float deltaTime) { }

    /// <summary>
    /// Handles a click at the specified world position.
    /// </summary>
    /// <param name="worldPosition">World position of the click.</param>
    public abstract void HandleClick(Vector3 worldPosition);

    /// <summary>
    /// Handles mouse hover at the specified world position.
    /// </summary>
    /// <param name="worldPosition">World position of the hover.</param>
    public abstract void HandleHover(Vector3 worldPosition);

    /// <summary>
    /// Clears hover state when mouse leaves the render area.
    /// </summary>
    public virtual void ClearHover() {
        HoveredPosition = null;
    }

    /// <summary>
    /// Cleans up all resources and destroys GameObjects.
    /// </summary>
    public abstract void Cleanup();

    #endregion

    #region Debug

    /// <summary>
    /// Draws debug gizmos. Called from IsoDebugRenderer.
    /// </summary>
    public virtual void DrawDebugGizmos() { }

    /// <summary>
    /// Gets debug information about the current state.
    /// </summary>
    public virtual string GetDebugInfo() {
        return $"Active: {IsActive}, Initialized: {IsInitialized}, " +
               $"TileSize: {TileWidth}, LevelHeight: {LevelHeight}, " +
               $"Hovered: {HoveredPosition?.ToString() ?? "none"}, " +
               $"Selected: {SelectedPosition?.ToString() ?? "none"}";
    }

    #endregion
}